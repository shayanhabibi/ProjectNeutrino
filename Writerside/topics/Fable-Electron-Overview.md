# Fable.Electron Overview

The <code>Fable.Electron</code> bindings are automatically generated from the same source
material as the TypeScript type files, with some features for usage from F# and Fable.

# Features

## Event Abstractions

Events can be listened to the same way as the source material

<code-block lang="js">
emitter.on('event-name', () => { return null })
</code-block>

But you are also provided abstractions which convert these into IDE explorable methods

<code-block lang="F#">
emitter.onEventName(fun () -> ())
</code-block>

These abstractions are provided for the most common event patterns, which is <code>on</code>,
<code>once</code>, and <code>off</code>.

### Event Literals

In the event that you want, or need, to utilise the standard method, you can also find helpful
predefined literals to the event names that are IDE explorable.

<code-block lang="F#">
// Renderer related
module Renderer =
    // Constants (event names)
    module Constants =
        module WebviewTag =
            // Pascal case name
            [&lt;Literal; Erase&gt;]
            let DevtoolsSearchQuery = "devtools-search-query"

            [&lt;Literal; Erase&lt;]
            let IpcMessage = "ipc-message"

</code-block>

## Callback Interfaces

Where we have a callback signature for a parameter such as <code>(event: Event, senderId: number): unit</code>, in Fable we would typically create a curried signature such as

<code-block lang="F#">
type Emitter =
  static member callbackExample (handler: Event -> float -> unit): unit
</code-block>

This creates a scenario where there is a loss of information in the name of the parameters,
and an inability to provide documentation for each parameter.

In this case, we automatically generate an interface which provides named access to the fields,
along with their documentation. These interfaces are hidden from IDEs to prevent polluting
the namespace.

<code-block lang="F#" collapsible="true" default-state="collapsed" collapsed-title="Example Generated Interface">
/// &lt;summary&gt;
/// Emitted when any frame navigation is done.&lt;br/&gt;&lt;br/&gt; This event is not emitted for in-page navigations, such as clicking anchor links or
/// updating the &lt;c&gt;window.location.hash&lt;/c&gt;, Use &lt;c&gt;did-navigate-in-page&lt;/c&gt; event for this purpose.
/// &lt;/summary&gt;
[&lt;System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
  AllowNullLiteral;
  Interface&gt;]
type IOnDidFrameNavigate =
    [&lt;Emit("$0[0]")&gt;]
    abstract member url: string with get, set

    /// &lt;summary&gt;
    /// -1 for non HTTP navigations
    /// &lt;/summary&gt;
    [&lt;Emit("$0[1]")&gt;]
    abstract member httpResponseCode: int with get, set

    /// &lt;summary&gt;
    /// empty for non HTTP navigations,
    /// &lt;/summary&gt;
    [&lt;Emit("$0[2]")&gt;]
    abstract member httpStatusText: string with get, set

    [&lt;Emit("$0[3]")&gt;]
    abstract member isMainFrame: bool with get, set

    [&lt;Emit("$0[4]")&gt;]
    abstract member frameProcessId: int with get, set

    [&lt;Emit("$0[5]")&gt;]
    abstract member frameRoutingId: int with get, set
</code-block>

We then provide the original binding two overloads; one with the typical Fable curried syntax, and one with the interface.

This coincides with the stipulation that the interface is only generated in the event that the
lambda has more than 1 argument/parameter.

> Notice that we use index access for the parameters. In the event that you have a greedy
> parameter using the triple dot syntax such as <code>...args</code>, then we emit a slice
> from that parameter index.

## String Enums

Like previous bindings, fieldless unions are used to represent string enums from JS.

<code-block lang="F#">
[&lt;StringEnum(CaseRules.None); RequireQualifiedAccess&gt;]
type MediaType =
    | [&lt;CompiledName("none")&gt;] None
    | [&lt;CompiledName("image")&gt;] Image
    | [&lt;CompiledName("audio")&gt;] Audio
    | [&lt;CompiledName("video")&gt;] Video
    | [&lt;CompiledName("canvas")&gt;] Canvas
    | [&lt;CompiledName("file")&gt;] File
    | [&lt;CompiledName("plugin")&gt;] Plugin
</code-block>

There are cases where the <tooltip term="electron-api">electron-api document</tooltip>
does not popoulate the fields for the values of an enum, and only includes it in the
documentation.

This can't really be managed safely by us in the generator. The onus is on the user to either
create a helper using the values suggested by the documentation of the parameter, or
use strings in coordination with the generated docs or the electron source docs themselves.

