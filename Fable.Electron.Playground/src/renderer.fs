module Fable.Electron.Playground.renderer

open Fable.Core
open Fable.Core.JsInterop
open Fable.Core.JS
open Fable.Electron
open Fable.Electron.Renderer
open Fable.Electron.Routing.Renderer
open Browser.Types
open Browser.Dom

importSideEffects "./index.css"

console.log "This message is being logged by 'renderer.js', included via VITE"

let api =
    Remoting.init
    |> Remoting.build<Shared.ExampleRouting>
(api.LogText "hello" 5).``then``(fun v -> console.log v)
|> ignore

