[<AutoOpen>]
module rec ElectronApi.Json.Parser.Types

open System

/// <summary>
/// A Discriminated Union which stores the source name, and the modified (if modified
/// at all)
/// </summary>
type Name =
    /// <summary>
    /// Unchanged name from source
    /// </summary>
    | Source of string
    /// <summary>
    /// A name that was modified in pre/post-processing
    /// </summary>
    /// <param name="source">The original source name</param>
    /// <param name="modified">The modified name</param>
    | Modified of source: string * modified: string
    /// <summary>
    /// The modified name (or source name if unmodified).
    /// </summary>
    member this.ValueOrModified =
        match this with
        | Source value | Modified(_,value) -> value
    /// <summary>
    /// The source name.
    /// </summary>
    member this.ValueOrSource =
        match this with
        | Source value | Modified(value, _) -> value
    /// <summary>
    /// Statically resolved type parameter that retrieves the name from a type
    /// which has a member providing it.
    /// </summary>
    /// <param name="objectWithName">The instance of the type to retrieve the name from.</param>
    static member inline GetFrom(objectWithName: ^T when ^T : (member Name: Name)) = objectWithName.Name
/// <summary>
/// Module containing helpers and the cache for creating <c>Name</c> objects.
/// </summary>
module Name =
    /// <summary>
    /// A cache of generated names; relevant for Types, where we may want to ensure that
    /// there are no conflicting names. This is mostly made redundant by the <c>PathKey</c> system.
    /// </summary>
    let private cache = System.Collections.Generic.Dictionary<string, Name>()
    /// <summary>
    /// A private helper which applies the given function to the input, and then
    /// ensures it has an apostrophe appended (if it is reserved).<br/><br/>
    /// The output is checked against the input, and a <c>Name</c> union is made.
    /// </summary>
    /// <param name="f">The string modifier function</param>
    /// <param name="input">The input string</param>
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
    let dumpCache () = cache.Clear()

