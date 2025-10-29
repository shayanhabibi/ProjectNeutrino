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
            name.ValueOrModified = path.Name.ValueOrModified
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
                            | Nested ({ PathKey = ValueSome path } as group) when path.Name.ValueOrModified = name.ValueOrModified ->
                                func group
                                |> Nested
                            | child -> child
                            )
            }
        else
            grouper
            |> GeneratorGrouper.addNestedGroup (
                makeModuleGrouper name.ValueOrModified
                |> mapGroupForPath func path
                )
    | None ->
        func grouper
let addToGroup (child: GeneratorGroupChild) (group: GeneratorGrouper) =
    let add = fun genGroup ->
        {
            genGroup with
                Children = child :: genGroup.Children
        }
    match child with
    | Nested generatorGrouper -> generatorGrouper.PathKey.Value
    | Child generatorContainer -> generatorContainer.PathKey
    | StringEnumType stringEnum -> stringEnum.PathKey
    | Delegate funcOrMethod -> funcOrMethod.Name
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

let inline addProcessMappedChild
    (child: GeneratorContainer)
    (grouper: GeneratorGrouper) =
    match child.Process with
    | Some processBlock ->
        grouper
        |> if processBlock.Main then
            mapNestedGroup "Main" (fun group ->
                { group with Children = Child child :: group.Children }
                )
            else id
        |> if processBlock.Renderer then
            mapNestedGroup "Renderer" (fun group ->
                { group with Children = Child child :: group.Children }
                )
            else id
        |> if processBlock.Utility then
            mapNestedGroup "Utility" (fun group ->
                { group with Children = Child child :: group.Children }
                )
            else id
    | None ->
        grouper
        |> addToGroup (Child child) 

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
    |> GeneratorGrouper.addOpen "Fable.Core.JsInterop"
    |> GeneratorGrouper.addOpen "Browser.Types"
    |> GeneratorGrouper.addNestedGroup(makeModuleGrouper "Main")
    |> GeneratorGrouper.addNestedGroup(makeModuleGrouper "Utility")
    |> GeneratorGrouper.addNestedGroup(makeModuleGrouper "Renderer")
    |> GeneratorGrouper.addNestedGroup(
        makeModuleGrouper "Enums"
        |> GeneratorGrouper.addAttribute "AutoOpen"
        |> GeneratorGrouper.addAttribute "Erase"
        )
    |> GeneratorGrouper.addNestedGroup(
        makeModuleGrouper "Types"
        |> GeneratorGrouper.addAttribute "AutoOpen"
        |> GeneratorGrouper.addAttribute "Erase"
        )
    |> GeneratorGrouper.addNestedGroup(makeModuleGrouper "Constants")
let generateFromApiFile (file: string) =
    Decode.fromString decode (File.ReadAllText(file))
    |> function
        | Ok values -> values
        | Error e -> failwith e
    |> readResults
    |> List.fold (fun state -> function
        | ModifiedResult.Module result ->
            result.ToGeneratorGrouper()
            |> Option.map (fun g -> state |> GeneratorGrouper.addNestedGroup g)
            |> Option.defaultValue state
            |> addProcessMappedChild (result.ToGeneratorContainer())
        | ModifiedResult.Class result ->
            state
            |> addProcessMappedChild (result.ToGeneratorContainer())
        | ModifiedResult.Structure result ->
            state
            |> addProcessMappedChild (
                result.ToGeneratorContainer()
                |> GeneratorContainer.mapPathKey _.AddRootModule(Path.Module.Module(Path.ModulePath.Root,Source "Types"))
                )
        | ModifiedResult.Element result ->
            state
            |> addProcessMappedChild (result.ToGeneratorContainer())
        ) rootGeneratorGroup
    |> fun group ->
        // Holy mackarel of inefficiency Doctor!
        finalizeGeneratorGroup group
        |> List.append [
                GeneratorGrouper.makeOpenListNode group
                |> ModuleDecl.OpenList
            ]
        |> (GeneratorGrouper.makeNamespace group)
        |> ignore
        group
        // I realised too late that this api format leaves me bojangled since the caches
        // aren't filled until the results have been transformed
        // ... so now there's this? hahaha
    |> fun group ->
        TypeCache.getAllTypeValues()
        |> Seq.toList
        |> fun l ->
            l
        |> List.fold (fun state item ->
            match item with
            // | String -> failwith "todo"
            // | Boolean -> failwith "todo"
            // | Integer -> failwith "todo"
            // | Float -> failwith "todo"
            // | Double -> failwith "todo"
            // | Number -> failwith "todo"
            // | Unit -> failwith "todo"
            // | Undefined -> failwith "todo"
            // | Any -> failwith "todo"
            // | Unknown -> failwith "todo"
            // | Date -> failwith "todo"
            // | Constant literalType -> failwith "todo"
            | Function funcOrMethod ->
                state
                |> addToGroup (Delegate funcOrMethod)
            | StringEnum stringEnum ->
                state
                |> addToGroup (StringEnumType stringEnum)
            // | StructureRef s -> failwith "todo"
            | Object structOrObject ->
                structOrObject.PathKey
                |> GeneratorContainer.create
                |> GeneratorContainer.withAttribute "JS.Pojo"
                |> GeneratorContainer.withConstructor (structOrObject.Properties |> List.map Parameter.InlinedObjectProp)
                |> GeneratorContainer.withInstanceProperties (structOrObject.Properties)
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
            | _ -> state
            ) group
    |> fun group ->
        finalizeGeneratorGroup group
        |> List.append [
                GeneratorGrouper.makeOpenListNode group
                |> ModuleDecl.OpenList
            ]
        |> (GeneratorGrouper.makeNamespace group)
        |> fun node ->
            Oak([], [
                node
                EventInterfaces.makeInterfaces()
            ], Range.Zero)
            |> CodeFormatter.FormatOakAsync
            |> Async.RunSynchronously
            |> fun txt ->
                File.WriteAllText("./test.fs", txt)
    
