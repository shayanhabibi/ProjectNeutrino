namespace ElectronApi.Json.Parser
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Spec =
    let [<Literal>] rootNamespace = "Fable.Electron"
    let [<Literal>] osWinDefine = "ELECTRON_OS_WIN"
    let [<Literal>] osMasDefine = "ELECTRON_OS_MAS"
    let [<Literal>] osMacDefine = "ELECTRON_OS_MAC"
    let [<Literal>] osLinDefine = "ELECTRON_OS_LIN"
    
    /// <summary>
    /// There exists a union which takes more than 9 types, and they are all touchbar classes.
    /// For this reason, we define our own union type with an erase to satisfy this.
    /// </summary>
    let [<Literal>] touchBarItemsName = "TouchBarItem"
    let [<Literal>] eventEmitterName = "EventEmitter"
    [<AutoOpen>]
    module private Helpers =
        let makeTextNode = fun text -> SingleTextNode(text, Range.Zero)
        let makeIdentListNode =
            fun text -> IdentListNode([IdentifierOrDot.Ident(makeTextNode text)], Range.Zero)
        let makeAttributeNode =
            fun text -> AttributeListNode(makeTextNode "[<", [
                AttributeNode(
                    makeIdentListNode text,
                    None,
                    None,
                    Range.Zero
                    )
            ], makeTextNode ">]", Range.Zero)
    let touchBarItemsDef =
        let touchBarItems = [
            "Button"
            "ColorPicker"
            "Group"
            "Label"
            "Popover"
            "Scrubber"
            "SegmentedControl"
            "Slider"
            "Spacer"
        ]
        let attributes = MultipleAttributeListNode([
            makeAttributeNode "Erase"
        ], Range.Zero)
        let typeNameNode = TypeNameNode(
            None, Some attributes,
            makeTextNode "type", None,
            makeIdentListNode touchBarItemsName,
            None, [], None,
            Some (makeTextNode "="),
            None, Range.Zero
            )
        TypeDefnUnionNode(
            typeNameNode,
            None,
            [
                let makeCaseNode =
                    fun text -> UnionCaseNode(
                        None,
                        None,
                        Some (makeTextNode "|"),
                        makeTextNode text, [
                            FieldNode(
                                None, None, None, None, None, None,
                                Type.Anon(makeTextNode ("TouchBar" + text)),
                                Range.Zero
                                )
                        ], Range.Zero
                        )
                yield! touchBarItems |> List.map makeCaseNode
            ],
            [], Range.Zero
            )
        |> TypeDefn.Union