/// <summary>
/// The Path module contains recursive types which are modeled after the 'path' of modules/namespaces that one
/// would take to reach their location.<br/>
/// </summary>
module Path =
    /// <summary>
    /// A discriminated union representing the module/namespace that is containing
    /// types and bindings.
    /// </summary>
    /// <remarks>
    /// Modules can be nested within one another.
    /// </remarks>
    type [<RequireQualifiedAccess>] ModulePath =
        | Root
        | Module of Module
        override this.ToString() =
            match this with
            | Root -> "Root"
            | Module m -> m.Name.ToString()
        member this.Name =
            match this with
            | Root -> Source "Root"
            | Module m -> m.Name
        static member Create(m: Module) = Module m
    /// <summary>
    /// A type can only derive from a container path (root or module)
    /// </summary>
    type Type = Type of path: ModulePath * name: Name
    /// <summary>
    /// InlineOptions can only be represented within an event,
    /// lambda, method, property or as a parameter.
    /// </summary>
    type InlineOptions = InlineOptions of parent: Choice<Parameter, Property, Method, InlineLambda, Event> * name: Name
    /// <summary>
    /// InlineLambdas can only be represented within an event,
    /// as a lambda in another lambda, or a method, property or parameter
    /// </summary>
    type InlineLambda = InlineLambda of parent: Choice<Parameter, Property, Method, InlineLambda, Event> * name: Name
    /// <summary>
    /// Not really represented by the api
    /// </summary>
    type Binding = Binding of path: ModulePath * name: Name
    /// <summary>
    /// Methods can only be defined as part of a type or in a module/root
    /// </summary>
    type Method = Method of from: Choice<Type, ModulePath> * name: Name
    /// <summary>
    /// Properties can only be represented within a type, options, event, or module
    /// </summary>
    type Property = Property of from: Choice<Type, InlineOptions, Event, Module> * name: Name
    /// <summary>
    /// Events can only be represented within a module, or a type
    /// </summary>
    type Event = Event of parent: Choice<ModulePath, Type> * name: Name
    /// <summary>
    /// Modules can only be represented within another module, or the root namespace.
    /// </summary>
    type Module = Module of path: ModulePath * name: Name
    /// <summary>
    /// Parameters can only be found on bindings, methods, types (in constructors), events, or lambdas
    /// </summary>
    type Parameter = Parameter of from: Choice<Binding, Method, Type, Event, InlineLambda> * name: Name
    
    /// <summary>
    /// The paths are used as a method of ensuring that types are predictably named.
    /// </summary>
    type [<RequireQualifiedAccess>] PathKey =
        | Binding of Binding
        | Type of Type
        | Property of Property
        | Method of Method
        | Event of Event
        | Parameter of Parameter
        | InlineOptions of InlineOptions
        | InlineLambda of InlineLambda
        | Module of Module

    type Type with
        member inline this.Destructured = let (Type(p,n)) = this in (p,n)
        member this.Path = this.Destructured |> fst
        member this.Name = this.Destructured |> snd
    type InlineOptions with
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
    type InlineLambda with
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
    type Binding with
        member inline this.Destructured = let (Binding(p,n)) = this in (p,n)
        member this.Path = this.Destructured |> fst
        member this.Name = this.Destructured |> snd
    type Method with
        member inline this.Destructured = let (Method(p,n)) = this in (p,n)
        member this.Parent = this.Destructured |> fst
        member this.Path =
            match this.Parent with
            | Choice1Of2 ``type`` -> ``type``.Path
            | Choice2Of2 path -> path
        member this.Name = this.Destructured |> snd
    type Property with
        member inline this.Destructured = let (Property(p,n)) = this in (p,n)
        member this.Parent = this.Destructured |> fst
        member this.Path =
            this.Parent |> function
                | Choice1Of4 t -> t.Path
                | Choice2Of4 o -> o.Path
                | Choice3Of4 e -> e.Path
                | Choice4Of4 m -> m.Path
        member this.Name = this.Destructured |> snd
    // An event is either contained in a module or of a class (type)
    type Event with
        member inline this.Destructured = let (Event (p,n)) = this in (p,n)
        member this.Name = this.Destructured |> snd
        member this.Path =
            this.Destructured
            |> fst |> function
                | Choice1Of2 p -> p
                | Choice2Of2 c -> c.Path
    type Module with
        member inline this.Destructured = let (Module(p,n)) = this in (p,n)
        member this.Path = this.Destructured |> fst
        member this.Name = this.Destructured |> snd
    type Parameter with
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
    type PathKey with        
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
            | Module p -> p.Path
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
                match p.Path with
                | ModulePath.Root -> Source "Root"
                | ModulePath.Module ``module`` -> ``module``.Name
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
            | Module m ->
                match m.Path with
                | ModulePath.Root -> Source "Root"
                | ModulePath.Module m -> m.Name
        static member CreateModule(name: Name) =
            Module(Path.Module.Module(ModulePath.Root, name))
        member this.CreateModule(name: Name) =
            match this with
            | Module m ->
                Path.Module.Module(m.Path, name)
                |> PathKey.Module
            | e ->
                failwith $"Tried to create a module pathkey for {e}"
        member this.CreateType(name: Name) =
            match this with
            | PathKey.Module m ->
                Path.Type(ModulePath.Module m, name)
                |> PathKey.Type
            | this ->
                Path.Type(this.Path, name)
                |> PathKey.Type
        member this.CreateBinding(name: Name) =
            match this with
            | PathKey.Module m ->
                Path.Binding(ModulePath.Module m, name)
                |> PathKey.Binding
            | this ->
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
                Path.Method(m |> ModulePath.Create |> Choice2Of2, name)
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
            | Module(m)->
                Path.Property(Choice4Of4 m, name)
                |> PathKey.Property
        member this.CreateEvent(name: Name) =
            match this with
            | Type ``type`` ->
                Path.Event(Choice2Of2 ``type``, name)
                |> PathKey.Event
            | Module m ->
                Path.Event(ModulePath.Module m |> Choice1Of2, name)
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
            | ModulePath.Root -> []
            | ModulePath.Module m ->
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
                    property.Name :: tracePathKey (PathKey.Module m)
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
            | PathKey.Module path -> path.Name :: tracePath path.Path
        tracePathKey entry
        |> List.rev
