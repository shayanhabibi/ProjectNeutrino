module ElectronApi.Json.Parser.Generator

open System.IO
open ElectronApi.Json.Parser.Decoder
open ElectronApi.Json.Parser.FSharpApi
open ElectronApi.Json.Parser.SourceMapper
open Thoth.Json.Net

let generateFromApiFile (file: string) =
    Decode.fromString decode (File.ReadAllText(file))
    |> function
        | Ok values -> values
        | Error e -> failwith e
    |> readResults
    |> List.map (function
        | ModifiedResult.Module result ->
            result.ToTypeDefn
        | ModifiedResult.Class result ->
            result.ToTypeDefn
        | ModifiedResult.Structure result ->
            result.ToPojoNode
        | ModifiedResult.Element result ->
            result.To
        )
