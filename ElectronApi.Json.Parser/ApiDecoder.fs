module rec ElectronApi.Json.Parser.Decoder
// This is a decoder for the electron-api.json
// The schema for the types is derived from the
// @electron/docs-parser ParsedDocumentation.d.ts
open Thoth.Json.Net
#nowarn 40

type DocumentationTag =
    | OS_MACOS
    | OS_MAS
    | OS_WINDOWS
    | OS_LINUX
    | STABILITY_EXPERIMENTAL
    | STABILITY_DEPRECATED
    | AVAILABILITY_READONLY

module DocumentationTag =
    let decode: Decoder<DocumentationTag> =
        Decode.string
        |> Decode.andThen (function
            | "os_macos" -> Decode.succeed OS_MACOS
            | "os_mas" -> Decode.succeed OS_MAS
            | "os_windows" -> Decode.succeed OS_WINDOWS
            | "os_linux" -> Decode.succeed OS_LINUX
            | "stability_experimental" -> Decode.succeed STABILITY_EXPERIMENTAL
            | "stability_deprecated" -> Decode.succeed STABILITY_DEPRECATED
            | "availability_readonly" -> Decode.succeed AVAILABILITY_READONLY
            | invalid -> Decode.fail $"{invalid} is not a known DocumentationTag"
            )

type PossibleStringValue = {
    Value: string
    Description: string
}
        
module PossibleStringValue =
    let decode: Decoder<PossibleStringValue> =
        Decode.object (fun get ->
            {
                Value = get.Required.Field "value" Decode.string
                Description = get.Required.Field "description" Decode.string
            }
            )

[<RequireQualifiedAccess>]
module TypeInformationKind =
    type InfoArray = {
        Collection: bool
        Type: TypeInformation[]
    }
    module InfoArray =
        let decode: Decoder<InfoArray> =
            Decode.object (fun get ->
                {
                    Collection = get.Required.Field "collection" Decode.bool
                    Type =
                        Decode.array TypeInformation.decode
                        |> get.Required.Field "type" 
                })
    type InfoString = {
        Collection: bool
        Type: string
    }
    module InfoString =
        let decode: Decoder<InfoString> =
            Decode.object (fun get ->
                {
                    Collection = get.Required.Field "collection" Decode.bool
                    Type = get.Required.Field "type" Decode.string
                })
    type String = {
        Collection: bool
        PossibleValues: PossibleStringValue[] option
    }
    module String =
        let decode: Decoder<String> =
            Decode.object (fun get ->
                get.Required.Field "type" Decode.string
                |> function
                    | "String" ->
                        {
                            Collection = get.Required.Field "collection" Decode.bool
                            PossibleValues =
                                Decode.array PossibleStringValue.decode
                                |> get.Optional.Field "possibleValues"
                        }
                    | _ ->
                        (Decode.string |> Decode.andThen Decode.fail)
                        |> get.Required.Field "type"
                )
    type Object = {
        Collection: bool
        Properties: PropertyDocumentationBlock[]
    }
    module Object =
        let decode: Decoder<Object> =
            Decode.object (fun get ->
                get.Required.Field "type" Decode.string
                |> function
                    | "Object" ->
                        {
                            Collection = get.Required.Field "collection" Decode.bool
                            Properties =
                                Decode.array PropertyDocumentationBlock.decode
                                |> get.Required.Field "properties"
                        }
                    | _ ->
                        get.Required.Field
                            "type"
                            (Decode.string
                             |> Decode.andThen Decode.fail)
            )
    type Event = {
        Collection: bool
        EventProperties: PropertyDocumentationBlock[]
    }
    module Event =
        let decode: Decoder<Event> =
            Decode.object (fun get ->
                get.Required.Field "type" Decode.string
                |> function
                    | "Event" ->
                        {
                            Collection = get.Required.Field "collection" Decode.bool
                            EventProperties =
                                Decode.array PropertyDocumentationBlock.decode
                                |> get.Required.Field "eventProperties"
                        }
                    | _ ->
                       get.Required.Field
                           "type"
                           (Decode.string
                            |> Decode.andThen Decode.fail)
            )
    type EventRef = {
        Collection: bool
        EventPropertiesReference: TypeInformation
    }
    module EventRef =
        let decode: Decoder<EventRef> =
            Decode.object (fun get ->
                get.Required.Field "type" Decode.string
                |> function
                    | "Event" ->
                        {
                            Collection = get.Required.Field "collection" Decode.bool
                            EventPropertiesReference = get.Required.Field "eventPropertiesReference" TypeInformation.decode
                        }
                    | _ ->
                        get.Required.Field
                            "type"
                            (Decode.string
                             |> Decode.andThen Decode.fail)
                )
    type Function = {
        Collection: bool
        Parameters: MethodParameterDocumentation[]
        Returns: TypeInformation option
    }
    module Function =
        let decode: Decoder<Function> =
            Decode.object (fun get ->
                get.Required.Field "type" Decode.string
                |> function
                    | "Function" ->
                        {
                            Collection = get.Required.Field "collection" Decode.bool
                            Parameters =
                                Decode.array MethodParameterDocumentation.decode
                                |> get.Required.Field "parameters"
                            Returns =
                                Decode.option TypeInformation.decode
                                |> get.Required.Field "returns"
                        }
                    | _ -> get.Required.Field
                               "type"
                               (Decode.string
                                |> Decode.andThen Decode.fail)
                )
