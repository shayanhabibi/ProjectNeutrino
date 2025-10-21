module rec ElectronApi.Json.Parser.FSharpApi

open System
open System.IO

open ElectronApi.Json.Parser.Decoder
open ElectronApi.Json.Parser.Utils
open Fantomas.Core
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open Thoth.Json.Net

/// <summary>
/// Known results in <c>TypeInformationKind.InfoString</c> that will not be
/// represented elsewhere in the code base, and are considered 'native'/'global'.
/// </summary>
module Constants =
    [<Literal>]
    let undefined = "undefined"
    [<Literal>]
    let boolean = "boolean"
    [<Literal>]
    let integer = "Integer"
    [<Literal>]
    let void' = "void"
    [<Literal>]
    let null' = "null"
    [<Literal>]
    let promise = "Promise"
    [<Literal>]
    let record = "Record"
    [<Literal>]
    let any = "any"
    [<Literal>]
    let unknown = "unknown"
    [<Literal>]
    let event = "Event"
    [<Literal>]
    let number = "number"
    [<Literal>]
    let date = "Date"
    [<Literal>]
    let float = "Float"
    [<Literal>]
    let partial = "Partial"
    [<Literal>]
    let double = "Double"

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
// We want to track names that we modify in case we require to reference the original name when
// emitting
type Name =
    | Source of string
    | Modified of source: string * modified: string
    member this.ValueOrModified =
        match this with
        | Source value | Modified(_,value) -> value
    member this.ValueOrSource =
        match this with
        | Source value | Modified(value, _) -> value
module Name =
    // We hold a cache of strings and their generated name
    let private cache = System.Collections.Generic.Dictionary<string, Name>()
    // All names must be checked for reserved identifiers
    // If a name has changed, then we wrap it in the 'Modified' case.
    let inline private map f input =
        f input
        |> appendApostropheToReservedKeywords
        |> function
            | output when String.Equals(input,output,StringComparison.Ordinal) -> Source output
            | output -> Modified(input,output)
    // Dumps name into cache
    let cacheName (name: Name) =
        let key = name.ValueOrSource
        cache.ContainsKey key
        |> function
            | false -> cache.Add(key, name)
            | true -> ()
    // Retrieves name from cache
    let retrieveName (key: string) =
        cache.TryGetValue(key)
        |> function
            | true, name -> ValueSome name
            | false,  _ -> ValueNone
    // Changes casing to 'camelCase'
    let createCamel (input: string) =
        map toCamelCase input
    // Changes casing to 'PascalCase'
    let createPascal (input: string) =
        map toPascalCase input
    // Checks cache for name; if not present, then creates
    // for the cache and returns that value.
    let cacheOrCreatePascal nameInput =
        retrieveName nameInput
        |> ValueOption.defaultWith(fun () ->
            createPascal nameInput
            |> fun outputName ->
                outputName
                |> cacheName
                outputName
            )
    // Checks cache for name; if not present, then creates
    // for the cache and returns that value
    let cacheOrCreateCamel nameInput =
        retrieveName nameInput
        |> ValueOption.defaultWith(fun () ->
            createCamel nameInput
            |> fun output ->
                output |> cacheName
                output
            )
type StringEnumCase = { Value: Name; Description: string voption }
type StringEnum = { Name: Name voption; Cases: StringEnumCase list }
type StructOrObject = {
    Name: Name option
    Properties: Property list
}
type ParameterInfo = {
    Description: string voption
    Required: bool
    Type: Type
}
type Parameter =
    | Positional of ParameterInfo
    | Named of name: Name * info: ParameterInfo

type FuncOrMethod = {
    Name: Name voption
    Parameters: Parameter list
    Returns: Type
}

