module Fable.Electron.Playground.renderer

open Fable.Core
open Fable.Core.JsInterop
open Fable.Core.JS
open Fable.Electron
open Fable.Electron.Renderer
open Fable.Electron.Remoting.Renderer
open Browser.Types
open Browser.Dom

importSideEffects "./index.css"

console.log "This message is being logged by 'renderer.js', included via VITE"

let api =
    Remoting.init
    |> Remoting.buildSender<Shared.ExampleRouting>
(api.LogText "hello" 5).``then``(fun v -> console.log v)
|> ignore

let apie = { Shared.CheckText = fun t i -> console.log i }
Remoting.init
|> Remoting.withApiNameBase "FE"
|> Remoting.buildReceiver apie
