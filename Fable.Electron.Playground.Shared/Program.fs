namespace Fable.Electron.Playground.Shared
open Fable.Core.JS
open Browser.Types
type ExampleRouting = {
    SayHelloWorld: string -> Promise<Result<string, unit>>
}

type ExampleMainToRenderer = {
    // Dummy parameter to demonstrate multi param functions
    LogMove: string -> int -> unit
}
