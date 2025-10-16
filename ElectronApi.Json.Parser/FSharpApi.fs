module ElectronApi.Json.Parser.FSharpApi

// This will be our FSharp representation of the parsed API
// Inspiration from FSharpAST.fs from https://github.com/glutinum-org/cli/blob/main/src/Glutinum.Converter/FsharpAST.fs

type FSharpCommentParam = { Name: string; Content: string }
type FSharpCommentTypeParam = { TypeName: string; Content: string }
type FSharpCommentSeeAlso = { Link: string; Content: string }

[<RequireQualifiedAccess>]
type FSharpXmlDoc =
    | Summary of string list
    | Param of FSharpCommentParam
    | Returns of string
    | Remarks of string list
    | DefaultValue of string
    | Example of string list
    | TypeParam of FSharpCommentTypeParam
    | SeeAlso of FSharpCommentSeeAlso
type FSharpXmlDocs = FSharpXmlDoc list

[<RequireQualifiedAccess>]
type FSharpLiteral =
    | String of string
    | Int of int
    | Float of float
    | Bool of bool
    | Null
    member this.ToText() =
        match this with
        | String value -> value
        | Int value -> string value
        | Float value -> string value
        | Bool value -> string value
        | Null -> "null"


type FSharpEnumCase = {
    Name: string
    Value: FSharpLiteral
    XmlDocs: FSharpXmlDocs option
}

type FSharpEnum = {
    Name: string
    Cases: FSharpEnumCase list
}

type RequiredPojoField = {
    Name: string
    XmlDocs: FSharpXmlDocs
    Type: Type
}
type FSharpPojoField =
    | Required of RequiredPojoField
    | Optional of OptionalPojoField
type FSharpPojo = {
    Name: string
    Fields: FSharpPojoField list
    XmlDocs: FSharpXmlDocs
}

[<RequireQualifiedAccess>]
type FSharpAttribute =
    | Text of string
    /// <summary>
    /// Generates <c>[&lt;Emit("$0($1...)")&gt;]</c> attribute.
    /// </summary>
    | EmitSelfInvoke
    /// <summary>
    /// Generates <c>[&lt;Emit("$0")&gt;]</c> attribute.
    /// </summary>
    | EmitSelf
    /// <summary>
    /// Generates <c>[&lt;Import(selector, from)&gt;]</c> attribute.
    /// </summary>
    | Import of selector: string * from: string
    /// <summary>
    /// Generates <c>[&lt;ImportAll(moduleName)&gt;]</c> attribute.
    /// </summary>
    | ImportAll of moduleName: string
    /// <summary>
    /// Generates <c>[&lt;ImportDefault(moduleName)&gt;]</c> attribute.
    /// </summary>
    | ImportDefault of moduleName: string
    | Erase
    | AbstractClass
    | AllowNullLiteral
    | Obsolete of string option
    | StringEnum of Fable.Core.CaseRules
    | CompiledName of string
    | RequireQualifiedAccess
    | EmitConstructor
    /// <summary>
    /// Generates <c>[&lt;Emit("new $0.className($1...)")&gt;]"</c> attribute.
    /// </summary>
    | EmitMacroConstructor of className: string
    /// <summary>
    /// Generates <c>[&lt;Emit("$0($1...)")&gt;]</c> attribute.
    /// </summary>
    | EmitMacroInvoke of methodName: string
    | EmitIndexer
    | Global
    | ParamObject
    | ParamArray
    | Interface
    | Pojo
    | Extension
    | EditorHidden
    | AutoOpen

type OSCompatibility =
    | MacOS
    | Mas
    | Windows
    | Linux

type Stability =
    | Experimental = 1
    | Deprecated = 2

module Stability =
    let isStable: Stability -> bool = fun flags ->
        not(
            flags.HasFlag(Stability.Experimental)
            || flags.HasFlag(Stability.Deprecated)
        )
    let isExperimental: Stability -> bool = fun flags ->
        flags.HasFlag(Stability.Experimental)
    let isDeprecated: Stability -> bool = _.HasFlag(Stability.Deprecated)
    [<Literal>]
    let Stable = enum<Stability> 0

type Availability =
    | Readonly
    
module DocumentationTag =
    type Payload = {
        OSCompatibility: OSCompatibility list
        Stability: Stability
        Availability: Availability option
    }
    let private (|OsTag|AvailabilityTag|StabilityTag|) = function
        | Decoder.DocumentationTag.OS_MACOS ->
            OsTag MacOS
        | Decoder.DocumentationTag.OS_WINDOWS ->
            OsTag Windows
        | Decoder.DocumentationTag.OS_MAS ->
            OsTag Mas
        | Decoder.DocumentationTag.OS_LINUX ->
            OsTag Linux
        | Decoder.DocumentationTag.STABILITY_EXPERIMENTAL ->
            StabilityTag Stability.Experimental
        | Decoder.DocumentationTag.STABILITY_DEPRECATED ->
            StabilityTag Stability.Deprecated
        | Decoder.DocumentationTag.AVAILABILITY_READONLY ->
            AvailabilityTag Readonly

    let map (source: Decoder.DocumentationTag[]): Payload =
        let state = { OSCompatibility = []; Stability = Stability.Stable; Availability = None }
        source
        |> Array.fold (
            fun state -> function
                | OsTag os ->
                    { state with OSCompatibility = os :: state.OSCompatibility }
                | AvailabilityTag availability ->
                    { state with Availability = Some availability }
                | StabilityTag stability ->
                    { state with Stability = state.Stability ||| stability }
            ) state
