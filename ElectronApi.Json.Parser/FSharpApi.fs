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
        
/// <summary>
/// The Path module contains recursive types which are modeled after the 'path' of modules/namespaces that one
/// would take to reach their location.<br/>
/// </summary>
module Path =
    /// <summary>
    /// Interface for path types that all provide the name accessibly.
    /// </summary>
    type IName =
        abstract Name: Name
    // each module has a name, and is occupied by types and bindings. We want them to track their path/origin
    /// <summary>
    /// A tracker for the named path to this type, along with its name.
    /// </summary>
    type Type = Type of path: Path * name: Name with
        member inline this.Destructured = let (Type(p,n)) = this in (p,n)
        member this.Path = this.Destructured |> fst
        member this.Name = this.Destructured |> snd
        interface IName with
            member this.Name = this.Name
    and InlineOptions = InlineOptions of parent: Choice<Parameter, Property, Method, InlineLambda, Event> * name: Name with
        member inline this.Destructured = let (InlineOptions(p,n)) = this in (p,n)
        member this.Parent = this.Destructured |> fst
        member this.Path =
            this.Parent
            |> function
                | Choice1Of5 p -> p.Path
                | Choice2Of5 p -> p.Path
                | Choice3Of5 m -> m.Path
                | Choice4Of5 l -> l.Path
                | Choice5Of5 e -> e.Path
        member this.Name = this.Destructured |> snd
        interface IName with
            member this.Name = this.Name
    and InlineLambda = InlineLambda of parent: Choice<Parameter, Property, Method, InlineLambda, Event> * name: Name with
        member inline this.Destructured = let (InlineLambda(p,n)) = this in (p,n)
        member this.Parent = this.Destructured |> fst
        member this.Path =
            this.Parent
            |> function
                | Choice1Of5 p -> p.Path
                | Choice2Of5 p -> p.Path
                | Choice3Of5 m -> m.Path
                | Choice4Of5 l -> l.Path
                | Choice5Of5 e -> e.Path
        member this.Name = this.Destructured |> snd
        interface IName with
            member this.Name = this.Name
    /// <summary>
    /// A tracker for the named path to this binding, along with its name.
    /// </summary>
    and Binding = Binding of path: Path * name: Name with
        member inline this.Destructured = let (Binding(p,n)) = this in (p,n)
        member this.Path = this.Destructured |> fst
        member this.Name = this.Destructured |> snd
        interface IName with
            member this.Name = this.Name
    and Method = Method of from: Choice<Type, Path> * name: Name with
        member inline this.Destructured = let (Method(p,n)) = this in (p,n)
        member this.Parent = this.Destructured |> fst
        member this.Path =
            match this.Parent with
            | Choice1Of2 ``type`` -> ``type``.Path
            | Choice2Of2 path -> path
        member this.Name = this.Destructured |> snd
        interface IName with
            member this.Name = this.Name
    and Property = Property of from: Choice<Type, InlineOptions, Event, Module> * name: Name with
        member inline this.Destructured = let (Property(p,n)) = this in (p,n)
        member this.Parent = this.Destructured |> fst
        member this.Path =
            this.Parent |> function
                | Choice1Of4 t -> t.Path
                | Choice2Of4 o -> o.Path
                | Choice3Of4 e -> e.Path
                | Choice4Of4 m -> m.Path
        member this.Name = this.Destructured |> snd
        interface IName with
            member this.Name = this.Name
    // An event is either contained in a module or of a class (type)
    and Event = Event of parent: Choice<Path, Type> * name: Name with
        member inline this.Destructured = let (Event (p,n)) = this in (p,n)
        member this.Name = this.Destructured |> snd
        member this.Path =
            this.Destructured
            |> fst |> function
                | Choice1Of2 p -> p
                | Choice2Of2 c -> c.Path
        interface IName with
            member this.Name = this.Name
    /// <summary>
    /// A tracker for the named path to this module, along with its name.
    /// </summary>
    and Module = Module of path: Path * name: Name with
        member inline this.Destructured = let (Module(p,n)) = this in (p,n)
        member this.Path = this.Destructured |> fst
        member this.Name = this.Destructured |> snd
        interface IName with
            member this.Name = this.Name
    and Parameter = Parameter of from: Choice<Binding, Method, Type, Event, InlineLambda> * name: Name with
        member inline this.Destructured = let (Parameter(p,n)) = this in (p,n)
        member this.Path =
            this.Destructured |> fst
            |> function
                | Choice1Of5 b -> b.Path
                | Choice2Of5 m -> m.Path
                | Choice3Of5 t -> t.Path
                | Choice4Of5 e -> e.Path
                | Choice5Of5 l -> l.Path
        member this.Parent =
            this.Destructured |> fst
        member this.Name = this.Destructured |> snd
        interface IName with
            member this.Name = this.Name
    and [<RequireQualifiedAccess>] Path =
        | Root
        | Module of Module
        member this.Name =
            match this with
            | Root -> Source "Root"
            | Module m -> m.Name
        override this.ToString() =
            match this with
            | Root -> "Root"
            | Module m -> m.Name.ToString()
    and [<RequireQualifiedAccess>] PathKey =
        | Binding of Binding
        | Type of Type
        | Property of Property
        | Method of Method
        | Event of Event
        | Parameter of Parameter
        | InlineOptions of InlineOptions
        | InlineLambda of InlineLambda
        | Module of Path
        member this.Path =
            match this with
            | Binding b -> b.Path
            | Type t -> t.Path
            | Method m -> m.Path
            | Property p -> p.Path
            | Event e -> e.Path
            | Parameter p -> p.Path
            | InlineOptions o -> o.Path
            | InlineLambda l -> l.Path
            | Module p -> p
        member this.Name =
            match this with
            | Binding b -> b.Name
            | Type t -> t.Name
            | Method m -> m.Name
            | Property p -> p.Name
            | Event e -> e.Name
            | Parameter p -> p.Name
            | InlineOptions o -> o.Name
            | InlineLambda l -> l.Name
            | Module p ->
                match p with
                | Path.Root -> Source "Root"
                | Path.Module ``module`` -> ``module``.Name
        member this.ParentName =
            match this with
            | Binding binding ->
                binding.Path.Name
            | Type typ ->
                typ.Path.Name
            | Property property ->
                property.Parent
                |> function
                    | Choice1Of4 p -> p.Name
                    | Choice2Of4 p -> p.Name
                    | Choice3Of4 p -> p.Name
                    | Choice4Of4 p -> p.Name
                    
            | Method method ->
                method.Parent
                |> function
                    | Choice1Of2 p -> p.Name
                    | Choice2Of2 p -> p.Name
            | Event event ->
                event.Path.Name
            | Parameter parameter ->
                parameter.Parent
                |> function
                    | Choice1Of5 p -> p.Name
                    | Choice2Of5 p -> p.Name
                    | Choice3Of5 p -> p.Name
                    | Choice4Of5 p -> p.Name
                    | Choice5Of5 p -> p.Name
            | InlineOptions inlineOptions ->
                inlineOptions.Parent
                |> function
                    | Choice1Of5 p -> p.Name
                    | Choice2Of5 p -> p.Name
                    | Choice3Of5 p -> p.Name
                    | Choice4Of5 p -> p.Name
                    | Choice5Of5 p -> p.Name
            | InlineLambda inlineLambda ->
                inlineLambda.Parent
                |> function
                    | Choice1Of5 p -> p.Name
                    | Choice2Of5 p -> p.Name
                    | Choice3Of5 p -> p.Name
                    | Choice4Of5 p -> p.Name
                    | Choice5Of5 p -> p.Name
            | Module path ->
                match path with
                | Path.Root -> Source "Root"
                | Path.Module m ->
                    m.Path.Name
        member this.CreateType(name: Name) =
            Path.Type(this.Path, name)
            |> PathKey.Type
        member this.CreateBinding(name: Name) =
            Path.Binding(this.Path, name)
            |> PathKey.Binding
        member this.CreateMethod(name: Name) =
            match this with
            | Binding binding -> failwith $"Tried to create a method name for the path of %A{binding}"
            | Type ``type`` ->
                Path.Method(Choice1Of2 ``type``, name)
                |> PathKey.Method
            | Property property -> failwith $"Tried to create a method name for the path of %A{property}"
            | Method method -> failwith $"Tried to create a method pathkey for the path of %A{method}"
            | Event event -> failwith $"Tried to create a method pathkey for the path of %A{event}"
            | Parameter p -> failwith $"Tried to create a method pathkey for the path of {p}"
            | InlineOptions o -> failwith $"Tried to create a method pathkey for the inline options {o}"
            | InlineLambda l -> failwith $"Tried to create a method pathkey for the inline lambda {l}"
            | Module m ->
                Path.Method(Choice2Of2 m, name)
                |> PathKey.Method
        member this.CreateLambda() =
            let name = Source "Delegate"
            match this with
            | Property property ->
                Path.InlineLambda(Choice2Of5 property, name)
                |> PathKey.InlineLambda
            | Parameter parameter ->
                Path.InlineLambda(Choice1Of5 parameter, name)
                |> PathKey.InlineLambda
            | Method m ->
                // this is a parameter of the method
                Path.InlineLambda(Choice3Of5 m, name)
                |> PathKey.InlineLambda
            | InlineLambda l ->
                // This is a parameter of the lambda
                Path.InlineLambda(Choice4Of5 l, name)
                |> PathKey.InlineLambda
            | Event e ->
                // This is a parameter/prop in the event
                Path.InlineLambda(Choice5Of5 e, name)
                |> PathKey.InlineLambda
            | e -> failwith $"Tried to create a lambda pathkey for {e}"
        member this.CreateProperty(name: Name) =
            match this with
            | Binding binding -> failwith $"Tried to create a property for the binding of %A{binding}"
            | Type ``type`` ->
                Path.Property(Choice1Of4 ``type``, name)
                |> PathKey.Property
            | Property property -> failwith $"Tried to create a property for the property %A{property}"
            | Method method -> failwith $"Tried to create a property for the method of %A{method}"
            | Event event ->
                Path.Property(Choice3Of4 event, name)
                |> PathKey.Property
            | Parameter p -> failwith $"Tried to create a property for the path {p}"
            | InlineLambda l -> failwith $"Tried to create a property for the lambda {l}"
            | InlineOptions o ->
                Path.Property(Choice2Of4 o, name)
                |> PathKey.Property
            | Module(Path.Module m)->
                Path.Property(Choice4Of4 m, name)
                |> PathKey.Property
            | p -> failwith $"Tried to create a property for the path {p}"
        member this.CreateEvent(name: Name) =
            match this with
            | Type ``type`` ->
                Path.Event(Choice2Of2 ``type``, name)
                |> PathKey.Event
            | _ ->
                Path.Event(Choice1Of2 this.Path, name)
                |> PathKey.Event
        member this.CreateParameter(name: Name) =
            match this with
            | Binding binding ->
                Path.Parameter(Choice1Of5 binding, name)
                |> PathKey.Parameter
            | Type ``type`` ->
                Path.Parameter(Choice3Of5 ``type``,name)
                |> PathKey.Parameter
            | Property property -> failwith $"Tried to create a parameter pathkey for the path {property}"
            | Method method ->
                Path.Parameter(Choice2Of5 method, name)
                |> PathKey.Parameter
            | Event event ->
                Path.Parameter(Choice4Of5 event, name)
                |> PathKey.Parameter
            | InlineLambda l ->
                Path.Parameter(Choice5Of5 l, name)
                |> PathKey.Parameter
            | Parameter parameter -> failwith $"Tried to create a parameter pathkey for the path {parameter}"
            | InlineOptions o -> failwith $"Tried to create a parameter pathkey for the inline options {o}"
            | Module p -> failwith $"Tried to create a parameter pathkey for the module {p}"
        member this.CreateInlineOptions(?name: Name) =
            let name = defaultArg name (Name.Source "Options")
            match this with
            | Method m ->
                // this is technically a parameter
                Path.InlineOptions(Choice3Of5 m, name)
                |> PathKey.InlineOptions
            | InlineLambda l ->
                Path.InlineOptions(Choice4Of5 l, name)
                |> PathKey.InlineOptions
            | Property property ->
                Path.InlineOptions(Choice2Of5 property, name)
                |> PathKey.InlineOptions
            | Parameter parameter ->
                Path.InlineOptions(Choice1Of5 parameter, name)
                |> PathKey.InlineOptions
            | Event e ->
                // This is a param/prop of the event
                Path.InlineOptions(Choice5Of5 e, name)
                |> PathKey.InlineOptions
            | e -> failwith $"Tried to create an inlineoptions pathkey for {e}"

    let tracePathOfEntry (entry: PathKey) =
        let rec tracePath = function
            | Path.Root -> []
            | Path.Module m ->
                m.Name :: tracePath m.Path
        and tracePathKey: PathKey -> Name list = function
            | PathKey.Binding binding ->
                binding.Name :: tracePath binding.Path
            | PathKey.Type typ ->
                typ.Name :: tracePath typ.Path
            | PathKey.Property property ->
                match property.Parent with
                | Choice1Of4 ``type`` ->
                    property.Name :: tracePathKey (PathKey.Type ``type``)
                | Choice2Of4 inlineOptions ->
                    property.Name :: tracePathKey (PathKey.InlineOptions inlineOptions)
                | Choice3Of4 event ->
                    property.Name :: tracePathKey (PathKey.Event event)
                | Choice4Of4 m ->
                    property.Name :: tracePath (Path.Path.Module m)
            | PathKey.Method method ->
                match method.Parent with
                | Choice1Of2 ``type`` ->
                    method.Name :: tracePathKey (PathKey.Type ``type``)
                | Choice2Of2 path ->
                    method.Name :: tracePath path
            | PathKey.Event event ->
                event.Name :: tracePath event.Path
            | PathKey.Parameter parameter ->
                match parameter.Parent with
                | Choice1Of5 binding ->
                    parameter.Name :: tracePathKey (PathKey.Binding binding)
                | Choice2Of5 method ->
                    parameter.Name :: tracePathKey (PathKey.Method method)
                | Choice3Of5 ``type`` ->
                    parameter.Name :: tracePathKey (PathKey.Type ``type``)
                | Choice4Of5 event ->
                    parameter.Name :: tracePathKey (PathKey.Event event)
                | Choice5Of5 inlineLambda ->
                    parameter.Name :: tracePathKey (PathKey.InlineLambda inlineLambda)
            | PathKey.InlineOptions inlineOptions ->
                match inlineOptions.Parent with
                | Choice1Of5 parameter ->
                    inlineOptions.Name :: tracePathKey (PathKey.Parameter parameter)
                | Choice2Of5 property -> inlineOptions.Name :: tracePathKey (PathKey.Property property)
                | Choice3Of5 method -> inlineOptions.Name :: tracePathKey (PathKey.Method method)
                | Choice4Of5 inlineLambda -> inlineOptions.Name :: tracePathKey (PathKey.InlineLambda inlineLambda)
                | Choice5Of5 event -> inlineOptions.Name :: tracePathKey (PathKey.Event event)
            | PathKey.InlineLambda inlineLambda ->
                match inlineLambda.Parent with
                | Choice1Of5 parameter ->
                    inlineLambda.Name :: tracePathKey (PathKey.Parameter parameter)
                | Choice2Of5 property -> inlineLambda.Name :: tracePathKey (PathKey.Property property)
                | Choice3Of5 method -> inlineLambda.Name :: tracePathKey (PathKey.Method method)
                | Choice4Of5 inlineLambda -> inlineLambda.Name :: tracePathKey (PathKey.InlineLambda inlineLambda)
                | Choice5Of5 event -> inlineLambda.Name :: tracePathKey (PathKey.Event event)
            | PathKey.Module path -> tracePath path
        tracePathKey entry
        |> List.rev


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
            let ctx = { ctx with PathKey = ctx.PathKey.CreateLambda() }
            Type.Function {
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
                Path.PathKey.Type(Path.Type(Path.Path.Root, name))
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
                let root = Path.Path.Root
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
                let pathKey = Path.Type(Path.Path.Root, name) |> Path.PathKey.Type
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
                { ctx with PathKey = ctx.PathKey.CreateType(name) }
            | None ->
                { PathKey = Path.PathKey.Module <| (Path.Path.Module <| Path.Module(Path.Path.Root, name)); Type = ContextType.NA }
                
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
            { PathKey =
                Path.Module(Path.Path.Root, Source "Types")
                |> Path.Path.Module
                |> Path.PathKey.Module
              Type = ContextType.NA }
            |> Some
        Structure.readFromDocContainer module' value
        |> ModifiedResult.Structure

let readResults = Array.map readResult >> Array.toList
