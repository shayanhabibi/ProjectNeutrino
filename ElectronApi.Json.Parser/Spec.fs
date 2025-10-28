namespace ElectronApi.Json.Parser
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

type Spec =
    static member val rootNamespace = "Fable.Electron" with get,set
module Spec =
    let [<Literal>] osWinDefine = "ELECTRON_OS_WIN"
    let [<Literal>] osMasDefine = "ELECTRON_OS_MAS"
    let [<Literal>] osMacDefine = "ELECTRON_OS_MAC"
    let [<Literal>] osLinDefine = "ELECTRON_OS_LIN"

type Injections =
    /// <summary>
    /// There exists a union which takes more than 9 types, and they are all touchbar classes.
    /// For this reason, we define our own union type with an erase to satisfy this.
    /// </summary>
    static member touchBarItemsName = "TouchBarItems"
    static member touchBarItemsDef =
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
        
        let attributes = MultipleAttributeListNode([
            makeAttributeNode "Erase"
        ], Range.Zero)
        let typeNameNode = TypeNameNode(
            None, Some attributes,
            makeTextNode "type", None,
            makeIdentListNode Injections.touchBarItemsName,
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
        |> ModuleDecl.TypeDefn
    static member header = "namespace " + Spec.rootNamespace + """
open System.Runtime.CompilerServices
open System.ComponentModel
open Fable.Core
"""
    static member eventEmitterName = "EventEmitter"
    /// <summary>
    /// The definition for the Node.JS EventEmitter class is defined within this snippet.
    /// </summary>
    static member eventEmitterDefinition = """
type EventEmitter = interface end
[<AutoOpen; EditorBrowsable(EditorBrowsableState.Never)>]
module AutoOpenExtensions =
    type EventEmitter with
        [<Emit("$0.eventNames()")>]
        member this.eventNames(): string[] = Unchecked.defaultof<_>
        [<Emit("$0.getMaxListeners()")>]
        member this.getMaxListeners(): int = Unchecked.defaultof<_>
        [<Emit("$0.listenerCount($1{{, $2}})")>]
        member inline this.listenerCount(eventName: string, ?listener: FSharpFunc<_, _>): int = Unchecked.defaultof<_>
        [<Emit("$0.listeners($1)")>]
        member inline this.listeners(eventName: string): obj[] = Unchecked.defaultof<_>
        [<Emit("$0.rawListeners($1)")>]
        member inline this.rawListeners(eventName: string): obj[] = Unchecked.defaultof<_>
    [<Erase; AutoOpen; EditorBrowsable(EditorBrowsableState.Never)>]
    type Extensions =
        [<Extension; Emit("$0.setMaxListeners($1)")>]
        static member inline setMaxListeners<'EventEmitter when 'EventEmitter :> EventEmitter>(this: 'EventEmitter, n: int): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.emit($1, ...$2)")>]
        static member inline emit<'EventEmitter when 'EventEmitter :> EventEmitter>(this: 'EventEmitter, event: string, [<System.ParamArray>] args: obj[]): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.on($1, $2)")>]
        static member inline on< 'EventEmitter, 'Arg when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.on($1, $2)")>]
        static member inline on< 'EventEmitter, 'Arg1, 'Arg2 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.on($1, $2)")>]
        static member inline on< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.on($1, $2)")>]
        static member inline on< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3, 'Arg4 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> 'Arg4 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.on($1, $2)")>]
        static member inline on< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3, 'Arg4, 'Arg5 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> 'Arg4 -> 'Arg5 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.on($1, $2)")>]
        static member inline on< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3, 'Arg4, 'Arg5, 'Arg6 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> 'Arg4 -> 'Arg5 -> 'Arg6 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.on($1, $2)")>]
        static member inline on< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3, 'Arg4, 'Arg5, 'Arg6, 'Arg7 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> 'Arg4 -> 'Arg5 -> 'Arg6 -> 'Arg7 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        
        [<Extension; Emit("$0.off($1, $2)")>]
        static member inline off< 'EventEmitter, 'Arg when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.off($1, $2)")>]
        static member inline off< 'EventEmitter, 'Arg1, 'Arg2 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.off($1, $2)")>]
        static member inline off< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.off($1, $2)")>]
        static member inline off< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3, 'Arg4 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> 'Arg4 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.off($1, $2)")>]
        static member inline off< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3, 'Arg4, 'Arg5 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> 'Arg4 -> 'Arg5 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.off($1, $2)")>]
        static member inline off< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3, 'Arg4, 'Arg5, 'Arg6 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> 'Arg4 -> 'Arg5 -> 'Arg6 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.off($1, $2)")>]
        static member inline off< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3, 'Arg4, 'Arg5, 'Arg6, 'Arg7 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> 'Arg4 -> 'Arg5 -> 'Arg6 -> 'Arg7 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>

        [<Extension; Emit("$0.once($1, $2)")>]
        static member inline once< 'EventEmitter, 'Arg when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.once($1, $2)")>]
        static member inline once< 'EventEmitter, 'Arg1, 'Arg2 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.once($1, $2)")>]
        static member inline once< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.once($1, $2)")>]
        static member inline once< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3, 'Arg4 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> 'Arg4 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.once($1, $2)")>]
        static member inline once< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3, 'Arg4, 'Arg5 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> 'Arg4 -> 'Arg5 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.once($1, $2)")>]
        static member inline once< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3, 'Arg4, 'Arg5, 'Arg6 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> 'Arg4 -> 'Arg5 -> 'Arg6 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.once($1, $2)")>]
        static member inline once< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3, 'Arg4, 'Arg5, 'Arg6, 'Arg7 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> 'Arg4 -> 'Arg5 -> 'Arg6 -> 'Arg7 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>

        [<Extension; Emit("$0.prependListener($1, $2)")>]
        static member inline prependListener< 'EventEmitter, 'Arg when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.prependListener($1, $2)")>]
        static member inline prependListener< 'EventEmitter, 'Arg1, 'Arg2 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.prependListener($1, $2)")>]
        static member inline prependListener< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.prependListener($1, $2)")>]
        static member inline prependListener< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3, 'Arg4 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> 'Arg4 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.prependListener($1, $2)")>]
        static member inline prependListener< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3, 'Arg4, 'Arg5 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> 'Arg4 -> 'Arg5 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.prependListener($1, $2)")>]
        static member inline prependListener< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3, 'Arg4, 'Arg5, 'Arg6 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> 'Arg4 -> 'Arg5 -> 'Arg6 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.prependListener($1, $2)")>]
        static member inline prependListener< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3, 'Arg4, 'Arg5, 'Arg6, 'Arg7 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> 'Arg4 -> 'Arg5 -> 'Arg6 -> 'Arg7 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        
        [<Extension; Emit("$0.prependOnceListener($1, $2)")>]
        static member inline prependOnceListener< 'EventEmitter, 'Arg when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.prependOnceListener($1, $2)")>]
        static member inline prependOnceListener< 'EventEmitter, 'Arg1, 'Arg2 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.prependOnceListener($1, $2)")>]
        static member inline prependOnceListener< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.prependOnceListener($1, $2)")>]
        static member inline prependOnceListener< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3, 'Arg4 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> 'Arg4 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.prependOnceListener($1, $2)")>]
        static member inline prependOnceListener< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3, 'Arg4, 'Arg5 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> 'Arg4 -> 'Arg5 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.prependOnceListener($1, $2)")>]
        static member inline prependOnceListener< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3, 'Arg4, 'Arg5, 'Arg6 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> 'Arg4 -> 'Arg5 -> 'Arg6 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.prependOnceListener($1, $2)")>]
        static member inline prependOnceListener< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3, 'Arg4, 'Arg5, 'Arg6, 'Arg7 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> 'Arg4 -> 'Arg5 -> 'Arg6 -> 'Arg7 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        
        [<Extension; Emit("$0.removeListener($1, $2)")>]
        static member inline removeListener< 'EventEmitter, 'Arg when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.removeListener($1, $2)")>]
        static member inline removeListener< 'EventEmitter, 'Arg1, 'Arg2 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.removeListener($1, $2)")>]
        static member inline removeListener< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.removeListener($1, $2)")>]
        static member inline removeListener< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3, 'Arg4 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> 'Arg4 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.removeListener($1, $2)")>]
        static member inline removeListener< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3, 'Arg4, 'Arg5 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> 'Arg4 -> 'Arg5 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.removeListener($1, $2)")>]
        static member inline removeListener< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3, 'Arg4, 'Arg5, 'Arg6 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> 'Arg4 -> 'Arg5 -> 'Arg6 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>
        [<Extension; Emit("$0.removeListener($1, $2)")>]
        static member inline removeListener< 'EventEmitter, 'Arg1, 'Arg2, 'Arg3, 'Arg4, 'Arg5, 'Arg6, 'Arg7 when 'EventEmitter :> EventEmitter >(
                this: 'EventEmitter,
                event: string,
                [<InlineIfLambda>] handler: 'Arg1 -> 'Arg2 -> 'Arg3 -> 'Arg4 -> 'Arg5 -> 'Arg6 -> 'Arg7 -> unit
            ): 'EventEmitter = Unchecked.defaultof<_>"""
