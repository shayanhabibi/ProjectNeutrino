#r "nuget: Thoth.Json.Net"
#r "nuget: Fantomas.Core"
#load "./Spec.fs"
#load "./Utils.fs"
#load "./ApiDecoder.fs"
#load "./Types.fs"
#load "./Fantomas.Utils.fs"
#load "./FSharpApi.fs"
#load "./SourceMapper.fs"
#load "./Generator.fs"

open System.IO
open ElectronApi.Json.Parser
open ElectronApi.Json.Parser.FSharpApi
open ElectronApi.Json.Parser.Decoder
open ElectronApi.Json.Parser.FSharpApi.Type
open ElectronApi.Json.Parser.SourceMapper.Type
open Fantomas.Core
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open Thoth.Json.Net

// let api =
//     Decode.fromString decode (File.ReadAllText("./electron-api.json"))
//     |> function
//         | Ok values ->
//             values
//         | Error e ->
//             failwith e
// let testCompatibility =
//     let inline collectTags (blocks: ^T array when ^T : (member DocumentationBlock: DocumentationBlock)) = blocks |> Array.collect _.DocumentationBlock.AdditionalTags
//     api
//     |> Array.map (function
//         | Module {
//                 Methods = methods
//                 Events = events
//                 Properties = properties
//             } ->
//             Array.concat [
//                 collectTags methods
//                 collectTags events
//                 collectTags properties
//             ]
//         | Structure {
//             Properties = props
//             } -> collectTags props
//         | _ -> [||]
//         )
//     |> Array.map Compatibility.fromTags
//
// let testStructures =
//     api
//     |> Array.choose (function
//         | Structure structure ->
//             Structure.readFromDocContainer None structure
//             |> Some
//         | _ -> None)

// let testMethods =
//     api
//     |> Array.choose(function
//         | Class {
//             InstanceMethods = methods
//             StaticMethods = staticMethods
//             } ->
//             methods |> Array.append staticMethods
//             |> Array.map Method.fromDocBlock
//             |> Some
//         | _ -> None
//         )
// let testFull =
//     api
//     |> readResults

// let testTypes =
//     Decode.fromString decode (File.ReadAllText("./electron-api.json"))
//     |> function
//         | Ok values ->
//             values
//         | Error e ->
//             failwith e
//     |> readResults
//     |> List.collect (function
//         | ModifiedResult.Module { Properties = props } ->
//             props
//         | ModifiedResult.Class { Properties = props } ->
//             props
//         | ModifiedResult.Structure { Properties = props } ->
//             props
//         | ModifiedResult.Element { Properties = props } ->
//             props
//     )
//     |> List.map (fun prop ->
//         prop.PathKey |> Path.tracePathOfEntry
//         |> List.map _.ValueOrModified
//         |> String.concat "."
//         |> printfn "%s"
//         prop.Type
//         )
//     |> FantomasFactory.tryDebugMapping
// let testProperties =
//     api
//     |> Array.map (function
//         | Module { Properties = props } ->
//             props |> Array.map extractFromPropertyDocumentationBlock
//         | Structure {
//             Properties = props
//             } ->
//             props
//             |> Array.map extractFromPropertyDocumentationBlock
//         | _ -> [||]
//         )
open SourceMapper
// let testStructs =
//     Decode.fromString decode (File.ReadAllText("./electron-api.json"))
//     |> function
//         | Ok values ->
//             values
//         | Error e ->
//             failwith e
//     |> readResults
//     |> List.choose (function
//         | ModifiedResult.Structure s ->
//             Some s
//         | _ -> None
//         )
//     |> Structure.tryDebugConstructor
//     TypeCache.getAllTypeValues()
//     |> Seq.toList
//     |> List.filter _.IsStringEnum
//     |> List.map (function StringEnum v -> v | _ -> failwith "")
//     |> StringEnum.tryDebugTypeGen
//     
// let testMethods =
//     Decode.fromString decode (File.ReadAllText("./electron-api.json"))
//     |> function
//         | Ok values ->
//             values
//         | Error e ->
//             failwith e
//     |> readResults
//     |> List.choose (function
//         | ModifiedResult.Module { Methods = methods } ->
//             Some methods
//         | _ -> None
//         )
//     |> List.iter Method.tryDebugStaticMemberGen
    
    
        
// let testEventConstants =
//     Decode.fromString decode (File.ReadAllText("./electron-api.json"))
//     |> function
//         | Ok values ->
//             values
//         | Error e ->
//             failwith e
//     |> readResults
//     |> List.choose (function
//         | ModifiedResult.Module { Events = events } ->
//             Some events
//         | ModifiedResult.Class { Events = events } ->
//             Some events
//         | _ -> None
//         )
//     |> List.collect id
//     |> EventConstants.tryDebugConstantProduction

