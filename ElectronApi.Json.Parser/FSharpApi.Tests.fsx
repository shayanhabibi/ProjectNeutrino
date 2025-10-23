#r "nuget: Thoth.Json.Net"
#r "nuget: Fantomas.Core"
#load "./Utils.fs"
#load "./ApiDecoder.fs"
#load "./Fantomas.Utils.fs"
#load "./Types.fs"
#load "./FSharpApi.fs"
#load "./SourceMapper.fs"

open System.IO
open ElectronApi.Json.Parser
open ElectronApi.Json.Parser.FSharpApi
open ElectronApi.Json.Parser.Decoder
open ElectronApi.Json.Parser.FSharpApi.Type
open ElectronApi.Json.Parser.SourceMapper.Type
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

let testTypes =
    Decode.fromString decode (File.ReadAllText("./electron-api.json"))
    |> function
        | Ok values ->
            values
        | Error e ->
            failwith e
    |> readResults
    |> List.collect (function
        | ModifiedResult.Module { Properties = props } ->
            props
        | ModifiedResult.Class { Properties = props } ->
            props
        | ModifiedResult.Structure { Properties = props } ->
            props
        | ModifiedResult.Element { Properties = props } ->
            props
    )
    |> List.map (fun prop ->
        prop.PathKey |> Path.tracePathOfEntry
        |> List.map _.ValueOrModified
        |> String.concat "."
        |> printfn "%s"
        prop.Type
        )
    |> FantomasFactory.tryDebugMapping
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
