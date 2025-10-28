module ElectronApi.Json.Parser.SourceMapper

open System.Collections.Generic
open System.ComponentModel
open ElectronApi.Json.Parser.FSharpApi
open Fantomas.Core
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open Fantomas.Utils

module ModuleC =
    [<RequireQualifiedAccess>]
    type ModuleOrName =
        | NestedModule of ModuleContainer
        | TypeOrBinding of string
    and ModuleContainer = Dictionary<string option, ModuleOrName list>
    type NamespaceContainer = NamespaceContainer of name: string * container: Dictionary<string option, ModuleContainer>
    module private NamespaceContainer =
        let name (NamespaceContainer(name,_)) = name
        let container (NamespaceContainer(_,container)) = container
        let create name = NamespaceContainer(name, Dictionary<string option, ModuleContainer>())
    let createNamespace = NamespaceContainer.create

// 1. ApiDecoder - parses the json directly into F#
// 2. FSharpApi - reads and maps; modifies names and ensures everything is accounted
// 3. SourceMapper - maps the types to their respective end types, including attributes et al

/// <summary>
/// <para>There are multitude of option objects, delegates, or structures that might/do have to be
/// lifted into types while maintaining a path of entry that is sensible.</para>
/// <para>The <c>TypeCache</c> contains helpers and dictionary caches to assist by allowing us to
/// cache a type that needs to be lifted when we are preparing the source generation.</para>
/// <para>By using a dictionoary, we can catch potential conflicts or other issues, and treat
/// them at the call site.</para>
/// </summary>
/// <remarks>
/// <para>Internally, there are two caches.</para>
/// <para>One cache caches a <c>FSharpApi.Type</c> against the <c>string</c> formatted path
/// of the object.</para>
/// <para>The second cache, stores the <c>Path.PathKey</c> against the <c>string</c> formatted key for the first cache.</para>
/// <para>This acts as a fallback.</para>
/// </remarks>
module TypeCache =
    module private Cache =
        let stringCache = Dictionary<string, FSharpApi.Type>()
        let stringKeyCache = Dictionary<Path.PathKey, string>()
        let add key value =
            try
            key
            |> Path.tracePathOfEntry
            |> List.map (_.ValueOrModified >> _.Trim(''') >> toPascalCase)
            |> String.concat "."
            |> fun stringKey ->
               stringKeyCache.Add(key,stringKey)
               stringCache.Add(stringKey, value)
            with e ->
                printfn $"%s{e.Message}\n%A{key}\nNewValue: %A{value}\nCurrentKey:%A{stringKeyCache[key]}\nCurrentValue: %A{stringCache[stringKeyCache[key]]}\n\n%A{e.StackTrace}"
                #if !DEBUG
//                 reraise()
                #endif
        let retrieveByString str =
            match stringCache.TryGetValue str with
            | true, value -> ValueSome value
            | _ -> ValueNone
        let retrieveByPath path =
            match stringKeyCache.TryGetValue path with
            | true, key -> retrieveByString key
            | _ -> ValueNone
        let getAllTypeValues () = stringCache.Values
    let addObject ({ StructOrObject.PathKey = pathKey } as object) =
        Cache.add pathKey (Type.Object object)
        pathKey
        |> Path.tracePathOfEntry
        |> List.map (_.ValueOrModified >> toPascalCase)
        |> String.concat "."
    let add = Cache.add
    let getByString = Cache.retrieveByString
    let getByPath = Cache.retrieveByPath
    let getAllTypeValues = Cache.getAllTypeValues
module EventConstants =
    let private cache = HashSet<Path.PathKey>()
    let addEvent (event: Event) =
        cache.Add(event.PathKey)
    type private ModulePath =
        { Module: Name; Class: Name option; Event: Name }
        static member Create(path: Path.PathKey) =
            Path.tracePathOfEntry path
            |> function
                | [] -> failwith $"path {path} is empty. Cannot make Event Constant"
                | [ _ ] -> failwith $"Cannot make an event {path} without a parent module or type"
                | [ m; e ] ->
                    {
                        Module = m
                        Class = None
                        Event = e
                    }
                | [ m; c; e ] ->
                    {
                        Module = m
                        Class = Some c
                        Event = e
                    }
                | paths -> failwith $"{paths}"
    
    let genEventConstantModule () =
        cache
        |> Seq.toArray
        |> Array.map ModulePath.Create
        |> Array.groupBy _.Module
        |> Array.map (fun (key,arr) ->
            arr
            |> Array.groupBy _.Class
            |> Array.toList
            |> List.collect (fun (maybeClass, events) ->
                events
                |> Array.map (_.Event >> fun eventName ->
                    BindingNode(
                        None,
                        Some (MultipleAttributeListNode.make "Literal"),
                        MultipleTextsNode.make "let",
                        false,
                        None,
                        None,
                        Choice1Of2(IdentListNode.make (eventName.ValueOrModified |> toCamelCase)),
                        None,
                        [],
                        None,
                        SingleTextNode.make "=",
                        Expr.Constant (Constant.FromText(SingleTextNode.make $"\"{eventName.ValueOrSource}\"")),
                        Range.Zero
                        )
                    |> ModuleDecl.TopLevelBinding
                )
                |> Array.toList
                |> fun constants ->
                    match maybeClass with
                    | Some name ->
                        NestedModuleNode(
                            None,
                            Some (MultipleAttributeListNode.make "RequireQualifiedAccess"),
                            SingleTextNode.make "module",
                            None,
                            false,
                            IdentListNode.make (name.ValueOrModified |> toPascalCase),
                            SingleTextNode.make "=",
                            constants,
                            Range.Zero
                            )
                        |> ModuleDecl.NestedModule
                        |> List.singleton
                    | None ->
                        constants
                )
            |> fun decls ->
                NestedModuleNode(None, Some (MultipleAttributeListNode.make "RequireQualifiedAccess"),
                                 SingleTextNode.make "module", None, false, IdentListNode.make key.ValueOrModified,
                                 SingleTextNode.make "=", decls, Range.Zero)
                |> ModuleDecl.NestedModule
            )
        |> Array.toList
        |> fun decls ->
            let xmlDoc = XmlDocNode(
                [|
                    "// THIS FILE IS AUTOMATICALLY GENERATED - DO NOT EDIT"
                    "/// <summary>"
                    "/// Literal strings for the event names of different modules and classes for manual event listening."
                    "/// </summary>"
                |]
                , Range.Zero
            )
            ModuleOrNamespaceNode(
                Some(ModuleOrNamespaceHeaderNode(
                    Some xmlDoc,
                    None,
                    MultipleTextsNode.make "module",
                    None,
                    false,
                    Some (IdentListNode.make $"{Spec.rootNamespace}.Constants.Events"),
                    Range.Zero
                    )),
                decls,
                Range.Zero
                )
    let tryDebugConstantProduction (events: Event list) =
        events
        |> List.iter (addEvent >> ignore)
        |> genEventConstantModule
        |> fun namemod ->
            Oak([], [ namemod ], Range.Zero)
            |> CodeFormatter.FormatOakAsync
            |> Async.RunSynchronously
            |> printfn "%s"

//
// module ModuleNameCache =
//     open Path
//     type Cache = Dictionary<Path.ModulePath, HashSet<Name>>
//     let private cache = Cache()
//     
//     /// <summary>
//     /// Tries to add the entry to the name cache. If it fails then it means there is a conflicting entry.
//     /// </summary>
//     /// <param name="entry"></param>
//     let add (entry: PathKey) =
//         // We try to add a fresh path/set. If that fails, then we just add the entry
//         // to the existing path key.
//         if
//             cache.TryAdd(entry.Path, HashSet([entry.Name]))
//             || cache[entry.Path].Add(entry.Name)
//         // If either of the above return true, then this a unique entry.
//         then Ok entry
//         // If both of the above return false, then this is a duplicate entry
//         else Error entry
//         // If this errors, then you will likely just need to nest the type within
//         // another module

module XmlDocs =
    let makeClosedSeeAlso (link: string) = $"<seealso href=\"{link}\"/>"
    let makeSeeAlso (link: string) (description: string) =
        $"<seealso href=\"{link}\">{description}</seealso>"
    let makeParam (name: string) (description: string) =
        $"<param name=\"{name}\">{description}</param>"
    module Boundaries =
        let [<Literal>] openRemarks = "<remarks>"
        let [<Literal>] closeRemarks = "</remarks>"
        
        let [<Literal>] openSummary = "<summary>"
        let [<Literal>] closeSummary = "</summary>"
        
        let [<Literal>] openExample = "<example>"
        let [<Literal>] closeExample = "</example>"
        
        let [<Literal>] openCode = "<code>"
        let [<Literal>] closeCode = "</code>"
        
        let [<Literal>] openFSharpCode = "<code lang=\"fsharp\">"
        let [<Literal>] closeFSharpCode = closeCode
        let [<Literal>] openPara = "<para>"
        let [<Literal>] closePara = "</para>"
    let [<Literal>] br = "<br/>"
    let inline wrapWith (openBoundary: string) (closeBoundary: string) (contents: string list) = [
        openBoundary
        yield! contents
        closeBoundary
    ]
    let wrapInFSharpCode = wrapWith Boundaries.openFSharpCode Boundaries.closeCode
    let wrapInCode = wrapWith Boundaries.openCode Boundaries.closeCode
    let wrapInExample = wrapWith Boundaries.openExample Boundaries.closeExample
    let wrapInSummary = wrapWith Boundaries.openSummary Boundaries.closeSummary
    let wrapInRemarks = wrapWith Boundaries.openRemarks Boundaries.closeRemarks
    let wrapInPara = wrapWith Boundaries.openPara Boundaries.closePara
    module Helpers =
        let makeCompatibilityLine (compat: Compatibility) =
            let makeEmoji = function
                | true -> "✔"
                | false -> "❌"
            match compat with
            | Unspecific -> ValueNone
            | Specified compatability ->
                [
                    "⚠ OS Compatibility: "
                    "WIN " + makeEmoji compatability.Windows + " | "
                    "MAC " + makeEmoji compatability.Mac + " | "
                    "LIN " + makeEmoji compatability.Linux + " | "
                    "MAS " + makeEmoji compatability.Mas
                ]
                |> ValueSome
        let makeProcessLine (processBlock: Decoder.ProcessBlock) =
            let makeEmoji = function
                | true -> "✔"
                | false -> "❌"
            [
                    "⚠ Process Availability: "
                    "Main " + makeEmoji processBlock.Main + " | "
                    "Renderer " + makeEmoji processBlock.Renderer + " | "
                    "Utility " + makeEmoji processBlock.Utility + " | "
                    "Exported " + makeEmoji processBlock.Exported
            ]
        let chunkDocStringBySize size (docString: string)=
            docString
                .ReplaceLineEndings(br)
                .Split(' ')
            |> Array.chunkBySize size
            |> Array.map (String.concat " ")
            |> Array.map (sprintf "/// %s")
        let makeDocs (docList: string list) =
            docList
            |> List.toArray
            |> Array.collect (chunkDocStringBySize 20)

module Type =
    type internal ApiType = FSharpApi.Type
    type internal FcsType = Fantomas.Core.SyntaxOak.Type
    let inline internal makeSimple text = FcsType.Anon (SingleTextNode.make text)
    type internal Fantomas.Core.SyntaxOak.Type with
        static member boolean = makeSimple "bool"
        static member integer = makeSimple "int"
        static member float = makeSimple "float"
        static member double = makeSimple "double"
        static member string = makeSimple "string"
        static member obj = makeSimple "obj"
        static member option = makeSimple "option"
        static member unit = makeSimple "unit"
        static member Option = makeSimple "Option"
        static member Promise = makeSimple "Promise"
    type FantomasFactory =
        static member makeUnion(fcsTypes: ApiType list): FcsType =
            // MAPPING KNOWN EXCEPTIONS
            match fcsTypes with
            | typs when typs |> List.forall _.IsPromise ->
                typs
                |> List.map (function
                    | Promise innerType -> innerType
                    | _ -> failwith "UNREACHABLE"
                    )
                |> ApiType.OneOf
                |> List.singleton
                |> FantomasFactory.makePromise
            | [ typ; ApiType.Unit | ApiType.Undefined ] ->
                [ typ ]
                |> FantomasFactory.makeOption
            | [ typ; ApiType.Unknown | ApiType.Any ] ->
                TypeAppPrefixNode.Create(
                    makeSimple "U2",
                    [ FantomasFactory.mapToFantomas typ; FcsType.obj ]
                    )
                |> FcsType.AppPrefix
            | typs ->
                let length = typs.Length
                match length with
                | _ when length < 8 ->
                    TypeAppPrefixNode.Create(
                        makeSimple $"U{length}",
                        typs |> List.map FantomasFactory.mapToFantomas
                        )
                    |> FcsType.AppPrefix
                | _ ->
                    failwith $"Cannot create a U# type union with a length of {length}"
        static member makeCollection(fcsType: FcsType): FcsType =
            TypeArrayNode(fcsType, 1, Range.Zero)
            |> FcsType.Array
        static member makeLambda(funcOrMethod: FuncOrMethod): FcsType =
            let maybeIsDelegate = FuncOrMethod.Cache.tryGet funcOrMethod.Name
            match funcOrMethod.Parameters.Length > 1, maybeIsDelegate with
            | true, ValueSome delegatedFunc ->
                delegatedFunc.Name
                |> Path.tracePathOfEntry
                |> List.map (_.ValueOrModified >> toPascalCase)
                |> String.concat "."
                |> makeSimple
            | _, _ ->
                let normalizedParameters =
                    match funcOrMethod.Parameters with
                    | [] ->
                        [ Parameter.Positional {
                            ParameterInfo.Description = ValueNone
                            Required = true
                            Type = ApiType.Unit
                        } ]
                    | parameters -> parameters
                let parameters =
                    normalizedParameters
                    |> List.map (function
                        | Named(_,info) 
                        | Positional info ->
                            let wrap: FcsType -> _ =
                                if info.Required
                                then id
                                else FantomasFactory.makeOption
                            info.Type
                            |> FantomasFactory.mapToFantomas
                            |> wrap
                        | InlinedObjectProp _ -> failwith "Lambdas should not have inlined object props"
                        )
                let names =
                    normalizedParameters
                    |> List.mapi(fun i _ ->
                        if i = normalizedParameters.Length - 1 then
                            SingleTextNode.make "->"
                        else SingleTextNode.make "*"
                        )
                let parameterNamePairs =
                    names
                    |> List.zip parameters
                TypeFunsNode(
                    parameterNamePairs,
                    funcOrMethod.Returns
                    |> FantomasFactory.mapToFantomas,
                    range = Range.Zero
                ) |> FcsType.Funs
        static member makePromise (apiTypes: ApiType list): FcsType =
            TypeAppPrefixNode.Create(
                FcsType.Promise,
                apiTypes |> List.map FantomasFactory.mapToFantomas
                )
            |> FcsType.AppPrefix
        static member makeOption (apiTypes: ApiType list): FcsType =
            TypeAppPrefixNode.Create(
                FcsType.Option,
                List.map FantomasFactory.mapToFantomas apiTypes
                )
            |> FcsType.AppPrefix
        static member makeOption (fcsType: FcsType): FcsType =
            TypeAppPrefixNode.Create(FcsType.Option, [ fcsType ])
            |> FcsType.AppPrefix
        static member makeTuple(fcsTypes: FcsType list): FcsType =
            TypeTupleNode(
                fcsTypes
                |> List.collect (fun t ->
                    [ Choice1Of2 t
                      Choice2Of2 (SingleTextNode.make "*") ])
                |> List.cutOffLast,
                Range.Zero
                )
            |> FcsType.Tuple
        static member makeTuple(apiTypes: ApiType list): FcsType =
            apiTypes
            |> List.map FantomasFactory.mapToFantomas
            |> FantomasFactory.makeTuple
        static member mapToFantomas (apiType: ApiType): FcsType =
            match apiType with
            | ApiType.Any | FSharpApi.Type.Undefined | ApiType.Unknown -> FcsType.obj
            | ApiType.Unit -> FcsType.unit
            | FSharpApi.Type.Boolean -> FcsType.boolean
            | FSharpApi.Type.Date -> FcsType.LongIdent(IdentListNode.make [ "System"; "DateTime" ])
            | FSharpApi.Type.Double -> FcsType.double
            | FSharpApi.Type.Float | FSharpApi.Type.Number -> FcsType.float
            | FSharpApi.Type.Integer -> FcsType.integer
            | FSharpApi.Type.String -> FcsType.string
            | FSharpApi.Type.StructureRef value ->
                makeSimple value
                // Name.retrieveName value
                // |> function
                //     | ValueNone ->
                //         makeSimple value
                //     | ValueSome name ->
                //         makeSimple name.ValueOrModified
            | Function funcOrMethod ->
                FantomasFactory.makeLambda funcOrMethod
            | Event { PathKey = pathKey } when pathKey.Name.ValueOrModified = "Event" ->
                makeSimple "Event"
            | Event { PathKey = pathKey }
            | Object { PathKey = pathKey }
            | StringEnum { PathKey = pathKey } ->
                if TypeCache.getByPath pathKey |> _.IsNone
                then TypeCache.add pathKey apiType
                Path.tracePathOfEntry pathKey
                |> List.map (_.ValueOrModified >> _.Trim(''') >> toPascalCase)
                |> String.concat "."
                |> makeSimple
            // | Object { PathKey = pathKey } ->
            //     // If we could have inlined it; it would have been done.
            //     // These objects have to be cached to be turned into types.
            //     if TypeCache.getByPath structOrObject.PathKey |> _.IsNone
            //     then TypeCache.add structOrObject.PathKey apiType
            //     // TypeCache.addObject structOrObject
            //     Path.tracePathOfEntry structOrObject.PathKey
            //     |> List.map (_.ValueOrModified >> _.Trim(''') >> toPascalCase)
            //     |> String.concat "."
            //     |> makeSimple
            | Promise innerType ->
                FantomasFactory.makePromise [innerType]
            | Record(key, value) ->
                TypeAppPrefixNode.Create(
                    makeSimple "Record",
                    [ key; value ] |> List.map FantomasFactory.mapToFantomas
                    )
                |> Type.AppPrefix
            | EventRef ``type`` ->
                FantomasFactory.mapToFantomas ``type``
            | Partial(``type``, _) ->
                // TODO - implementation that accounts for string args
                FantomasFactory.mapToFantomas ``type``
            | Omit(``type``, l) ->
                // TODO - implementation that accounts for string args
                FantomasFactory.mapToFantomas ``type``
            | Pick(``type``, l) ->
                // TODO - implementation that accounts for string args
                FantomasFactory.mapToFantomas ``type``
            | Collection ``type`` ->
                FantomasFactory.mapToFantomas ``type``
                |> FantomasFactory.makeCollection
            | Array ``type`` ->
                FantomasFactory.mapToFantomas ``type``
                |> FantomasFactory.makeCollection
            | OneOf [
                    FSharpApi.Type.StructureRef "TouchBarButton"
                    FSharpApi.Type.StructureRef "TouchBarColorPicker"
                    FSharpApi.Type.StructureRef "TouchBarGroup"
                    FSharpApi.Type.StructureRef "TouchBarLabel"
                    FSharpApi.Type.StructureRef "TouchBarPopover"
                    FSharpApi.Type.StructureRef "TouchBarScrubber"
                    FSharpApi.Type.StructureRef "TouchBarSegmentedControl"
                    FSharpApi.Type.StructureRef "TouchBarSlider"
                    FSharpApi.Type.StructureRef "TouchBarSpacer"
                ] ->
                    FantomasFactory.mapToFantomas (FSharpApi.Type.StructureRef Injections.touchBarItemsName)
            | OneOf types when types |> List.truncate 9 |> (=) [
                    FSharpApi.Type.StructureRef "TouchBarButton"
                    FSharpApi.Type.StructureRef "TouchBarColorPicker"
                    FSharpApi.Type.StructureRef "TouchBarGroup"
                    FSharpApi.Type.StructureRef "TouchBarLabel"
                    FSharpApi.Type.StructureRef "TouchBarPopover"
                    FSharpApi.Type.StructureRef "TouchBarScrubber"
                    FSharpApi.Type.StructureRef "TouchBarSegmentedControl"
                    FSharpApi.Type.StructureRef "TouchBarSlider"
                    FSharpApi.Type.StructureRef "TouchBarSpacer"
                ] ->
                    FSharpApi.Type.OneOf [
                        yield FSharpApi.Type.StructureRef Injections.touchBarItemsName
                        yield! types |> List.skip 9
                    ]
                    |> FantomasFactory.mapToFantomas 
            | OneOf types ->
                FantomasFactory.makeUnion types
            | Constant literalType ->
                // match literalType with
                // | LiteralType.Float f -> failwith "todo"
                // | LiteralType.Int i -> failwith "todo"
                // | LiteralType.String s -> failwith "todo"
                // | LiteralType.Char c -> failwith "todo"
                // TODO - where this is encountered, we actually have
                // to emit the expression so we can add the constant arg.
                FcsType.string
            | Tuple types -> FantomasFactory.makeTuple types
            | Join(structureRef, props) ->
                // TODO - for the ONE case that this occurs =,=
                makeSimple structureRef
        static member tryDebugMapping (apiTypes: ApiType list) =
            let cache = ResizeArray<FcsType>()
            try
            ApiType.Tuple([Type.Number; Type.Number]) :: apiTypes
            |> List.iter (FantomasFactory.mapToFantomas >> cache.Add)
            with e ->
                printfn $"%A{e}"
            let binds =
                cache
                |> Seq.mapi (fun i typ ->
                    BindingNode(
                        None,
                        None,
                        MultipleTextsNode.make "let",
                        false,
                        None,
                        None,
                        Choice1Of2 (IdentListNode.make $"type{i}"),
                        None,
                        [],
                        BindingReturnInfoNode(
                            SingleTextNode.make ":",
                            typ,
                            Range.Zero
                            )
                        |> Some,
                        SingleTextNode.make "=",
                        Expr.Constant(Constant.Unit <| UnitNode(SingleTextNode.make "(", SingleTextNode.make ")", Range.Zero)),
                        Range.Zero
                        )
                    |> ModuleDecl.TopLevelBinding
                    )
                |> Seq.toList
            Oak([], [ ModuleOrNamespaceNode(None, binds, Range.Zero) ], Range.Zero)
            |> CodeFormatter.FormatOakAsync
            |> Async.RunSynchronously
            |> printfn "%s"

