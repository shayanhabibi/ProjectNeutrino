module ElectronApi.Json.Parser.Generator

open System.IO
open ElectronApi.Json.Parser.Decoder
open ElectronApi.Json.Parser.FSharpApi
open ElectronApi.Json.Parser.SourceMapper
open Fantomas.Core
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open Thoth.Json.Net

(* TODO -
    Will need to create the name path for types that we are creating as they are inserted
    and create nested modules as required, and follow the name path to see if it can just
    be inserted

eg: InterfaceEvent has path [ Enums; Session; SetPermissionRequestHandler; Handler; Permission ]
We search the top level group for the head of the path (enums). If found, we recurse the search pattern with the tail.
If we fail at any point, then groups are created for the remaining paths until the tail is empty. The tail being empty
denotes that we have arrived at the name of the type more likely than not.

At this point, we add the type as a child of the most recently created group, or the last tail group found. *)

let makeModuleGrouper text = GeneratorGrouper.create (Path.PathKey.CreateModule(Source text))
type private NamePath = Name list
let getNamePath: Path.PathKey -> NamePath = Path.tracePathOfEntry
let pathHead: NamePath -> Name option  = function
    | [ _ ] -> None
    | head :: _ -> Some head
    | [] -> failwith "Should never reach this point"
let pathTail: NamePath -> NamePath = function
    | [] -> failwith "Should never reach this point"
    | [ _ ] as tail -> tail
    | _ :: tail -> tail

let groupContainsNestedModuleName (name: Name) (grouper: GeneratorGrouper) =
    grouper.Children
    |> List.exists(function
        | Nested { PathKey = ValueSome path } ->
            name.ValueOrSource |> toPascalCase = (path.Name.ValueOrSource |> toPascalCase)
        | _ -> false
            )
let rec mapGroupForPath (func: GeneratorGrouper -> GeneratorGrouper) (path: NamePath) (grouper: GeneratorGrouper) =
    match pathHead path with
    | Some name ->
        let path = pathTail path
        if groupContainsNestedModuleName name grouper then
            {
                grouper with
                    Children =
                        grouper.Children
                        |> List.map (function
                            | Nested ({ PathKey = ValueSome nestedPath } as group)
                                when (nestedPath.Name.ValueOrSource |> toPascalCase)
                                     = (name.ValueOrSource |> toPascalCase) ->
                                mapGroupForPath func path group
                                |> Nested
                            | child -> child
                            )
            }
        else
            grouper
            |> GeneratorGrouper.addNestedGroup (
                makeModuleGrouper (name.ValueOrModified |> toPascalCase)
                |> mapGroupForPath func path
                )
    | None ->
        func grouper
and addToGroup (child: GeneratorGroupChild) (group: GeneratorGrouper) =
    let add = fun genGroup ->
        {
            genGroup with
                Children = child :: genGroup.Children
        }
    match child with
    | Nested generatorGrouper -> generatorGrouper.PathKey.Value
    | Child generatorContainer -> generatorContainer.PathKey
    | StringEnumType stringEnum -> stringEnum.PathKey
    | Delegate funcOrMethod -> funcOrMethod.PathKey
    | EventConstant pathKey | EventInterface(pathKey, _) -> pathKey
    |> getNamePath
    |> fun namePath ->
        mapGroupForPath add namePath group

let mapNestedGroup
    (keyName: string)
    (func: GeneratorGrouper -> GeneratorGrouper)
    (generatorGrouper: GeneratorGrouper) =
    let mutable successfullyMapped = false
    let result = {
        generatorGrouper with
            Children =
                generatorGrouper.Children
                |> List.map (function
                    | Nested ({ PathKey = ValueSome path } as group) when path.Name.ValueOrModified = keyName ->
                        successfullyMapped <- true
                        Nested (func group)
                    | child -> child
                    )
    }
    result

