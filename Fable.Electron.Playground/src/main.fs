module Fable.Electron.Playground.main
open Fable.Electron
open Fable.Core
open Fable.Core.JsInterop
open Fable.Core.JS
open Node.Api
open Node.Base
open Fable.Electron.Main
open Fable.Electron.Routing.Main


if SquirrelStartup.started then
    app.quit()
    
let createWindow() =
    let mainWindowOptions =
        BrowserWindowConstructorOptions(
            width = 800,
            height = 600,
            webPreferences = WebPreferences(
                preload = path.join(__dirname, "preload.js")
                )
            )
    let mainWindow = BrowserWindow(mainWindowOptions)
    
    if isNullOrUndefined MAIN_WINDOW_VITE_DEV_SERVER_URL
    then mainWindow.loadFile(path.join(__dirname, $"../renderer/{MAIN_WINDOW_VITE_NAME}/index.html"))
    else mainWindow.loadURL(MAIN_WINDOW_VITE_DEV_SERVER_URL)
    |> ignore
    
    mainWindow.webContents.openDevTools(Enums.WebContents.OpenDevTools.Options.Mode.Right)

app.whenReady().``then``(fun () ->
    let api: Shared.ExampleRouting = {
        LogText = fun text i -> promise {
            if text = "hello" then
                return Ok <| text + "Yep"
            else
                return Error()
        }
        LogBanana = fun _ -> promise { return true }
    }
    Remoting.init
    |> Remoting.build(api)
    createWindow()
    app.onActivate(fun _ ->
        if BrowserWindow.getAllWindows().Length = 0 then
            createWindow()
        )
    )
|> ignore

app.onWindowAllClosed(fun () ->
    if not Node.Api.process.platform.IsDarwin then
        app.quit()
    )
