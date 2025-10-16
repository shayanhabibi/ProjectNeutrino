module ElectronApi.Json.Parser.FSharpApi

// This will be our FSharp representation of the parsed API

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
