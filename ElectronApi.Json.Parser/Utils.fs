[<AutoOpen>]
module ElectronApi.Json.Parser.Utils

open System.Collections.Frozen
open System.Text.RegularExpressions

let private reserved =
    [
    "checked"; "static"; "fixed"; "inline"; "default"; "component";
    "inherit"; "open"; "type"; "true"; "false"; "in"; "end"; "global"
    ]
    |> _.ToFrozenSet()
let isReserved = reserved.Contains
let appendApostropheToReservedKeywords =
    fun s -> if reserved.Contains s then s + "'" else s

// Taken from Fable.Transforms
let dashify (separator: string) (input: string) =
    Regex.Replace(
        input,
        "[a-z]?[A-Z]",
        fun m ->
            if m.Value.Length = 1 then
                m.Value.ToLowerInvariant()
            else
                m.Value.Substring(0,1)
                + separator
                + m.Value
                      .Substring(1,1)
                      .ToLowerInvariant()
        )

let toCamelCase (input: string) =
    Regex.Replace(
        input,
        "[-_ ]([a-z])",
        _.Groups.[1].Value.ToUpperInvariant()
        )
let toPascalCase (input: string) =
    let camel = toCamelCase input
    camel.Substring(0,1).ToUpper() + camel.Substring(1)