type GeneratorContainer = {
    PathKey: Path.PathKey
    Constructor: Parameter list option
    InstanceProperties: Property list
    InstanceMethods: Method list
    InstanceEvents: Event list
    StaticProperties: Property list
    StaticMethods: Method list
    StaticEvents: Event list
    TypeAttributes: string list
    Description: string voption
    Process: Decoder.ProcessBlock option
    Compatibility: Compatibility option
}

module GeneratorContainer =
    let create path = {
        PathKey = path
        Constructor = None
        InstanceProperties = []
        InstanceMethods = []
        InstanceEvents = []
        StaticProperties = []
        StaticMethods = []
        StaticEvents = []
        TypeAttributes = []
        Description = ValueNone
        Process = None
        Compatibility = None
    }
    let withConstructor (props: Parameter list) container =
        { container with GeneratorContainer.Constructor = Some props }
    let withInstanceProperties (props: Property list) container =
        { container with InstanceProperties = props }
    let withInstanceMethods (methods: Method list) container =
        { container with InstanceMethods = methods }
    let withInstanceEvents (events: Event list) container =
        { container with InstanceEvents = events }
    let withStaticProperties (props: Property list) container =
        { container with GeneratorContainer.StaticProperties = props }
    let withStaticMethods (methods: Method list) container =
        { container with GeneratorContainer.StaticMethods = methods }
    let withStaticEvents (events: Event list) container =
        { container with GeneratorContainer.StaticEvents = events }
    let withAttribute (attr: string) container =
        { container with TypeAttributes = attr :: container.TypeAttributes }
    let withAttributes (attr: string list) container =
        { container with TypeAttributes = attr }
    let makeImported container =
        withAttribute $"Import(\"{container.PathKey.Name.ValueOrSource}\", \"electron\")" container
    let inline mergeCompatability (object: ^T when ^T : (member Compatibility: Compatibility)) container=
        { container with GeneratorContainer.Compatibility = Some object.Compatibility }
    let inline mergeDescription (object: ^T when ^T : (member Description: string voption)) container =
        { container with GeneratorContainer.Description = object.Description }
    let inline mergeProcess (object: ^T when ^T : (member Process: Decoder.ProcessBlock)) container =
        { container with GeneratorContainer.Process = Some object.Process }
    let private makeSimpleInterfaceNode name=
        MemberDefnInterfaceNode(
            SingleTextNode.make "interface",
            Type.makeSimple name,
            None, [], Range.Zero
            )
    let inline private makeEventEmitterInterface (object: ^T when ^T : (member Events: Event list) and ^T : (member Description: string voption)) =
        [
            if
                object.Events.Length > 0
                || object.Description
                |> ValueOption.exists _.Contains("is an EventEmitter", System.StringComparison.Ordinal)
            then
                makeSimpleInterfaceNode Injections.eventEmitterName
        ]
    let private makeParamObjectParameterAttribute (parameters: Parameter list) =
        parameters
        |> List.tryFindIndex _.IsInlinedObjectProp
        |> Option.map (
            fun idx -> $"ParamObject({idx})"
            >> List.singleton
            )
        |> Option.defaultValue []
        
    let private makeNamedNode (attributes: string list option) required typ name =
        PatParameterNode(
            attributes |> Option.map MultipleAttributeListNode.make,
            Pattern.Named (PatNamedNode(
                None,
                (match required with
                | true -> SingleTextNode.make name
                | false -> SingleTextNode.makeOptional name),
                Range.Zero
                )),
            Some typ,
            Range.Zero
        )
        
    let private makeMappedNamedNode (attributes: string list option) required typ name =
        match name with
        | "...args" ->
            struct {|
                Attributes =
                    attributes
                    |> function
                        | Some attrs ->  Some ("System.ParamArray" :: attrs)
                        | None -> Some ([ "System.ParamArray" ])
                Type = typ
                Required = required
                Name = "args"
            |}
        | _ ->
            struct {|
                Attributes = attributes
                Type = typ
                Required = required
                Name = name
            |}
        |> fun data ->
            makeNamedNode
                data.Attributes
                data.Required
                data.Type
                data.Name
        
    let private makePropParameterDocLine (prop: Property) =
        [
            match prop.Compatibility |> XmlDocs.Helpers.makeCompatibilityLine with
            | ValueSome docs -> docs |> String.concat ""
            | _ -> ()
            match prop.Description with
            | ValueSome docs -> docs
            | _ -> ()
        ]
        |> function
            | [] -> ""
            | docs ->
                docs |> String.concat " || "
        |> XmlDocs.makeParam prop.PathKey.Name.ValueOrModified
    let private makeParameterDocLine (parameter: ParameterInfo) (name: string) =
        parameter.Description
        |> ValueOption.defaultValue ""
        |> XmlDocs.makeParam name
    let private makePropMemberDocsLines (prop: Property) =
        [
            match prop.Compatibility |> XmlDocs.Helpers.makeCompatibilityLine with
            | ValueSome docs -> docs |> XmlDocs.wrapInPara |> String.concat ""
            | ValueNone -> ()
            match prop.Description with
            | ValueSome docs -> docs
            | _ -> ()
        ]
        |> function
            | [] -> None
            | docs ->
                docs
                |> Some
    let private makeParametersXmlDocs (parameter: Parameter list) =
        parameter |> List.mapi (fun idx -> function
        | Positional parameterInfo -> makeParameterDocLine parameterInfo $"arg{idx}"
        | Named(name, info) -> makeParameterDocLine info name.Name.ValueOrModified
        | InlinedObjectProp prop -> makePropParameterDocLine prop
            )

    let private makeDocNode (wrapperFunction: string list -> string list) (lines: string list) = lines |> function
        | [] -> None
        | docs ->
            docs
            |> wrapperFunction
            |> XmlDocs.Helpers.makeDocs
            |> fun docs ->
                XmlDocNode(docs, Range.Zero)
                |> Some
    let private makePropParameter (prop: Property) =
        let typ = prop.Type |> Type.FantomasFactory.mapToFantomas
        let required = prop.Required
        let name = prop.PathKey.Name.ValueOrModified
        let attributes = None
        makeMappedNamedNode attributes required typ name
    let private makePropParameterWithDirectives (prop: Property) =
        [
            makePropParameter prop
            |> Compatibility.wrapInCompatibilityDirectiveBefore prop.Compatibility
            |> Pattern.Parameter
            |> Choice1Of2
            SingleTextNode.make ","
            |> Compatibility.wrapInCompatibilityDirectiveAfter prop.Compatibility
            |> Choice2Of2
        ]
    let makeReturnInfoNode typ =
        BindingReturnInfoNode(SingleTextNode.make ":", Type.FantomasFactory.mapToFantomas typ, Range.Zero)
    let unitReturnInfo = makeReturnInfoNode Type.Unit
    let private cutOffLastTextNodeWithDirectiveMaybe (compatibility: Compatibility)  (parameters: Choice<Pattern, SingleTextNode> list) =
        match compatibility, parameters with
        | Specified _, []
        | Unspecific, _ ->
            parameters |> List.cutOffLast
        | Specified _, _ ->
            parameters
            |> List.cutOffLast
            |> fun hangingDirective ->
                [
                    yield! hangingDirective |> List.cutOffLast
                    hangingDirective
                    |> List.last
                    |> function
                        | Choice1Of2 (Pattern.Parameter para) ->
                            para
                            |> Compatibility.wrapInCompatibilityDirectiveAfter compatibility
                            |> Pattern.Parameter
                            |> Choice1Of2
                        | _ -> failwith "UNREACHABLE"
                ]
    let private makeParameter (parameter: ParameterInfo) (name: string) =
        let typ = parameter.Type |> Type.FantomasFactory.mapToFantomas
        let required = parameter.Required
        let attributes = None
        makeMappedNamedNode attributes required typ name
    let private makePositionalParameter (idx: int) (parameterInfo: ParameterInfo) =
        [
            $"arg%i{idx}"
            |> makeParameter parameterInfo
            |> Pattern.Parameter
            |> Choice1Of2
            SingleTextNode.make ","
            |> Choice2Of2
        ]
    let private makeNamedParameter (name: Path.PathKey) (parameterInfo: ParameterInfo) =
        [
            name.Name.ValueOrModified
            |> makeParameter parameterInfo
            |> Pattern.Parameter
            |> Choice1Of2
            SingleTextNode.make ","
            |> Choice2Of2
        ]
    let private makeFunsPattern (typ: Type.ApiType) =
        typ |> Type.FantomasFactory.mapToFantomas,
        SingleTextNode.make "->"
    let private makeFunsTypeNode returnInfo (types: Type.ApiType list) =
        types |> List.map makeFunsPattern
        |> function
            | [] ->
                (Type.makeSimple "unit", SingleTextNode.make "->")
                |> List.singleton
            | typs -> typs
        |> fun typs ->
            TypeFunsNode(typs, returnInfo, Range.Zero)
            |> Type.Funs
    let private makeFunsUnitTypeNode (types: Type.ApiType list) =
        types
        |> makeFunsTypeNode (Type.makeSimple "unit")
    let private makeFunsParameter (parameterInfo: Parameter) =
        parameterInfo
        |> function
            | Positional { Type = typ }
            | InlinedObjectProp { Type = typ }
            | Named(_, { Type = typ }) ->
                makeFunsPattern typ
    let private makeFunsTypeNodeFromParameters returnInfo (parameters: Parameter list) =
        parameters
        |> List.map makeFunsParameter
        |> function
            | [] ->
                makeFunsPattern Type.ApiType.Unit
                |> List.singleton
            | patts -> patts
        |> fun typeFunsPat ->
            TypeFunsNode(
                typeFunsPat,
                returnInfo,
                Range.Zero
                )
            |> Type.Funs
    let private makeUnitFunsTypeNode parameters =
        parameters
        |> makeFunsTypeNodeFromParameters (Type.makeSimple "unit")
    
    let private makeUnitFunsParameterNode required name parameters =
        (makeUnitFunsTypeNode parameters, name)
        ||> makeNamedNode None required
                
    let private wrapParametersIntoParenNode (parameters: Choice<Pattern, SingleTextNode> list) =
        PatTupleNode(parameters, Range.Zero)
        |> Pattern.Tuple
        |> PatParenNode.make
        |> Pattern.Paren
    let private makeAttributesNode (attributes: string list) =
        attributes
        |> function
            | [] -> None
            | attrs -> MultipleAttributeListNode.make attrs |> Some
    let inline private makeAttributesForStability object =
        [
            if StabilityStatus.IsExperimental object then
                "Experimental(\"Experimental according to Electron\")"
            if StabilityStatus.IsDeprecated object then
                "System.Obsolete"
        ]
    let private makeAttributesForStaticProperty (prop: Property) =
        let isStropped: string -> bool = fun str -> str.StartsWith("``") && str.EndsWith("``")
        let isSourceName =
            prop.PathKey.Name |> function
                | Modified(_, text) -> isStropped text
                | Source _ -> true
        [
            if not isSourceName then
                $"Emit(\"{prop.PathKey.ParentName.ValueOrSource}.{prop.PathKey.Name.ValueOrSource}{{{{ = $0 }}}}\")"
            yield! makeAttributesForStability prop
        ]
    let private makeAttributesForInstanceProperty (prop: Property) =
        let isStropped: string -> bool = fun str -> str.StartsWith("``") && str.EndsWith("``")
        let isSourceName =
            prop.PathKey.Name |> function
                | Modified(_, text) -> isStropped text
                | Source _ -> true
        [
            if not isSourceName then
                $"Emit(\"$0.{prop.PathKey.Name.ValueOrSource}{{{{ = $1 }}}}\")"
            yield! makeAttributesForStability prop
        ]
    let private makePropertyMemberDefn (isStatic: bool) (prop: Property) =
        let isStropped: string -> bool = fun str -> str.StartsWith("``") && str.EndsWith("``")
        let isSourceName =
            prop.PathKey.Name |> function
                | Modified(_, text) -> isStropped text
                | Source _ -> true
        let docs =
            makePropMemberDocsLines prop
            |> Option.bind (makeDocNode XmlDocs.wrapInSummary)
        let attributes =
            if isStatic then
                makeAttributesForStaticProperty prop
            else
                makeAttributesForInstanceProperty prop
            |> makeAttributesNode
        match isSourceName with
        | true ->
            MemberDefnAutoPropertyNode(
                docs,
                attributes,
                MultipleTextsNode.make [
                    if isStatic then "static"
                    "member"; "val"
                ],
                None,
                SingleTextNode.make prop.PathKey.Name.ValueOrSource,
                Some (prop.Type |> Type.FantomasFactory.mapToFantomas),
                SingleTextNode.make "=",
                Expr.Ident(SingleTextNode.make "Unchecked.defaultof<_>"),
                Some (MultipleTextsNode.make (if prop.ReadOnly then "with get" else "with get, set")),
                Range.Zero
                )
            |> Choice1Of2
        | false ->
            MemberDefnPropertyGetSetNode(
                docs,
                attributes,
                MultipleTextsNode.make [
                    if isStatic then "static"
                    "member"
                ], None, None,
                IdentListNode.make [
                    if not isStatic then "_"
                    prop.PathKey.Name.ValueOrModified
                ],
                SingleTextNode.make "with",
                PropertyGetSetBindingNode(
                    None,None,None,SingleTextNode.make "get",
                    [
                        PatTupleNode([], Range.Zero)
                        |> Pattern.Tuple
                        |> PatParenNode.make
                        |> Pattern.Paren
                    ],
                    Some(makeReturnInfoNode prop.Type),
                    SingleTextNode.make "=",
                    Expr.Ident(SingleTextNode.make "Unchecked.defaultof<_>"),
                    Range.Zero
                    ),
                (if prop.ReadOnly then None else
                 SingleTextNode.make "and" |> Some),
                (if prop.ReadOnly then
                     None
                else
                    PropertyGetSetBindingNode(
                        None,None,None,
                        SingleTextNode.make "set",
                        [
                            makeNamedNode
                                None
                                true
                                (Type.FantomasFactory.mapToFantomas prop.Type)
                                "value"
                            |> Pattern.Parameter
                            |> PatParenNode.make
                            |> Pattern.Paren
                        ], None, SingleTextNode.make "=",
                        Expr.Ident(SingleTextNode.make "()"), Range.Zero
                        )
                    |> Some
                ),
                Range.Zero
            )
            |> Choice2Of2
    let private makePropertyMemberDefnWithDirectives (isStatic: bool) (prop: Property) =
        prop
        |> makePropertyMemberDefn isStatic
        |> function
            | Choice1Of2 node ->
                node
                |> Compatibility.wrapInCompatibilityDirective prop.Compatibility
                |> MemberDefn.AutoProperty
            | Choice2Of2 node ->
                node
                |> Compatibility.wrapInCompatibilityDirective prop.Compatibility
                |> MemberDefn.PropertyGetSet
    let private makeStaticPropertyMemberDefnWithDirectives = makePropertyMemberDefnWithDirectives true
    let private makeInstancePropertyMemberDefnWithDirectives = makePropertyMemberDefnWithDirectives false
    let private makeStaticBindingNode inlined xmlDocs attributes (name: string) parameters returnInfo =
        BindingNode(
            xmlDocs,
            attributes,
            MultipleTextsNode.make ["static"; "member"],
            inlined,
            None, None,
            Choice1Of2 (IdentListNode.make name),
            None,
            parameters,
            Some returnInfo,
            SingleTextNode.make "=",
            Expr.Ident(SingleTextNode.make "Unchecked.defaultof<_>"),
            Range.Zero
            )
    let private makeStaticBindingNodeWithDirectives compatibility inlined xmlDocs attributes (name: string) parameters returnInfo =
        makeStaticBindingNode inlined xmlDocs attributes name parameters returnInfo
        |> Compatibility.wrapInCompatibilityDirective compatibility
        
    let private makeInstanceBindingNode inlined xmlDocs attributes (name: string) parameters returnInfo =
        BindingNode(
            xmlDocs,
            attributes,
            MultipleTextsNode.make ["member"],
            false,
            (if inlined
             then Some (SingleTextNode.make "inline")
             else None), None,
            Choice1Of2 (IdentListNode.make [ "_"; name ]),
            None,
            parameters,
            Some returnInfo,
            SingleTextNode.make "=",
            Expr.Ident(SingleTextNode.make "Unchecked.defaultof<_>"),
            Range.Zero
            )
    
    let private makeInstanceBindingNodeWithDirectives compatibility inlined xmlDocs attributes (name: string) parameters returnInfo =
        makeInstanceBindingNode inlined xmlDocs attributes name parameters returnInfo
        |> Compatibility.wrapInCompatibilityDirective compatibility
    let makeInterfaces (this: GeneratorContainer) = [
        if
            this.InstanceEvents.Length > 0 || this.StaticEvents.Length > 0
            || this.Description
            |> ValueOption.exists _.Contains("is an EventEmitter", System.StringComparison.Ordinal)
        then
            makeSimpleInterfaceNode Injections.eventEmitterName
            |> MemberDefn.Interface
    ]
    let makeAbstractPropertyNode xmlDocs attributes name typ isWritable =
        MemberDefnAbstractSlotNode(
            xmlDocs,
            attributes,
            MultipleTextsNode.make [
                "abstract"
                "member"
            ],
            SingleTextNode.make name,
            None,
            typ,
            (
                if isWritable then
                    Some(MultipleTextsNode.make [ "with"; "get,"; "set" ] )
                else None
            ),
            Range.Zero
            )
    let makeParameterNodesForParameters (parameters: Parameter list) =
        let mutable idx = 0
        parameters
        |> List.collect(function
            | Parameter.InlinedObjectProp prop ->
                (
                    prop.Type |> Type.FantomasFactory.mapToFantomas
                    , prop
                ) |> Choice1Of2
            | Parameter.Positional info ->
                let name = Name.createCamel $"arg{idx}"
                idx <- idx + 1
                (
                    info.Type |> Type.FantomasFactory.mapToFantomas
                    , name, info
                ) |> Choice2Of2
            | Parameter.Named(name, info) ->
                (
                    info.Type |> Type.FantomasFactory.mapToFantomas
                    , name.Name,
                    info
                ) |> Choice2Of2
            >> function
                | Choice1Of2 (typ,prop) ->
                    [
                        makeNamedNode None prop.Required typ prop.PathKey.Name.ValueOrModified
                        |> Pattern.Parameter
                    ]
                | Choice2Of2(typ,name,paramInfo) ->
                    [
                        makeNamedNode None paramInfo.Required typ name.ValueOrModified
                        |> Pattern.Parameter
                    ]
            )

    let makeConstructor (this: GeneratorContainer) =
        let attributes (parameters: Parameter list) =
            makeParamObjectParameterAttribute parameters
            |> function
                | [] -> None
                | attrs -> MultipleAttributeListNode.make attrs |> Some
        this.Constructor
        |> function
            | None -> None
            | Some parameters ->
                parameters
                |> List.collect(function
                    | Parameter.InlinedObjectProp prop ->
                        (
                            prop.Type |> Type.FantomasFactory.mapToFantomas
                            , prop
                        ) |> Choice1Of2
                    | Parameter.Positional info ->
                        (
                            info.Type |> Type.FantomasFactory.mapToFantomas
                            , Name.createCamel "arg",
                            info
                        ) |> Choice2Of2
                    | Parameter.Named(name, info) ->
                        (
                            info.Type |> Type.FantomasFactory.mapToFantomas
                            , name.Name,
                            info
                        ) |> Choice2Of2
                    >> function
                        | Choice1Of2 (typ,prop) ->
                            [
                                makeNamedNode None prop.Required typ prop.PathKey.Name.ValueOrModified
                                |> Pattern.Parameter
                                |> Choice1Of2
                                SingleTextNode.make ","
                                |> Choice2Of2
                            ]
                        | Choice2Of2(typ,name,paramInfo) ->
                            [
                                makeNamedNode None paramInfo.Required typ name.ValueOrModified
                                |> Pattern.Parameter
                                |> Choice1Of2
                                SingleTextNode.make ","
                                |> Choice2Of2
                            ]
                    )
                |> wrapParametersIntoParenNode
                |> fun pat ->
                    ImplicitConstructorNode(
                        makeParametersXmlDocs parameters
                        |> makeDocNode id,
                        attributes parameters,
                        None,
                        pat,
                        None,
                        Range.Zero
                        )
                |> Some
                    
    let makeMethodAttributes (method: Method) =
        [
            match method.PathKey.Name with
            | Modified(source,_) -> $"CompiledName(\"{source}\")" 
            | Source _ -> ()
            
            yield!
                method.Parameters
                |> makeParamObjectParameterAttribute
            
            yield!
                method
                |> makeAttributesForStability
        ]
    let inline makeCompatibilityXmlDocs (wrapperFunction: string list -> string list) (object: ^T when ^T : (member Compatibility: Compatibility)) =
        XmlDocs.Helpers.makeCompatibilityLine object.Compatibility
        |> ValueOption.map wrapperFunction
        |> ValueOption.defaultValue []
    let inline makeDescriptionXmlDocs (object: ^T when ^T : (member Description: string voption)) =
        object.Description |> ValueOption.map List.singleton |> ValueOption.defaultValue []
    let inline makeProcessXmlDocs (object: ^T when ^T : (member Process: Decoder.ProcessBlock)) =
        object.Process |> XmlDocs.Helpers.makeProcessLine
    let inline makeParameterXmlDocs (object: ^T when ^T : (member Parameters: Parameter list)) =
        object.Parameters
        |> makeParametersXmlDocs
    let makeMethodBindingNode isInline (isStatic: bool) (method: Method) =
        let xmlDocs =
            method |> makeParameterXmlDocs
            |> List.append (
                [
                    yield! method |> makeCompatibilityXmlDocs (String.concat "" >> List.singleton >> XmlDocs.wrapInPara)
                    yield! method |> makeDescriptionXmlDocs
                ]
                |> XmlDocs.wrapInSummary
            )
            |> makeDocNode id
        let attributes = makeMethodAttributes method |> makeAttributesNode
        (if isStatic then
            makeStaticBindingNodeWithDirectives
        else
            makeInstanceBindingNodeWithDirectives)
                method.Compatibility
                isInline
                xmlDocs
                attributes
                method.PathKey.Name.ValueOrModified
                (method.Parameters |> List.mapi ( fun idx ->
                    function
                    | Positional info ->
                        makePositionalParameter idx info
                    | Named(name,info) ->
                        makeNamedParameter name info
                    | InlinedObjectProp prop ->
                        [
                            makePropParameter prop
                            |> Pattern.Parameter
                            |> Choice1Of2
                            Choice2Of2 (SingleTextNode.make ",")
                        ]
                    )
                |> List.collect id
                |> cutOffLastTextNodeWithDirectiveMaybe method.Compatibility
                |> wrapParametersIntoParenNode
                |> List.singleton
                // |> function
                    // | [] ->
                        // Pattern.Unit (UnitNode(PatParenNode.makeOpening, PatParenNode.makeClosing, Range.Zero))
                        // |> List.singleton
                    // | pats -> pats
                    )
                (makeReturnInfoNode method.ReturnType)
    let makeEventAttributes (isStatic: bool) (functionPrefix: string) (event: Event) =
        [
            $"Emit(\"$0.{functionPrefix}('{event.PathKey.Name.ValueOrSource}', $1)\")"
            if isStatic then
                $"Import(\"{event.PathKey.ParentName.ValueOrSource}\", \"electron\")"
        ]
    let makeEventBindingNode (isInline: bool) (isStatic: bool) (event: Event) =
        let prefixes = [
            "on"
            "once"
            "off"
        ]
        let docs =
            [
                yield!
                    event
                    |> makeCompatibilityXmlDocs (String.concat "" >> List.singleton >> XmlDocs.wrapInPara)
                yield!
                    event
                    |> makeDescriptionXmlDocs
            ]
            |> makeDocNode XmlDocs.wrapInSummary
        prefixes |> List.map (fun functionPrefix ->
            let attributes =
                makeEventAttributes isStatic functionPrefix event
                |> makeAttributesNode
            let parameter =
                makeUnitFunsParameterNode true "handler" event.Parameters
                |> Pattern.Parameter
                |> Choice1Of2
                |> List.singleton
                |> wrapParametersIntoParenNode
                |> List.singleton
            (if isStatic then
                makeStaticBindingNodeWithDirectives
            else
                makeInstanceBindingNodeWithDirectives)
                    event.Compatibility
                    isInline
                    docs
                    attributes
                    $"{functionPrefix}{event.PathKey.Name.ValueOrModified}"
                    parameter
                    unitReturnInfo
            )
        
    let makeXmlDocs (this: GeneratorContainer) =
        [
            match this.Process with
            | Some text ->
                text
                |> XmlDocs.Helpers.makeProcessLine
                |> XmlDocs.wrapInPara
                |> String.concat ""
            | None -> ()
            match this.Compatibility with
            | Some compat ->
                match
                    compat
                    |> XmlDocs.Helpers.makeCompatibilityLine
                    |> ValueOption.map(
                        XmlDocs.wrapInPara
                        >> String.concat ""
                        )
                with
                | ValueSome text -> text
                | ValueNone -> ()
            | None -> ()
            match this.Description with
            | ValueSome text ->
                text
            | ValueNone -> ()
        ]
        |> function
            | [] -> None
            | docs -> Some docs
        |> Option.map (
            XmlDocs.wrapInSummary
            >> XmlDocs.Helpers.makeDocs
            >> fun docs ->
                XmlDocNode(docs, Range.Zero)
            )
    let makeTypeNameNodeWithNameMap (mapper: Path.PathKey -> string) (this: GeneratorContainer) =
        let docs = makeXmlDocs this
        let attributes =
            match this.TypeAttributes with
            | [] -> None
            | attrs ->
                MultipleAttributeListNode.make attrs
                |> Some
        let name = IdentListNode.make (mapper this.PathKey)
        TypeNameNode(
            docs, attributes,
            SingleTextNode.make "type",
            None, name,
            None, [],
            (this |> makeConstructor),
            Some (SingleTextNode.make "="),
            None, Range.Zero
            )
    let makeTypeNameNode = makeTypeNameNodeWithNameMap _.Name.ValueOrModified
    
    let makeStaticMethods (this: GeneratorContainer) =
        this.StaticMethods
        |> List.map (makeMethodBindingNode true true >> MemberDefn.Member)
    let makeInstanceMethods (this: GeneratorContainer) =
        this.InstanceMethods
        |> List.map (makeMethodBindingNode true false >> MemberDefn.Member)
    let makeStaticProperties (this: GeneratorContainer) =
        this.StaticProperties
        |> List.map makeStaticPropertyMemberDefnWithDirectives
    let makeInstanceProperties (this: GeneratorContainer) =
        this.InstanceProperties
        |> List.map makeInstancePropertyMemberDefnWithDirectives
    let makeInstanceEvents (this: GeneratorContainer) =
        this.InstanceEvents
        |> List.collect (makeEventBindingNode true false)
        |> List.map MemberDefn.Member
    let makeStaticEvents (this: GeneratorContainer) =
        this.StaticEvents
        |> List.collect (makeEventBindingNode true true)
        |> List.map MemberDefn.Member
    
    let makeDefaultTypeDefn (this: GeneratorContainer) =
        TypeDefnRegularNode(
            makeTypeNameNode this,
            [
                yield! makeInterfaces this
                yield! makeInstanceEvents this
                yield! makeInstanceMethods this
                yield! makeInstanceProperties this
                yield! makeStaticEvents this
                yield! makeStaticMethods this
                yield! makeStaticProperties this
            ],
            Range.Zero
            )
    let makeDefaultTypeDecl = makeDefaultTypeDefn >> TypeDefn.Regular >> ModuleDecl.TypeDefn


