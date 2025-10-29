module rec ElectronApi.Json.Parser.FSharpApi

open System
open System.IO

open ElectronApi.Json.Parser.Decoder
open ElectronApi.Json.Parser.Utils
open Fantomas.Core
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open Thoth.Json.Net

#nowarn 40

type ExtensionHelper =
    static let cache = System.Collections.Generic.Dictionary<Name, ModifiedResult>()
    static let toResolve = ResizeArray<ModifiedResult>()
    static member add(input: Class) =
        if input.Extends.IsSome then toResolve.Add(ModifiedResult.Class input)
        cache.Add(input.PathKey.Name, ModifiedResult.Class input)
        input
    static member add(input: Structure) =
        if input.Extends.IsSome then toResolve.Add(ModifiedResult.Structure input)
        cache.Add(input.Name, ModifiedResult.Structure input)
        input
    static member add(input: Element) =
        if input.Extends.IsSome then toResolve.Add(ModifiedResult.Element input)
        cache.Add(input.PathKey.Name, ModifiedResult.Element input)
        input
    static member add(input: Module) =
        if input.Extends.IsSome then toResolve.Add(ModifiedResult.Module input)
        cache.Add(input.PathKey.Name, ModifiedResult.Module input)
        input
    static member private merge(structure: Structure, incoming: ModifiedResult) =
        match incoming with
        | Module ``module`` ->
            failwith "structure vs module"
            {
                structure with
                    Properties = structure.Properties @ ``module``.Properties
            }
        | Class ``class`` ->
            failwith "structure vs class"
            {
                structure with
                    Properties = structure.Properties @ ``class``.Properties
            }
        | Element element ->
            failwith "structure vs element"
            {
                structure with
                    Properties = structure.Properties @ element.Properties
            }
        | Structure incomingStructure ->
            {
                structure with
                    Properties = structure.Properties @ incomingStructure.Properties
            }
        |> fun structure ->
            {
                structure with Extends = ValueNone
            }
    static member private merge(class': Class, incoming: ModifiedResult) =
        match incoming with
        | Module ``module`` ->
            failwith "class vs module"
            {
                class' with
                    Events = class'.Events @ ``module``.Events // TODO - this seems incorrect; module events are static
                    StaticMethods = ``module``.Methods
                    StaticProperties = ``module``.Properties
            }
        | Class ``class`` ->
            {
                class' with
                    Properties = class'.Properties @ ``class``.Properties
                    Methods = class'.Methods @ ``class``.Methods
                    Events = class'.Events @ ``class``.Events
                    StaticMethods = class'.StaticMethods @ ``class``.StaticMethods
                    StaticProperties = class'.StaticProperties @ ``class``.StaticProperties
            }
        | Element element ->
            failwith "class vs element"
            {
                class' with
                    Properties = class'.Properties @ element.Properties
                    Methods = class'.Methods @ element.Methods
                    Events = class'.Events @ element.Events
            }
        | Structure structure ->
            failwith "class vs structure"
            {
                class' with
                    Properties = class'.Properties @ structure.Properties
            }
        |> fun class' ->
            { class' with Extends = None }
    static member private merge(module': Module, incoming: ModifiedResult) =
        match incoming with
        | Module incoming ->
            {
                module' with
                    Properties = module'.Properties @ incoming.Properties
                    Methods = module'.Methods @ incoming.Methods
                    Events = module'.Events @ incoming.Events
            }
        | Class incoming ->
            failwith "module vs class"
            {
                module' with
                    Properties = module'.Properties @ incoming.StaticProperties
                    Methods = module'.Methods @ incoming.StaticMethods
                    Events = module'.Events @ incoming.Events // TODO - this seems incorrect
            }
        | Element incoming ->
            failwith "module vs element"
            {
                module' with
                    Properties = module'.Properties @ incoming.Properties
                    Methods = module'.Methods @ incoming.Methods
                    Events = module'.Events @ incoming.Events
            }
        | Structure incoming ->
            failwith "module vs structure"
            {
                module' with
                    Properties = module'.Properties @ incoming.Properties
            }
        |> fun module' ->
            { module' with Extends = ValueNone }
    static member private merge(element: Element, incoming: ModifiedResult) =
        match incoming with
        | Module incoming ->
            failwith "element vs module"
            {
                element with
                    Properties = element.Properties @ incoming.Properties
                    Methods = element.Methods @ incoming.Methods
                    Events = element.Events @ incoming.Events
            }
        | Class incoming ->
            failwith "element vs class"
            {
                element with
                    Properties = element.Properties @ incoming.Properties
                    Methods = element.Methods @ incoming.Methods
                    Events = element.Events @ incoming.Events
            }
        | Element incoming ->
            {
                element with
                    Properties = element.Properties @ incoming.Properties
                    Methods = element.Methods @ incoming.Methods
                    Events = element.Events @ incoming.Events
            }
        | Structure incoming ->
            failwith "element vs structure"
            {
                element with
                    Properties = element.Properties @ incoming.Properties
            }
        |> fun element ->
            { element with Extends = None }
    static member resolveAndRetrieve() =
        toResolve
        |> Seq.toArray
        |> Array.map (function
            | ModifiedResult.Class item ->
                match cache.TryGetValue(item.Extends.Value) with
                | true, value ->
                    ExtensionHelper.merge(item,value)
                    |> ModifiedResult.Class
                | false, _ ->
                    ModifiedResult.Class item
            | ModifiedResult.Module item ->
                match cache.TryGetValue(item.Extends.Value) with
                | true, value ->
                    ExtensionHelper.merge(item,value)
                    |> ModifiedResult.Module
                | false, _ ->
                    ModifiedResult.Module item
            | ModifiedResult.Structure item ->
                match cache.TryGetValue(item.Extends.Value) with
                | true, value ->
                    ExtensionHelper.merge(item,value)
                    |> ModifiedResult.Structure 
                | false, _ ->
                    ModifiedResult.Structure item
            | ModifiedResult.Element item ->
                match cache.TryGetValue(item.Extends.Value) with
                | true, value ->
                    ExtensionHelper.merge(item,value)
                    |> ModifiedResult.Element
                | false, _ ->
                    ModifiedResult.Element item
            )
        |> fun result ->
            toResolve.Clear()
            cache.Clear()
            result

type ContextType =
    | MethodParameter
    | ObjectProperty
    | NA
[<Struct>]
type FactoryContext = {
    PathKey: Path.PathKey
    Type: ContextType
}

/// <summary>
/// Known results in <c>TypeInformationKind.InfoString</c> that will not be
/// represented elsewhere in the code base, and are considered 'native'/'global'.
/// </summary>
module Constants =
    let [<Literal>] undefined = "undefined"
    let [<Literal>] boolean = "boolean"
    let [<Literal>] integer = "Integer"
    let [<Literal>] void' = "void"
    let [<Literal>] null' = "null"
    let [<Literal>] promise = "Promise"
    let [<Literal>] record = "Record"
    let [<Literal>] any = "any"
    let [<Literal>] unknown = "unknown"
    let [<Literal>] event = "Event"
    let [<Literal>] number = "number"
    let [<Literal>] date = "Date"
    let [<Literal>] float = "Float"
    let [<Literal>] partial = "Partial"
    let [<Literal>] double = "Double"

// A representation of the OS compatibility tags as a record
[<Struct>]
type SpecifiedCompatibility = {
    Mac: bool
    Windows: bool
    Mas: bool
    Linux: bool
} with
    static member Init = {
        Mac = false
        Windows = false
        Mas = false
        Linux = false
    }
// If compatibility tags are not found, then we can assume it is a generalised API.
type Compatibility =
    | Specified of SpecifiedCompatibility
    | Unspecific
    member this.ToTargetCompatibility =
        match this with
        | Unspecific -> Target.Compatibility.all
        | Specified spec ->
            [
                if spec.Linux then
                    Target.Compatibility.Lin
                if spec.Mac then
                    Target.Compatibility.Mac
                if spec.Mas then
                    Target.Compatibility.Mas
                if spec.Windows then
                    Target.Compatibility.Win
            ]
            |> Target.Compatibility.fromList
        
            
module Compatibility =
    let inline wrapInCompatibilityDirectiveBefore
        (compatability: Compatibility)
        (node : ^T when ^T : (member AddBefore: TriviaNode -> unit) and ^T : (member AddAfter: TriviaNode -> unit)) =
        match compatability with
        | Unspecific -> node
        | Specified compat ->
            [
                if compat.Linux then
                    Spec.osLinDefine
                if compat.Mas then
                    Spec.osMasDefine
                if compat.Mac then
                    Spec.osMacDefine
                if compat.Windows then
                    Spec.osWinDefine
            ]
            |> String.concat " || "
            |> sprintf "!(!%s || !%s || !%s || !%s) || %s" Spec.osLinDefine Spec.osWinDefine Spec.osMacDefine Spec.osMasDefine
            |> fun directive ->
                let triviaBefore = TriviaNode(
                    TriviaContent.Directive $"#if {directive}"
                    , Range.Zero
                    )
                node.AddBefore(triviaBefore)
                node
    let inline wrapInCompatibilityDirectiveAfter
        (compatability: Compatibility)
        (node : ^T when ^T : (member AddBefore: TriviaNode -> unit) and ^T : (member AddAfter: TriviaNode -> unit)) =
        match compatability with
        | Unspecific -> node
        | Specified _ ->
            let triviaAfter = TriviaNode(
                TriviaContent.Directive "#endif",
                Range.Zero
                )
            node.AddAfter(triviaAfter)
            node
 
    let inline wrapInCompatibilityDirective
        (compatability: Compatibility)
        (node : ^T when ^T : (member AddBefore: TriviaNode -> unit) and ^T : (member AddAfter: TriviaNode -> unit)) =
        match compatability with
        | Unspecific -> node
        | Specified compat ->
            [
                if compat.Linux then
                    Spec.osLinDefine
                if compat.Mas then
                    Spec.osMasDefine
                if compat.Mac then
                    Spec.osMacDefine
                if compat.Windows then
                    Spec.osWinDefine
            ]
            |> String.concat " || "
            |> sprintf "!(!%s || !%s || !%s || !%s) || %s" Spec.osLinDefine Spec.osWinDefine Spec.osMacDefine Spec.osMasDefine
            |> fun directive ->
                let triviaBefore = TriviaNode(
                    TriviaContent.Directive $"#if {directive}"
                    , Range.Zero
                    )
                let triviaAfter = TriviaNode(
                    TriviaContent.Directive "#endif",
                    Range.Zero
                    )
                node.AddBefore(triviaBefore)
                node.AddAfter(triviaAfter)
                node
            
    let fromTags (tags: DocumentationTag array) =
        let state = Unspecific
        tags |> Array.fold (
            fun state tag ->
                match tag with
                | OS_MACOS | OS_MAS | OS_WINDOWS | OS_LINUX ->
                    let currentCompat =
                        match state with
                        | Unspecific ->
                            SpecifiedCompatibility.Init
                        | Specified compat ->
                            compat
                    match tag with
                    | OS_MACOS -> { currentCompat with Mac = true }
                    | OS_MAS -> { currentCompat with Mas = true }
                    | OS_WINDOWS -> { currentCompat with Windows = true }
                    | OS_LINUX -> { currentCompat with Linux = true }
                    | _ -> failwith "Unreachable"
                    |> Specified
                | _ -> state
            ) state

type StabilityStatus = {
    Experimental: bool
    Deprecated: bool
} with
    static member inline IsExperimental(object: 'T when 'T : (member Stability: StabilityStatus)) = object.Stability.Experimental
    static member inline IsDeprecated(object: 'T when 'T : (member Stability: StabilityStatus)) = object.Stability.Deprecated
    static member Init = { Experimental = false; Deprecated = false }

module StabilityStatus =
    let fromTags (tags: DocumentationTag array) =
        let state = StabilityStatus.Init
        tags |> Array.fold (fun state -> function
            | DocumentationTag.STABILITY_DEPRECATED -> { state with Deprecated = true }
            | DocumentationTag.STABILITY_EXPERIMENTAL -> { state with Experimental = true }
            | _ -> state
            ) state

module ReadOnly =
    let fromTags (tags: DocumentationTag array) = tags |> Array.exists _.IsAVAILABILITY_READONLY
type StringEnumCase = { Value: Name; Description: string voption }
type StringEnum = { PathKey: Path.PathKey; Cases: StringEnumCase list }
type StructOrObject = {
    PathKey: Path.PathKey
    Properties: Property list
}
type ParameterInfo = {
    Description: string voption
    Required: bool
    Type: Type
}
type Parameter =
    | Positional of ParameterInfo
    | Named of name: Path.PathKey * info: ParameterInfo
    | InlinedObjectProp of prop: Property
    member this.Required =
        match this with
        | Positional { Required = required } 
        | Named(_, { Required = required })
        | InlinedObjectProp { Required = required } -> required
    member this.Description =
        match this with
        | Positional { Description = description } 
        | Named(_, { Description = description })
        | InlinedObjectProp { Description = description } -> description
    member this.Compatibility =
        match this with
        | InlinedObjectProp { Compatibility = value } -> value
        | _ -> Unspecific
    member this.Type =
        match this with
        | Positional { Type = ``type`` } 
        | Named(_, { Type = ``type`` })
        | InlinedObjectProp { Type = ``type`` } -> ``type``
    member this.PathKey =
        match this with
        | Named(path, _) 
        | InlinedObjectProp { PathKey = path } -> path |> ValueSome
        | Positional _ -> ValueNone
        

type FuncOrMethod = {
    Name: Path.PathKey
    Parameters: Parameter list
    Returns: Type
}

module FuncOrMethod =
    // We want to store lambdas/funcs so we can raise them to delegates IF they have
    // more than 1 parameter.
    let private cache = System.Collections.Generic.Dictionary<Path.PathKey, FuncOrMethod>()
    module Cache =
        let add key value = cache.Add(key,value)
        let tryAdd key value =
            if cache.TryAdd(key,value)
            then Ok value
            else Error value
        let get key = cache[key]
        let tryGet key =
            match cache.TryGetValue(key) with
            | true, value -> ValueSome value
            | false, _ -> ValueNone
    let getCacheValues = cache.Values

type EventInfo = {
    PathKey: Path.PathKey
    Properties: Property list
}

[<RequireQualifiedAccess>]
type LiteralType =
    | Float of float
    | Int of int
    | String of string
    | Char of char
type Type =
    // Primitives
    | String
    | Boolean
    | Integer
    | Float | Double | Number
    | Unit | Undefined
    | Any
    | Unknown
    | Date
    | Constant of LiteralType
    //
    | Function of FuncOrMethod
    | StringEnum of StringEnum
    | StructureRef of string
    | Object of StructOrObject
    | Promise of innerType: Type
    | Record of key: Type * value: Type
    | Event of EventInfo
    | EventRef of Type
    | Partial of Type * string list
    | Omit of Type * string list
    | Pick of Type * string list
    | Collection of Type
    | Array of Type
    | OneOf of Type list
    | Tuple of Type list
    | Join of structureRef: string * props: Property list


module Type =
    let readStringEnumCase (_: FactoryContext): PossibleStringValue -> StringEnumCase = fun { Value = value; Description = desc } ->
        {
            Value =
                Name.createPascal value
            Description =
                if String.IsNullOrWhiteSpace desc
                then ValueNone
                else ValueSome desc
        }
    let readStringEnum (ctx: FactoryContext) (innerTypes: TypeInformation array voption): TypeInformationKind.String -> StringEnum =
        fun { PossibleValues = possibleValues } ->
            if innerTypes.IsSome then
                failwith $"Unexpected inner types for a String {innerTypes}"
            // If we're not reading the string enum as a parameter or something else which gives us a name, then
            // we'll have to generate a name later.
            {
                PathKey = ctx.PathKey.CreateStringEnum()
                Cases =
                    possibleValues
                    |> Option.map (
                        Array.filter (_.Value.Length >> (<) 0)
                        >> Array.map (readStringEnumCase ctx)
                        >> Array.toList
                        )
                    |> Option.defaultValue []
            }
    // If we hit an unknown, we want to throw so we can see how that type should be managed.
    let readInfoString (ctx: FactoryContext) (innerTypes: TypeInformation array voption): TypeInformationKind.InfoString -> Type = function
        | info when innerTypes.IsNone ->
            match info with
            | { Type = Constants.float | Constants.number | "Number" | "Float" } -> Type.Float
            | { Type = Constants.double } -> Type.Double
            | { Type = Constants.integer } -> Type.Integer
            | { Type = Constants.unknown | Constants.any } -> Type.Any
            | { Type = Constants.undefined } -> Type.Undefined
            | { Type = Constants.event } ->
                {
                    PathKey =
                        Name.createPascal "Event"
                        |> ctx.PathKey.CreateEvent
                    Properties = []
                }
                |> Type.Event
            | { Type = Constants.boolean | "Boolean" } -> Type.Boolean
            | { Type = Constants.void' | Constants.null' } -> Type.Unit
            | { Type = Constants.date } -> Type.Date
            | { Type = Constants.partial | Constants.record | Constants.promise } as infoString ->
                #if DEBUG
                failwith $"Type expects inner types {infoString}"
                #else
                Type.Any
                #endif
            // If an identifier is not a combination of strings and '.', then it's probably not
            // valid according to the electron-api docs parser spec. It is likely injected directly
            // to TypeScript definitions. We inspect it, and match against them and handle them appropriately where they
            // come up
            | { Type = ident }
                when ident |> String.forall (function
                    | c when Char.IsAsciiLetterOrDigit c -> true
                    | '.' -> true
                    | _ -> false) |> not ->
                match ident with
                // I guess these are constants?
                | "'file'" | "'rawData'" ->
                    ident.Trim(''')
                    |> LiteralType.String
                    |> Type.Constant
                | "(...args: any[]) => any" ->
                    // TODO
                    Type.StructureRef ident
                | "RequestInit & { bypassCustomProtocolHandlers?: boolean }" ->
                    // TODO - what is the request init type?. It's not a structure.
                    // ANSWER - it's node.js or mdn browser type
                    Type.Join("RequestInit", [
                        {
                            Property.Required = false
                            PathKey = ctx.PathKey.CreateInlineOptions().CreateProperty(Name.createCamel "bypassCustomProtocolHandlers")
                            Description = ValueNone
                            UrlFragment = ValueNone
                            Type = Type.Boolean
                            Compatibility = Unspecific
                            Stability = StabilityStatus.Init
                            ReadOnly = false
                        }
                    ])
                | "[number, number]" ->
                    Type.Tuple [Type.Number; Type.Number]
                | "(options: BrowserWindowConstructorOptions) => WebContents" ->
                    let funcName = ctx.PathKey.CreateLambda()
                    {
                        FuncOrMethod.Name = funcName
                        Parameters = [
                            Named(
                                funcName.CreateParameter(Source "options"),
                                { Description = ValueNone
                                  Required = true
                                  Type = Type.StructureRef "BrowserWindowConstructorOptions" }
                            )
                        ]
                        Returns = Type.StructureRef "WebContents"
                    }
                    |> Type.Function
                // Not sure what this is supposed to be indicating?
                | "typeof TouchBarSegmentedControl"
                | "typeof TouchBarScrubber"
                | "typeof TouchBarPopover"
                | "typeof TouchBarLabel"
                | "typeof TouchBarGroup"
                | "typeof TouchBarColorPicker"
                | "typeof TouchBarButton"
                | "typeof TouchBarSlider"
                | "typeof TouchBarSpacer"
                | "typeof TouchBarOtherItemsProxy"
                //
                // This relates to providing typing for TS for 'types' like url etc. Not relevant for our code.
                | "UserDefaultTypes[Type]" -> Type.StructureRef ident
                //
                | _ -> failwith $"Unhandled simple string type: %s{ident}"
            | { Type = struc } -> Type.StructureRef struc
        | { Type = Constants.promise } when innerTypes.IsSome && innerTypes.Value.Length = 1 ->
            innerTypes.Value
            |> Array.head
            |> (fromTypeInformation ctx)
            |> Type.Promise
        | { Type = Constants.record } when innerTypes.IsSome && innerTypes.Value.Length = 2 ->
            let left,right = innerTypes.Value.[0], innerTypes.Value.[1]
            Type.Record(
                key = fromTypeInformation ctx left,
                value = fromTypeInformation ctx right
                )
        | { Type = "Array" } when innerTypes.IsSome && innerTypes.Value.Length = 1 ->
            Type.Array(fromTypeInformation ctx innerTypes.Value.[0])
        | { Type = Constants.partial } when innerTypes.IsSome && innerTypes.Value.Length = 1 -> // TODO
            // as it currently stands, there is only one case of this
            innerTypes.Value.[0]
            |> (fromTypeInformation ctx)
        | { Type = Constants.promise | Constants.record | Constants.partial } as kind ->
            failwithf $"Failed to read %A{kind} with innerTypes %A{innerTypes}"
        | { Type = typ } ->
            // Throw for unencountered types so we can inspect and modify our approach
            failwithf $"Unexpected parts for %s{typ} %A{innerTypes}"
    
    let readObjectKind (ctx: FactoryContext) (innerTypes: TypeInformation array voption): TypeInformationKind.Object -> Type = function
        | o when innerTypes.IsSome ->
            failwith $"unhandled object {o} with inner types {innerTypes}"
        | { Properties = props } ->
            let ctx = { ctx with PathKey = ctx.PathKey.CreateInlineOptions() }
            props
            |> Array.map (extractFromPropertyDocumentationBlock ctx)
            |> Array.toList
            |> fun props ->
                { StructOrObject.PathKey = ctx.PathKey; Properties = props }
                |> Type.Object
    let readInfoArray (ctx: FactoryContext) (innerTypes: TypeInformation array voption): TypeInformationKind.InfoArray -> Type = function
        | a when innerTypes.IsSome ->
            failwith $"Unhandled array {a} with inner types {innerTypes}"
        | { Type = typs } ->
            // The simple info array is just a union of different types
            typs
            |> Array.map (fromTypeInformation ctx)
            |> Array.toList
            |> Type.OneOf
    let inline private readParameter
        (ctx: FactoryContext)
        (block: ^T when
            ^T: (member Name: string)
            and ^T: (member Required: bool)
            and ^T: (member Description: string)
            and ^T: (member TypeInformation: TypeInformation)) =
        // TODO - this might not be hit anymore.
        let name =
            if String.IsNullOrWhiteSpace block.Name
            // nameless/positional parameter
            then ValueNone
            else Name.createCamel block.Name |> ValueSome
        let desc =
            if String.IsNullOrWhiteSpace block.Description
            then ValueNone
            else block.Description |> ValueSome
        let required = block.Required
        let ctx =
            { ctx with
                PathKey = 
                    match name with
                    | ValueSome name ->
                        ctx.PathKey.CreateParameter name
                    | ValueNone ->
                        ctx.PathKey.CreateParameter (Source "arg")
            }
        let typ = fromTypeInformation ctx block.TypeInformation
        match name with
        | ValueSome _ ->
            Parameter.Named(
                ctx.PathKey,
                {
                    Description = desc
                    Required = required
                    Type = typ
                }
            )
        | ValueNone ->
            Parameter.Positional {
                Description = desc
                Required = required
                Type = typ
            }
    let readMethodParameter (ctx: FactoryContext) (block: MethodParameterDocumentation): Parameter = readParameter ctx block
    let readEventParameter (ctx: FactoryContext) (block: EventParameterDocumentation): Parameter = readParameter ctx block
    let readFunctionKind (ctx: FactoryContext) (innerTypes: TypeInformation array voption): TypeInformationKind.Function -> Type = function
        | f when innerTypes.IsSome ->
            failwith $"Unhandled function type {f} with innerTypes {innerTypes}"
        | { Parameters = parameters; Returns = returns } ->
            let ctx = { ctx with PathKey = ctx.PathKey.CreateLambda() }
            {
                // Lambda probably, we're only really interested in the signature then
                Name = ctx.PathKey
                Parameters =
                    parameters
                    |> Array.map (readMethodParameter ctx)
                    |> Array.toList
                Returns =
                    returns
                    |> Option.toValueOption
                    |> ValueOption.map (fromTypeInformation ctx)
                    |> ValueOption.defaultValue Type.Unit
            }
            |> fun lambda ->
                if lambda.Parameters.Length > 1 then
                    FuncOrMethod.Cache.tryAdd
                        lambda.Name
                        lambda
                    |> function
                        | Ok lambda -> Type.Function lambda
                        | Error lambda ->
                            failwith $"Duplicate definitions of lambda {lambda}"
                else Type.Function lambda
            
    let readEventKind (ctx: FactoryContext) (innerTypes: TypeInformation array voption): TypeInformationKind.Event -> Type = function
        | e when innerTypes.IsSome ->
            failwith $"unhandled event type {e} with inner types {innerTypes}"
        | { EventProperties = props } ->
            let ctx =
                 { ctx with PathKey = (
                         ctx.PathKey.Name.ValueOrModified
                         |> Name.createPascal
                         |> ctx.PathKey.CreateEvent ) }
            props
            |> Array.map (extractFromPropertyDocumentationBlock ctx)
            |> Array.toList
            |> fun props ->
                { PathKey = ctx.PathKey
                  Properties = props }
                |> Type.Event
    let readEventRefKind (ctx: FactoryContext) (innerTypes: TypeInformation array voption): TypeInformationKind.EventRef -> Type = function
        | e when innerTypes.IsSome ->
            failwith $"Unhandled event type {e} with innerTypes {innerTypes}"
        | { EventPropertiesReference = props } ->
            props |> (fromTypeInformation ctx)
            |> Type.EventRef
    let inline isCollection (kind: ^T when ^T : (member Collection: bool)) = kind.Collection
    let inline wrap kind input =
        if isCollection kind then Type.Collection input else input
    let fromTypeInformation (ctx: FactoryContext) (typeInfo: TypeInformation): Type = 
        let kind,innerTypes =
            match typeInfo with
            | WithInnerTypes(kind, typs) -> kind, ValueSome typs
            | TypeInformation.TypeInformation typ -> typ, ValueNone
        match kind with
        | InfoArray ({ Type = typs } as kind) ->
            typs
            |> Array.map (fromTypeInformation ctx)
            |> Array.toList
            |> Type.OneOf
            |> wrap kind
        | InfoString infoString ->
            infoString
            |> readInfoString ctx innerTypes
            |> wrap infoString
        | TypeInformationKind.String typeString ->
            if innerTypes.IsSome then 
                failwithf $"Unhandled type that is indicated to be a string but had inner types %A{typeString}"
            else
            match typeString with
            | { PossibleValues = Some [||] | None } ->
                wrap typeString Type.String
            | _ ->
                typeString
                |> readStringEnum ctx innerTypes
                |> Type.StringEnum
                |> wrap typeString
        | TypeInformationKind.Object o ->
            readObjectKind ctx innerTypes o
            |> wrap o
        | TypeInformationKind.Event event ->
            readEventKind ctx innerTypes event
            |> wrap event
        | TypeInformationKind.EventRef eventRef ->
            readEventRefKind ctx innerTypes eventRef
            |> wrap eventRef
        | TypeInformationKind.Function ``function`` ->
            ``function``
            |> readFunctionKind ctx innerTypes
            |> wrap ``function``
type Property = {
    Required: bool
    PathKey: Path.PathKey
    Description: string voption
    UrlFragment: string voption
    Type: Type
    Compatibility: Compatibility
    Stability: StabilityStatus
    ReadOnly: bool
}
module Property =
    let inline isRequired (t: ^T when ^T : (member Required: bool)) = t.Required
    let inline isReadOnly (t: ^T when ^T : (member ReadOnly: bool)) = t.ReadOnly
let extractFromPropertyDocumentationBlock (ctx: FactoryContext) (propBlock: PropertyDocumentationBlock) =
    let name = Name.createCamel propBlock.DocumentationBlock.Name
    let pathKey = ctx.PathKey.CreateProperty name
    let ctx = { PathKey = pathKey; Type = ContextType.ObjectProperty }
    {
        Required = Property.isRequired propBlock
        PathKey = pathKey
        Description =
            propBlock.DocumentationBlock.Description
            |> function
                | text when String.IsNullOrWhiteSpace text ->
                    ValueNone
                | text -> ValueSome text
        UrlFragment = propBlock.DocumentationBlock.UrlFragment |> Option.toValueOption
        Type =
            Type.fromTypeInformation ctx propBlock.TypeInformation
        Compatibility =
            propBlock.DocumentationBlock.AdditionalTags
            |> Compatibility.fromTags
        Stability =
            propBlock.DocumentationBlock.AdditionalTags
            |> StabilityStatus.fromTags
        ReadOnly = propBlock.DocumentationBlock.AdditionalTags |> ReadOnly.fromTags
    }

type Event = {
    PathKey: Path.PathKey
    Description: string voption
    Compatibility: Compatibility
    UrlFragment: string voption
    Parameters: Parameter list
    Stability: StabilityStatus
}

module Event =
    let fromDocBlock (ctx: FactoryContext) (block: EventDocumentationBlock): Event =
        let name = block.DocumentationBlock.Name |> Name.cacheOrCreatePascal
        // We make assumptions that these event blocks are always the child of a module or a type.
        // We then create the binding off this.
        if not (ctx.PathKey.IsModule || ctx.PathKey.IsType) then
            failwith $"Tried to read an event docblock from a non-module/non-type {ctx}"
        let ctx = { PathKey = ctx.PathKey.CreateEvent(name); Type = ctx.Type }
        {
            PathKey = ctx.PathKey
            Description =
                block.DocumentationBlock.Description
                |> function
                    | text when String.IsNullOrWhiteSpace text ->
                        ValueNone
                    | text -> ValueSome text
            Compatibility =
                block.DocumentationBlock.AdditionalTags
                |> Compatibility.fromTags
            UrlFragment =
                block.DocumentationBlock
                    .UrlFragment
                |> Option.toValueOption
            Parameters =
                block.Parameters
                |> Array.map (Type.readEventParameter ctx)
                |> Array.toList
            Stability =
                block.DocumentationBlock.AdditionalTags
                |> StabilityStatus.fromTags
        }

type Structure = {
    Name: Name
    Extends: Name voption
    Description: string voption
    WebsiteUrl: string voption
    Properties: Property list
}

module Structure =
    let readFromDocContainer (ctx: FactoryContext option) (container: StructureDocumentationContainer) =
        let name =
            container.BaseDocumentationContainer.Name
            |> Name.cacheOrCreatePascal
        let pathKey =
            match ctx with
            | None ->
                Path.PathKey.Type(Path.Type(Path.ModulePath.Root, name))
            | Some { PathKey = pathKey } ->
                pathKey.CreateType(name)
        let ctx = { PathKey = pathKey; Type = ContextType.ObjectProperty }
        {
            Name = name
            Extends =
                container.BaseDocumentationContainer.Extends
                |> Option.map Name.cacheOrCreatePascal
                |> Option.toValueOption
            Description =
                container.BaseDocumentationContainer.Description
                |> function
                    | text when String.IsNullOrWhiteSpace text -> ValueNone
                    | text -> ValueSome text
            WebsiteUrl =
                container.BaseDocumentationContainer.WebsiteUrl
                |> function
                    | text when String.IsNullOrWhiteSpace text -> ValueNone
                    | text -> ValueSome text
            Properties =
                container.Properties
                |> Array.map (extractFromPropertyDocumentationBlock ctx)
                |> Array.toList
        }
        |> ExtensionHelper.add

type Method = {
    PathKey: Path.PathKey
    Compatibility: Compatibility
    Stability: StabilityStatus
    Description: string voption
    UrlFragment: string voption
    RawGenerics: string option
    Parameters: Parameter list
    ReturnType: Type
}
module Method =
    let private objectParameterCache = ResizeArray<StructOrObject>()
    let inlineObjectParameters ({ Parameters = parameters } as method) =
        // This case does not occur in the current iteration of the api. Either way, we have to ensure
        // the integrity of our model by pre-empting it.
        let cacheNonLastObjects: Parameter list -> unit =
            List.rev
            >> List.tail
            >> List.map (function
                | Named(_, { Type = typ })
                | Positional { Type = typ } -> typ
                // This should not exist before this function
                | InlinedObjectProp _ -> failwith "Tried to inline an already inlined parameter option"
            )
            >> List.filter _.IsObject
            >> List.iter (function Type.Object o -> objectParameterCache.Add o | _ -> failwith "UNREACHABLE")
        let inlineObject: StructOrObject -> Parameter list = _.Properties >> List.map Parameter.InlinedObjectProp
        match parameters |> List.tryLast with
        | Some (Named(_, { Type = Type.Object structOrObject }))
        | Some (Positional { Type = Type.Object structOrObject })
        // We can inline the last parameter if it is an object and none of the fields are required
            when structOrObject.Properties |> List.forall (_.Required >> not) ->
            cacheNonLastObjects method.Parameters
            // Remove the tail from the parameters and replace it with the inlined properties of the object
            method.Parameters
            |> List.rev
            |> function
                | [] as tail
                | _ :: tail ->
                    (inlineObject structOrObject |> List.rev) @ tail
            |> List.rev
            |> fun parameters ->
                { method with Parameters = parameters }
        // We can also inline an object that has required props so long as it is the only parameter in the method.
        | Some (Named(_, { Type = Type.Object structOrObject }))
        | Some (Positional { Type = Type.Object structOrObject }) when parameters.Length = 1 ->
            // yes we can inline
            { method with Parameters = inlineObject structOrObject }
        // Otherwise, we cannot inline the object
        | Some (Named(_, { Type = Type.Object structOrObject }))
        | Some (Positional { Type = Type.Object structOrObject }) ->
            // no we cannot inline; cache the object
            structOrObject |> objectParameterCache.Add
            cacheNonLastObjects method.Parameters
            method
        // NA
        | _ -> method
            

    let fromDocBlock (ctx: FactoryContext) (block: MethodDocumentationBlock) =
        let name = block.DocumentationBlock.Name |> Name.createCamel
        let pathKey = name |> ctx.PathKey.CreateMethod
        let ctx = { PathKey = pathKey; Type = ContextType.MethodParameter }
        {
            PathKey = pathKey
            Compatibility = block.DocumentationBlock.AdditionalTags |> Compatibility.fromTags
            Stability = block.DocumentationBlock.AdditionalTags |> StabilityStatus.fromTags
            Description =
                block
                    .DocumentationBlock
                    .Description
                |> function
                    | text when String.IsNullOrWhiteSpace text -> ValueNone
                    | text -> ValueSome text
            UrlFragment =
                block.DocumentationBlock.UrlFragment
                |> Option.toValueOption
            RawGenerics =
                block.RawGenerics
            Parameters =
                block.Parameters
                |> Array.map (Type.readMethodParameter ctx)
                |> Array.toList
            ReturnType =
                block.Returns
                |> Option.map (Type.fromTypeInformation ctx)
                |> Option.defaultValue Type.Unit
        }
        |> inlineObjectParameters

type Class = {
    PathKey: Path.PathKey
    Process: ProcessBlock
    Constructor: Parameter list option
    InstanceName: Name option
    StaticMethods: Method list
    StaticProperties: Property list
    Methods: Method list
    Events: Event list
    Properties: Property list
    Extends: Name option
    Description: string voption
    WebsiteUrl: string
}

module Class =
    let private objectParameterCache = ResizeArray<StructOrObject>()
    let inlineObjectParameters ({ Constructor = parameters } as class'): Class =
        match parameters with
        | Some parameters ->
            // This case does not occur in the current iteration of the api. Either way, we have to ensure
            // the integrity of our model by pre-empting it.
            let cacheNonLastObjects: Parameter list -> unit =
                List.rev
                >> List.tail
                >> List.map (function
                    | Named(_, { Type = typ })
                    | Positional { Type = typ } -> typ
                    // This should not exist before this function
                    | InlinedObjectProp _ -> failwith "Tried to inline an already inlined parameter option"
                )
                >> List.filter _.IsObject
                >> List.iter (function Type.Object o -> objectParameterCache.Add o | _ -> failwith "UNREACHABLE")
            let inlineObject: StructOrObject -> Parameter list = _.Properties >> List.map Parameter.InlinedObjectProp
            match parameters |> List.tryLast with
            | Some (Named(_, { Type = Type.Object structOrObject }))
            | Some (Positional { Type = Type.Object structOrObject })
            // We can inline the last parameter if it is an object and none of the fields are required
                when structOrObject.Properties |> List.forall (_.Required >> not) ->
                cacheNonLastObjects parameters
                // Remove the tail from the parameters and replace it with the inlined properties of the object
                parameters
                |> List.rev
                |> function
                    | [] as tail
                    | _ :: tail ->
                        (inlineObject structOrObject |> List.rev) @ tail
                |> List.rev
                |> fun parameters ->
                    { class' with Constructor = Some parameters }
            // We can also inline an object that has required props so long as it is the only parameter in the method.
            | Some (Named(_, { Type = Type.Object structOrObject }))
            | Some (Positional { Type = Type.Object structOrObject }) when parameters.Length = 1 ->
                // yes we can inline
                { class' with Constructor = Some(inlineObject structOrObject |> List.sortBy (_.Required >> not)) }
            // Otherwise, we cannot inline the object
            | Some (Named(_, { Type = Type.Object structOrObject }))
            | Some (Positional { Type = Type.Object structOrObject }) ->
                // no we cannot inline; cache the object
                structOrObject |> objectParameterCache.Add
                cacheNonLastObjects parameters
                class'
            // NA
            | _ -> class'
        | None ->
            class'
    let fromDocContainer (ctx: FactoryContext option) (container: ClassDocumentationContainer): Class =
        let name =
            container.BaseDocumentationContainer.Name
            |> Name.cacheOrCreatePascal
        let ctx =
            match ctx with
            | Some ctx ->
                { ctx with PathKey = ctx.PathKey.CreateType(name) }
            | None ->
                let root = Path.ModulePath.Root
                let name = name
                let pathKey = Path.PathKey.Type(Path.Type(root,name))
                { PathKey = pathKey; Type = ContextType.NA }
        {
            Process = container.Process
            Constructor =
                container.ConstructorMethod
                |> Option.map (function
                    | { Parameters = parameters } ->
                        let ctx = { ctx with PathKey = ctx.PathKey }
                        parameters
                        |> Array.map (Type.readMethodParameter ctx)
                        |> Array.toList
                    )
            InstanceName =
                container.InstanceName
                |> function
                    | text when String.IsNullOrWhiteSpace text ->
                        None
                    | text -> Some text
                |> Option.map Name.createPascal
            StaticMethods =
                container.StaticMethods
                |> Array.map (Method.fromDocBlock { ctx with Type = ContextType.MethodParameter })
                |> Array.toList
            StaticProperties =
                container.StaticProperties
                |> Array.map (extractFromPropertyDocumentationBlock { ctx with Type = ContextType.ObjectProperty })
                |> Array.toList
            Methods =
                container.InstanceMethods
                |> Array.map (Method.fromDocBlock { ctx with Type = ContextType.MethodParameter })
                |> Array.toList
            Events =
                container.InstanceEvents
                |> Array.map (Event.fromDocBlock { ctx with Type = ContextType.ObjectProperty })
                |> Array.toList
            Properties =
                container.InstanceProperties
                |> Array.map (extractFromPropertyDocumentationBlock { ctx with Type = ContextType.ObjectProperty })
                |> Array.toList
            PathKey = ctx.PathKey
            Extends =
                container.BaseDocumentationContainer.Extends
                |> Option.map Name.cacheOrCreatePascal
            Description =
                container.BaseDocumentationContainer.Description
                |> function
                    | text when String.isNotNullOrWhitespace text ->
                        ValueSome text
                    | _ ->
                        ValueNone
            WebsiteUrl =
                container.BaseDocumentationContainer
                    .WebsiteUrl
        }
        |> inlineObjectParameters
        |> ExtensionHelper.add

type Element = {
    PathKey: Path.PathKey
    Extends: Name option
    Description: string voption
    WebsiteUrl: string
    Process: ProcessBlock
    Methods: Method list
    Events: Event list
    Properties: Property list
}

module Element =
    let fromDocContainer (ctx: FactoryContext option) (container: ElementDocumentationContainer): Element =
        let name =
            container.BaseDocumentationContainer.Name
            |> Name.cacheOrCreatePascal
        let ctx =
            match ctx with
            | Some ctx ->
                { ctx with PathKey = ctx.PathKey.CreateType(name) }
            | None ->
                let pathKey = Path.Type(Path.ModulePath.Root, name) |> Path.PathKey.Type
                { PathKey = pathKey; Type = ContextType.NA }
        {
            PathKey = ctx.PathKey
            Extends =
                container.BaseDocumentationContainer.Extends
                |> Option.map Name.cacheOrCreatePascal
            Description =
                container.BaseDocumentationContainer.Description
                |> function
                    | text when String.IsNullOrWhiteSpace text ->
                        ValueNone
                    | text -> ValueSome text
            WebsiteUrl =
                container.BaseDocumentationContainer.WebsiteUrl
            Process = container.Process
            Methods =
                container.Methods
                |> Array.map (Method.fromDocBlock { ctx with Type = ContextType.MethodParameter })
                |> Array.toList
            Events =
                container.Events
                |> Array.map (Event.fromDocBlock { ctx with Type = ContextType.ObjectProperty })
                |> Array.toList
            Properties =
                container.Properties
                |> Array.map (extractFromPropertyDocumentationBlock { ctx with Type = ContextType.ObjectProperty })
                |> Array.toList
        }
        |> ExtensionHelper.add

type Module = {
    PathKey: Path.PathKey
    // feel like this shouldnt exist for modules though
    Extends: Name voption
    WebsiteUrl: string voption
    Description: string voption
    Process: ProcessBlock
    Methods: Method list
    Events: Event list
    Properties: Property list
    ExportedClasses: Class list
}

module Module =
    let fromDocContainer (ctx: FactoryContext option) (container: ModuleDocumentationContainer): Module =
        let name =
            container.BaseDocumentationContainer.Name
            |> Name.cacheOrCreatePascal
        let ctx =
            match ctx with
            | Some ctx ->
                { ctx with PathKey = ctx.PathKey.CreateModule(name) }
            | None ->
                { PathKey = Path.PathKey.CreateModule(name); Type = ContextType.NA }
                
        {
            PathKey = ctx.PathKey
            Extends =
                container.BaseDocumentationContainer.Extends
                |> Option.map Name.cacheOrCreatePascal
                |> Option.toValueOption
            WebsiteUrl =
                container.BaseDocumentationContainer.WebsiteUrl
                |> function
                    | text when String.IsNullOrWhiteSpace text ->
                        ValueNone
                    | text -> ValueSome text
            Description =
                container.BaseDocumentationContainer.Description
                |> function
                    | text when String.IsNullOrWhiteSpace text ->
                        ValueNone
                    | text -> ValueSome text
            Process = container.Process
            Methods =
                container.Methods
                |> Array.map (Method.fromDocBlock { ctx with Type = ContextType.MethodParameter })
                |> Array.toList
            Events =
                container.Events
                |> Array.map (Event.fromDocBlock { ctx with Type = ContextType.ObjectProperty })
                |> Array.toList
            Properties =
                container.Properties
                |> Array.map (extractFromPropertyDocumentationBlock { ctx with Type = ContextType.ObjectProperty })
                |> Array.toList
            ExportedClasses =
                container.ExportedClasses
                |> Array.map (Class.fromDocContainer (Some ctx))
                |> Array.toList
        }
        |> ExtensionHelper.add
        
type ModifiedResult =
    | Module of Module
    | Class of Class
    | Element of Element
    | Structure of Structure
let readResult = function
    | ParsedDocumentation.Module value ->
        Module.fromDocContainer None value
        |> ModifiedResult.Module
    | ParsedDocumentation.Class value ->
        Class.fromDocContainer None value
        |> ModifiedResult.Class
    | ParsedDocumentation.Element value ->
        // TODO - place elements in a 'Tags' or 'Elements' module?
        Element.fromDocContainer None value
        |> ModifiedResult.Element
    | ParsedDocumentation.Structure value ->
        // we'll place root structs into a "Structure" or "Types" module
        let module' =
            { PathKey = Path.PathKey.CreateModule(Source "Types")
              Type = ContextType.NA }
            |> Some
        Structure.readFromDocContainer module' value
        |> ModifiedResult.Structure

let readResults =
    Array.map readResult >> (fun results ->
        let resolved = ExtensionHelper.resolveAndRetrieve()
        results
        |> Array.map (fun item ->
            let findItem name  = function
            | Module ``module`` ->
                ``module``.PathKey.Name = name
            | Class ``class`` ->
                ``class``.PathKey.Name = name
            | Element element ->
                element.PathKey.Name = name
            | Structure structure ->
                structure.Name = name
            match item with
            | Module ``module`` ->
                resolved
                |> Array.tryFind (findItem ``module``.PathKey.Name)
            | Class ``class`` ->
                resolved
                |> Array.tryFind (findItem ``class``.PathKey.Name)
            | Element element ->
                resolved
                |> Array.tryFind (findItem element.PathKey.Name)
            | Structure structure -> 
                resolved
                |> Array.tryFind (findItem structure.Name)
            |> Option.defaultValue item
            )
        ) >> Array.toList
