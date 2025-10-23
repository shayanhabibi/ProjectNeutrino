module rec ElectronApi.Json.Parser.FSharpApi

open System
open System.IO

open ElectronApi.Json.Parser.Decoder
open ElectronApi.Json.Parser.Utils
open Fantomas.Core
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open Thoth.Json.Net

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
module Compatibility =
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

type EventInfo = {
    PathKey: Path.PathKey
    Properties: Property list
}

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
    //
    | Function of FuncOrMethod
    | StringEnum of StringEnum
    | Structure of StructOrObject
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


module Type =
    let readStringEnumCase (ctx: FactoryContext): PossibleStringValue -> StringEnumCase = fun { Value = value; Description = desc } ->
        {
            Value =
                Name.createPascal value
            Description =
                if String.IsNullOrWhiteSpace desc then
                    ValueSome desc
                else ValueNone
        }
    let readStringEnum (ctx: FactoryContext) (innerTypes: TypeInformation array voption): TypeInformationKind.String -> StringEnum =
        fun { PossibleValues = possibleValues } ->
            if innerTypes.IsSome then
                failwith $"Unexpected inner types for a String {innerTypes}"
            // If we're not reading the string enum as a parameter or something else which gives us a name, then
            // we'll have to generate a name later.
            {
                PathKey =
                    // if its a parameter then we'll just make it the same name
                    if ctx.Type.IsMethodParameter then
                        ctx.PathKey.Name.ValueOrModified
                        |> Name.createPascal
                        |> ctx.PathKey.CreateType
                    else
                        let normalize: string -> string = _.Trim(''') >> toPascalCase
                        ctx.PathKey.Name.ValueOrModified
                        |> normalize
                        |> Source
                        |> ctx.PathKey.CreateType
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
    // TODO - collect errors and report in bulk
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
        let typ = fromTypeInformation ctx block.TypeInformation
        match name with
        | ValueSome name ->
            Parameter.Named(
                ctx.PathKey.CreateParameter name,
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
            // TODO - should we be caching the name of these lambdas now that we are lifting them to Delegates if they have more than 1 parameter??
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
            props
            |> Array.map (extractFromPropertyDocumentationBlock ctx)
            |> Array.toList
            |> fun props ->
                { PathKey =
                    ctx.PathKey.Name.ValueOrModified
                    |> Name.createPascal
                    |> ctx.PathKey.CreateEvent
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
    Description: string
    UrlFragment: string option
    Type: Type
    Compatibility: Compatibility
}
module Property =
    let inline isRequired (t: ^T when ^T : (member Required: bool)) = t.Required

let extractFromPropertyDocumentationBlock (ctx: FactoryContext) (propBlock: PropertyDocumentationBlock) =
    let name = Name.createCamel propBlock.DocumentationBlock.Name
    let pathKey = ctx.PathKey.CreateProperty name
    let ctx = { PathKey = pathKey; Type = ContextType.ObjectProperty }
    {
        Required = Property.isRequired propBlock
        PathKey = pathKey
        Description =
            propBlock.DocumentationBlock.Description
        UrlFragment = propBlock.DocumentationBlock.UrlFragment
        Type =
            Type.fromTypeInformation ctx propBlock.TypeInformation
        Compatibility =
            propBlock.DocumentationBlock.AdditionalTags
            |> Compatibility.fromTags
    }

type Event = {
    PathKey: Path.PathKey
    Description: string voption
    Compatibility: Compatibility
    UrlFragment: string voption
    Parameters: Parameter list
}

module Event =
    let fromDocBlock (ctx: FactoryContext) (block: EventDocumentationBlock): Event =
        let name = block.DocumentationBlock.Name |> Name.cacheOrCreatePascal
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
        }

type Structure = {
    Name: Name
    Extends: Name option
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

type Method = {
    PathKey: Path.PathKey
    Compatibility: Compatibility
    Description: string voption
    UrlFragment: string voption
    RawGenerics: string option
    Parameters: Parameter list
    ReturnType: Type
}
module Method =
    let fromDocBlock (ctx: FactoryContext) (block: MethodDocumentationBlock) =
        let name = block.DocumentationBlock.Name |> Name.createCamel
        let pathKey = name |> ctx.PathKey.CreateMethod
        let ctx = { PathKey = pathKey; Type = ContextType.MethodParameter }
        {
            PathKey = pathKey
            Compatibility = block.DocumentationBlock.AdditionalTags |> Compatibility.fromTags
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
                        let ctx = { ctx with PathKey = ctx.PathKey.CreateMethod(Source "$CONS$") }
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

let readResults = Array.map readResult >> Array.toList