[<Struct>]
type OpenModule = private OpenModule of string list
module OpenModule =
    let create: string -> _ =
        // make sure that there are no spaces.
        _.Split() >> Array.exactlyOne
        // Split the dots
        >> _.Split('.') >> Array.toList
        // Create
        >> OpenModule
    let value (OpenModule value) = value
    let toOpenNode =
        value
        >> IdentListNode.make
        >> fun identNode ->
            OpenModuleOrNamespaceNode(identNode, Range.Zero)
            |> Open.ModuleOrNamespace
type GeneratorGroupChild =
    | Nested of GeneratorGrouper
    | Child of GeneratorContainer
    | StringEnumType of StringEnum
and GeneratorGrouper = {
    PathKey: Path.PathKey
    Description: string list voption
    Opens: OpenModule list
    Children: GeneratorGroupChild list
    Attributes: string list
}
module GeneratorGrouper =
    let create pathKey = {
        PathKey = pathKey
        Description = ValueNone
        Opens = []
        Children = []
        Attributes = []
    }
    /// <summary>
    /// Modules to open added will be opened in the order given
    /// </summary>
    /// <param name="targetModule"></param>
    /// <param name="grouper"></param>
    let addOpen targetModule grouper =
        { grouper with Opens = OpenModule.create targetModule :: grouper.Opens }
    let addAttribute attr grouper = { grouper with Attributes = attr :: grouper.Attributes }
    let withAttributes attributes grouper = { grouper with Attributes = attributes }
    let makeMultipleAttributeListNode =
        _.Attributes >> function
            | [] -> None
            | attrs -> attrs |> MultipleAttributeListNode.make |> Some
    let makeOpenListNode =
        _.Opens >> List.rev >> List.map OpenModule.toOpenNode
        >> OpenListNode
    let makeXmlDocNode =
        _.Description >> function
            | ValueSome [] | ValueNone -> None
            | ValueSome docs ->
                docs |> XmlDocs.Helpers.makeDocs
                |> fun docs ->
                    XmlDocNode(docs, Range.Zero)
                    |> Some
    let withOpens opens grouper = { grouper with Opens = opens }
    let withDescription desc grouper = { grouper with GeneratorGrouper.Description = desc }
    let withPathKey path grouper = { grouper with GeneratorGrouper.PathKey = path }
    let withChildren children grouper = { grouper with Children = children }
    let private addChild child grouper = {
        grouper with
            Children = child :: grouper.Children
    }
    let addNestedGroup group = addChild (GeneratorGroupChild.Nested group)
    let addTypeChild  typ = addChild (GeneratorGroupChild.Child typ)
    let addStringEnumChild stringEnum = addChild (GeneratorGroupChild.StringEnumType stringEnum)
    let isNested = _.PathKey.Path.IsRoot >> not
    let private makeNestedModule grouper =
        fun decls ->
            NestedModuleNode(
                makeXmlDocNode grouper,
                makeMultipleAttributeListNode grouper,
                SingleTextNode.make "module",
                None, false,
                IdentListNode.make grouper.PathKey.Name.ValueOrModified,
                SingleTextNode.make "=",
                decls,
                Range.Zero
            )
            |> ModuleDecl.NestedModule
    let private makeModuleOrNamespace addRootNamespace isNamespace grouper =
        let leader =
            if isNamespace then
                "namespace"
            else "module"
        let pathName =
            IdentListNode.make [
                if addRootNamespace then Spec.rootNamespace
                match grouper.PathKey.Name.ValueOrSource with
                | "Root" -> ()
                | _ -> grouper.PathKey.Name.ValueOrModified
            ]
        let header = ModuleOrNamespaceHeaderNode(
            makeXmlDocNode grouper,
            makeMultipleAttributeListNode grouper,
            MultipleTextsNode.make leader,
            None, false,
            Some pathName,
            Range.Zero
            )
        fun decls ->
            ModuleOrNamespaceNode(
                Some header,
                decls,
                Range.Zero
                )
    let private makeNamespace grouper =
        makeModuleOrNamespace true true grouper
    let private makeModule grouper =
        makeModuleOrNamespace true false grouper
    let makeModuleOrNamespaceNode grouper =
        if isNested grouper then
            makeNestedModule grouper
            |> Choice1Of2
        else
            match grouper.PathKey.Path with
            | Path.ModulePath.Root ->
                makeNamespace grouper
            | _ ->
                makeModule grouper
            |> Choice2Of2
    let makeChildren func: GeneratorGrouper -> ModuleDecl list = _.Children >> List.map func

    let makeDefaultStringEnumType = function
        | GeneratorGroupChild.StringEnumType stringEnum ->
            TypeNameNode.makeSimple(
                stringEnum.PathKey.Name.ValueOrModified,
                attributes = [
                    "StringEnum(CaseRules.None)"
                    "RequireQualifiedAccess"
                ]
                )
            |> fun typeNameNode ->
                stringEnum.Cases
                |> List.map (fun case ->
                    case.Description
                    |> ValueOption.map (
                        List.singleton
                        >> XmlDocs.wrapInSummary
                        >> XmlDocs.Helpers.makeDocs
                        >> fun docs ->
                            XmlDocNode(docs, Range.Zero)
                        )
                    |> ValueOption.toOption
                    |> fun docs ->
                        UnionCaseNode(
                            docs,
                            Some (MultipleAttributeListNode.make
                                    $"CompiledName(\"{case.Value.ValueOrSource}\")"),
                            Some (SingleTextNode.make "|"),
                            SingleTextNode.make case.Value.ValueOrModified,
                            [],
                            Range.Zero
                            )
                    )
                |> fun cases ->
                    TypeDefnUnionNode(
                        typeNameNode,
                        None,
                        cases,
                        [],
                        Range.Zero
                        )
                    |> TypeDefn.Union
                    |> ModuleDecl.TypeDefn
                |> Some
        | _ -> None

