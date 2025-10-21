module Samples
open System.IO
open EasyBuild.FileSystemProvider
open ElectronApi.Json.Parser.Decoder
open Thoth.Json.Net

type Root = AbsoluteFileSystem<__SOURCE_DIRECTORY__>
// TODO - make this independent of the decoder
let api =
    File.ReadAllText(Root.``..``.``electron-api.json``)
    |> Decode.fromString decode
    |> function
        | Ok results -> results
        | Error e -> failwith e
    
let typeInformation =
    api
    |> Array.collect (function
        | Module { Methods = methods
                   Events = events
                   Properties = props } ->
            [|
                methods
                |> Array.collect (_.Parameters >> Array.map _.TypeInformation)
                
                events
                |> Array.collect (_.Parameters >> Array.map _.TypeInformation)
                
                props
                |> Array.map _.TypeInformation
            |] |> Array.collect id
        | Structure { Properties = props } ->
            props |> Array.map _.TypeInformation
        | Class { StaticMethods = staticMethods
                  StaticProperties = staticProperties
                  InstanceMethods = instanceMethods
                  InstanceEvents = instanceEvents
                  InstanceProperties = instanceProperties } ->
            [|
                staticMethods
                |> Array.collect (_.Parameters >> Array.map _.TypeInformation)
                staticProperties
                |> Array.map _.TypeInformation 
                instanceMethods
                |> Array.collect (_.Parameters >> Array.map _.TypeInformation)
                instanceProperties
                |> Array.map _.TypeInformation
                instanceEvents
                |> Array.collect (_.Parameters >> Array.map _.TypeInformation)
            |] |> Array.collect id
        | Element { Methods = methods
                    Events = events
                    Properties = props } ->
            [|
                methods
                |> Array.collect (_.Parameters >> Array.map _.TypeInformation)
                
                events
                |> Array.collect (_.Parameters >> Array.map _.TypeInformation)
                
                props
                |> Array.map _.TypeInformation
                
            |] |> Array.collect id
        )
let structures =
    api
    |> Array.choose (function
        | Structure structure ->
            Some structure
        | _ -> None
        )
