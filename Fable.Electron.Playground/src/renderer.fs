module Fable.Electron.Playground.renderer

open Fable.Core.JsInterop
open Fable.Electron.Remoting.Renderer
open Browser.Dom

importSideEffects "./index.css"

console.log "This message is being logged by 'renderer.js', included via VITE"

let api =
    Remoting.init
    |> Remoting.buildClient<Shared.ExampleRouting>
(api.SayHelloWorld "hello").``then``(fun v -> console.log v)
|> ignore

let apie = { Shared.LogMove = fun t i -> console.log i }
Remoting.init
|> Remoting.withApiNameBase "FE"
|> Remoting.buildHandler apie