let tryWith (funcs: ('a -> 'b option) list) (orElse: 'a -> 'b) (input: 'a) =
    funcs
    |> List.fold (fun state func ->
        match state with
        | None -> func input
        | _ -> state
        ) None
    |> Option.defaultWith(fun () -> orElse input)
#nowarn 40
let rec finalizeGeneratorGroup: GeneratorGrouper -> ModuleDecl list =
    GeneratorGrouper.makeChildren (tryWith [
        GeneratorGrouper.makeDefaultDelegateType
        GeneratorGrouper.makeDefaultStringEnumType
        GeneratorGrouper.makeDefaultEventInterfaceAndTypeAlias
        GeneratorGrouper.makeDefaultEventStringConstant
    ] (function
        | Child typ -> GeneratorContainer.makeDefaultTypeDecl typ
        | Nested group ->
            finalizeGeneratorGroup group
            |> (GeneratorGrouper.makeNestedModule group)
        | _ -> failwith "handled"
        )
    )
let rootGeneratorGroup =
    GeneratorGrouper.createRoot()
    |> GeneratorGrouper.addOpen "Fable.Core"
    |> GeneratorGrouper.addOpen "Fable.Core.JS"
    |> GeneratorGrouper.addOpen "Browser.Types"
    |> GeneratorGrouper.addOpen "Node.Buffer"
    |> GeneratorGrouper.addOpen "Node.WorkerThreads"
    |> GeneratorGrouper.addOpen "Node.Base"
    |> GeneratorGrouper.addOpen "Node.Stream"
    |> GeneratorGrouper.addOpen "Fetch"
    |> GeneratorGrouper.addNestedGroup(makeModuleGrouper "Main")
    |> GeneratorGrouper.addNestedGroup(makeModuleGrouper "Utility")
    |> GeneratorGrouper.addNestedGroup(makeModuleGrouper "Renderer")
    |> GeneratorGrouper.addNestedGroup(
        makeModuleGrouper "Enums"
        |> GeneratorGrouper.addAttribute "AutoOpen"
        |> GeneratorGrouper.addAttribute "Fable.Core.Erase"
        )
    |> GeneratorGrouper.addNestedGroup(
        makeModuleGrouper "Types"
        |> GeneratorGrouper.addAttribute "AutoOpen"
        |> GeneratorGrouper.addAttribute "Fable.Core.Erase"
        )
let generateFromApiFile (file: string) (destination: string) =
    Decode.fromString decode (File.ReadAllText(file))
    |> function
        | Ok values -> values
        | Error e -> failwith e
    |> readResults
    |> List.fold (fun state -> function
        | ModifiedResult.Module result ->
            let moduleContainer = result.ToGeneratorContainer()
            result.ToGeneratorGrouper()
            |> Option.map (fun g -> state |> GeneratorGrouper.addNestedGroup g)
            |> Option.defaultValue state
            |> addToGroup (moduleContainer |> Child)
        | ModifiedResult.Class result ->
            state
            |> addToGroup (result.ToGeneratorContainer() |> Child)
        | ModifiedResult.Structure result ->
            state
            |> addToGroup (
                result.ToGeneratorContainer()
                |> GeneratorContainer.mapPathKey _.AddRootModule(Path.Module.Module(Path.ModulePath.Root,Source "Types"))
                |> Child
                )
        | ModifiedResult.Element result ->
            state
            |> addToGroup (result.ToGeneratorContainer() |> Child)
        ) rootGeneratorGroup
    |> fun group ->
        // Holy mackarel of inefficiency Doctor!
        // Dummy run to fill our caches
        finalizeGeneratorGroup group
        |> List.append [
                GeneratorGrouper.makeOpenListNode group
                |> ModuleDecl.OpenList
            ]
        |> (GeneratorGrouper.makeNamespace false group)
        |> ignore
        group
    |> fun group ->
        // TypeCache.getAllTypeValues()
        // |> Seq.toList
        Type.Cache.GetInlineObjects()
        |> List.map (fun structOrObject ->
            structOrObject.PathKey, Type.Object structOrObject
            )
        |> List.append (
            Type.Cache.GetFuncOrMethods()
            |> List.map (
                fun funcOrMethod ->
                funcOrMethod.PathKey, Type.Function funcOrMethod
                )
            )
        |> List.append(
            Type.Cache.GetStringEnums()
            |> List.map (
                fun stringEnum ->
                    stringEnum.PathKey, Type.StringEnum stringEnum
                )
            )
        |> List.append(
            Type.Cache.GetEventInfos()
                |> List.map (
                    fun eventInfo ->
                        eventInfo.PathKey, Type.Event eventInfo
                    )
            )
        |> List.distinctBy fst
        |> List.fold (fun state (_, item) ->
            match item with
            | Function funcOrMethod ->
                state
                |> addToGroup (Delegate funcOrMethod)
            | StringEnum stringEnum ->
                state
                |> addToGroup (StringEnumType stringEnum)
            | Event eventInfo ->
                eventInfo.PathKey
                |> GeneratorContainer.create
                |> GeneratorContainer.withAttribute "Interface"
                |> GeneratorContainer.withAttribute "AllowNullLiteral"
                |> GeneratorContainer.withExtends "Event"
                |> GeneratorContainer.withInstanceProperties eventInfo.Properties
                |> GeneratorContainer.makeDefaultEventInfoDefn
                |> fun decl ->
                    state
                    |> addToGroup (
                        (eventInfo.PathKey, TypeDefn.Regular decl)
                        |> EventInterface
                        )
            // | StructureRef s -> failwith "todo"
            | Object structOrObject ->
                structOrObject.PathKey
                |> GeneratorContainer.create
                |> GeneratorContainer.withAttribute "JS.Pojo"
                |> GeneratorContainer.withConstructor (structOrObject.Properties |> List.map Parameter.InlinedObjectProp)
                |> GeneratorContainer.withInstanceProperties structOrObject.Properties
                |> fun child ->
                    state
                    |> addToGroup (Child child)
            // | Promise innerType -> failwith "todo"
            // | Record(key, value) -> failwith "todo"
            // | Event eventInfo -> failwith "todo"
            // | EventRef ``type`` -> ``type``
            // | Partial(``type``, l) -> failwith "todo"
            // | Omit(``type``, l) -> failwith "todo"
            // | Pick(``type``, l) -> failwith "todo"
            // | Collection ``type`` -> failwith "todo"
            // | Array ``type`` -> failwith "todo"
            // | OneOf types -> failwith "todo"
            // | Tuple types -> failwith "todo"
            // | Join(structureRef, props) -> failwith "todo"
            | _ ->
                printfn "%A" item
                state
            ) group
    |> fun group ->
        EventInterfaces.makeInterfaces()
        |> List.fold (fun state eventInterface ->
            addToGroup
                (GeneratorGroupChild.EventInterface eventInterface)
                state
            ) group
        |> addToGroup (
                (Path.PathKey.Module(
                    Path.Module.Module(Path.ModulePath.Root,Source "Main")
                ).CreateType(Source Injections.touchBarItemsName), Injections.touchBarItemsDef)
                |> GeneratorGroupChild.EventInterface
            )
    |> fun group ->
        Type.Cache.GetEventStrings()
        |> List.fold (fun state item ->
            state
            |> addToGroup (
                item.AddRootModule(Path.Module.Module(Path.ModulePath.Root, Source "Constants"))
                |> GeneratorGroupChild.EventConstant
                )
            ) group
    |> fun group ->
        finalizeGeneratorGroup group
        |> List.append [
                GeneratorGrouper.makeOpenListNode group
                |> ModuleDecl.OpenList
            ]
        |> (GeneratorGrouper.makeNamespace true group)
        |> fun node ->
            Oak([
                // let makeDirective text = ParsedHashDirectiveNode(text, [], Range.Zero)
                // makeDirective "nowarn 1182"
                // makeDirective "nowarn 47"
            ], [ node ], Range.Zero)
            |> CodeFormatter.FormatOakAsync
            |> Async.RunSynchronously
            |> fun txt ->
                File.WriteAllText(destination, txt)
    
