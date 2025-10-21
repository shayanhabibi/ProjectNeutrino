#r "nuget: Thoth.Json.Net"
#r "nuget: Fantomas.Core"
#load "./Utils.fs"
#load "./ApiDecoder.fs"
#load "./Fantomas.Utils.fs"
#load "./FSharpApi.fs"

open System.IO
open ElectronApi.Json.Parser.FSharpApi
open ElectronApi.Json.Parser.Decoder
open ElectronApi.Json.Parser.FSharpApi.Type
open Thoth.Json.Net

let api =
    Decode.fromString decode (File.ReadAllText("./electron-api.json"))
    |> function
        | Ok values ->
            values
        | Error e ->
            failwith e
let testCompatibility =
    let inline collectTags (blocks: ^T array when ^T : (member DocumentationBlock: DocumentationBlock)) = blocks |> Array.collect _.DocumentationBlock.AdditionalTags
    api
    |> Array.map (function
        | Module {
                Methods = methods
                Events = events
                Properties = properties
            } ->
            Array.concat [
                collectTags methods
                collectTags events
                collectTags properties
            ]
        | Structure {
            Properties = props
            } -> collectTags props
        | _ -> [||]
        )
    |> Array.map Compatibility.fromTags
let testTypes =
    api
    |> Array.map (function
        | Module {
            Properties= props
            Methods = methods
            } ->
            props
            |> Array.choose (_.TypeInformation >> Type.fromTypeInformation >> function  t -> Some t )
            |> Array.append (methods |> Array.map (_.Returns >> Option.map fromTypeInformation >> Option.defaultValue Type.Unit))
        | Structure {
            Properties = props
            } ->
            props
            |> Array.choose (_.TypeInformation >> Type.fromTypeInformation >> Some  )
        | _ -> [||]
        )

let testStructures =
    api
    |> Array.choose (function
        | Structure structure ->
            Structure.readFromDocContainer structure
            |> Some
        | _ -> None)

let testMethods =
    api
    |> Array.choose(function
        | Class {
            InstanceMethods = methods
            StaticMethods = staticMethods
            } ->
            methods |> Array.append staticMethods
            |> Array.map Method.fromDocBlock
            |> Some
        | _ -> None
        )
let testFull =
    api
    |> readResults
    |> printfn "%A"
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
