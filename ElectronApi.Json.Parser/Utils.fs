[<AutoOpen>]
module ElectronApi.Json.Parser.Utils

open System.Collections.Frozen
open System.Text.RegularExpressions

/// <summary>
/// Reserved keywords
/// </summary>
let private reserved =
    [
      "abstract"; "and"; "as"; "assert"; "base"; "begin"; "class"; "default"
      "delegate"; "do"; "done"; "downcast"; "downto"; "elif"; "else"
      "end"; "exception"; "extern"; "false"; "finally"; "fixed"
      "fun"; "function"; "global"; "if"; "in"; "inherit"; "inline"
      "interface"; "internal"; "lazy"; "let"; "match"; "member"
      "module"; "mutable"; "namespace"; "new"; "null"; "of"; "open"
      "or"; "override"; "private"; "public"; "rec"; "return"
      "static"; "struct"; "then"; "to"; "true"; "try"; "type"
      "upcast"; "use"; "val"; "void"; "when"; "while" ;"with"; "yield"
      "const"
      // not actually reserved, but better not to obfuscate
      "not"; "select"
      // reserved because they are keywords in OCaml
      "asr"; "land"; "lor"; "lsl"; "lsr"; "lxor"; "mod"; "sig"
      // reserved for future expansion
      "break"; "checked"; "component"; "constraint"; "continue"; "event"
      "external"; "include"; "mixin"; "parallel"; "process"; "protected"
      "pure"; "sealed"; "tailcall"; "trait"; "virtual"
    ].ToFrozenSet()

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