// let testEvents =
//     Decode.fromString decode (File.ReadAllText("./electron-api.json"))
//     |> function
//         | Ok values ->
//             values
//         | Error e ->
//             failwith e
//     |> readResults
//     |> List.choose (function
//         | ModifiedResult.Module { Events = events } ->
//             Some events
//         | ModifiedResult.Class { Events = events } ->
//             Some events
//         | _ -> None
//         )
//     |> List.collect id
//     |> Event.tryDebugEventMemberGen

// let testEvents =
//     Decode.fromString decode (File.ReadAllText("./electron-api.json"))
//     |> function
//         | Ok values ->
//             values
//         | Error e ->
//             failwith e
//     |> readResults
//     |> List.choose (function
//         | ModifiedResult.Module { Events = events } ->
//             Some events
//         | ModifiedResult.Class { Events = events } ->
//             Some events
//         | _ -> None
//         )
//     |> List.collect id
//     |> fun e ->
//         e |> EventInterfaces.tryDebugEventInterfaces
//         // e |>Event.tryDebugEventMemberGen
//     
   
// let testClasses =
//     Decode.fromString decode (File.ReadAllText("./electron-api.json"))
//     |> function
//         | Ok values ->
//             values
//         | Error e ->
//             failwith e
//     |> readResults
//     |> List.choose (function
//         | ModifiedResult.Module { ExportedClasses = classes } ->
//             Some classes
//         | ModifiedResult.Class class' ->
//             Some [ class' ]
//         | _ -> None
//         )
//     |> List.collect id
//     |> fun e ->
//         // e |> Class.tryDebugClassTypeGen
//         e |> Class.tryDebugFileMaker
//         // e |>Event.tryDebugEventMemberGen
//

// let testModules =
//     Decode.fromString decode (File.ReadAllText("./electron-api.json"))
//     |> function
//         | Ok values ->
//             values
//         | Error e ->
//             failwith e
//     |> readResults
//     |> List.choose (function
//         | ModifiedResult.Module modules ->
//             Some modules
//         | _ -> None
//         )
//     |> List.map _.ToTypeDefn
//     |> List.collect _.Decls
//     |> fun decls ->
//         Oak([], [ ModuleOrNamespaceNode(None, decls, Range.Zero) ], Range.Zero)
//         |> CodeFormatter.FormatOakAsync
//         |> Async.RunSynchronously
//         |> printfn "%s"
// let testElements =
//     Decode.fromString decode (File.ReadAllText("./electron-api.json"))
//     |> function
//         | Ok values ->
//             values
//         | Error e ->
//             failwith e
//     |> readResults
//     |> List.map (function
//         | ModifiedResult.Element element ->
//             element.ToGeneratorContainer()
//         | ModifiedResult.Class cl ->
//             cl.ToGeneratorContainer()
//         | ModifiedResult.Module m ->
//             m.ToGeneratorContainer()
//         | ModifiedResult.Structure s ->
//             s.ToGeneratorContainer()
//         >> GeneratorContainer.makeDefaultTypeDecl
//         )
//     |> ignore
//     FuncOrMethod.getCacheValues
//     |> Seq.toList
//     |> List.map GeneratorContainer.makeDefaultDelegateTypeDecl
//     |> fun decls ->
//     Oak([], [ ModuleOrNamespaceNode(None, decls, Range.Zero) ], Range.Zero)
//     |> CodeFormatter.FormatOakAsync
//     |> Async.RunSynchronously
//     |> printfn "%s"

// let testResult =
//      Decode.fromString decode (File.ReadAllText("./electron-api.json"))
//      |> function
//          | Ok values ->
//              values
//          | Error e ->
//              failwith e
//      |> readResults
//
// let testStringEnumGeneration =
//      TypeCache.getAllTypeValues()
//      |> Seq.toList
//      |> List.filter _.IsStringEnum
//      |> List.map (function StringEnum v -> v | _ -> failwith "")
//      |> StringEnum.tryDebugTypeGen

let testGenerate =
    Transpiler.generateFromApiFile "./electron-api.json" "./Fable.Electron/Program.fs"
    // Transpiler.generateMainProcessOnlyFromApiFile "./electron-api.json" "./Fable.Electron.Main/Program.fs"
    // Transpiler.generateRendererProcessOnlyFromApiFile "./electron-api.json" "./Fable.Electron.Renderer/Program.fs"
    // Transpiler.generateUtilityProcessOnlyFromApiFile "./electron-api.json" "./Fable.Electron.Utility/Program.fs"
    exit 0
