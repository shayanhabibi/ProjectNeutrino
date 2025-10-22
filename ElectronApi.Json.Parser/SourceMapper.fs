module ElectronApi.Json.Parser.SourceMapper

open System.Collections.Generic
open ElectronApi.Json.Parser.FSharpApi
open Fantomas.Core
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open Fantomas.Utils

// 1. ApiDecoder - parses the json directly into F#
// 2. FSharpApi - reads and maps; modifies names and ensures everything is accounted
// 3. SourceMapper - maps the types to their respective end types, including attributes et al

module Cache =
    open Path
    type Cache = Dictionary<Path.Path, HashSet<Name>>
    let private cache = Cache()
    
    /// <summary>
    /// Tries to add the entry to the name cache. If it fails then it means there is a conflicting entry.
    /// </summary>
    /// <param name="entry"></param>
    let add (entry: PathKey) =
        // We try to add a fresh path/set. If that fails, then we just add the entry
        // to the existing path key.
        if
            cache.TryAdd(entry.Path, HashSet([entry.Name]))
            || cache[entry.Path].Add(entry.Name)
        // If either of the above return true, then this a unique entry.
        then Ok entry
        // If both of the above return false, then this is a duplicate entry
        else Error entry
        // If this errors, then you will likely just need to nest the type within
        // another module
module StringEnums =
    let cache = ResizeArray<StringEnum>()
    let add stringEnum = cache.Add stringEnum

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
        
    let [<Literal>] br = "<br/>"

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
            match fcsTypes with
            | [ typ; ApiType.Unit ] ->
                [ typ ] |> FantomasFactory.makeOption
            | [ typ; ApiType.Unknown | ApiType.Undefined | ApiType.Any ] ->
                TypeAppPrefixNode.Create(
                    makeSimple "U2",
                    [ FantomasFactory.mapToFantomas typ; FcsType.obj ]
                    )
                |> FcsType.AppPrefix
            | typs ->
                let length = typs.Length
                if length < 8 then
                    TypeAppPrefixNode.Create(
                        makeSimple $"U{length}",
                        typs |> List.map FantomasFactory.mapToFantomas
                        )
                    |> FcsType.AppPrefix
                else failwith $"Cannot create a U# type union with a length of {length}"
        static member makeCollection(fcsType: FcsType): FcsType =
            TypeArrayNode(fcsType, 1, Range.Zero)
            |> FcsType.Array
        static member makeLambda(funcOrMethod: FuncOrMethod): FcsType =
            let parameters =
                funcOrMethod.Parameters
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
                    )
            let names =
                funcOrMethod.Parameters
                |> List.mapi(fun i _ ->
                    if i = funcOrMethod.Parameters.Length - 1 then
                        SingleTextNode.make "->"
                    else SingleTextNode.make "*"
                    )
                // |> List.mapi (fun i -> function
                    // | Named(name, _) ->
                        // SingleTextNode.make name.Name.ValueOrModified
                    // | Positional _ ->
                        // SingleTextNode.make $"arg{i}"
                    // )
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
        static member mapToFantomas (apiType: ApiType): FcsType =
            match apiType with
            | ApiType.Any | FSharpApi.Type.Undefined | ApiType.Unknown -> FcsType.obj
            | ApiType.Unit -> FcsType.unit
            | FSharpApi.Type.Boolean -> FcsType.boolean
            | FSharpApi.Type.Date -> FcsType.LongIdent(IdentListNode.make [ "System"; "."; "DateTime" ])
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
            | Event { PathKey = pathKey }
            | ApiType.Structure { PathKey = pathKey } 
            | StringEnum { PathKey = pathKey } ->
                makeSimple pathKey.Name.ValueOrModified
            | Object structOrObject ->
                makeSimple "OBJECT_TODO"
            | Promise innerType ->
                FantomasFactory.makePromise [innerType]
            | Record(key, value) ->
                makeSimple "RECORD_TODO"
            | EventRef ``type`` ->
                FantomasFactory.mapToFantomas ``type``
            | Partial(``type``, l) ->
                // TODO
                FantomasFactory.mapToFantomas ``type``
            | Omit(``type``, l) ->
                // TODO
                FantomasFactory.mapToFantomas ``type``
            | Pick(``type``, l) ->
                // TODO
                FantomasFactory.mapToFantomas ``type``
            | Collection ``type`` ->
                FantomasFactory.mapToFantomas ``type``
                |> FantomasFactory.makeCollection
            | Array ``type`` ->
                FantomasFactory.mapToFantomas ``type``
                |> FantomasFactory.makeCollection
            | OneOf types ->
                FantomasFactory.makeUnion types
        // #if DEBUG
        static member tryDebugMapping (apiTypes: ApiType list) =
            let cache = ResizeArray<FcsType>()
            try
            apiTypes
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
        // #endif


type Structure with
    member this.ToPojoNode =
        let requiredProps,optionalProps =
            this.Properties
            |> List.partition _.Required
        let xmlDocs = [
            if this.Description.IsSome then
                XmlDocs.Boundaries.openSummary
                this.Description.Value
                XmlDocs.Boundaries.closeSummary
            if this.WebsiteUrl.IsSome then
                XmlDocs.makeClosedSeeAlso this.WebsiteUrl.Value
            yield!
                requiredProps
                |> List.map ( fun prop ->
                    XmlDocs.makeParam
                        prop.PathKey.Name.ValueOrModified
                        prop.Description )
            yield!
                optionalProps
                |> List.map ( fun prop ->
                    XmlDocs.makeParam
                        prop.PathKey.Name.ValueOrModified
                        prop.Description )
        ]
        let xmlDocs =
            XmlDocNode(
                xmlDocs
                |> List.map (sprintf "/// %s")
                |> List.toArray,
                Range.Zero
            )
        let implicitConstructor = ImplicitConstructorNode(
            xmlDoc = Some xmlDocs,
            attributes = None,
            accessibility = None,
            pat = Pattern.Tuple (PatTupleNode(
                [
                    for required in requiredProps do
                        ()
                ],
                Range.Zero
                )),
            self = None,
            range = Range.Zero
            )
        ()
