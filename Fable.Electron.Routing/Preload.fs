// An immensely simple implementation to allow Fable.Remoting like configuration.
// All we really care about in this implementation, is creating the proxy for making the calls.
// Serialization is handled entirely by electron, and argument calls for the proxies is immaterial since
// type safety is assured by FCS
// However, we will ensure the type signatures are correct for completeness.
module Fable.Electron.Routing.Preload
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
module private Proxy =
    let proxyFetch (typeName: string) (func: RecordField) (config: RemotingConfig) =
        let funcArgs : TypeInfo[] =
            match func.FieldType with
            | TypeInfo.Async _ -> [| func.FieldType |]
            | TypeInfo.Promise _ -> [| func.FieldType |]
            | TypeInfo.Func getArgs -> getArgs()
            | _ -> failwithf $"Field %s{func.FieldName} does not have a valid definition"

        let makeChannelName = config.ChannelNameMap
        let argumentCount = (Array.length funcArgs) - 1
        let channelName = makeChannelName typeName func.FieldName

        let funcNeedParameters =
            match funcArgs with
            | [| TypeInfo.Async _ |] -> false
            | [| TypeInfo.Promise _ |] -> false
            | [| TypeInfo.Unit; TypeInfo.Async _ |] -> false
            | _ -> true

        fun arg0 arg1 arg2 arg3 arg4 arg5 arg6 arg7 ->
            let inputArguments =
               if funcNeedParameters
               then Array.take argumentCount [| box arg0;box arg1;box arg2;box arg3; box arg4; box arg5; box arg6; box arg7 |]
               else [| |]
            unbox<FSharpFunc<_,_>> <| ipcRenderer.invoke(channelName, inputArguments)

[<Erase>]
type Remoting =
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    static member buildProxy(config: RemotingConfig, resolvedType: Type) =
        let schemaType = createTypeInfo resolvedType
        match schemaType with
        | TypeInfo.Record getFields ->
            let fields, recordType = getFields()
            let fieldTypes = Reflection.FSharpType.GetRecordFields recordType |> Array.map (fun prop -> prop.Name, prop.PropertyType)
            let recordFields = [|
                for field in fields do
                    let normalize n =
                        let fieldType = fieldTypes |> Array.pick (fun (name, typ) -> if name = field.FieldName then Some typ else None)
                        let fn = Proxy.proxyFetch recordType.Name field config
                        match n with
                        | 0 -> box (fn null null null null null null null null)
                        | 1 -> box (fun a ->
                            fn a null null null null null null null)
                        | 2 ->
                            let proxyF a b = fn a b null null null null null null
                            unbox (System.Func<_,_,_> proxyF)
                        | 3 ->
                            let proxyF a b c = fn a b c null null null null null
                            unbox (System.Func<_,_,_,_> proxyF)
                        | 4 ->
                            let proxyF a b c d = fn a b c d null null null null
                            unbox (System.Func<_,_,_,_,_> proxyF)
                        | 5 ->
                            let proxyF a b c d e = fn a b c d e null null null
                            unbox (System.Func<_,_,_,_,_,_> proxyF)
                        | 6 ->
                            let proxyF a b c d e f = fn a b c d e f null null
                            unbox (System.Func<_,_,_,_,_,_,_> proxyF)
                        | 7 ->
                            let proxyF a b c d e f g = fn a b c d e f g null
                            unbox (System.Func<_,_,_,_,_,_,_,_> proxyF)
                        | 8 ->
                            let proxyF a b c d e f g h = fn a b c d e f g h
                            unbox (System.Func<_,_,_,_,_,_,_,_,_> proxyF)
                        | _ ->
                            failwithf
                                $"Cannot generate proxy function for %s{field.FieldName}. \
                                Only up to 8 arguments are supported. Consider using a record type as input"

                    let argumentCount =
                        match field.FieldType with
                        | TypeInfo.Async _  -> 0
                        | TypeInfo.Promise _  -> 0
                        | TypeInfo.Func getArgs -> Array.length (getArgs()) - 1
                        | _ -> 0

                    normalize argumentCount
                |]

            let proxy = FSharpValue.MakeRecord(recordType, recordFields)
            contextBridge.exposeInMainWorld(config.ApiName, proxy)
        | _ ->
            failwithf
                $"Cannot build proxy. Exepected type %s{resolvedType.FullName} to be \
                a valid protocol definition which is a record of functions"

    static member inline build<'t>(config: RemotingConfig) =
        Remoting.buildProxy(config, typeof<'t>)