/// <summary>
/// Module for functions types and caches relating to creating interfaces for Event parameters to provide
/// a simplified DX when working with multi argument events in Fable.
/// </summary>
module EventInterfaces =
    // We will provide two means for using event handlers. One is
    // curried, the other is accessing the args via an interface for
    // named and documented details
    let private cache = HashSet<Event>()
    let private rootModuleName = $"{Spec.rootNamespace}.EventInterfaces"
    let private getInterfaceName: Event -> string = _.PathKey.Name.ValueOrModified >> sprintf "IOn%s"
    let private getInterfaceModuleRoot: Event -> _ =
        _.PathKey.ParentName >> _.ValueOrModified >> toPascalCase >> sprintf "%s.%s" rootModuleName
    /// <summary>
    /// Indicates why an event failed to get added to the interface cache
    /// </summary>
    type CacheRejectionReason =
        | AlreadyExists of EventInterfaceDetails
        | HasLessThanTwoParameters
    /// <summary>
    /// A simple DU returned when adding an event which provides the interface name and module path
    /// to reference as the type parameter for the overloaded handler that uses the interface.
    /// </summary>
    and EventInterfaceDetails = EventInterfaceDetails of interfaceName: string * modulePath: string
    module EventInterfaceDetails =
        let create modulePath interfaceName = EventInterfaceDetails(interfaceName,modulePath)
        let interfaceName (EventInterfaceDetails(value,_))= value
        let modulePath (EventInterfaceDetails(_,value))= value
        let fullPath (EventInterfaceDetails(interfaceName,modulePath)) = $"{modulePath}.{interfaceName}"
    let eventInterfaceFullPath = EventInterfaceDetails.fullPath
    /// <summary>
    /// Try to add an Event to be lifted into an interface. If the Event is a candidate, then it will return
    /// the intended path to access the interface when it is generated later.
    /// </summary>
    /// <remarks>
    /// It will fail and indicate the reason as either being: because it had one or less parameters; because
    /// it already existed in the cache (with the details attached to access that event).
    /// </remarks>
    let addEvent: Event -> Result<EventInterfaceDetails, CacheRejectionReason> = function
        // We don't need to create an interface for events with one or less parameters
        | { Parameters = [] | [ _ ] } ->
            Error CacheRejectionReason.HasLessThanTwoParameters
        | event ->
            let modulePath = getInterfaceModuleRoot event
            event
            |> getInterfaceName
            |> EventInterfaceDetails.create modulePath
            |> match cache.Add event with
                | true ->
                    Ok
                | false ->
                    CacheRejectionReason.AlreadyExists
                    >> Error
    let makeInterfaces () =
        cache
        |> Seq.toList
        |> List.choose(fun event ->
            let name = getInterfaceName event
            let typeAttributes =
                MultipleAttributeListNode.make [
                    if StabilityStatus.IsExperimental event then
                        "Experimental(\"Indicated to be Experimental by Electron\")"
                    if StabilityStatus.IsDeprecated event then
                        "System.Obsolete()"
                    "EditorBrowsable(EditorBrowsableState.Never)"
                    "AllowNullLiteral"
                    "Interface"
                ]
            let parameterMembers =
                event.Parameters
                |> List.mapi(fun idx -> function
                    | Parameter.InlinedObjectProp prop ->
                        // TODO - there are no cases of this, but this is not handled
                        // correctly because the attributes should emit different
                        let name = prop.PathKey.Name.ValueOrModified
                        let xmlDocs =
                            [
                                match XmlDocs.Helpers.makeCompatibilityLine prop.Compatibility with
                                | ValueSome docs -> docs |> XmlDocs.wrapInPara |> String.concat ""
                                | ValueNone -> ()
                                match prop.Description with
                                | ValueSome docs -> docs
                                | ValueNone -> ()
                            ]
                            |> function
                                | [] -> None
                                | docs ->
                                    docs
                                    |> XmlDocs.wrapInSummary
                                    |> XmlDocs.Helpers.makeDocs
                                    |> fun docs ->
                                        XmlDocNode(docs, Range.Zero)
                                    |> Some
                        let attributes =
                            MultipleAttributeListNode.make [
                                $"Emit(\"$0[{idx}]\")"
                                if StabilityStatus.IsExperimental event then
                                    "Experimental(\"Indicated to be Experimental by Electron\")"
                                if StabilityStatus.IsDeprecated event then
                                    "System.Obsolete()"
                            ]
                        MemberDefnAbstractSlotNode(
                            xmlDocs,
                            Some attributes,
                            MultipleTextsNode.make "abstract member",
                            SingleTextNode.make name,
                            None,
                            prop.Type |> Type.FantomasFactory.mapToFantomas,
                            Some (MultipleTextsNode.make "with get, set"),
                            Range.Zero
                        )
                        |> MemberDefn.AbstractSlot
                    | p ->
                        match p with
                        | Named (name,paramInfo) ->
                            name.Name.ValueOrModified, paramInfo
                        | Positional paramInfo ->
                            $"arg{idx}", paramInfo
                        | _ -> failwith "UNREACHABLE"
                        |> fun (name,paramInfo) ->
                            let name,attributes =
                                match name with
                                | "...args" ->
                                    "args", MultipleAttributeListNode.make $"Emit(\"$0.slice({idx})\")"
                                | _ ->
                                    name, MultipleAttributeListNode.make $"Emit(\"$0[{idx}]\")"
                            let xmlDocs =
                                paramInfo.Description
                                |> ValueOption.map(
                                    List.singleton
                                    >> XmlDocs.wrapInSummary
                                    >> XmlDocs.Helpers.makeDocs
                                    >> fun docs ->
                                        XmlDocNode(docs, Range.Zero)
                                    )
                                |> ValueOption.toOption
                            MemberDefnAbstractSlotNode(
                                xmlDocs,
                                Some attributes,
                                MultipleTextsNode.make "abstract member",
                                SingleTextNode.make name,
                                None,
                                paramInfo.Type |> Type.FantomasFactory.mapToFantomas,
                                Some (MultipleTextsNode.make "with get, set"),
                                Range.Zero
                            )
                            |> MemberDefn.AbstractSlot
                    )
            let typeName =
                let docs =
                    [
                        match XmlDocs.Helpers.makeCompatibilityLine event.Compatibility with
                        | ValueSome docs -> docs |> XmlDocs.wrapInPara |> String.concat ""
                        | ValueNone -> ()
                        match event.Description with
                        | ValueSome docs -> docs
                        | ValueNone -> ()
                    ]
                    |> function
                        | [] -> None
                        | docs ->
                            docs
                            |> XmlDocs.wrapInSummary
                            |> XmlDocs.Helpers.makeDocs
                            |> fun docs ->
                                XmlDocNode(docs, Range.Zero)
                            |> Some
                TypeNameNode(
                    docs,
                    Some typeAttributes,
                    SingleTextNode.make "type",
                    None,
                    IdentListNode.make name,
                    None, [], None,
                    Some (SingleTextNode.make "="),
                    None, Range.Zero
                    )
            // This should not be possible anyway. We filter out no member parameters
            // when adding the events.
            match parameterMembers with
            | [] ->
                None
            | parameterMembers ->
                Some (
                    event.PathKey,
                    TypeDefnRegularNode(typeName, parameterMembers, Range.Zero)
                    |> Compatibility.wrapInCompatibilityDirective event.Compatibility
                    |> TypeDefn.Regular
                )
            )
        |> List.groupBy (fst >> _.ParentName)
        |> List.map(fun (modName, typeDefs) ->
            NestedModuleNode(
                 None,
                 Some (MultipleAttributeListNode.make ["AutoOpen"; "EditorBrowsable(EditorBrowsableState.Never)"]),
                 SingleTextNode.make "module",
                 None, false, IdentListNode.make (modName.ValueOrModified |> toPascalCase),
                 SingleTextNode.make "=",
                 typeDefs
                 |> List.map (snd >> ModuleDecl.TypeDefn)
                 , Range.Zero
                 )
            |> ModuleDecl.NestedModule
            )
        |> fun mods ->
            let header =
                ModuleOrNamespaceHeaderNode(
                    None,
                    Some (MultipleAttributeListNode.make "AutoOpen"),
                    MultipleTextsNode.make "module",
                    None,
                    false,
                    Some(IdentListNode.make rootModuleName),
                    Range.Zero
                )
            let openModules =
                let inline openModule (text: string) = Open.ModuleOrNamespace(OpenModuleOrNamespaceNode(IdentListNode.make text, Range.Zero))
                OpenListNode([
                    openModule "System"
                    openModule "System.ComponentModel"
                    openModule "Fable.Core"
                    openModule "Fable.Core.JsInterop"
                    openModule "Fable.Electron"
                ])
                |> ModuleDecl.OpenList
            ModuleOrNamespaceNode(Some header, openModules :: mods, Range.Zero)
    let tryDebugEventInterfaces (events: Event list) =
        events
        |> List.iter (addEvent >> ignore)
        makeInterfaces()
        |> fun namemod ->
            Oak([], [ namemod ], Range.Zero)
            |> CodeFormatter.FormatOakAsync
            |> Async.RunSynchronously
            |> printfn "%s"
                