type TypeInformationKind =
    | InfoArray of TypeInformationKind.InfoArray
    | InfoString of TypeInformationKind.InfoString
    | String of TypeInformationKind.String
    | Object of TypeInformationKind.Object
    | Event of TypeInformationKind.Event
    | EventRef of TypeInformationKind.EventRef
    | Function of TypeInformationKind.Function
type TypeInformation =
    | WithInnerTypes of kind: TypeInformationKind * innerTypes: TypeInformation[]
    | TypeInformation of kind: TypeInformationKind
module TypeInformation =
    let decode: Decoder<TypeInformation> = Decode.object (fun root ->
        // first we check if we have the innerTypes field
        root.Optional.Field
            "innerTypes"
            (Decode.array decode)
        |> function
            | Some typs ->
                fun kind -> WithInnerTypes(kind, typs)
            | None -> TypeInformation.TypeInformation
        |> fun wrapper ->
            // now lets determine what the 'type' is
            root.Required.Raw
                (Decode.oneOf [
                    // if its typeinfo array then this should work
                    TypeInformationKind.InfoArray.decode
                    |> Decode.map (TypeInformationKind.InfoArray >> wrapper)
                    // Else we will decode the string field of type and see which type it is
                    TypeInformationKind.Object.decode
                    |> Decode.map (TypeInformationKind.Object >> wrapper)
                    TypeInformationKind.Event.decode
                    |> Decode.map (TypeInformationKind.Event >> wrapper)
                    TypeInformationKind.EventRef.decode
                    |> Decode.map (TypeInformationKind.EventRef >> wrapper)
                    TypeInformationKind.Function.decode
                    |> Decode.map (TypeInformationKind.Function >> wrapper)
                    TypeInformationKind.String.decode
                    |> Decode.map (TypeInformationKind.String >> wrapper)
                    // V This must be last
                    TypeInformationKind.InfoString.decode
                    |> Decode.map (TypeInformationKind.InfoString >> wrapper)
                ])
        )

type MethodParameterDocumentation = {
    Name: string
    Description: string
    Required: bool
    // Embedded
    TypeInformation: TypeInformation
}
module MethodParameterDocumentation =
    let decode: Decoder<MethodParameterDocumentation> =
        Decode.object (fun get ->
            {
                Name = get.Required.Field "name" Decode.string
                Description = get.Required.Field "description" Decode.string
                Required = get.Required.Field "required" Decode.bool
                TypeInformation = get.Required.Raw TypeInformation.decode
            })

type EventParameterDocumentation = {
    Name: string
    Description: string
    Required: bool
    // Embedded
    TypeInformation: TypeInformation
}
    
module EventParameterDocumentation =
    let decode: Decoder<EventParameterDocumentation> =
        Decode.object (fun get ->
            {
                Name = get.Required.Field "name" Decode.string
                Description = get.Required.Field "description" Decode.string
                Required = get.Required.Field "required" Decode.bool
                TypeInformation = get.Required.Raw TypeInformation.decode
            })
type DocumentationBlock = {
    Name: string
    Description: string
    AdditionalTags: DocumentationTag[]
    // optional field
    UrlFragment: string option
}

module DocumentationBlock =
    let decode: Decoder<DocumentationBlock> =
        Decode.object (fun get ->
            {
                Name = get.Required.Field "name" Decode.string
                Description = get.Required.Field "description" Decode.string
                AdditionalTags = get.Required.Field "additionalTags" (Decode.array DocumentationTag.decode)
                UrlFragment = get.Optional.Field "urlFragment" Decode.string
            })
