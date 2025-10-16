#r "nuget: Thoth.Json.Net"
#load "./ElectronApi.Json.Parser/ApiDecoder.fs"

// A script so you can load an electron-api file and explore it in FSI.

let parseApiFile filePath =
    (ElectronApi.Json.Parser.Decoder.decode, System.IO.File.ReadAllText(filePath))
    ||> Thoth.Json.Net.Decode.fromString
    |> function
        | Ok result -> result
        | Error error -> failwith error