module Property =
    let makeConstructorParameter (optional: bool) (prop: Property, typ: Type.FcsType) =
        PatParameterNode(
            None,
            Pattern.Named (PatNamedNode(
                None,
                (if optional
                then SingleTextNode.makeOptional
                else SingleTextNode.make) prop.PathKey.Name.ValueOrModified,
                Range.Zero)
            ),
            Some typ,
            Range.Zero
            )
        |> Pattern.Parameter
    let makeRequiredConstructorParameter = makeConstructorParameter false
    let makeOptionalConstructorParameter = makeConstructorParameter true
type Structure with
    member this.ToPojoNode =
        let requiredProps,optionalProps =
            this.Properties
            |> List.partition _.Required
        let requiredTypes =
            requiredProps
            |> List.map (_.Type >> Type.FantomasFactory.mapToFantomas)
        let optionalTypes =
            optionalProps
            |> List.map (_.Type >> Type.FantomasFactory.mapToFantomas)
        let xmlDocs =
            [
                if this.Description.IsSome then
                    XmlDocs.Boundaries.openSummary
                    this.Description.Value
                    XmlDocs.Boundaries.closeSummary
                if this.WebsiteUrl.IsSome then
                    XmlDocs.makeClosedSeeAlso this.WebsiteUrl.Value
            ]
            |> XmlDocs.Helpers.makeDocs
        let paramXmlDocs =
            [
                yield!
                    requiredProps @ optionalProps
                    |> List.choose(fun prop ->
                        [
                            match XmlDocs.Helpers.makeCompatibilityLine prop.Compatibility with
                            | ValueSome s -> s |> String.concat ""
                            | ValueNone -> ()
                            match prop.Description with
                            | ValueSome desc -> desc
                            | _ -> ()
                        ]
                        |> function
                            | [] -> None
                            | strings ->
                                (prop.PathKey.Name.ValueOrModified, strings |> String.concat " | ")
                                |> Some
                        )
                    |> List.map ( fun (name, docs) ->
                        XmlDocs.makeParam name docs )
            ]
            |> XmlDocs.Helpers.makeDocs
        let xmlDocs =
            match xmlDocs with
            | [||] ->
                None
            | xmlDocs ->
                XmlDocNode(
                    xmlDocs,
                    Range.Zero
                ) |> Some
        let constructorXmlDocs =
            match paramXmlDocs with
            | [||] -> None
            | paramXmlDocs ->
                XmlDocNode(
                    paramXmlDocs,
                    Range.Zero
                    )
                |> Some
        let implicitConstructor = ImplicitConstructorNode(
            xmlDoc = constructorXmlDocs,
            attributes = None,
            accessibility = None,
            pat = Pattern.Paren (PatParenNode.make (
                Pattern.Tuple (PatTupleNode(
                [
                    for required in List.zip requiredProps requiredTypes do
                        Property.makeRequiredConstructorParameter required
                        |> Choice1Of2
                        SingleTextNode.make "," |> Choice2Of2
                    for optional in List.zip optionalProps optionalTypes do
                        Property.makeOptionalConstructorParameter optional
                        |> Choice1Of2
                        SingleTextNode.make "," |> Choice2Of2
                ] |> List.cutOffLast,
                Range.Zero
                )))),
            self = None,
            range = Range.Zero
            )
        let typeNameNode = TypeNameNode(
            xmlDoc = xmlDocs,
            attributes = Some (MultipleAttributeListNode.make "JS.Pojo"),
            leadingKeyword = SingleTextNode.make "type",
            ao = None,
            identifier = IdentListNode.make this.Name.ValueOrModified,
            typeParams = None,
            constraints = [],
            implicitConstructor = Some implicitConstructor,
            equalsToken = Some (SingleTextNode.make "="),
            withKeyword = None,
            range = Range.Zero
            )
        TypeDefnRegularNode(typeNameNode, [
            for prop in requiredProps @ optionalProps do
                let xmlDoc =
                    [
                        match prop.Compatibility |> XmlDocs.Helpers.makeCompatibilityLine with
                        | ValueSome compat ->
                            XmlDocs.Boundaries.openPara
                            + (compat |> String.concat "") +
                            XmlDocs.Boundaries.closePara
                        | ValueNone -> ()
                        match prop.Description with
                        | ValueSome desc -> desc
                        | ValueNone -> ()
                    ]
                    |> function
                        | [] -> ValueNone
                        | texts -> ValueSome texts
                    |> ValueOption.bind (
                        XmlDocs.wrapInSummary
                        >> XmlDocs.Helpers.makeDocs
                        >> function
                            | [||] -> ValueNone
                            | docs ->
                                XmlDocNode(docs, Range.Zero) |> ValueSome
                        )
                    |> ValueOption.toOption
                let attributes = MultipleAttributeListNode.make [
                    "Erase"
                    if StabilityStatus.IsExperimental prop then
                        "Experimental(\"Indicated to be Experimental by Electron\")"
                    if StabilityStatus.IsDeprecated prop then
                        "System.Obsolete()"
                ]    
                MemberDefnAutoPropertyNode(
                    xmlDoc = xmlDoc,
                    attributes = Some attributes,
                    leadingKeyword = MultipleTextsNode.make [ "member"; "val" ],
                    accessibility = None,
                    identifier = SingleTextNode.make prop.PathKey.Name.ValueOrModified,
                    t = None,
                    equals = SingleTextNode.make "=",
                    expr = Expr.Ident(SingleTextNode.make prop.PathKey.Name.ValueOrModified),
                    withGetSet = Some (MultipleTextsNode.make [ "with"; "get"; "set" ]),
                    range = Range.Zero
                )
                |> Compatibility.wrapInCompatibilityDirective prop.Compatibility
                |> MemberDefn.AutoProperty
        ], Range.Zero)
        |> TypeDefn.Regular
            
    static member tryDebugConstructor (structures: Structure list) =
        structures
        |> List.map (_.ToPojoNode >> ModuleDecl.TypeDefn)
        |> fun decls ->
            Oak([], [ ModuleOrNamespaceNode(None, decls, Range.Zero) ], Range.Zero)
            |> CodeFormatter.FormatOakAsync
            |> Async.RunSynchronously
            |> printfn "%s"

type StringEnum with
    static member tryDebugTypeGen (stringEnums: StringEnum list) =
        GeneratorGrouper.create <| Path.PathKey.CreateModule(Source "Enums")
        |> GeneratorGrouper.withChildren (stringEnums |> List.map GeneratorGroupChild.StringEnumType)
        |> _.Children |> List.choose GeneratorGrouper.makeDefaultStringEnumType
        |> fun decls ->
            Oak([], [ ModuleOrNamespaceNode(None, decls, Range.Zero) ], Range.Zero)
            |> CodeFormatter.FormatOakAsync
            |> Async.RunSynchronously
            |> printfn "%s"
module Method =
    let makePropParameter (prop: Property, typ: Type.FcsType) =
        PatParameterNode(
            None,
            Pattern.Named (PatNamedNode(
                None,
                (if prop.Required |> not
                then SingleTextNode.makeOptional
                else SingleTextNode.make) prop.PathKey.Name.ValueOrModified,
                Range.Zero)
            ),
            Some typ,
            Range.Zero
            )
        |> Pattern.Parameter
    let makeParameter (optional: bool) (name: Name, param: ParameterInfo, typ: Type.FcsType) =
        let attributes =
            match name.ValueOrSource with
            | "...args" ->
                Some (MultipleAttributeListNode.make "System.ParamArray")
            | _ -> None
        let name =
            if attributes.IsSome then "args"
            else name.ValueOrModified
        PatParameterNode(
            attributes,
            Pattern.Named (PatNamedNode(
                None,
                (if optional
                then SingleTextNode.makeOptional
                else SingleTextNode.make) name,
                Range.Zero)
            ),
            Some typ,
            Range.Zero
            )
        |> Pattern.Parameter
