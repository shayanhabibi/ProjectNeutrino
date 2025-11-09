namespace Fable.Electron.Playground.Shared
open Fable.Core.JS
open Browser.Types
type ExampleRouting = {
    LogText: string -> int -> Promise<Result<string, unit>>
    LogBanana: int -> Promise<bool>
}

type ExampleMainToRenderer = {
    CheckText: string -> int -> unit
}