type MethodDocumentationBlock = {
    // Embedded
    DocumentationBlock: DocumentationBlock
    // Optional field
    RawGenerics: string option
    Signature: string
    Parameters: MethodParameterDocumentation[]
    Returns: TypeInformation option
}
module MethodDocumentationBlock =
    let decode: Decoder<MethodDocumentationBlock> =
        Decode.object (fun get ->
            {
                DocumentationBlock = get.Required.Raw DocumentationBlock.decode
                RawGenerics = get.Optional.Field "rawGenerics" Decode.string
                Signature = get.Required.Field "signature" Decode.string
                Parameters =
                    Decode.array MethodParameterDocumentation.decode
                    |> get.Required.Field "parameters"
                Returns =
                    Decode.option TypeInformation.decode
                    |> get.Required.Field "returns"
            })
type EventDocumentationBlock = {
    // Embedded
    DocumentationBlock: DocumentationBlock
    Parameters: EventParameterDocumentation[]
}
module EventDocumentationBlock =
    let decode: Decoder<EventDocumentationBlock> =
        Decode.object (fun get ->
            {
                DocumentationBlock = get.Required.Raw DocumentationBlock.decode
                Parameters =
                    Decode.array EventParameterDocumentation.decode
                    |> get.Required.Field "parameters"
            })

type PropertyDocumentationBlock = {
    // Embedded
    DocumentationBlock: DocumentationBlock
    Required: bool
    // Embedded
    TypeInformation: TypeInformation
}

module PropertyDocumentationBlock =
    let decode: Decoder<PropertyDocumentationBlock> =
        Decode.object (fun get ->
            {
                DocumentationBlock = get.Required.Raw DocumentationBlock.decode
                Required = get.Required.Field "required" Decode.bool
                TypeInformation = get.Required.Raw TypeInformation.decode
            })

type BaseDocumentationContainer = {
    Name: string
    // Optional field
    Extends: string option
    Description: string
    Version: string
    Slug: string
    WebsiteUrl: string
    RepoUrl: string
}

module BaseDocumentationContainer =
    let decode: Decoder<BaseDocumentationContainer> =
        Decode.object (fun get ->
            {
                Name = get.Required.Field "name" Decode.string
                Extends = get.Optional.Field "extends" Decode.string
                Description = get.Required.Field "description" Decode.string
                Version = get.Required.Field "version" Decode.string
                Slug = get.Required.Field "slug" Decode.string
                WebsiteUrl = get.Required.Field "websiteUrl" Decode.string
                RepoUrl = get.Required.Field "repoUrl" Decode.string
            })

type ProcessBlock = {
    Main: bool
    Renderer: bool
    Utility: bool
    Exported: bool
}

module ProcessBlock =
    let decode: Decoder<ProcessBlock> =
        Decode.object (fun get ->
            {
                Main = get.Required.Field "main" Decode.bool
                Renderer = get.Required.Field "renderer" Decode.bool
                Utility = get.Required.Field "utility" Decode.bool
                Exported = get.Required.Field "exported" Decode.bool
            })

type ModuleDocumentationContainer = {
    Process: ProcessBlock
    Methods: MethodDocumentationBlock[]
    Events: EventDocumentationBlock[]
    Properties: PropertyDocumentationBlock[]
    ExportedClasses: ClassDocumentationContainer[]
    // Embedded
    BaseDocumentationContainer: BaseDocumentationContainer
}

module ModuleDocumentationContainer =
    let decode: Decoder<ModuleDocumentationContainer> =
        Decode.object (fun get ->
            get.Required.Field "type" Decode.string
            |> function
                | "Module" ->
                    {
                        Process = get.Required.Field "process" ProcessBlock.decode
                        Methods =
                            Decode.array MethodDocumentationBlock.decode
                            |> get.Required.Field "methods"
                        Events =
                            Decode.array EventDocumentationBlock.decode
                            |> get.Required.Field "events"
                        Properties =
                            Decode.array PropertyDocumentationBlock.decode
                            |> get.Required.Field "properties"
                        ExportedClasses =
                            Decode.array ClassDocumentationContainer.decode
                            |> get.Required.Field "exportedClasses"
                        BaseDocumentationContainer =
                            get.Required.Raw BaseDocumentationContainer.decode
                    }
                | _ ->
                    Decode.string
                    |> Decode.andThen Decode.fail
                    |> get.Required.Field "type"
        )
    
type StructureDocumentationContainer = {
    Properties: PropertyDocumentationBlock[]
    // Embedded
    BaseDocumentationContainer: BaseDocumentationContainer
}