type Method with
    member this.ToMemberNode =
        let xmlDoc =
            this.Description
            |> ValueOption.map (
                List.singleton
                >> XmlDocs.wrapInSummary
                >> XmlDocs.Helpers.makeDocs
                >> fun docs ->
                    XmlDocNode(docs, Range.Zero)
                )
            |> ValueOption.toOption
        let attributes =
            [
                // TODO - this depends on whether we need to emit an expression
                this.PathKey
                |> Path.tracePathOfEntry
                |> List.map _.ValueOrSource
                |> String.concat "."
                |> fun path ->
                    $"Import(\"{path}\", \"electron\")"
                //
                match
                    this.Parameters
                    |> List.tryFindIndex _.IsInlinedObjectProp
                with
                | Some idx -> $"ParamObject(%i{idx})"
                | None -> ()
            ]
            |> function
                | [] -> None
                | attrs -> MultipleAttributeListNode.make attrs |> Some
        let returnInfo =
            BindingReturnInfoNode(SingleTextNode.make ":", Type.FantomasFactory.mapToFantomas this.ReturnType, Range.Zero)
        let inlineObjectParameters,parameters =
            this.Parameters
            |> List.partition _.IsInlinedObjectProp
            |> fun (inlined,pars) ->
                inlined
                |> List.map (
                    function InlinedObjectProp prop -> prop | _ -> failwith "UNREACHABLE"
                    >> fun prop ->
                        prop, Type.FantomasFactory.mapToFantomas prop.Type
                    ),
                pars
                |> List.mapi (fun i -> function
                    | Positional paramInfo ->
                        Source $"arg{i}", paramInfo, Type.FantomasFactory.mapToFantomas paramInfo.Type
                    | Named(name,paramInfo) ->
                        name.Name, paramInfo, Type.FantomasFactory.mapToFantomas paramInfo.Type
                    | _ -> failwith "UNREACHABLE"
                    )
                
        let requiredParameters,optionalParameters =
            parameters |> List.partition (fun (_,p,_) -> p.Required)
        let parameterList =
            [
                for name,required,typ in requiredParameters do
                    Choice1Of2 <| Method.makeParameter false (name,required,typ)
                    Choice2Of2 (SingleTextNode.make ",")
                for name,optional,typ in optionalParameters do
                    Choice1Of2 <| Method.makeParameter true (name,optional,typ)
                    Choice2Of2 (SingleTextNode.make ",")
                for prop,typ in inlineObjectParameters do
                    Choice1Of2 <| Method.makePropParameter (prop,typ)
                    Choice2Of2 (SingleTextNode.make ",")
            ]
            |> List.cutOffLast
            |> fun pats -> PatTupleNode(pats,Range.Zero)
            |> Pattern.Tuple
            |> PatParenNode.make
            |> Pattern.Paren
        BindingNode(
            xmlDoc,
            attributes,
            MultipleTextsNode.make ["static"; "member"],
            false,
            None,
            None,
            Choice1Of2 (IdentListNode.make this.PathKey.Name.ValueOrModified),
            None,
            [ parameterList ],
            Some returnInfo,
            SingleTextNode.make "=",
            (
                // TODO - this depends on whether we need to inline or emit
                Expr.Ident(SingleTextNode.make "nativeOnly")
            ),
            Range.Zero
        )
        |> Compatibility.wrapInCompatibilityDirective this.Compatibility
        |> MemberDefn.Member
    static member tryDebugStaticMemberGen (methods: Method list) =
        methods
        |> List.map _.ToMemberNode
        |> fun members ->
            TypeNameNode.makeSimple "Test"
            |> TypeDefnRegularNode.make members
            |> TypeDefn.Regular
            |> ModuleDecl.TypeDefn
            |> List.singleton
        |> fun decls ->
            Oak([], [ ModuleOrNamespaceNode(None, decls, Range.Zero) ], Range.Zero)
            |> CodeFormatter.FormatOakAsync
            |> Async.RunSynchronously
            |> printfn "%s"

[<RequireQualifiedAccess>]
type EventMemberType =
    | Instance of Path.Type
    | Module of Path.ModulePath
type Event with
    /// <summary>
    /// Creates the event handler member(s) for the instance.
    /// </summary>
    /// <remarks>
    /// If the event handler takes more than one argument, then multiple event handlers are created.
    /// One event handler is the standard curried F# type. The second is a single argument handler, where
    /// the first argument is an interface which abstracts away indexed access to the arguments via named field access
    /// through the interface.
    /// </remarks>
    member this.MakeMemberNodes (functionPrefix: string) =
        // There are three (four) scenarios we must account for:
        // 1. Module/'static' events.
        // 2. Instance/'member' events.
        // 3. One of the above, but we also have to make an interface overload
        //
        // When the parent is a type, it is the latter; ie - the events are only
        // accessible from an instance of the class/interface.
        // When the parent is a module, it can be used without an instance of a class/interface.
        let eventType =
            match this.PathKey with
            | Path.PathKey.Event e ->
                match e.Parent with
                | Choice1Of2 modulePath -> EventMemberType.Module modulePath
                | Choice2Of2 ``type`` -> EventMemberType.Instance ``type``
            | pathKey ->
                failwith $"An event type unexpectedly did not have a matching PathKey: {pathKey}"
        let xmlDocs =
            [
                match
                    this.Compatibility
                    |> XmlDocs.Helpers.makeCompatibilityLine
                with
                | ValueSome compats ->
                    compats |> XmlDocs.wrapInPara
                    |> String.concat ""
                | ValueNone -> ()
                match this.Description with
                | ValueSome docs -> docs
                | ValueNone -> ()
            ]
            |> function
                | [] -> ValueNone
                | texts -> ValueSome texts
            |> ValueOption.map(
                XmlDocs.wrapInSummary
                >> XmlDocs.Helpers.makeDocs
                >> fun docs ->
                    XmlDocNode(docs, Range.Zero)
                )
            |> ValueOption.toOption
        let functionName =
            if eventType.IsInstance
            then IdentListNode.make [ "_"; functionPrefix + this.PathKey.Name.ValueOrModified ]
            else IdentListNode.make (functionPrefix + this.PathKey.Name.ValueOrModified)
        let leadingKeyword =
            if eventType.IsInstance
            then MultipleTextsNode.make [ "member" ]
            else MultipleTextsNode.make [ "static member" ]
        let expr = Expr.Ident(SingleTextNode.make "nativeOnly")
        let attributes = MultipleAttributeListNode.make [
            match eventType with
            | EventMemberType.Instance _ ->
                yield $"Emit(\"$0.{functionPrefix}('{this.PathKey.Name.ValueOrSource}', $1)\")"
            | EventMemberType.Module modulePath ->
                let name = modulePath.Name.ValueOrSource
                yield $"Import(\"{name}\",\"electron\")"
                yield $"Emit(\"{name}.{functionPrefix}('{this.PathKey.Name.ValueOrSource}', $0\")"
        ]
        let returnType =
            BindingReturnInfoNode(
                SingleTextNode.make ":",
                Type.FcsType.Anon(SingleTextNode.make "unit"),
                Range.Zero
            )
        
        let parameters =
            this.Parameters
            |> List.mapi (fun i -> function
                | Positional parameterInfo ->
                    Choice1Of2(
                        Source $"arg{i}",
                        parameterInfo,
                        parameterInfo.Type
                        |> Type.FantomasFactory.mapToFantomas
                    )
                | Named(name, info) ->
                    Choice1Of2(
                        name.Name,
                        info,
                        info.Type
                        |> Type.FantomasFactory.mapToFantomas
                    )
                | InlinedObjectProp prop ->
                    Choice2Of2(
                        prop,
                        prop.Type |> Type.FantomasFactory.mapToFantomas
                    )
            )
        let parameters =
            parameters
            |> List.mapi (fun i -> function
                | Choice1Of2(_,_,typ) 
                | Choice2Of2(_, typ) ->
                    typ,
                    // if i = parameters.Length - 1 then
                        // SingleTextNode.make "->"
                    // else SingleTextNode.make "*"
                    SingleTextNode.make "->"
            )
            |> function
                | [] -> [ Type.Anon(SingleTextNode.make "unit"), SingleTextNode.make "->" ]
                | list -> list
            |> fun typeFunsPat ->
                TypeFunsNode(
                    typeFunsPat,
                    Type.Anon(SingleTextNode.make "unit"),
                    Range.Zero
                    )
                |> Type.Funs
            |> fun typ ->
                PatParameterNode(
                    None,
                    Pattern.Named (PatNamedNode(
                        None,
                        SingleTextNode.make "handler",
                        Range.Zero)
                    ),
                    Some typ,
                    Range.Zero
                    )
                |> Pattern.Parameter
                |> PatParenNode.make
                |> Pattern.Paren
                |> List.singleton
        [
            BindingNode(
                xmlDocs,
                Some attributes,
                leadingKeyword,
                false,
                Some (SingleTextNode.make "inline"),
                None,
                Choice1Of2 functionName,
                None,
                parameters,
                Some returnType,
                SingleTextNode.make "=",
                expr,
                Range.Zero
                )
            |> Compatibility.wrapInCompatibilityDirective this.Compatibility
            |> MemberDefn.Member
            match EventInterfaces.addEvent this with
            | Error (EventInterfaces.CacheRejectionReason.AlreadyExists resultValue)
            | Ok resultValue ->
                let parameters =
                    [
                        Type.Anon(
                            resultValue
                            |> EventInterfaces.eventInterfaceFullPath
                            |> SingleTextNode.make
                        ),
                        SingleTextNode.make "->"
                    ]
                    |> fun funsTyps ->
                        TypeFunsNode(funsTyps, Type.Anon(SingleTextNode.make "unit"), Range.Zero)
                    |> Type.Funs
                    |> fun typ ->
                        PatParameterNode(
                            None,
                            Pattern.Named (PatNamedNode(
                                None,
                                SingleTextNode.make "handler",
                                Range.Zero)
                            ),
                            Some typ,
                            Range.Zero
                            )
                        |> Pattern.Parameter
                        |> PatParenNode.make
                        |> Pattern.Paren
                        |> List.singleton
                BindingNode(
                    xmlDocs,
                    Some attributes,
                    leadingKeyword,
                    false,
                    Some (SingleTextNode.make "inline"),
                    None,
                    Choice1Of2 functionName,
                    None,
                    parameters,
                    Some returnType,
                    SingleTextNode.make "=",
                    expr,
                    Range.Zero
                    )
                |> Compatibility.wrapInCompatibilityDirective this.Compatibility
                |> MemberDefn.Member
            | Error _ -> ()
        ]
    member this.ToMemberNode =
        [
            yield! this.MakeMemberNodes("on")
            yield! this.MakeMemberNodes("once")
            yield! this.MakeMemberNodes("off")
        ]
    static member tryDebugEventMemberGen (events: Event list) =
        events
        |> List.collect _.ToMemberNode
        |> fun members ->
            TypeNameNode.makeSimple "Test"
            |> TypeDefnRegularNode.make members
            |> TypeDefn.Regular
            |> ModuleDecl.TypeDefn
            |> List.singleton
        |> fun decls ->
            Oak([], [ ModuleOrNamespaceNode(None, decls, Range.Zero) ], Range.Zero)
            |> CodeFormatter.FormatOakAsync
            |> Async.RunSynchronously
            |> printfn "%s"

