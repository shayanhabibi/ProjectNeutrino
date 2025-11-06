[<AutoOpen>]
module ElectronApi.Json.Parser.Utils

open System
open System.Collections.Frozen
open System.Collections.Generic
open System.Text.RegularExpressions

/// Remapping of any lower case enums into a format that is regexd as separate words
/// This DOES NOT effect the 'Source' of a name.
let private remaps =
    let inline (==>) l r = System.Collections.Generic.KeyValuePair(l,r)
    [
        "iskeypad" ==> "is keypad"
        "isautorepeat" ==> "is auto repeat"
        "leftbuttondown" ==> "left button down"
        "middlebuttondown" ==> "middle button down"
        "rightbuttondown" ==> "right button down"
        "capslock" ==> "caps lock"
        "numlock" ==> "num lock"
        
    ].ToFrozenDictionary()

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
      "break"; "checked"; "component"; "constraint"; "continue"; (* "event" *)
      "external"; "include"; "mixin"; "parallel"; "process"; "protected"
      "pure"; "sealed"; "tailcall"; "trait"; "virtual"; "params"
    ].ToFrozenSet()

let isReserved = reserved.Contains
let appendApostropheToReservedKeywords =
    fun s -> if reserved.Contains s then s + "'" else s
let stropInvalidIdentifiers: string -> string = function
    | text when text.Contains('-', System.StringComparison.Ordinal) ->
        $"``{text}``"
    | text ->
        text
let stropReservedKeywords =
    fun s ->
        if
            reserved.Contains s
            || s[0] |> Char.IsAsciiLetter |> not
            || s.Contains('.')
        then "``" + s + "``" else s

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
let inline private prelude (input: string): string =
    match remaps.TryGetValue(input) with
    | false, _ -> input
    | true, value -> value
let toCamelCase (input: string) =
    let input = prelude input
    let value =
        Regex.Replace(
            input,
            "[-_ ]([a-z])",
            _.Groups.[1].Value.ToUpperInvariant()
            )
    value.Substring(0,1).ToLower() + value.Substring(1)
    
let toPascalCase (input: string) =
    // let input = prelude input // redundant; already done in toCamelCase
    let camel = (toCamelCase input).Trim('`')
    camel.Substring(0,1).ToUpper() + camel.Substring(1)

let private transformCodeBlocks (input: string) =
    let fencedReplaced = Regex.Replace(input, @"```(.*?)```", "<code>$1</code>", RegexOptions.Singleline)
    Regex.Replace(fencedReplaced, @"`([^`]+)`", "<c>$1</c>")
let normalizeDocs =
    String.collect (function
        | '<' -> "&lt;"
        | '>' -> "&gt;"
        | '&' -> "&amp;"
        | c -> string c
        )
    >> transformCodeBlocks

module XmlDcs =
    module Boundaries =
        let [<Literal>] openRemarks = "<remarks>"
        let [<Literal>] closeRemarks = "</remarks>"
        
        let [<Literal>] openSummary = "<summary>"
        let [<Literal>] closeSummary = "</summary>"
        
        let [<Literal>] openExample = "<example>"
        let [<Literal>] closeExample = "</example>"
        
        let [<Literal>] openCode = "<code>"
        let [<Literal>] closeCode = "</code>"
        
        let [<Literal>] openFSharpCode = "<code lang=\"fsharp\">"
        let [<Literal>] closeFSharpCode = closeCode
        let [<Literal>] openPara = "<para>"
        let [<Literal>] closePara = "</para>"
        module Token =
            [<Struct>]
            type BoundaryToken = BoundaryToken of string * string with
                member inline this.Open = let (BoundaryToken(value,_)) = this in value
                member inline this.Close = let (BoundaryToken(_,value)) = this in value
            let private create opener closer = BoundaryToken(opener,closer)
            let remarks: BoundaryToken = create openRemarks closeRemarks
            let summary: BoundaryToken = create openSummary closeSummary
            let example: BoundaryToken = create openExample closeExample
            let code: BoundaryToken = create openCode closeCode
            let fsharpCode: BoundaryToken = create openFSharpCode closeFSharpCode
            let para: BoundaryToken = create openPara closePara
    let [<Literal>] br = "<br/>"
    let wrapAround (openBoundary: string) (closeBoundary: string) (contents: string) =
        openBoundary + contents + closeBoundary
    let wrapWith (openBoundary: string) (closeBoundary: string) (contents: string list) = [
        openBoundary
        yield! contents
        closeBoundary
    ]
    let inline wrapStringWith (boundaryToken: Boundaries.Token.BoundaryToken) (text: string): string =
        wrapAround boundaryToken.Open boundaryToken.Close text
    let inline wrapStringsWith (boundaryToken: Boundaries.Token.BoundaryToken) (texts: string list): string list =
        wrapWith boundaryToken.Open boundaryToken.Close texts

module Directives =
    open Fantomas.Core.SyntaxOak
    open Fantomas.FCS.Text
    let inline wrapWithIf(text: string) (node: 'T when 'T : (member AddAfter: TriviaNode -> unit) and 'T : (member AddBefore: TriviaNode -> unit)) =
        let beforeTrivia = TriviaNode(
            TriviaContent.Directive $"#if {text}",
            Range.Zero
            )
        let afterTrivia = TriviaNode(TriviaContent.Directive "#endif", Range.Zero)
        node.AddBefore(beforeTrivia)
        node.AddAfter(afterTrivia)
        node