module StructureDocumentationContainer =
    let decode: Decoder<StructureDocumentationContainer> =
        Decode.object (fun get ->
            get.Required.Field "type" Decode.string
            |> function
                | "Structure" ->
                    {
                        Properties =
                            Decode.array PropertyDocumentationBlock.decode
                            |> get.Required.Field "properties"
                        BaseDocumentationContainer =
                            get.Required.Raw BaseDocumentationContainer.decode
                    }
                | _ ->
                    Decode.string
                    |> Decode.andThen Decode.fail
                    |> get.Required.Field "type"
            )

type ConstructorMethod = {
    Signature: string
    Parameters: MethodParameterDocumentation[]
}
module ConstructorMethod =
    let decode: Decoder<ConstructorMethod> =
        Decode.object (fun get ->
            {
                Signature = get.Required.Field "signature" Decode.string
                Parameters =
                    Decode.array MethodParameterDocumentation.decode
                    |> get.Required.Field "parameters"
            })
type ClassDocumentationContainer = {
    Process: ProcessBlock
    ConstructorMethod: ConstructorMethod option
    InstanceName: string
    StaticMethods: MethodDocumentationBlock[]
    StaticProperties: PropertyDocumentationBlock[]
    InstanceMethods: MethodDocumentationBlock[]
    InstanceEvents: EventDocumentationBlock[]
    InstanceProperties: PropertyDocumentationBlock[]
    // embedded
    BaseDocumentationContainer: BaseDocumentationContainer
}
module ClassDocumentationContainer =
    let decode: Decoder<ClassDocumentationContainer> =
        Decode.object (fun get ->
            get.Required.Field "type" Decode.string
            |> function
                | "Class" ->
                    {
                        Process = get.Required.Field "process" ProcessBlock.decode
                        ConstructorMethod =
                            Decode.option ConstructorMethod.decode
                            |> get.Required.Field "constructorMethod"
                        InstanceName = get.Required.Field "instanceName" Decode.string
                        StaticMethods =
                            Decode.array MethodDocumentationBlock.decode
                            |> get.Required.Field "staticMethods"
                        StaticProperties =
                            Decode.array PropertyDocumentationBlock.decode
                            |> get.Required.Field "staticProperties"
                        InstanceMethods =
                            Decode.array MethodDocumentationBlock.decode
                            |> get.Required.Field "instanceMethods"
                        InstanceEvents =
                            Decode.array EventDocumentationBlock.decode
                            |> get.Required.Field "instanceEvents"
                        InstanceProperties =
                            Decode.array PropertyDocumentationBlock.decode
                            |> get.Required.Field "instanceProperties"
                        BaseDocumentationContainer = get.Required.Raw BaseDocumentationContainer.decode
                    }
                | _ ->
                    Decode.string
                    |> Decode.andThen Decode.fail
                    |> get.Required.Field "type"
            )

type ElementDocumentationContainer = {
    Process: ProcessBlock
    Methods: MethodDocumentationBlock[]
    Events: EventDocumentationBlock[]
    Properties: PropertyDocumentationBlock[]
    BaseDocumentationContainer: BaseDocumentationContainer
}

module ElementDocumentationContainer =
    let decode: Decoder<ElementDocumentationContainer> =
        Decode.object (fun get ->
            get.Required.Field "type" Decode.string
            |> function
                | "Element" ->
                    {
                        Process = get.Required.Field "process" ProcessBlock.decode
                        Methods =
                            Decode.array MethodDocumentationBlock.decode
                            |> get.Required.Field "methods"
                        Events =
                            Decode.array EventDocumentationBlock.decode
                            |> get.Required.Field "events"
                        Properties =
                            Decode.array PropertyDocumentationBlock.decode
                            |> get.Required.Field "properties"
                        BaseDocumentationContainer = get.Required.Raw BaseDocumentationContainer.decode
                    }
                | _ ->
                    Decode.string
                    |> Decode.andThen Decode.fail
                    |> get.Required.Field "type"
            )

type ParsedDocumentation =
    | Module of ModuleDocumentationContainer
    | Class of ClassDocumentationContainer
    | Structure of StructureDocumentationContainer
    | Element of ElementDocumentationContainer

module ParsedDocumentation =
    let decode: Decoder<ParsedDocumentation> =
        Decode.oneOf [
            ModuleDocumentationContainer.decode
            |> Decode.map Module
            
            ClassDocumentationContainer.decode
            |> Decode.map Class
            
            StructureDocumentationContainer.decode
            |> Decode.map Structure
            
            ElementDocumentationContainer.decode
            |> Decode.map Element
        ]
        
type ParsedDocumentationResults = ParsedDocumentation[]

let decode: Decoder<ParsedDocumentationResults> = Decode.array ParsedDocumentation.decode