type Class with
    static member MapPathKeyToProcess (processBlock: Decoder.ProcessBlock) (pathKey: Path.PathKey) =
        match processBlock, pathKey with
        | { Renderer = true; Main = true }, (Path.PathKey.Type _ | Path.PathKey.Module _) -> pathKey
        | { Renderer = true }, Path.PathKey.Type typ ->
            typ.AddRootModule(Path.Module.Module(Path.ModulePath.Root, Name.createPascal "Renderer"))
            |> Path.PathKey.Type 
        | { Renderer = true }, Path.PathKey.Module typ ->
            typ.AddRootModule(Path.Module.Module(Path.ModulePath.Root, Name.createPascal "Renderer"))
            |> Path.PathKey.Module 
        | { Main = true }, Path.PathKey.Type typ ->
            typ.AddRootModule(Path.Module.Module(Path.ModulePath.Root, Name.createPascal "Main"))
            |> Path.PathKey.Type 
        | { Main = true }, Path.PathKey.Module typ ->
            typ.AddRootModule(Path.Module.Module(Path.ModulePath.Root, Name.createPascal "Main"))
            |> Path.PathKey.Module 
        | { Utility = true }, Path.PathKey.Module typ ->
            typ.AddRootModule(Path.Module.Module(Path.ModulePath.Root, Name.createPascal "Utility"))
            |> Path.PathKey.Module
        | { Utility = true }, Path.PathKey.Type typ ->
            typ.AddRootModule(Path.Module.Module(Path.ModulePath.Root, Name.createPascal "Utility"))
            |> Path.PathKey.Type
        | _, pathKey ->
            failwith $"Class pathkeys that are not of type PathKey.Type cannot be mapped to a process: {pathKey}"
    member this.SpecialStaticPropertyHandler (props: Property list) =
        match this.PathKey.Name.ValueOrModified with
        | "TouchBar" ->
            props
            |> List.filter (
                _.PathKey.Name.ValueOrSource
                >> function
                    | "TouchBarButton" | "TouchBarColorPicker"
                    | "TouchBarGroup" | "TouchBarLabel"
                    | "TouchBarPopover" | "TouchBarScrubber"
                    | "TouchBarSegmentedControl" | "TouchBarSlider"
                    | "TouchBarSpacer" | "TouchBarOtherItemsProxy" -> false
                    | _ -> true
                )
        | _ -> props
    member private this.MakeProperties(isStatic: bool) =
        if isStatic then
            this.StaticProperties
            |> this.SpecialStaticPropertyHandler
        else this.Properties
        |> List.map (fun prop ->
            let isSourceName =
                prop.PathKey.Name.IsSource
                || prop.PathKey.Name |> function
                    | Modified(_, text) when text.StartsWith("``") && text.EndsWith("``") -> true
                    | Source _ -> true
                    | _ -> false
            let attributes =
                [
                    if not isSourceName && isStatic then
                        $"Emit(\"{this.PathKey.Name.ValueOrSource}.{prop.PathKey.Name.ValueOrSource}{{{{ = $0}}}}\")"
                    elif not isSourceName then
                        $"Emit(\"$0.{prop.PathKey.Name.ValueOrSource}{{{{ = $1 }}}}\")"
                    if StabilityStatus.IsExperimental prop then
                        "Experimental(\"Experimental according to Electron\")"
                    if StabilityStatus.IsDeprecated prop then
                        "System.Obsolete"
                ]
                |> function
                    | [] -> None
                    | attrs -> MultipleAttributeListNode.make attrs |> Some
            let xmlDocs =
                [
                    match prop.Compatibility |> XmlDocs.Helpers.makeCompatibilityLine with
                    | ValueSome docs -> XmlDocs.wrapInPara docs |> String.concat ""
                    | ValueNone -> ()
                    match prop.Description with
                    | ValueSome docs -> docs
                    | ValueNone -> ()
                ]
                |> function
                    | [] -> None
                    | docs ->
                        XmlDocs.wrapInSummary docs
                        |> XmlDocs.Helpers.makeDocs
                        |> fun docs ->
                            XmlDocNode(docs, Range.Zero)
                            |> Some
            match isSourceName with
            | true ->
                // make autoStaticProperty
                MemberDefnAutoPropertyNode(
                    xmlDocs,
                    attributes,
                    MultipleTextsNode.make [
                        if isStatic then "static"
                        "member"
                        "val"
                    ],
                    None,
                    SingleTextNode.make prop.PathKey.Name.ValueOrSource,
                    Some (prop.Type |> Type.FantomasFactory.mapToFantomas),
                    SingleTextNode.make "=",
                    Expr.Ident(SingleTextNode.make "nativeOnly"),
                    Some (MultipleTextsNode.make (if prop.ReadOnly then "with get" else "with get, set")),
                    Range.Zero
                    )
                |> Compatibility.wrapInCompatibilityDirective prop.Compatibility
                |> MemberDefn.AutoProperty
            | false ->
                MemberDefnPropertyGetSetNode(
                    xmlDocs,
                    attributes,
                    MultipleTextsNode.make [
                        if isStatic then "static"
                        "member"
                    ],
                    None, None,
                    IdentListNode.make [
                        if not isStatic then "_"
                        prop.PathKey.Name.ValueOrModified
                    ],
                    SingleTextNode.make "with",
                    PropertyGetSetBindingNode(
                        None, None, None,
                        SingleTextNode.make "get",
                        [
                            PatTupleNode([], Range.Zero)
                            |> Pattern.Tuple
                            |> PatParenNode.make
                            |> Pattern.Paren
                        ],
                        Some(BindingReturnInfoNode(
                            SingleTextNode.make ":",
                            Type.FantomasFactory.mapToFantomas prop.Type,
                            Range.Zero
                            )),
                        SingleTextNode.make "=",
                        Expr.Ident(SingleTextNode.make "Unchecked.defaultof<_>"),
                        Range.Zero
                        ),
                    (
                        if prop.ReadOnly then None else
                        SingleTextNode.make "and" |> Some
                    ),
                    (
                        if prop.ReadOnly then
                            None
                        else
                            Some (
                                PropertyGetSetBindingNode(
                                    None,None,None,
                                    SingleTextNode.make "set",
                                    [
                                        PatParameterNode.make
                                            (SingleTextNode.make "value")
                                            (Type.FantomasFactory.mapToFantomas prop.Type |> Some)
                                        |> Pattern.Parameter
                                        |> PatParenNode.make
                                        |> Pattern.Paren
                                    ],
                                    None, SingleTextNode.make "=",
                                    Expr.Ident(SingleTextNode.make "()"), Range.Zero
                                    )
                                )
                    ),
                    Range.Zero
                    )
                |> Compatibility.wrapInCompatibilityDirective prop.Compatibility
                |> MemberDefn.PropertyGetSet
            )
    member this.MakeStaticProperties() = this.MakeProperties(true)
    member this.MakeInstanceProperties() = this.MakeProperties(false)
    member this.MakeConstructor() =
        this.Constructor
        |> function
            | None -> None
            | Some parameters ->
                let attributes =
                    parameters
                    |> List.tryFindIndex _.IsInlinedObjectProp
                    |> Option.map (fun idx -> MultipleAttributeListNode.make $"ParamObject({idx})")
                parameters
                |> List.collect (function
                    | Parameter.InlinedObjectProp prop ->
                        (prop.Type |> Type.FantomasFactory.mapToFantomas
                         , prop)
                        |> Choice1Of2
                    | Parameter.Positional info ->
                        (info.Type
                        |> Type.FantomasFactory.mapToFantomas,
                        Name.createCamel "arg",
                        info)
                        |> Choice2Of2
                    | Parameter.Named(name,info) ->
                        (info.Type
                        |> Type.FantomasFactory.mapToFantomas
                        , name.Name,
                        info)
                        |> Choice2Of2
                    >> function
                        | Choice1Of2(typ,prop) ->
                            [
                                Method.makePropParameter(prop,typ)
                                |> Choice1Of2
                                SingleTextNode.make "," |> Choice2Of2
                            ]
                        | Choice2Of2(typ,name,info) ->
                            [
                                Method.makeParameter info.Required (name,info,typ)
                                |> Choice1Of2
                                SingleTextNode.make "," |> Choice2Of2
                            ]
                )
                |> List.cutOffLast
                |> fun patterns ->
                    PatTupleNode(patterns, Range.Zero)
                    |> Pattern.Tuple
                    |> PatParenNode.make
                    |> Pattern.Paren
                |> fun parameters ->
                    ImplicitConstructorNode(
                        None, attributes, None,
                        parameters,
                        None, Range.Zero
                        )
                |> Some
    member private this.MakeMethods(isStatic: bool) =
        if isStatic then
            this.StaticMethods
        else this.Methods
        |> List.map (fun method ->
            let xmlDocs =
                [
                    yield! [
                        match
                            XmlDocs.Helpers.makeCompatibilityLine method.Compatibility
                        with
                        | ValueSome docs -> docs |> XmlDocs.wrapInPara |> String.concat ""
                        | _ -> ()
                        match method.Description with
                        | ValueSome docs -> docs
                        | _ -> ()
                    ]
                    |> function
                        | [] -> []
                        | docs ->
                            docs |> XmlDocs.wrapInSummary
                    yield!
                        method.Parameters
                        |> List.mapi (fun i -> function
                            | InlinedObjectProp prop -> prop.PathKey.Name.ValueOrModified, prop.Description |> ValueOption.defaultValue ""
                            | Positional para -> $"arg{i}", para.Description |> ValueOption.defaultValue ""
                            | Named(name,para) -> name.Name.ValueOrModified, para.Description |> ValueOption.defaultValue ""
                        )
                        |> List.map(fun nameDesc -> nameDesc ||> XmlDocs.makeParam)
                ]
                |> XmlDocs.Helpers.makeDocs
                |> fun docs ->
                    XmlDocNode(docs, Range.Zero)
                    |> Some
            let attributes = [
                match method.PathKey.Name with
                | Modified(source,_) -> $"CompiledName(\"{source}\")"
                | Source _ -> ()
                match
                    method.Parameters
                    |> List.tryFindIndex _.IsInlinedObjectProp
                with
                | Some value ->
                    $"ParamObject({value})"
                | None -> ()
                if StabilityStatus.IsExperimental method then
                    "Experimental(\"Marked Experimental by Electron\")"
                if StabilityStatus.IsDeprecated method then
                    "System.Obsolete"
            ]
            BindingNode(
                xmlDocs,
                (
                    match attributes with
                    | [] -> None
                    | attrs -> MultipleAttributeListNode.make attrs |> Some
                ),
                MultipleTextsNode.make [
                    if isStatic then
                        "static"
                    "member"
                ],
                false,
                None, None,
                Choice1Of2(
                    IdentListNode.make [
                        if not isStatic then "_"
                        method.PathKey.Name.ValueOrModified
                    ]
                    ),
                None,
                (
                    let inlined,nonInlined =
                        method.Parameters
                        |> List.partition _.IsInlinedObjectProp
                    let required,optional =
                        nonInlined |> List.partition _.Required
                    required @ optional @ inlined
                    |> List.mapi (fun i -> function
                        | InlinedObjectProp prop ->
                            Choice1Of2 (prop, prop.Type |> Type.FantomasFactory.mapToFantomas)
                        | Positional para ->
                            Choice2Of2(Source $"arg{i}", para.Type |> Type.FantomasFactory.mapToFantomas, para)
                        | Named(name,para) ->
                            Choice2Of2(name.Name, para.Type |> Type.FantomasFactory.mapToFantomas, para)
                        )
                    |> List.collect (function
                        | Choice1Of2(prop,typ) ->
                            [
                                Choice1Of2 <| Method.makePropParameter(prop,typ)
                                Choice2Of2 (SingleTextNode.make ",")
                            ]
                        | Choice2Of2(name,typ,para) ->
                            [
                                Choice1Of2 <| Method.makeParameter (not para.Required) (name,para,typ)
                                Choice2Of2(SingleTextNode.make ",")
                            ]
                        )
                    |> function
                        | [] -> []
                        | nodes ->
                            nodes
                            |> List.cutOffLast
                    |> fun items -> PatTupleNode(items, Range.Zero)
                    |> Pattern.Tuple
                    |> PatParenNode.make
                    |> Pattern.Paren
                    |> List.singleton
                ),       
                (
                    method.ReturnType
                    |> Type.FantomasFactory.mapToFantomas
                    |> fun typ ->
                        BindingReturnInfoNode(SingleTextNode.make ":", typ, Range.Zero)
                        |> Some
                ),
                SingleTextNode.make "=",
                Expr.Ident(SingleTextNode.make "Unchecked.defaultof<_>"),
                Range.Zero
                )
            |> Compatibility.wrapInCompatibilityDirective method.Compatibility
            |> MemberDefn.Member
            )
    member this.MakeStaticMethods() = this.MakeMethods(true)
    member this.MakeInstanceMethods() = this.MakeMethods(false)
    member this.MakeInstanceEvents() =
        this.Events
        |> List.collect _.ToMemberNode
    member this.ToTypeDefn =
        // The classes have a series of properties, members, and events to manage.
        // They are all fairly trivial to implement within F#.
        let path = Class.MapPathKeyToProcess this.Process this.PathKey
        let attributes =
            [
                if
                    this.Constructor.IsNone
                    || this.Description
                    |> ValueOption.exists _.Contains("cannot be subclassed in user code.")
                then
                    "Sealed"
                match this.PathKey.Name.ValueOrSource with
                | "TouchBarSpacer" | "TouchBarButton" | "TouchBarColorPicker"
                | "TouchBarGroup" | "TouchBarLabel" | "TouchBarPopover" | "TouchBarScrubber"
                | "TouchBarSegmentedControl" | "TouchBarSlider" | "TouchBarSpacer" as name ->
                    $"Import(\"TouchBar.{name}\", \"electron\")"
                | name ->
                    $"Import(\"{name}\", \"electron\")"
                
            ]
            |> MultipleAttributeListNode.make
        let interfaces =
            [
                if
                    this.Events.Length > 0
                    || this.Description
                    |> ValueOption.exists _.Contains("is an EventEmitter", System.StringComparison.Ordinal)
                then
                    MemberDefnInterfaceNode(
                        SingleTextNode.make "interface",
                        Type.makeSimple Injections.eventEmitterName,
                        None,
                        [],
                        Range.Zero
                        )
                    |> MemberDefn.Interface
            ]
        let typeDocs =
            [
                this.Process
                |> XmlDocs.Helpers.makeProcessLine
                |> XmlDocs.wrapInPara
                |> String.concat ""
                
                match this.Description with
                | ValueSome desc -> desc
                | ValueNone -> ()
            ]
            |> function
                | [] -> None
                | docs -> Some docs
            |> Option.map(
                XmlDocs.wrapInSummary
                >> XmlDocs.Helpers.makeDocs
                >> fun docs ->
                    XmlDocNode(docs,Range.Zero)
                )
        let members =
            interfaces
            @ this.MakeStaticProperties()
            @ this.MakeInstanceProperties()
            @ this.MakeStaticMethods()
            @ this.MakeInstanceMethods()
            @ this.MakeInstanceEvents()
        TypeNameNode(
            typeDocs,
            Some attributes,
            SingleTextNode.make "type",
            None,
            // IdentListNode.make (path |> Path.tracePathOfEntry |> List.map (_.ValueOrModified >> toPascalCase)),
            IdentListNode.make path.Name.ValueOrModified,
            None,
            [],
            this.MakeConstructor(),
            Some (SingleTextNode.make "="),
            None,
            Range.Zero
            )
        |> TypeDefnRegularNode.make members
        |> TypeDefn.Regular
        |> ModuleDecl.TypeDefn
        |> fun moduleDecl ->
            SourcePackage.Empty
            |> SourcePackage.withDecl moduleDecl
            |> SourcePackage.withPathKey path
            |> SourcePackage.withTarget (
                SourceTarget.Empty
                |> SourceTarget.setProcesses [
                    if this.Process.Main then Target.Process.Main
                    if this.Process.Renderer then Target.Process.Renderer
                    if this.Process.Utility then Target.Process.Utility
                ]
                )
            // special case
            |> if this.PathKey.Name.ValueOrSource = "TouchBar" then
                   SourcePackage.withDecl Injections.touchBarItemsDef
                else id
    static member MakeFile(classes: Class list) =
        let sharedMainRenderer =
            classes
            |> List.filter (fun c -> c.Process.Main && c.Process.Renderer)
        let mainProcess =
            classes
            |> List.filter (fun c -> c.Process.Main && not c.Process.Renderer)
        let rendererProcess =
            classes
            |> List.filter (fun c -> (not c.Process.Main) && c.Process.Renderer)
        let rest = classes |> List.except (sharedMainRenderer @ mainProcess @ rendererProcess)
        let makeTypes autoOpen moduleName : Class list -> _ = fun classes ->
            classes
            |> List.map _.ToTypeDefn
            |> fun decls ->
               NestedModuleNode(
                   None, Some (MultipleAttributeListNode.make [
                       "Erase"
                       if autoOpen then "AutoOpen"
                   ]),
                   SingleTextNode.make "module", None, false,
                   IdentListNode.make (moduleName: string), SingleTextNode.make "=",
                   decls |> List.collect _.Decls, Range.Zero
                   )
               |> ModuleDecl.NestedModule
        Oak([], [ ModuleOrNamespaceNode(None, (
                [
                   true, "Common", sharedMainRenderer
                   false, "Main", mainProcess
                   false, "Renderer", rendererProcess
                   false, "Utility", rest
                ]
                |> List.map (fun p -> p |||> makeTypes)
            ), Range.Zero) ], Range.Zero)
        |> CodeFormatter.FormatOakAsync
        |> Async.RunSynchronously
                
    static member tryDebugFileMaker (classes: Class list) =
        Class.MakeFile(classes)
        |> fun txt ->
            System.IO.File.WriteAllText(System.IO.Path.Combine(__SOURCE_DIRECTORY__, "test.fs"), txt)
    // static member tryDebugClassTypeGen (classes: Class list) =
    //     classes
    //     |> List.map _.ToTypeDefn
    //     |> List.insertAt (classes |> List.findIndex (_.PathKey.Name.ValueOrModified >> (=) "TouchBar")) Injections.touchBarItemsDef
    //     |> fun decls ->
    //         Oak([], [ ModuleOrNamespaceNode(None, decls, Range.Zero) ], Range.Zero)
    //         |> CodeFormatter.FormatOakAsync
    //         |> Async.RunSynchronously
    //         |> fun txt ->
    //             System.IO.File.WriteAllText(System.IO.Path.Combine(__SOURCE_DIRECTORY__, "test.fs"), txt)
    //             txt
    //         |> printfn "%s"

