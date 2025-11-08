// An immensely simple implementation to allow Fable.Remoting like configuration.
// All we really care about in this implementation, is creating the proxy for making the calls.
// Serialization is handled entirely by electron, and argument calls for the proxies is immaterial since
// type safety is assured by FCS
// However, we will ensure the type signatures are correct for completeness.
module Fable.Electron.Routing.Main
open System
open System.ComponentModel
open Browser
open FSharp.Core
open Fable.Core
open Fable.Core.DynamicExtensions
open Fable.Core.JsInterop
open Fable.Core.Reflection
open FSharp.Reflection
open Fable.Electron
open Fable.Electron.Main
open Fable.Electron.Renderer
open Fable.SimpleJson

type RemotingConfig = {
    ApiName: string
    ChannelNameMap: string -> string -> string
}
[<Erase>]
module Remoting =
    let init = {
        ApiName = "FABLE_REMOTING"
        ChannelNameMap = fun typeName fieldName -> sprintf $"%s{typeName}:%s{fieldName}"
    }
    let withApiName apiName config = { config with ApiName = apiName }
    let withChannelNameMap func config = { config with ChannelNameMap = func }
[<EditorBrowsable(EditorBrowsableState.Never)>]
module internal Proxy =
    let rec getReturnType typ =
        if Reflection.FSharpType.IsFunction typ then
            let _, res = Reflection.FSharpType.GetFunctionElements typ
            getReturnType res
        else
            typ

[<Erase>]
type Remoting =
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    static member buildProxy(config: RemotingConfig, impl, resolvedType: Type) =
        let schemaType = createTypeInfo resolvedType
        match schemaType with
        | TypeInfo.Record getFields ->
            let fields, recordType = getFields()
            let makeChannelName = config.ChannelNameMap
            for field in fields do
                let returnType = Proxy.getReturnType field.PropertyInfo.PropertyType |> createTypeInfo
                let isPromiseOrAsyncReturn =
                    match returnType with
                    | TypeInfo.Async _ -> true
                    | TypeInfo.Promise _ -> true
                    | _ -> false
                let handlesIpcMainEvent =
                    field.FieldType
                    |> function
                        | TypeInfo.Func _ -> 
                            field.PropertyInfo.PropertyType
                            |> FSharpType.GetFunctionElements
                            |> fst
                            |> _.Name
                            |> (=) typeof<IpcMainEvent>.Name
                        | _ -> false
                let channelName = makeChannelName recordType.Name field.FieldName
                match isPromiseOrAsyncReturn, handlesIpcMainEvent with
                | true, true ->
                    // If async and handles the IpcEvent, then we pass the ipc event
                    // (index 0) and then spread the args and await the promise
                    ipcMain.handle(channelName, emitJsExpr (impl.Item(field.FieldName)) "async (...args) => { return await $0(args[0], ...(args[1])) }")
                | false, true ->
                    // If not async and handles the IpcEvent, then we pass the ipc event
                    // (index 0) and then spread the args, but there is no need to await the promise
                    ipcMain.handle(channelName, emitJsExpr (impl.Item(field.FieldName)) "async (...args) => { return $0(args[0], ...(args[1])) }")
                | true, false ->
                    // If we dont handle the IpcMainEvent, then we do not pass it to the proxy (index 0).
                    ipcMain.handle(channelName, emitJsExpr (impl.Item(field.FieldName)) "async (...args) => { return await $0(...(args[1])) }")
                | false, false ->
                    ipcMain.handle(channelName, emitJsExpr (impl.Item(field.FieldName)) "async (...args) => { return $0(...(args[1])) }")
        | _ ->
            failwithf
                $"Cannot build proxy. Exepected type %s{resolvedType.FullName} to be \
                a valid protocol definition which is a record of functions"
    static member inline build<'t> (implementation: 't) (config: RemotingConfig) : unit =
        Remoting.buildProxy(config, implementation, typeof<'t>)
