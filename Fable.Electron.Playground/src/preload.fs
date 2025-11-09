module Fable.Electron.Playground.preload
open Fable.Core
open Fable.Core.JS
open Fable.Core.JsInterop
open Fable.Electron
open Fable.Electron.Playground.Shared
open Fable.Electron.Remoting.Preload

Remoting.init
|> Remoting.buildRendererToMain<ExampleRouting>

Remoting.init
|> Remoting.withApiName "FE"
|> Remoting.buildMainToRenderer<ExampleMainToRenderer>