type Module with
    member this.MakeEvents() =
        this.Events
        |> List.collect _.ToMemberNode
    member this.MakeMethods() =
        this.Methods
        |> List.map (fun method ->
            let xmlDocs =
                [
                    yield! [
                        match
                            XmlDocs.Helpers.makeCompatibilityLine method.Compatibility
                        with
                        | ValueSome docs -> docs |> XmlDocs.wrapInPara |> String.concat ""
                        | _ -> ()
                        match method.Description with
                        | ValueSome docs -> docs
                        | _ -> ()
                    ]
                    |> function
                        | [] -> []
                        | docs ->
                            docs |> XmlDocs.wrapInSummary
                    yield!
                        method.Parameters
                        |> List.mapi (fun i -> function
                            | InlinedObjectProp prop -> prop.PathKey.Name.ValueOrModified, prop.Description |> ValueOption.defaultValue ""
                            | Positional para -> $"arg{i}", para.Description |> ValueOption.defaultValue ""
                            | Named(name,para) -> name.Name.ValueOrModified, para.Description |> ValueOption.defaultValue ""
                        )
                        |> List.map(fun nameDesc -> nameDesc ||> XmlDocs.makeParam)
                ]
                |> XmlDocs.Helpers.makeDocs
                |> fun docs ->
                    XmlDocNode(docs, Range.Zero)
                    |> Some
            let attributes = [
                match method.PathKey.Name with
                | Modified(source,_) -> $"CompiledName(\"{source}\")"
                | Source _ -> ()
                match
                    method.Parameters
                    |> List.tryFindIndex _.IsInlinedObjectProp
                with
                | Some value ->
                    $"ParamObject({value})"
                | None -> ()
                if StabilityStatus.IsExperimental method then
                    "Experimental(\"Marked Experimental by Electron\")"
                if StabilityStatus.IsDeprecated method then
                    "System.Obsolete"
            ]
            BindingNode(
                xmlDocs,
                (
                    match attributes with
                    | [] -> None
                    | attrs -> MultipleAttributeListNode.make attrs |> Some
                ),
                MultipleTextsNode.make [ "static"; "member" ],
                false,
                None, None,
                Choice1Of2( IdentListNode.make [ method.PathKey.Name.ValueOrModified ] ),
                None,
                (
                    let inlined,nonInlined =
                        method.Parameters
                        |> List.partition _.IsInlinedObjectProp
                    let required,optional =
                        nonInlined |> List.partition _.Required
                    required @ optional @ inlined
                    |> List.mapi (fun i -> function
                        | InlinedObjectProp prop ->
                            Choice1Of2 (prop, prop.Type |> Type.FantomasFactory.mapToFantomas)
                        | Positional para ->
                            Choice2Of2(Source $"arg{i}", para.Type |> Type.FantomasFactory.mapToFantomas, para)
                        | Named(name,para) ->
                            Choice2Of2(name.Name, para.Type |> Type.FantomasFactory.mapToFantomas, para)
                        )
                    |> List.collect (function
                        | Choice1Of2(prop,typ) ->
                            [
                                Choice1Of2 <| Method.makePropParameter(prop,typ)
                                Choice2Of2 (SingleTextNode.make ",")
                            ]
                        | Choice2Of2(name,typ,para) ->
                            [
                                Choice1Of2 <| Method.makeParameter (not para.Required) (name,para,typ)
                                Choice2Of2(SingleTextNode.make ",")
                            ]
                        )
                    |> function
                        | [] -> []
                        | nodes ->
                            nodes
                            |> List.cutOffLast
                    |> fun items -> PatTupleNode(items, Range.Zero)
                    |> Pattern.Tuple
                    |> PatParenNode.make
                    |> Pattern.Paren
                    |> List.singleton
                ),       
                (
                    method.ReturnType
                    |> Type.FantomasFactory.mapToFantomas
                    |> fun typ ->
                        BindingReturnInfoNode(SingleTextNode.make ":", typ, Range.Zero)
                        |> Some
                ),
                SingleTextNode.make "=",
                Expr.Ident(SingleTextNode.make "Unchecked.defaultof<_>"),
                Range.Zero
                )
            |> Compatibility.wrapInCompatibilityDirective method.Compatibility
            |> MemberDefn.Member
            |> fun memberDefn ->
                SourcePacket<MemberDefn>.Empty
                |> SourcePacket.withMember memberDefn
                |> SourcePacket.withPathKey method.PathKey
                |> SourcePacket.withTarget (
                    SourceTarget.Empty
                    |> SourceTarget.withCompatibility method.Compatibility.ToTargetCompatibility
                    )
            )
    member this.MakeProperties() =
        this.Properties
        |> List.map (fun prop ->
            let isSourceName =
                prop.PathKey.Name.IsSource
                || prop.PathKey.Name |> function
                    | Modified(_, text) when text.StartsWith("``") && text.EndsWith("``") -> true
                    | Source _ -> true
                    | _ -> false
            let attributes =
                [
                    if not isSourceName then
                        $"Emit(\"{this.PathKey.Name.ValueOrSource}.{prop.PathKey.Name.ValueOrSource}{{{{ = $0}}}}\")"
                    elif not isSourceName then
                        $"Emit(\"$0.{prop.PathKey.Name.ValueOrSource}{{{{ = $1 }}}}\")"
                    if StabilityStatus.IsExperimental prop then
                        "Experimental(\"Experimental according to Electron\")"
                    if StabilityStatus.IsDeprecated prop then
                        "System.Obsolete"
                ]
                |> function
                    | [] -> None
                    | attrs -> MultipleAttributeListNode.make attrs |> Some
            let xmlDocs =
                [
                    match prop.Compatibility |> XmlDocs.Helpers.makeCompatibilityLine with
                    | ValueSome docs -> XmlDocs.wrapInPara docs |> String.concat ""
                    | ValueNone -> ()
                    match prop.Description with
                    | ValueSome docs -> docs
                    | ValueNone -> ()
                ]
                |> function
                    | [] -> None
                    | docs ->
                        XmlDocs.wrapInSummary docs
                        |> XmlDocs.Helpers.makeDocs
                        |> fun docs ->
                            XmlDocNode(docs, Range.Zero)
                            |> Some
            match isSourceName with
            | true ->
                // make autoStaticProperty
                MemberDefnAutoPropertyNode(
                    xmlDocs,
                    attributes,
                    MultipleTextsNode.make [
                        "static"
                        "member"
                        "val"
                    ],
                    None,
                    SingleTextNode.make prop.PathKey.Name.ValueOrSource,
                    Some (prop.Type |> Type.FantomasFactory.mapToFantomas),
                    SingleTextNode.make "=",
                    Expr.Ident(SingleTextNode.make "nativeOnly"),
                    Some (MultipleTextsNode.make (if prop.ReadOnly then "with get" else "with get, set")),
                    Range.Zero
                    )
                |> Compatibility.wrapInCompatibilityDirective prop.Compatibility
                |> MemberDefn.AutoProperty
            | false ->
                MemberDefnPropertyGetSetNode(
                    xmlDocs,
                    attributes,
                    MultipleTextsNode.make [ "static"; "member" ],
                    None, None,
                    IdentListNode.make [ prop.PathKey.Name.ValueOrModified ],
                    SingleTextNode.make "with",
                    PropertyGetSetBindingNode(
                        None, None, None,
                        SingleTextNode.make "get",
                        [
                            PatTupleNode([], Range.Zero)
                            |> Pattern.Tuple
                            |> PatParenNode.make
                            |> Pattern.Paren
                        ],
                        Some(BindingReturnInfoNode(
                            SingleTextNode.make ":",
                            Type.FantomasFactory.mapToFantomas prop.Type,
                            Range.Zero
                            )),
                        SingleTextNode.make "=",
                        Expr.Ident(SingleTextNode.make "Unchecked.defaultof<_>"),
                        Range.Zero
                        ),
                    (
                        if prop.ReadOnly then None else
                        SingleTextNode.make "and" |> Some
                    ),
                    (
                        if prop.ReadOnly then
                            None
                        else
                            Some (
                                PropertyGetSetBindingNode(
                                    None,None,None,
                                    SingleTextNode.make "set",
                                    [
                                        PatParameterNode.make
                                            (SingleTextNode.make "value")
                                            (Type.FantomasFactory.mapToFantomas prop.Type |> Some)
                                        |> Pattern.Parameter
                                        |> PatParenNode.make
                                        |> Pattern.Paren
                                    ],
                                    None, SingleTextNode.make "=",
                                    Expr.Ident(SingleTextNode.make "()"), Range.Zero
                                    )
                                )
                    ),
                    Range.Zero
                    )
                |> Compatibility.wrapInCompatibilityDirective prop.Compatibility
                |> MemberDefn.PropertyGetSet
            |> fun memberDefn ->
                SourcePacket<MemberDefn>.Empty
                |> SourcePacket.withMember memberDefn
                |> SourcePacket.withPathKey prop.PathKey
                |> SourcePacket.withTarget (
                    SourceTarget.Empty
                    |> SourceTarget.withCompatibility prop.Compatibility.ToTargetCompatibility
                    )
            )
    member this.MakeClasses() =
        this.ExportedClasses
        |> List.map _.ToTypeDefn
    member this.ToTypeDefn=
        let path = Class.MapPathKeyToProcess this.Process this.PathKey
        let attributes =
            [
                $"Import(\"{this.PathKey.Name.ValueOrSource}\", \"electron\")"
            ]
            |> MultipleAttributeListNode.make
        let interfaces =
            [
                if
                    this.Events.Length > 0
                    || this.Description
                    |> ValueOption.exists _.Contains("is an EventEmitter", System.StringComparison.Ordinal)
                then
                    MemberDefnInterfaceNode(
                        SingleTextNode.make "interface",
                        Type.makeSimple Injections.eventEmitterName,
                        None,[],Range.Zero
                    )
                    |> MemberDefn.Interface
            ]
        let typeDocs =
            [
                this.Process
                |> XmlDocs.Helpers.makeProcessLine
                |> XmlDocs.wrapInPara
                |> String.concat ""
                
                match this.Description with
                | ValueSome desc -> desc
                | ValueNone -> ()
            ]
            |> function
                | [] -> None
                | docs -> Some docs
            |> Option.map (
                XmlDocs.wrapInSummary
                >> XmlDocs.Helpers.makeDocs
                >> fun docs ->
                    XmlDocNode(docs, Range.Zero)
                )
        let members =
            interfaces
            @ this.MakeEvents()
            @ (this.MakeMethods() |> List.collect _.Members)
            @ (this.MakeProperties() |> List.collect _.Members)
        TypeNameNode(
            typeDocs, Some attributes,
            SingleTextNode.make "type",
            None,
            IdentListNode.make path.Name.ValueOrModified,
            None,
            [],
            None,
            Some (SingleTextNode.make "="),
            None,
            Range.Zero
            )
        |> TypeDefnRegularNode.make members
        |> TypeDefn.Regular
        |> ModuleDecl.TypeDefn
        |> fun decl ->
            if this.ExportedClasses.Length > 0 then
                NestedModuleNode(
                    None,
                    Some (MultipleAttributeListNode.make "Erase"),
                    SingleTextNode.make "module",
                    None, false,
                    IdentListNode.make path.Name.ValueOrModified, SingleTextNode.make "=",
                    (this.ExportedClasses |> List.collect (_.ToTypeDefn >> _.Decls)),
                    Range.Zero
                    )
                |> ModuleDecl.NestedModule
                |> List.singleton
                |> fun l -> decl :: l
            else
                [ decl ]
        |> fun decls ->
            SourcePackage.Empty
            |> SourcePackage.setDecls decls
            |> SourcePackage.withTarget (
                SourceTarget.Empty
                |> SourceTarget.setProcesses [
                    if this.Process.Main then
                        Target.Process.Main
                    if this.Process.Renderer then
                        Target.Process.Renderer
                    if this.Process.Utility then
                        Target.Process.Utility
                ]
                )
type Element with
    member this.MakeProperties =
        this.Properties
    member this.ToTypeDefn =
        let path = Class.MapPathKeyToProcess this.Process this.PathKey
        let attributes =
            [
                $"Import(\"{this.PathKey.Name.ValueOrSource}\", \"electron\")"
            ]
            |> MultipleAttributeListNode.make
        let interfaces =
            [
                if
                    this.Events.Length > 0
                    || this.Description
                    |> ValueOption.exists _.Contains("is an EventEmitter", System.StringComparison.Ordinal)
                then
                    MemberDefnInterfaceNode(
                        SingleTextNode.make "interface",
                        Type.makeSimple Injections.eventEmitterName,
                        None, [], Range.Zero
                        )
                    |> MemberDefn.Interface
            ]
        let typeDocs =
            [
                this.Process
                |> XmlDocs.Helpers.makeProcessLine
                |> XmlDocs.wrapInPara
                |> String.concat ""
                
                match this.Description with
                | ValueSome docs -> docs
                | ValueNone -> ()
            ]
            |> function
                | [] -> None
                | docs -> Some docs
            |> Option.map (
                XmlDocs.wrapInSummary
                >> XmlDocs.Helpers.makeDocs
                >> fun docs ->
                    XmlDocNode(docs, Range.Zero)
                )
        let members =
            interfaces
        TypeNameNode(
            typeDocs, Some attributes,
            SingleTextNode.make "type",
            None, IdentListNode.make path.Name.ValueOrModified,
            None,
            [],
            None,
            Some (SingleTextNode.make "="),
            None,
            Range.Zero
        )
        |> TypeDefnRegularNode.make members
        |> TypeDefn.Regular
        |> ModuleDecl.TypeDefn
        |> List.singleton
        |> fun decls ->
            SourcePackage.Empty
            |> SourcePackage.setDecls decls
            |> SourcePackage.withTarget (
                SourceTarget.Empty
                |> SourceTarget.setProcesses [
                    if this.Process.Main then
                        Target.Process.Main
                    if this.Process.Renderer then
                        Target.Process.Renderer
                    if this.Process.Utility then
                        Target.Process.Utility
                ]
                )
            
