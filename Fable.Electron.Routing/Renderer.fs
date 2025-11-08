module Fable.Electron.Routing.Renderer
open System
open System.ComponentModel
open Browser
open FSharp.Core
open Fable.Core
open Fable.Core.DynamicExtensions
open FSharp.Reflection
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
[<Erase>]
type Remoting =
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    static member buildProxy(config: RemotingConfig, resolvedType: Type) =
        let schemaType = createTypeInfo resolvedType
        match schemaType with
        | TypeInfo.Record getFields ->
            let fields, recordType = getFields()
            let recordFields = [|
                for field in fields do
                    let bridgeName = config.ApiName
                    box (window.Item(bridgeName).Item(field.FieldName))
                |]

            let proxy = FSharpValue.MakeRecord(recordType, recordFields)
            unbox proxy
        | _ ->
            failwithf
                $"Cannot build proxy. Exepected type %s{resolvedType.FullName} to be \
                a valid protocol definition which is a record of functions"

    static member inline build<'t> (config: RemotingConfig) : 't =
        Remoting.buildProxy(config, typeof<'t>)