## Inlined Objects

Consider a common method typing from TypeScript where a parameter takes an object as options.
Where it is safe to do so, we automatically inline the properties of the object as optional
parameters/arguments of the method, and use the Fable attribute <code>ParamObject</code> to
have it compile the option object.


<code-block lang="F#">
/// &lt;summary&gt;
/// Creates a new &lt;c&gt;NativeImage&lt;/c&gt; instance from &lt;c&gt;buffer&lt;/c&gt;. Tries to decode as PNG or JPEG first.
/// &lt;/summary&gt;
/// &lt;param name="buffer"&gt;&lt;/param&gt;
/// &lt;param name="width"&gt;Required for bitmap buffers.&lt;/param&gt;
/// &lt;param name="height"&gt;Required for bitmap buffers.&lt;/param&gt;
/// &lt;param name="scaleFactor"&gt;Defaults to 1.0.&lt;/param&gt;
[&lt;Erase; ParamObject(1)&gt;]
static member inline createFromBuffer
    (buffer: Buffer, ?width: int, ?height: int, ?scaleFactor: float)
    : Main.NativeImage =
    Unchecked.defaultof&lt;_&gt;
</code-block>

> Notice that the documentation of the properties that are inlined are also lifted.

Where this is not possible, a class is created to allow the optional assignment of properties
in a type safe manner.

## Electron Deprecated/Experimental tagging

Where the bindings indicate an obsolete or experimental API, this is a direct reflection from
the Electron source material.

There is usually a reason provided in the comments of the API itself, but not always.

## OS Compatibility

There are a variety of electron APIs that are only available on specific operating systems.
We provide a small header in the comments where this is indicated

<code-block lang="F#">
/// &lt;summary&gt;
/// &lt;para&gt;
/// ⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌
/// &lt;/para&gt;
/// &lt;/summary&gt;
[&lt;Emit("$0.on('accent-color-changed', $1)"); Import("systemPreferences", "electron")&gt;]
static member inline onAccentColorChanged(handler: Event -&gt; string -&gt; unit) : unit = Unchecked.defaultof&lt;_&gt;
</code-block>

We also wrap these in conditional directives, allowing the visibility of incompatible APIs to be
hidden from the IDE completely.

This is purely opt in, via a constant definition in your project files.

<code-block lang="F#">
#if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
/// &lt;summary&gt;
/// &lt;para&gt;
/// ⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌
/// &lt;/para&gt;
/// &lt;/summary&gt;
[&lt;Emit("$0.on('accent-color-changed', $1)"); Import("systemPreferences", "electron")&gt;]
static member inline onAccentColorChanged(handler: Event -&gt; string -&gt; unit) : unit = Unchecked.defaultof&lt;_&gt;
#endif
</code-block>

<warning>
If you have no target OS set, then all of the API is compiled.

If you have a target OS set, then incompatible API will not be compiled/included.
</warning>

It also would provide compiler safety when targetting multiple platforms, as it forces you to satisfy the compiler for the various conditions using your own conditional compilation.

Currently, we use the following definition names 
* <code>ELECTRON_OS_LIN</code> for linux 
* <code>ELECTRON_OS_WIN</code> for windows 
* <code>ELECTRON_OS_MAC</code> for MacOS
* <code>ELECTRON_OS_MAS</code> for mas

## Process Availability as Modules

If you follow the Electron guidelines, you will respect the separation of API availabilities by
the default configuration of sandboxing Renderer processes.

To assist with this, the API is wrapped in modules to help restrict the API you have available,
while also providing the process compatibility in the documentation (similar to OS compatibility
comments).

<code-block lang="F#">
open Fable.Electron.Main
open Fable.Electron.Renderer
open Fable.Electron.Utility
</code-block>

Where an API is available in multiple processes, then we duplicate them to both (as this provides
the best IDE suggestion experience; for example, abbrev types do not show the inherited members
when exploring the definition of the type).

However, due to the current nature of our path resolution system, we may end up referencing the
type from a different process within a type signature, in the scenario where it exists in both.

There is no effect on end outcomes, and this is an issue that will be resolved at a later time.

<note>
Alternatively, you can utilise separate <code>.fsproj</code> files with
dependency on one of the process dependent generations such as
<code>Fable.Electron.Main</code> instead of <code>Fable.Electron</code>.

This is generated and used the same way; the difference being that there
is an intermediate step which filters and remaps the processes of types
to only one process. 

Unfortunately, this affects the generated documentation.
</note>
