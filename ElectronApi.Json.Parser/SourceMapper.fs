module ElectronApi.Json.Parser.SourceMapper

open ElectronApi.Json.Parser.FSharpApi
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open Fantomas.Utils

// 1. ApiDecoder - parses the json directly into F#
// 2. FSharpApi - reads and maps; modifies names and ensures everything is accounted
// 3. SourceMapper - maps the types to their respective end types, including attributes et al

// Classes -> We can use F# classes.
// Modules -> modules.

module StringEnums =
    let cache = ResizeArray<StringEnum>()
    let add stringEnum = cache.Add stringEnum

module XmlDocs =
    let makeClosedSeeAlso (link: string) = $"<seealso href=\"{link}\"/>"
    let makeSeeAlso (link: string) (description: string) =
        $"<seealso href=\"{link}\">{description}</seealso>"
    let makeParam (name: string) (description: string) =
        $"<param name=\"{name}\">{description}</param>"
    module Boundaries =
        let [<Literal>] openRemarks = "<remarks>"
        let [<Literal>] closeRemarks = "</remarks>"
        
        let [<Literal>] openSummary = "<summary>"
        let [<Literal>] closeSummary = "</summary>"
        
        let [<Literal>] openExample = "<example>"
        let [<Literal>] closeExample = "</example>"
        
        let [<Literal>] openCode = "<code>"
        let [<Literal>] closeCode = "</code>"
        
        let [<Literal>] openFSharpCode = "<code lang=\"fsharp\">"
        let [<Literal>] closeFSharpCode = closeCode
        
    let [<Literal>] br = "<br/>"

module Type =
    type internal ApiType = FSharpApi.Type
    type internal FcsType = Fantomas.Core.SyntaxOak.Type
    let inline internal makeSimple text = FcsType.Anon (SingleTextNode.make text)
    type FantomasFactory =
        static member makePromise (apiType: ApiType): FcsType =
            TypeAppPrefixNode(
                identifier = makeSimple "Promise",
                postIdentifier = None,
                lessThan = SingleTextNode.make "<",
                arguments = [],
                greaterThan = SingleTextNode.make ">",
                range = Range.Zero
                )
            |> FcsType.AppPrefix
        static member mapToFantomas (apiType: ApiType): FcsType =
            match apiType with
            | ApiType.Any 
            | FSharpApi.Type.Undefined
            | ApiType.Unknown -> makeSimple "obj"
            | ApiType.Unit -> makeSimple "unit"
            | FSharpApi.Type.Boolean -> makeSimple "bool"
            | FSharpApi.Type.Date -> FcsType.LongIdent(IdentListNode.make [ "System"; "DateTime" ])
            | FSharpApi.Type.Double -> makeSimple "double"
            | FSharpApi.Type.Float | FSharpApi.Type.Number -> makeSimple "float"
            | FSharpApi.Type.Integer -> makeSimple "int"
            | FSharpApi.Type.String -> makeSimple "string"
            | FSharpApi.Type.StructureRef value ->
                Name.retrieveName value
                |> function
                    | ValueNone ->
                        makeSimple value
                    | ValueSome name ->
                        makeSimple name.ValueOrModified

        static member mapToFantomas (context: ^T when ^T : (member Name: Name), apiType: ApiType): FcsType =
            match apiType with
            | FSharpApi.Type.StringEnum stringEnum ->
                match stringEnum with
                | { Name = ValueNone } ->
                    { stringEnum with Name = ValueSome context.Name }
                    |> StringEnums.add
                    // TODO - fix
                    makeSimple context.Name.ValueOrModified
                | { Name = ValueSome name } ->
                    StringEnums.add stringEnum
                    // TODO - fix
                    makeSimple name.ValueOrModified
            | _ -> FantomasFactory.mapToFantomas apiType


type Structure with
    member this.ToPojoNode =
        let requiredProps,optionalProps =
            this.Properties
            |> List.partition _.Required
        let xmlDocs = [
            if this.Description.IsSome then
                XmlDocs.Boundaries.openSummary
                this.Description.Value
                XmlDocs.Boundaries.closeSummary
            if this.WebsiteUrl.IsSome then
                XmlDocs.makeClosedSeeAlso this.WebsiteUrl.Value
            yield!
                requiredProps
                |> List.map ( fun prop ->
                    XmlDocs.makeParam prop.Name.ValueOrModified prop.Description )
            yield!
                optionalProps
                |> List.map ( fun prop ->
                    XmlDocs.makeParam prop.Name.ValueOrModified prop.Description )
        ]
        let xmlDocs =
            XmlDocNode(
                xmlDocs
                |> List.map (sprintf "/// %s")
                |> List.toArray,
                Range.Zero
            )
        let implicitConstructor = ImplicitConstructorNode(
            xmlDoc = Some xmlDocs,
            attributes = None,
            accessibility = None,
            pat = Pattern.Tuple (PatTupleNode(
                [
                    for required in requiredProps do
                        let typ = Type. required.Type
                ],
                Range.Zero
                )),
            self = None,
            range = Range.Zero
            )
        let typeNameNode = TypeNameNode(
            xmlDoc = Some xmlDocs,
            attributes = Some (MultipleAttributeListNode.make "JS.Pojo"),
            leadingKeyword = SingleTextNode.make "type",
            ao = None,
            identifier = IdentListNode.make this.Name.ValueOrModified,
            typeParams = None,
            constraints = [],
            
            )
