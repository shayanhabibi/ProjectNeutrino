module ElectronApi.Json.Parser.Fantomas
open Fantomas.Core.SyntaxOak
open Fantomas.Core
open Fantomas.FCS
open Fantomas.FCS.Text
/// <summary>
/// Provides extensions for Oak nodes to create nodes given simple primitive types
/// </summary>
[<AutoOpen>]
module Utils =
    type SingleTextNode with
        static member make text = SingleTextNode(text, Range.Zero)
        /// text -> ?text
        static member makeOptional (text: string) = SingleTextNode.make $"?{text}"
        /// text -> ?text
        member this.MakeOptional with get() =
            this.Text |> SingleTextNode.makeOptional
    type MultipleTextsNode with
        static member make texts = MultipleTextsNode([ for text in texts do SingleTextNode.make text ], Range.Zero)
        static member make text = SyntaxOak.MultipleTextsNode.make [ text ]
    type IdentifierOrDot with
        static member makeIdent text = IdentifierOrDot.Ident <| SingleTextNode.make text
        static member makeDot with get() = IdentifierOrDot.KnownDot <| SingleTextNode.make "."
        /// This.That -> IdentifierOrDotList [ This; KnownDot; That ]
        static member ListFromString (text: string) =
            text.Split '.'
            |> Array.map IdentifierOrDot.makeIdent
            |> fun arr ->
                let length = arr.Length
                arr
                |> Array.mapi (fun idx ident ->
                        if idx = length then
                            [ ident ]
                        else
                            [ ident; IdentifierOrDot.makeDot ]
                    )
            |> Array.toList
            |> List.collect id
    type IdentListNode with
        static member make (text: string) = IdentListNode([ IdentifierOrDot.makeIdent text ], Range.Zero)
        static member make (text: string list) = IdentListNode(
            text
            |> List.collect( fun text ->
                [ IdentifierOrDot.makeIdent text
                  IdentifierOrDot.makeDot ]
                )
            |> List.cutOffLast,
            Range.Zero
            )
        member this.identList with get() =
            this.Content
            |> List.choose (function
                | IdentifierOrDot.Ident ident ->
                    ident.Text
                    |> Some
                | _ -> None
                )
        member this.firstIdent with get() =
            this.identList |> List.head
        member this.lastIdent with get() =
            this.identList |> List.last
        member this.isStropped = this.firstIdent |> _.StartsWith('`')
        member this.isKebab =
            this.Content
            |> List.exists(function
                | IdentifierOrDot.Ident ident ->
                    ident.Text.Contains('-')
                | IdentifierOrDot.KnownDot ident ->
                    ident.Text.Contains('-')
                | _ -> false
                )
        member this.toCamelCase =
            this.Content
            |> List.map(function
                | IdentifierOrDot.Ident ident
                | IdentifierOrDot.KnownDot ident ->
                    ident.Text
                | _ -> ""
                    )
            |> List.toArray
            |> String.concat ""
            |> _.Trim('`')
            |> _.Split('-')
            |> Array.mapi(
                fun i str ->
                    if i = 0 then str
                    else
                    let chars = str.ToCharArray()
                    chars[0] <- chars[0] |> System.Char.ToUpperInvariant
                    chars |> string
                )
            |> String.concat ""
    type AttributeNode with
        static member make text =
            AttributeNode( IdentListNode.make text, None, None, Range.Zero )
    type AttributeListNode with
        static member makeOpening with get() = SingleTextNode.make "[<"
        static member makeClosing with get() = SingleTextNode.make ">]"
        static member make texts =
            AttributeListNode(
                AttributeListNode.makeOpening,
                [ for text in texts do AttributeNode.make text ],
                AttributeListNode.makeClosing,
                Range.Zero
                )
        static member make text =
            AttributeListNode.make [ text ]
    type MultipleAttributeListNode with
        static member make (texts: string seq seq) =
            MultipleAttributeListNode([ for text in texts do AttributeListNode.make text ], Range.Zero)
        static member make (text: string seq)= MultipleAttributeListNode.make [ text ]
        static member make (text: string)= MultipleAttributeListNode.make [ text ]
    type PatParenNode with
        static member makeOpening with get() = SingleTextNode.make "("
        static member makeClosing with get() = SingleTextNode.make ")"
        static member make pattern = PatParenNode(PatParenNode.makeOpening, pattern, PatParenNode.makeClosing, Range.Zero)
    type PatParameterNode with
        static member make ident typ = PatParameterNode(None, Pattern.Null(ident), typ, Range.Zero)
    type PatTupleNode with
        static member makeItem (node: SingleTextNode): Choice<Pattern, SingleTextNode> = Choice2Of2 node
        static member makeItem (node: Pattern): Choice<Pattern, SingleTextNode> = Choice1Of2 node
        static member makeConstructor (members: (SingleTextNode * Type) list) =
            PatTupleNode(
                [ for ident,typ in members do
                    yield Pattern.Parameter(PatParameterNode.make ident.MakeOptional (Some typ)) |> PatTupleNode.makeItem
                    yield SingleTextNode.make "," |> PatTupleNode.makeItem
                ] |> List.cutOffLast, Range.Zero
                )
    type MemberDefnInheritNode with
        /// Gets the last identifier in a chain which should be the type without accessors
        member this.tryGetIdentifier with get() =
            let rec getIdent: Node -> string option = function
                | :? IdentListNode as ident ->
                    ident.lastIdent
                    |> Some
                | :? TypeAppPrefixNode as node ->
                    getIdent node.Children[0]
                | _ -> None
            this.Children[1] |> getIdent
    type ITypeDefn with
        member this.getInheritMembers with get() =
            this.Members
            |> List.choose (function MemberDefn.Inherit node -> Some node | _ -> None)
        member this.getInterfaceMembers with get() =
            this.Members
            |> List.choose (function MemberDefn.Interface node -> Some node | _ -> None)
        member this.getAbstractMembers with get() =
            this.Members
            |> List.choose(function MemberDefn.AbstractSlot node -> Some node | _ -> None)
        member this.getIdentifier = this.TypeName.Identifier.lastIdent
        member this.getAttributes = this.TypeName.Attributes
        member this.getInheritMembersIdentifiers with get() =
            this.getInheritMembers
            |> List.map _.tryGetIdentifier
            |> List.choose id
    type TypeDefnRegularNode with
        member this.TypeDefn = this :> ITypeDefn
    type XmlDocNode with
        static member make (docs: string seq) =
            XmlDocNode([|
                yield "/// <summary>"
                for line in docs do
                    for subline in
                        line |> Seq.chunkBySize 120
                        |> Seq.map (string >> sprintf "/// %s")
                        do yield subline
                yield "/// </summary>"
            |], Range.Zero)
    type MemberDefnInheritNode with
        static member make identifier =
            MemberDefnInheritNode(
                SingleTextNode.make "inherit",
                Type.LongIdent (IdentListNode.make identifier),
                Range.Zero
                )
    type MemberDefnAbstractSlotNode with
        member this.getIdentifierTypeTuple =
            this.Identifier.Text, this.Type
        static member makeSimple(
            identifier: string,
            typ: Type,
            ?docs: string seq,
            ?attributes: string seq,
            ?withGetSet: bool
            ) =
            let withGetSet = defaultArg withGetSet false
            MemberDefnAbstractSlotNode(
                docs |> Option.map XmlDocNode.make,
                attributes |> Option.map MultipleAttributeListNode.make,
                MultipleTextsNode.make ["abstract"; "member"],
                SingleTextNode.make identifier,
                None,
                typ,
                if withGetSet then
                    MultipleTextsNode.make [ "with"; "get,"; "set" ]
                    |> Some
                else None
                ,Range.Zero
                )
    type MemberDefn with
        static member makeInherit identifier =
            MemberDefnInheritNode.make identifier |> MemberDefn.Inherit
        static member makeAbstract (attrName: string, typ: Type) =
            MemberDefnAbstractSlotNode.makeSimple(attrName, typ)
            |> MemberDefn.AbstractSlot
        static member makeExtensionGetSetWith (name: string, typ: Type, ?inlineOverload: string) =
            MemberDefnPropertyGetSetNode(
                None,Some(MultipleAttributeListNode.make "Erase"),MultipleTextsNode.make "member",None,None,
                IdentListNode.make $"_.{name}", SingleTextNode.make "with",
                PropertyGetSetBindingNode(
                      None, None, None, SingleTextNode.make "set", [
                          Pattern.Parameter(PatParameterNode.make (SingleTextNode.make "_") (Some typ))
                          |> PatParenNode.make
                          |> Pattern.Paren
                      ], None, SingleTextNode.make "=", Expr.Null(SingleTextNode.make "()"), Range.Zero
                    ),
                SingleTextNode.make "and" |> Some,
                PropertyGetSetBindingNode(
                    None,
                    MultipleAttributeListNode.make "Erase" |> Some,
                    None, SingleTextNode.make "get", [
                        Pattern.Unit(UnitNode(PatParenNode.makeOpening, PatParenNode.makeClosing, Range.Zero))
                    ],
                    BindingReturnInfoNode(SingleTextNode.make ":", typ, Range.Zero)
                    |> Some,
                    SingleTextNode.make "=",
                    Expr.Ident(SingleTextNode.make "JS.undefined"),
                    Range.Zero
                    ) |> Some,
                Range.Zero
                ) |> MemberDefn.PropertyGetSet
        static member makeExtensionGetSet (name: string) (typ: Type) =
            MemberDefn.makeExtensionGetSetWith(name, typ)
    type TypeNameNode with
        static member makeSimple (
            identifier: string,
            ?docs: string list,
            ?attributes: string list,
            ?suffix: string
            ) =
            TypeNameNode(
                docs |> Option.map XmlDocNode.make,
                attributes |> Option.map MultipleAttributeListNode.make,
                SingleTextNode.make "type",
                None,
                IdentListNode.make identifier,
                None,
                [],
                None,
                SingleTextNode.make "=" |> Some,
                suffix |> Option.map SingleTextNode.make,
                Range.Zero
                )
        static member makeExtension (identifier: IdentListNode) =
            TypeNameNode(None,None,SingleTextNode.make "type", None,
                         identifier, None, [], None,
                         SingleTextNode.make "with" |> Some, None, Range.Zero)
        static member makeExtension (identifier: string) =
            IdentListNode.make identifier |> TypeNameNode.makeExtension
    type TypeDefnRegularNode with
        static member make members typeNameNode =
            TypeDefnRegularNode(typeNameNode, members, Range.Zero)
    type ModuleDecl with
        static member wrapInNestedModule name (attributes: string seq option) decls =
            NestedModuleNode(
                None, attributes |> Option.map MultipleAttributeListNode.make,
                SingleTextNode.make "module",None,false,IdentListNode.make name,
                SingleTextNode.make "=", decls, Range.Zero
                )
            |> ModuleDecl.NestedModule
    type ExprQuoteNode with
        static member make text =
            ExprQuoteNode(
                openToken = SingleTextNode.make "\"",
                expr = Expr.Constant (Constant.FromText (SingleTextNode.make text)),
                closeToken = SingleTextNode.make "\"",
                range = Range.Zero)
    type Expr with
        static member makeString text =
            ExprQuoteNode.make text |> Expr.Quote
    type TypeAppPrefixNode with
        static member Create(identifier, postIdentifier, types) =
            TypeAppPrefixNode(
                identifier,
                postIdentifier,
                SingleTextNode.make "<",
                types,
                SingleTextNode.make ">",
                Range.Zero
            )
        static member Create(identifier, types) = TypeAppPrefixNode.Create(identifier, None, types)
            