type EventInfo = {
    Name: Name voption
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
    let readStringEnumCase: PossibleStringValue -> StringEnumCase = fun { Value = value; Description = desc } ->
        {
            Value =
                Name.createPascal value
            Description =
                if String.IsNullOrWhiteSpace desc then
                    ValueSome desc
                else ValueNone
        }
    let readStringEnum (innerTypes: TypeInformation array voption): TypeInformationKind.String -> StringEnum =
        fun { PossibleValues = possibleValues } ->
            if innerTypes.IsSome then
                failwith $"Unexpected inner types for a String {innerTypes}"
            // If we're not reading the string enum as a parameter or something else which gives us a name, then
            // we'll have to generate a name later.
            {
                Name = ValueNone
                Cases =
                    possibleValues
                    |> Option.map (
                        Array.filter (_.Value.Length >> (<) 0)
                        >> Array.map readStringEnumCase
                        >> Array.toList
                        )
                    |> Option.defaultValue []
            }
    // If we hit an unknown, we want to throw so we can see how that type should be managed.
    // TODO - collect errors and report in bulk
    let readInfoString (innerTypes: TypeInformation array voption): TypeInformationKind.InfoString -> Type = function
        | info when innerTypes.IsNone ->
            match info with
            | { Type = Constants.float | Constants.number | "Number" | "Float" } -> Type.Float
            | { Type = Constants.double } -> Type.Double
            | { Type = Constants.integer } -> Type.Integer
            | { Type = Constants.unknown | Constants.any } -> Type.Any
            | { Type = Constants.undefined } -> Type.Undefined
            | { Type = Constants.event } ->
                {
                    Name = Name.createPascal "Event" |> ValueSome
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
            |> fromTypeInformation
            |> Type.Promise
        | { Type = Constants.record } when innerTypes.IsSome && innerTypes.Value.Length = 2 ->
            let left,right = innerTypes.Value.[0], innerTypes.Value.[1]
            Type.Record(
                key = fromTypeInformation left,
                value = fromTypeInformation right
                )
        | { Type = "Array" } when innerTypes.IsSome && innerTypes.Value.Length = 1 ->
            Type.Array(fromTypeInformation innerTypes.Value.[0])
        | { Type = Constants.partial } when innerTypes.IsSome && innerTypes.Value.Length = 1 -> // TODO
            // as it currently stands, there is only one case of this
            innerTypes.Value.[0]
            |> fromTypeInformation
        | { Type = Constants.promise | Constants.record | Constants.partial } as kind ->
            failwithf $"Failed to read %A{kind} with innerTypes %A{innerTypes}"
        | { Type = typ } ->
            // Throw for unencountered types so we can inspect and modify our approach
            failwithf $"Unexpected parts for %s{typ} %A{innerTypes}"
    
    let readObjectKind (innerTypes: TypeInformation array voption): TypeInformationKind.Object -> Type = function
        | o when innerTypes.IsSome ->
            failwith $"unhandled object {o} with inner types {innerTypes}"
        | { Properties = props } ->
            props
            |> Array.map extractFromPropertyDocumentationBlock
            |> Array.toList
            |> fun props ->
                { StructOrObject.Name = None; Properties = props }
                |> Type.Object
    let readInfoArray (innerTypes: TypeInformation array voption): TypeInformationKind.InfoArray -> Type = function
        | a when innerTypes.IsSome ->
            failwith $"Unhandled array {a} with inner types {innerTypes}"
        | { Type = typs } ->
            // The simple info array is just a union of different types
            typs
            |> Array.map fromTypeInformation
            |> Array.toList
            |> Type.OneOf
    let inline private readParameter
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
        let typ = fromTypeInformation block.TypeInformation
        match name with
        | ValueSome name ->
            Parameter.Named(
                name,
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
    let readMethodParameter (block: MethodParameterDocumentation): Parameter = readParameter block
    let readEventParameter (block: EventParameterDocumentation): Parameter = readParameter block
    let readFunctionKind (innerTypes: TypeInformation array voption): TypeInformationKind.Function -> Type = function
        | f when innerTypes.IsSome ->
            failwith $"Unhandled function type {f} with innerTypes {innerTypes}"
        | { Parameters = parameters; Returns = returns } ->
            Type.Function {
                // Lambda probably, we're only really interested in the signature then
                Name = ValueNone
                Parameters =
                    parameters
                    |> Array.map readMethodParameter
                    |> Array.toList
                Returns =
                    returns
                    |> Option.toValueOption
                    |> ValueOption.map fromTypeInformation
                    |> ValueOption.defaultValue Type.Unit
            }
    let readEventKind (innerTypes: TypeInformation array voption): TypeInformationKind.Event -> Type = function
        | e when innerTypes.IsSome ->
            failwith $"unhandled event type {e} with inner types {innerTypes}"
        | { EventProperties = props } ->
            props
            |> Array.map extractFromPropertyDocumentationBlock
            |> Array.toList
            |> fun props ->
                { Name = ValueNone; Properties = props }
                |> Type.Event
    let readEventRefKind (innerTypes: TypeInformation array voption): TypeInformationKind.EventRef -> Type = function
        | e when innerTypes.IsSome ->
            failwith $"Unhandled event type {e} with innerTypes {innerTypes}"
        | { EventPropertiesReference = props } ->
            props |> fromTypeInformation
            |> Type.EventRef
    let inline isCollection (kind: ^T when ^T : (member Collection: bool)) = kind.Collection
    let inline wrap kind input =
        if isCollection kind then Type.Collection input else input
    let fromTypeInformation (typeInfo: TypeInformation): Type = 
        let kind,innerTypes =
            match typeInfo with
            | WithInnerTypes(kind, typs) -> kind, ValueSome typs
            | TypeInformation.TypeInformation typ -> typ, ValueNone
        match kind with
        | InfoArray ({ Type = typs } as kind) ->
            typs
            |> Array.map fromTypeInformation
            |> Array.toList
            |> Type.OneOf
            |> wrap kind
        | InfoString infoString ->
            infoString
            |> readInfoString innerTypes
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
                |> readStringEnum innerTypes
                |> Type.StringEnum
                |> wrap typeString
        | TypeInformationKind.Object o ->
            readObjectKind innerTypes o
            |> wrap o
        | TypeInformationKind.Event event ->
            readEventKind innerTypes event
            |> wrap event
        | TypeInformationKind.EventRef eventRef ->
            readEventRefKind innerTypes eventRef
            |> wrap eventRef
        | TypeInformationKind.Function ``function`` ->
            ``function``
            |> readFunctionKind innerTypes
            |> wrap ``function``
type Property = {
    Required: bool
    Name: Name
    Description: string
    UrlFragment: string option
    Type: Type
    Compatibility: Compatibility
}
module Property =
    let inline isRequired (t: ^T when ^T : (member Required: bool)) = t.Required

let extractFromPropertyDocumentationBlock (propBlock: PropertyDocumentationBlock) = 
    {
        Required = Property.isRequired propBlock
        Name =
            Name.createCamel propBlock.DocumentationBlock.Name
        Description =
            propBlock.DocumentationBlock.Description
        UrlFragment = propBlock.DocumentationBlock.UrlFragment
        Type =
            Type.fromTypeInformation propBlock.TypeInformation
        Compatibility =
            propBlock.DocumentationBlock.AdditionalTags
            |> Compatibility.fromTags
    }

type Event = {
    Name: Name
    Description: string voption
    Compatibility: Compatibility
    UrlFragment: string voption
    Parameters: Parameter list
}

module Event =
    let fromDocBlock (block: EventDocumentationBlock): Event =
        {
            Name = block.DocumentationBlock.Name |> Name.cacheOrCreatePascal
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
                |> Array.map Type.readEventParameter
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
    let readFromDocContainer (container: StructureDocumentationContainer) =
        {
            Name =
                container.BaseDocumentationContainer.Name
                |> Name.cacheOrCreatePascal
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
                |> Array.map extractFromPropertyDocumentationBlock
                |> Array.toList
        }

type Method = {
    Name: Name
    Compatibility: Compatibility
    Description: string voption
    UrlFragment: string voption
    RawGenerics: string option
    Parameters: Parameter list
    ReturnType: Type
}
module Method =
    let fromDocBlock (block: MethodDocumentationBlock) =
        {
            Name = block.DocumentationBlock.Name |> Name.createCamel
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
                |> Array.map Type.readMethodParameter
                |> Array.toList
            ReturnType =
                block.Returns
                |> Option.map Type.fromTypeInformation
                |> Option.defaultValue Type.Unit
        }

type Class = {
    Process: ProcessBlock
    Constructor: Parameter list option
    InstanceName: Name option
    StaticMethods: Method list
    StaticProperties: Property list
    Methods: Method list
    Events: Event list
    Properties: Property list
    Name: Name
    Extends: Name option
    Description: string voption
    WebsiteUrl: string
}

module Class =
    let fromDocContainer (container: ClassDocumentationContainer): Class =
        {
            Process = container.Process
            Constructor =
                container.ConstructorMethod
                |> Option.map (function
                    | { Parameters = parameters } ->
                        parameters
                        |> Array.map Type.readMethodParameter
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
                |> Array.map Method.fromDocBlock
                |> Array.toList
            StaticProperties =
                container.StaticProperties
                |> Array.map extractFromPropertyDocumentationBlock
                |> Array.toList
            Methods =
                container.InstanceMethods
                |> Array.map Method.fromDocBlock
                |> Array.toList
            Events =
                container.InstanceEvents
                |> Array.map Event.fromDocBlock
                |> Array.toList
            Properties =
                container.InstanceProperties
                |> Array.map extractFromPropertyDocumentationBlock
                |> Array.toList
            Name =
                container.BaseDocumentationContainer.Name
                |> Name.cacheOrCreatePascal
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
    Name: Name
    Extends: Name option
    Description: string voption
    WebsiteUrl: string
    Process: ProcessBlock
    Methods: Method list
    Events: Event list
    Properties: Property list
}

module Element =
    let fromDocContainer (container: ElementDocumentationContainer): Element =
        {
            Name = container.BaseDocumentationContainer.Name |> Name.cacheOrCreatePascal
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
                |> Array.map Method.fromDocBlock
                |> Array.toList
            Events =
                container.Events
                |> Array.map Event.fromDocBlock
                |> Array.toList
            Properties =
                container.Properties
                |> Array.map extractFromPropertyDocumentationBlock
                |> Array.toList
        }

type Module = {
    Name: Name
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
    let fromDocContainer (container: ModuleDocumentationContainer): Module =
        {
            Name =
                container.BaseDocumentationContainer.Name
                |> Name.cacheOrCreatePascal
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
                |> Array.map Method.fromDocBlock
                |> Array.toList
            Events =
                container.Events
                |> Array.map Event.fromDocBlock
                |> Array.toList
            Properties =
                container.Properties
                |> Array.map extractFromPropertyDocumentationBlock
                |> Array.toList
            ExportedClasses =
                container.ExportedClasses
                |> Array.map Class.fromDocContainer
                |> Array.toList
        }

type ModifiedResult =
    | Module of Module
    | Class of Class
    | Element of Element
    | Structure of Structure
let readResult = function
    | ParsedDocumentation.Module value ->
        Module.fromDocContainer value
        |> ModifiedResult.Module
    | ParsedDocumentation.Class value ->
        Class.fromDocContainer value
        |> ModifiedResult.Class
    | ParsedDocumentation.Element value ->
        Element.fromDocContainer value
        |> ModifiedResult.Element
    | ParsedDocumentation.Structure value ->
        Structure.readFromDocContainer value
        |> ModifiedResult.Structure

let readResults = Array.map readResult >> Array.toList
