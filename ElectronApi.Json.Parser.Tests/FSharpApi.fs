module FSharpApi

open Expecto
open ElectronApi.Json.Parser.Prelude
open Samples

[<Tests>]
let tests =
    testList "Reader" [
        test "Can Read All" {
            try
               typeInformation
               |> Array.map Type.fromTypeInformation
            with e ->
                // Tests.failtestf $"%A{e}"
                raise e
            |> Tests.printfn "%A"
        }
    ]
