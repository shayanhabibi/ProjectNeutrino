module Fable.Electron.Playground.main
open Fable.Electron
open Fable.Core
open Fable.Core.JsInterop
open Fable.Core.JS
open Node.Api
open Node.Base
open Fable.Electron.Main
open Fable.Electron.Remoting.Main


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
    let sendMsg =
        Remoting.init
        |> Remoting.withApiNameBase "FE"
        |> Remoting.withWindow mainWindow
        |> Remoting.buildClient<Shared.ExampleMainToRenderer>
    mainWindow.onMove(fun _ ->
        sendMsg.LogMove "Testing" 5
    )
    
app.whenReady().``then``(fun () ->
    let api: Shared.ExampleRouting = {
        SayHelloWorld = fun text -> promise {
            if text = "hello" then
                return Ok <| text + " world!"
            else
                return Error()
        }
    }
    Remoting.init
    |> Remoting.buildHandler(api)
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
