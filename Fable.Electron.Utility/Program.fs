namespace rec Fable.Electron

open Fable.Core
open Fable.Core.JS
open Browser.Types
open Node.Buffer
open Node.WorkerThreads
open Node.Base
open Node.Stream
open Fetch

module Constants =
    module UtilityProcess =
        [<Literal; Erase>]
        let Error = "error"

    module SystemPreferences =
        [<Literal; Erase>]
        let AccentColorChanged = "accent-color-changed"

    module ClientRequest =
        [<Literal; Erase>]
        let Redirect = "redirect"

        [<Literal; Erase>]
        let Login = "login"

module ClientRequest =
    /// <summary>
    /// Emitted when the server returns a redirect response (e.g. 301 Moved Permanently). Calling <c>request.followRedirect</c> will continue with the redirection.
    /// If this event is handled, <c>request.followRedirect</c> must be called **synchronously**, otherwise the request will be cancelled.
    /// </summary>
    [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
      AllowNullLiteral;
      Interface>]
    type IOnRedirect =
        [<Emit("$0[0]")>]
        abstract member statusCode: int with get, set

        [<Emit("$0[1]")>]
        abstract member method: string with get, set

        [<Emit("$0[2]")>]
        abstract member redirectUrl: string with get, set

        [<Emit("$0[3]")>]
        abstract member responseHeaders: Record<string, string[]> with get, set

    /// <summary>
    /// Emitted when an authenticating proxy is asking for user credentials.<br/><br/>The <c>callback</c> function is expected to be called back with user
    /// credentials:<br/><br/>* <c>username</c> string<br/>* <c>password</c> string<br/><br/>Providing empty credentials will cancel the request and report an authentication error on the response object:
    /// </summary>
    [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
      AllowNullLiteral;
      Interface>]
    type IOnLogin =
        [<Emit("$0[0]")>]
        abstract member authInfo: ClientRequest.Login.AuthInfo with get, set

        [<Emit("$0[1]")>]
        abstract member callback: ClientRequest.Login.Callback with get, set

module SystemPreferences =
    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_LIN || ELECTRON_OS_WIN
    /// <summary>
    /// <para>⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ✔ | MAS ❌</para>
    /// </summary>
    [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
      AllowNullLiteral;
      Interface>]
    type IOnAccentColorChanged =
        [<Emit("$0[0]")>]
        abstract member event: Event with get, set

        /// <summary>
        /// The new RGBA color the user assigned to be their system accent color.
        /// </summary>
        [<Emit("$0[1]")>]
        abstract member newColor: string with get, set
    #endif


module UtilityProcess =
    /// <summary>
    /// Emitted when the child process needs to terminate due to non continuable error from V8.<br/><br/>No matter if you listen to
    /// the <c>error</c> event, the <c>exit</c> event will be emitted after the child process terminates.
    /// </summary>
    [<Experimental("Indicated to be Experimental by Electron");
      System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
      AllowNullLiteral;
      Interface>]
    type IOnError =
        /// <summary>
        /// Type of error. One of the following values:
        /// </summary>
        [<Emit("$0[0]")>]
        abstract member ``type``: Enums.UtilityProcess.Error.Type with get, set

        /// <summary>
        /// Source location from where the error originated.
        /// </summary>
        [<Emit("$0[1]")>]
        abstract member location: string with get, set

        /// <summary>
        /// <c>Node.js diagnostic report</c>.
        /// </summary>
        [<Emit("$0[2]")>]
        abstract member report: string with get, set

/// <summary>
/// <para>⚠ Process Availability: Main ❌ | Renderer ❌ | Utility ❌ | Exported ❌</para>
/// </summary>
[<Import("UtilityProcess", "electron")>]
type UtilityProcess private () =
    class
    end

    interface EventEmitter

    /// <summary>
    /// Emitted once the child process has spawned successfully.
    /// </summary>
    [<Emit("$0.on('spawn', $1)")>]
    member inline _.onSpawn(handler: unit -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted once the child process has spawned successfully.
    /// </summary>
    [<Emit("$0.once('spawn', $1)")>]
    member inline _.onceSpawn(handler: unit -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted once the child process has spawned successfully.
    /// </summary>
    [<Emit("$0.off('spawn', $1)")>]
    member inline _.offSpawn(handler: unit -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when the child process needs to terminate due to non continuable error from V8.<br/><br/>No matter if you listen to
    /// the <c>error</c> event, the <c>exit</c> event will be emitted after the child process terminates.
    /// </summary>
    [<Emit("$0.on('error', $1)")>]
    member inline _.onError(handler: Enums.UtilityProcess.Error.Type -> string -> string -> unit) : unit =
        Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when the child process needs to terminate due to non continuable error from V8.<br/><br/>No matter if you listen to
    /// the <c>error</c> event, the <c>exit</c> event will be emitted after the child process terminates.
    /// </summary>
    [<Emit("$0.on('error', $1)")>]
    member inline _.onError(handler: UtilityProcess.IOnError -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when the child process needs to terminate due to non continuable error from V8.<br/><br/>No matter if you listen to
    /// the <c>error</c> event, the <c>exit</c> event will be emitted after the child process terminates.
    /// </summary>
    [<Emit("$0.once('error', $1)")>]
    member inline _.onceError(handler: Enums.UtilityProcess.Error.Type -> string -> string -> unit) : unit =
        Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when the child process needs to terminate due to non continuable error from V8.<br/><br/>No matter if you listen to
    /// the <c>error</c> event, the <c>exit</c> event will be emitted after the child process terminates.
    /// </summary>
    [<Emit("$0.once('error', $1)")>]
    member inline _.onceError(handler: UtilityProcess.IOnError -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when the child process needs to terminate due to non continuable error from V8.<br/><br/>No matter if you listen to
    /// the <c>error</c> event, the <c>exit</c> event will be emitted after the child process terminates.
    /// </summary>
    [<Emit("$0.off('error', $1)")>]
    member inline _.offError(handler: Enums.UtilityProcess.Error.Type -> string -> string -> unit) : unit =
        Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when the child process needs to terminate due to non continuable error from V8.<br/><br/>No matter if you listen to
    /// the <c>error</c> event, the <c>exit</c> event will be emitted after the child process terminates.
    /// </summary>
    [<Emit("$0.off('error', $1)")>]
    member inline _.offError(handler: UtilityProcess.IOnError -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted after the child process ends.
    /// </summary>
    [<Emit("$0.on('exit', $1)")>]
    member inline _.onExit(handler: float -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted after the child process ends.
    /// </summary>
    [<Emit("$0.once('exit', $1)")>]
    member inline _.onceExit(handler: float -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted after the child process ends.
    /// </summary>
    [<Emit("$0.off('exit', $1)")>]
    member inline _.offExit(handler: float -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when the child process sends a message using <c>process.parentPort.postMessage()</c>.
    /// </summary>
    [<Emit("$0.on('message', $1)")>]
    member inline _.onMessage(handler: obj -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when the child process sends a message using <c>process.parentPort.postMessage()</c>.
    /// </summary>
    [<Emit("$0.once('message', $1)")>]
    member inline _.onceMessage(handler: obj -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when the child process sends a message using <c>process.parentPort.postMessage()</c>.
    /// </summary>
    [<Emit("$0.off('message', $1)")>]
    member inline _.offMessage(handler: obj -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Send a message to the child process, optionally transferring ownership of zero or more <c>MessagePortMain</c> objects.<br/><br/>For example:
    /// </summary>
    /// <param name="message"></param>
    /// <param name="transfer"></param>
    [<Erase>]
    member inline _.postMessage(message: obj, ?transfer: MessagePortMain[]) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Terminates the process gracefully. On POSIX, it uses SIGTERM but will ensure the process is reaped on exit. This function
    /// returns true if the kill is successful, and false otherwise.
    /// </summary>
    [<Erase>]
    member inline _.kill() : bool = Unchecked.defaultof<_>

    /// <summary>
    /// A <c>Integer | undefined</c> representing the process identifier (PID) of the child process. Until the child process has spawned successfully,
    /// the value is <c>undefined</c>. When the child process exits, then the value is <c>undefined</c> after the <c>exit</c> event is emitted.<br/><br/>&gt;
    /// [!NOTE] You can use the <c>pid</c> to determine if the process is currently running.
    /// </summary>
    [<Erase>]
    member val pid: Option<int> = Unchecked.defaultof<_> with get, set

    /// <summary>
    /// A <c>NodeJS.ReadableStream | null</c> that represents the child process's stdout. If the child was spawned with options.stdio[1] set to anything
    /// other than 'pipe', then this will be <c>null</c>. When the child process exits, then the value is <c>null</c> after the
    /// <c>exit</c> event is emitted.
    /// </summary>
    [<Erase>]
    member val stdout: Option<Readable<obj>> = Unchecked.defaultof<_> with get, set

    /// <summary>
    /// A <c>NodeJS.ReadableStream | null</c> that represents the child process's stderr. If the child was spawned with options.stdio[2] set to anything
    /// other than 'pipe', then this will be <c>null</c>. When the child process exits, then the value is <c>null</c> after the
    /// <c>exit</c> event is emitted.
    /// </summary>
    [<Erase>]
    member val stderr: Option<Readable<obj>> = Unchecked.defaultof<_> with get, set

/// <summary>
/// <para>⚠ Process Availability: Main ❌ | Renderer ❌ | Utility ❌ | Exported ✔</para>
/// &gt; Get system preferences.<br/><br/>Process: Main, Utility
/// </summary>
[<Import("systemPreferences", "electron")>]
type systemPreferences private () =
    class
    end

    interface EventEmitter
    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_LIN || ELECTRON_OS_WIN
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ✔ | MAS ❌
    /// </para>
    /// </summary>
    [<Emit("$0.on('accent-color-changed', $1)"); Import("systemPreferences", "electron")>]
    static member inline onAccentColorChanged(handler: Event -> string -> unit) : unit = Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_LIN || ELECTRON_OS_WIN
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ✔ | MAS ❌
    /// </para>
    /// </summary>
    [<Emit("$0.on('accent-color-changed', $1)"); Import("systemPreferences", "electron")>]
    static member inline onAccentColorChanged(handler: SystemPreferences.IOnAccentColorChanged -> unit) : unit =
        Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_LIN || ELECTRON_OS_WIN
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ✔ | MAS ❌
    /// </para>
    /// </summary>
    [<Emit("$0.once('accent-color-changed', $1)"); Import("systemPreferences", "electron")>]
    static member inline onceAccentColorChanged(handler: Event -> string -> unit) : unit = Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_LIN || ELECTRON_OS_WIN
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ✔ | MAS ❌
    /// </para>
    /// </summary>
    [<Emit("$0.once('accent-color-changed', $1)"); Import("systemPreferences", "electron")>]
    static member inline onceAccentColorChanged(handler: SystemPreferences.IOnAccentColorChanged -> unit) : unit =
        Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_LIN || ELECTRON_OS_WIN
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ✔ | MAS ❌
    /// </para>
    /// </summary>
    [<Emit("$0.off('accent-color-changed', $1)"); Import("systemPreferences", "electron")>]
    static member inline offAccentColorChanged(handler: Event -> string -> unit) : unit = Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_LIN || ELECTRON_OS_WIN
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ✔ | MAS ❌
    /// </para>
    /// </summary>
    [<Emit("$0.off('accent-color-changed', $1)"); Import("systemPreferences", "electron")>]
    static member inline offAccentColorChanged(handler: SystemPreferences.IOnAccentColorChanged -> unit) : unit =
        Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌
    /// </para>
    /// </summary>
    [<Emit("$0.on('color-changed', $1)"); Import("systemPreferences", "electron")>]
    static member inline onColorChanged(handler: Event -> unit) : unit = Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌
    /// </para>
    /// </summary>
    [<Emit("$0.once('color-changed', $1)"); Import("systemPreferences", "electron")>]
    static member inline onceColorChanged(handler: Event -> unit) : unit = Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌
    /// </para>
    /// </summary>
    [<Emit("$0.off('color-changed', $1)"); Import("systemPreferences", "electron")>]
    static member inline offColorChanged(handler: Event -> unit) : unit = Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌
    /// </para>
    /// Whether the Swipe between pages setting is on.
    /// </summary>
    [<Erase>]
    static member inline isSwipeTrackingFromScrollEventsEnabled() : bool = Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌
    /// </para>
    /// Posts <c>event</c> as native notifications of macOS. The <c>userInfo</c> is an Object that contains the user information dictionary sent along
    /// with the notification.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="userInfo"></param>
    /// <param name="deliverImmediately"><c>true</c> to post notifications immediately even when the subscribing app is inactive.</param>
    [<Erase>]
    static member inline postNotification
        (event: string, userInfo: Record<string, obj>, ?deliverImmediately: bool)
        : unit =
        Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌
    /// </para>
    /// Posts <c>event</c> as native notifications of macOS. The <c>userInfo</c> is an Object that contains the user information dictionary sent along
    /// with the notification.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="userInfo"></param>
    [<Erase>]
    static member inline postLocalNotification(event: string, userInfo: Record<string, obj>) : unit =
        Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌
    /// </para>
    /// Posts <c>event</c> as native notifications of macOS. The <c>userInfo</c> is an Object that contains the user information dictionary sent along
    /// with the notification.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="userInfo"></param>
    [<Erase>]
    static member inline postWorkspaceNotification(event: string, userInfo: Record<string, obj>) : unit =
        Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌
    /// </para>
    /// The ID of this subscription<br/><br/>Subscribes to native notifications of macOS, <c>callback</c> will be called with <c>callback(event, userInfo)</c> when the corresponding
    /// <c>event</c> happens. The <c>userInfo</c> is an Object that contains the user information dictionary sent along with the notification. The <c>object</c>
    /// is the sender of the notification, and only supports <c>NSString</c> values for now.<br/><br/>The <c>id</c> of the subscriber is returned, which
    /// can be used to unsubscribe the <c>event</c>.<br/><br/>Under the hood this API subscribes to <c>NSDistributedNotificationCenter</c>, example values of <c>event</c> are:<br/><br/>* <c>AppleInterfaceThemeChangedNotification</c><br/>*
    /// <c>AppleAquaColorVariantChanged</c><br/>* <c>AppleColorPreferencesChangedNotification</c><br/>* <c>AppleShowScrollBarsSettingChanged</c><br/><br/>If <c>event</c> is null, the <c>NSDistributedNotificationCenter</c> doesn’t use it as criteria for delivery to the observer. See docs
    ///  for more information.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="callback"></param>
    [<Erase>]
    static member inline subscribeNotification
        (event: Option<string>, callback: SystemPreferences.SubscribeNotification.Callback)
        : float =
        Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌
    /// </para>
    /// The ID of this subscription<br/><br/>Same as <c>subscribeNotification</c>, but uses <c>NSNotificationCenter</c> for local defaults. This is necessary for events such as
    /// <c>NSUserDefaultsDidChangeNotification</c>.<br/><br/>If <c>event</c> is null, the <c>NSNotificationCenter</c> doesn’t use it as criteria for delivery to the observer. See docs for more
    /// information.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="callback"></param>
    [<Erase>]
    static member inline subscribeLocalNotification
        (event: Option<string>, callback: SystemPreferences.SubscribeLocalNotification.Callback)
        : float =
        Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌
    /// </para>
    /// The ID of this subscription<br/><br/>Same as <c>subscribeNotification</c>, but uses <c>NSWorkspace.sharedWorkspace.notificationCenter</c>. This is necessary for events such as <c>NSWorkspaceDidActivateApplicationNotification</c>.<br/><br/>If <c>event</c> is
    /// null, the <c>NSWorkspaceNotificationCenter</c> doesn’t use it as criteria for delivery to the observer. See docs for more information.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="callback"></param>
    [<Erase>]
    static member inline subscribeWorkspaceNotification
        (event: Option<string>, callback: SystemPreferences.SubscribeWorkspaceNotification.Callback)
        : float =
        Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌
    /// </para>
    /// Removes the subscriber with <c>id</c>.
    /// </summary>
    /// <param name="id"></param>
    [<Erase>]
    static member inline unsubscribeNotification(id: int) : unit = Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌
    /// </para>
    /// Same as <c>unsubscribeNotification</c>, but removes the subscriber from <c>NSNotificationCenter</c>.
    /// </summary>
    /// <param name="id"></param>
    [<Erase>]
    static member inline unsubscribeLocalNotification(id: int) : unit = Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌
    /// </para>
    /// Same as <c>unsubscribeNotification</c>, but removes the subscriber from <c>NSWorkspace.sharedWorkspace.notificationCenter</c>.
    /// </summary>
    /// <param name="id"></param>
    [<Erase>]
    static member inline unsubscribeWorkspaceNotification(id: int) : unit = Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌
    /// </para>
    /// Add the specified defaults to your application's <c>NSUserDefaults</c>.
    /// </summary>
    /// <param name="defaults">a dictionary of (<c>key: value</c>) user defaults</param>
    [<Erase>]
    static member inline registerDefaults(defaults: Record<string, U3<string, bool, float>>) : unit =
        Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌
    /// </para>
    /// The value of <c>key</c> in <c>NSUserDefaults</c>.<br/><br/>Some popular <c>key</c> and <c>type</c>s are:<br/><br/>* <c>AppleInterfaceStyle</c>: <c>string</c><br/>* <c>AppleAquaColorVariant</c>: <c>integer</c><br/>* <c>AppleHighlightColor</c>: <c>string</c><br/>* <c>AppleShowScrollBars</c>: <c>string</c><br/>* <c>NSNavRecentPlaces</c>:
    /// <c>array</c><br/>* <c>NSPreferredWebServices</c>: <c>dictionary</c><br/>* <c>NSUserDictionaryReplacementItems</c>: <c>array</c>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="type">Can be <c>string</c>, <c>boolean</c>, <c>integer</c>, <c>float</c>, <c>double</c>, <c>url</c>, <c>array</c> or <c>dictionary</c>.</param>
    [<Erase>]
    static member inline getUserDefault(key: string, ``type``: UserType) : string = Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌
    /// </para>
    /// Set the value of <c>key</c> in <c>NSUserDefaults</c>.<br/><br/>Note that <c>type</c> should match actual type of <c>value</c>. An exception is thrown if
    /// they don't.<br/><br/>Some popular <c>key</c> and <c>type</c>s are:<br/><br/>* <c>ApplePressAndHoldEnabled</c>: <c>boolean</c>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="type">Can be <c>string</c>, <c>boolean</c>, <c>integer</c>, <c>float</c>, <c>double</c>, <c>url</c>, <c>array</c> or <c>dictionary</c>.</param>
    /// <param name="value"></param>
    [<Erase>]
    static member inline setUserDefault(key: string, ``type``: UserType, value: string) : unit = Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌
    /// </para>
    /// Removes the <c>key</c> in <c>NSUserDefaults</c>. This can be used to restore the default or global value of a <c>key</c> previously
    /// set with <c>setUserDefault</c>.
    /// </summary>
    /// <param name="key"></param>
    [<Erase>]
    static member inline removeUserDefault(key: string) : unit = Unchecked.defaultof<_>
    #endif


    /// <summary>
    /// The users current system wide accent color preference in RGBA hexadecimal form.<br/><br/>This API is only available on macOS 10.14 Mojave
    /// or newer.
    /// </summary>
    [<Erase>]
    static member inline getAccentColor() : string = Unchecked.defaultof<_>
    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌
    /// </para>
    /// The system color setting in RGBA hexadecimal form (<c>#RRGGBBAA</c>). See the Windows docs and the macOS docs for more details.<br/><br/>The
    /// following colors are only available on macOS 10.14: <c>find-highlight</c>, <c>selected-content-background</c>, <c>separator</c>, <c>unemphasized-selected-content-background</c>, <c>unemphasized-selected-text-background</c>, and <c>unemphasized-selected-text</c>.
    /// </summary>
    /// <param name="color">One of the following values:</param>
    [<Erase>]
    static member inline getColor(color: Enums.SystemPreferences.GetColor.Color) : string = Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌
    /// </para>
    /// The standard system color formatted as <c>#RRGGBBAA</c>.<br/><br/>Returns one of several standard system colors that automatically adapt to vibrancy and changes
    /// in accessibility settings like 'Increase contrast' and 'Reduce transparency'. See Apple Documentation for  more details.
    /// </summary>
    /// <param name="color">One of the following values:</param>
    [<Erase>]
    static member inline getSystemColor(color: Enums.SystemPreferences.GetSystemColor.Color) : string =
        Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌
    /// </para>
    /// Can be <c>dark</c>, <c>light</c> or <c>unknown</c>.<br/><br/>Gets the macOS appearance setting that is currently applied to your application, maps to NSApplication.effectiveAppearance
    /// </summary>
    [<Erase>]
    static member inline getEffectiveAppearance() : Enums.SystemPreferences.GetEffectiveAppearance =
        Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌
    /// </para>
    /// whether or not this device has the ability to use Touch ID.
    /// </summary>
    [<Erase>]
    static member inline canPromptTouchID() : bool = Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌
    /// </para>
    /// resolves if the user has successfully authenticated with Touch ID.<br/><br/>This API itself will not protect your user data; rather, it
    /// is a mechanism to allow you to do so. Native apps will need to set Access Control Constants like <c>kSecAccessControlUserPresence</c>
    /// on their keychain entry so that reading it would auto-prompt for Touch ID biometric consent. This could be done with
    /// <c>node-keytar</c>, such that one would store an encryption key with <c>node-keytar</c> and only fetch it if <c>promptTouchID()</c> resolves.
    /// </summary>
    /// <param name="reason">The reason you are asking for Touch ID authentication</param>
    [<Erase>]
    static member inline promptTouchID(reason: string) : Promise<unit> = Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌
    /// </para>
    /// <c>true</c> if the current process is a trusted accessibility client and <c>false</c> if it is not.
    /// </summary>
    /// <param name="prompt">whether or not the user will be informed via prompt if the current process is untrusted.</param>
    [<Erase>]
    static member inline isTrustedAccessibilityClient(prompt: bool) : bool = Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌
    /// </para>
    /// Can be <c>not-determined</c>, <c>granted</c>, <c>denied</c>, <c>restricted</c> or <c>unknown</c>.<br/><br/>This user consent was not required on macOS 10.13 High Sierra so this
    /// method will always return <c>granted</c>. macOS 10.14 Mojave or higher requires consent for <c>microphone</c> and <c>camera</c> access. macOS 10.15 Catalina
    /// or higher requires consent for <c>screen</c> access.<br/><br/>Windows 10 has a global setting controlling <c>microphone</c> and <c>camera</c> access for all win32
    /// applications. It will always return <c>granted</c> for <c>screen</c> and for all media types on older versions of Windows.
    /// </summary>
    /// <param name="mediaType">Can be <c>microphone</c>, <c>camera</c> or <c>screen</c>.</param>
    [<Erase>]
    static member inline getMediaAccessStatus
        (mediaType: Enums.SystemPreferences.GetMediaAccessStatus.MediaType)
        : Enums.SystemPreferences.GetMediaAccessStatus =
        Unchecked.defaultof<_>
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
    /// <summary>
    /// <para>
    /// ⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌
    /// </para>
    /// A promise that resolves with <c>true</c> if consent was granted and <c>false</c> if it was denied. If an invalid <c>mediaType</c>
    /// is passed, the promise will be rejected. If an access request was denied and later is changed through the System
    /// Preferences pane, a restart of the app will be required for the new permissions to take effect. If access has
    /// already been requested and denied, it _must_ be changed through the preference pane; an alert will not pop up and
    /// the promise will resolve with the existing access status.<br/><br/>**Important:** In order to properly leverage this API, you must set the
    /// <c>NSMicrophoneUsageDescription</c> and <c>NSCameraUsageDescription</c> strings in your app's <c>Info.plist</c> file. The values for these keys will be used to populate the
    /// permission dialogs so that the user will be properly informed as to the purpose of the permission request. See Electron
    /// Application Distribution for more information about how to set these in the context of Electron.<br/><br/>This user consent was not required
    /// until macOS 10.14 Mojave, so this method will always return <c>true</c> if your system is running 10.13 High Sierra.
    /// </summary>
    /// <param name="mediaType">the type of media being requested; can be <c>microphone</c>, <c>camera</c>.</param>
    [<Erase>]
    static member inline askForMediaAccess
        (mediaType: Enums.SystemPreferences.AskForMediaAccess.MediaType)
        : Promise<bool> =
        Unchecked.defaultof<_>
    #endif


    /// <summary>
    /// * <c>shouldRenderRichAnimation</c> boolean - Returns true if rich animations should be rendered. Looks at session type (e.g. remote desktop) and
    /// accessibility settings to give guidance for heavy animations.<br/>* <c>scrollAnimationsEnabledBySystem</c> boolean - Determines on a per-platform basis whether scroll animations (e.g.
    /// produced by home/end key) should be enabled.<br/>* <c>prefersReducedMotion</c> boolean - Determines whether the user desires reduced motion based on platform
    /// APIs.<br/><br/>Returns an object with system animation settings.
    /// </summary>
    [<Erase>]
    static member inline getAnimationSettings() : SystemPreferences.GetAnimationSettings = Unchecked.defaultof<_>
    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
    /// <summary>
    /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
    /// A <c>boolean</c> property which determines whether the app avoids using semitransparent backgrounds. This maps to NSWorkspace.accessibilityDisplayShouldReduceTransparency<br/><br/>**Deprecated:** Use the new <c>nativeTheme.prefersReducedTransparency</c>
    /// API.
    /// </summary>
    [<Erase; System.Obsolete>]
    static member val accessibilityDisplayShouldReduceTransparency: bool = Unchecked.defaultof<_> with get, set
    #endif

    #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
    /// <summary>
    /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
    /// A <c>string</c> property that can be <c>dark</c>, <c>light</c> or <c>unknown</c>.<br/><br/>Returns the macOS appearance setting that is currently applied to your
    /// application, maps to NSApplication.effectiveAppearance
    /// </summary>
    [<Erase>]
    static member val effectiveAppearance: Enums.SystemPreferences.EffectiveAppearance = Unchecked.defaultof<_> with get
    #endif


/// <summary>
/// <para>⚠ Process Availability: Main ❌ | Renderer ❌ | Utility ❌ | Exported ✔</para>
/// &gt; Interface for communication with parent process.<br/><br/>Process: Utility<br/><br/><c>parentPort</c> is an EventEmitter. _This object is not exported from the <c>'electron'</c> module.
/// It is only available as a property of the process object in the Electron API._
/// </summary>
[<Import("parentPort", "electron")>]
type parentPort =
    class
    end

    interface EventEmitter

    /// <summary>
    /// Emitted when the process receives a message. Messages received on this port will be queued up until a handler is
    /// registered for this event.
    /// </summary>
    [<Emit("$0.on('message', $1)"); Import("parentPort", "electron")>]
    static member inline onMessage(handler: ParentPort.Message.MessageEvent -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when the process receives a message. Messages received on this port will be queued up until a handler is
    /// registered for this event.
    /// </summary>
    [<Emit("$0.once('message', $1)"); Import("parentPort", "electron")>]
    static member inline onceMessage(handler: ParentPort.Message.MessageEvent -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when the process receives a message. Messages received on this port will be queued up until a handler is
    /// registered for this event.
    /// </summary>
    [<Emit("$0.off('message', $1)"); Import("parentPort", "electron")>]
    static member inline offMessage(handler: ParentPort.Message.MessageEvent -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Sends a message from the process to its parent.
    /// </summary>
    /// <param name="message"></param>
    [<Erase>]
    static member inline postMessage(message: obj) : unit = Unchecked.defaultof<_>

/// <summary>
/// <para>⚠ Process Availability: Main ❌ | Renderer ❌ | Utility ❌ | Exported ✔</para>
/// &gt; Issue HTTP/HTTPS requests using Chromium's native networking library<br/><br/>Process: Main, Utility<br/><br/>The <c>net</c> module is a client-side API for issuing HTTP(S)
/// requests. It is similar to the HTTP and HTTPS modules of Node.js but uses Chromium's native networking library instead of
/// the Node.js implementation, offering better support for web proxies. It also supports checking network status.<br/><br/>The following is a non-exhaustive list
/// of why you may consider using the <c>net</c> module instead of the native Node.js modules:<br/><br/>* Automatic management of system proxy
/// configuration, support of the wpad protocol and proxy pac configuration files.<br/>* Automatic tunneling of HTTPS requests.<br/>* Support for authenticating proxies
/// using basic, digest, NTLM, Kerberos or negotiate authentication schemes.<br/>* Support for traffic monitoring proxies: Fiddler-like proxies used for access control
/// and monitoring.<br/><br/>The API components (including classes, methods, properties and event names) are similar to those used in Node.js.<br/><br/>Example usage:<br/><br/><code><br/>const {
/// app } = require('electron')<br/><br/>app.whenReady().then(() =&gt; {<br/>  const { net } = require('electron')<br/>  const request = net.request('https://github.com')<br/>  request.on('response',
/// (response) =&gt; {<br/>    console.log(<c>STATUS: ${response.statusCode}</c>)<br/>    console.log(<c>HEADERS: ${JSON.stringify(response.headers)}</c>)<br/>    response.on('data', (chunk) =&gt; {<br/>
///      console.log(<c>BODY: ${chunk}</c>)<br/>    })<br/>    response.on('end', () =&gt; {<br/>
///    console.log('No more data in response.')<br/>    })<br/>  })<br/>  request.end()<br/>})<br/></code><br/><br/>The <c>net</c> API can be
/// used only after the application emits the <c>ready</c> event. Trying to use the module before the <c>ready</c> event will throw
/// an error.
/// </summary>
[<Import("net", "electron")>]
type net private () =
    class
    end

    /// <summary>
    /// Creates a <c>ClientRequest</c> instance using the provided <c>options</c> which are directly forwarded to the <c>ClientRequest</c> constructor. The <c>net.request</c> method would
    /// be used to issue both secure and insecure HTTP requests according to the specified protocol scheme in the <c>options</c> object.
    /// </summary>
    /// <param name="options">The <c>ClientRequest</c> constructor options.</param>
    [<Erase>]
    static member inline request(options: U2<ClientRequest.Options, string>) : ClientRequest = Unchecked.defaultof<_>

    /// <summary>
    /// see Response.<br/><br/>Sends a request, similarly to how <c>fetch()</c> works in the renderer, using Chrome's network stack. This differs from Node's
    /// <c>fetch()</c>, which uses Node.js's HTTP stack.<br/><br/>Example:<br/><br/>This method will issue requests from the default session. To send a <c>fetch</c> request from
    /// another session, use ses.fetch().<br/><br/>See the MDN documentation for <c>fetch()</c> for more details.<br/><br/>Limitations:<br/><br/>* <c>net.fetch()</c> does not support the <c>data:</c> or <c>blob:</c>
    /// schemes.<br/>* The value of the <c>integrity</c> option is ignored.<br/>* The <c>.type</c> and <c>.url</c> values of the returned <c>Response</c> object are
    /// incorrect.<br/><br/>By default, requests made with <c>net.fetch</c> can be made to custom protocols as well as <c>file:</c>, and will trigger webRequest
    /// handlers if present. When the non-standard <c>bypassCustomProtocolHandlers</c> option is set in RequestInit, custom protocol handlers will not be called for
    /// this request. This allows forwarding an intercepted request to the built-in handler. webRequest handlers will still be triggered when bypassing
    /// custom protocols.<br/><br/>&gt; [!NOTE] In the utility process, custom protocols are not supported.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="init"></param>
    [<Erase>]
    static member inline fetch(input: U2<string, Request>, ?init: RequestInit) : Promise<Response> =
        Unchecked.defaultof<_>

    /// <summary>
    /// Whether there is currently internet connection.<br/><br/>A return value of <c>false</c> is a pretty strong indicator that the user won't be
    /// able to connect to remote sites. However, a return value of <c>true</c> is inconclusive; even if some link is up,
    /// it is uncertain whether a particular connection attempt to a particular remote site will be successful.
    /// </summary>
    [<Erase>]
    static member inline isOnline() : bool = Unchecked.defaultof<_>

    /// <summary>
    /// Resolves with the resolved IP addresses for the <c>host</c>.<br/><br/>This method will resolve hosts from the default session. To resolve a
    /// host from another session, use ses.resolveHost().
    /// </summary>
    /// <param name="host">Hostname to resolve.</param>
    /// <param name="queryType">Requested DNS query type. If unspecified, resolver will pick A or AAAA (or both) based on IPv4/IPv6 settings:</param>
    /// <param name="source">The source to use for resolved addresses. Default allows the resolver to pick an appropriate source. Only affects use
    /// of big external sources (e.g. calling the system for resolution or using DNS). Even if a source is specified, results
    /// can still come from cache, resolving "localhost" or IP literals, etc. One of the following values:</param>
    /// <param name="cacheUsage">Indicates what DNS cache entries, if any, can be used to provide a response. One of the following values:</param>
    /// <param name="secureDnsPolicy">Controls the resolver's Secure DNS behavior for this request. One of the following values:</param>
    [<Erase; ParamObject(1)>]
    static member inline resolveHost
        (
            host: string,
            ?queryType: Enums.Net.ResolveHost.Options.QueryType,
            ?source: Enums.Net.ResolveHost.Options.Source,
            ?cacheUsage: Enums.Net.ResolveHost.Options.CacheUsage,
            ?secureDnsPolicy: Enums.Net.ResolveHost.Options.SecureDnsPolicy
        ) : Promise<ResolvedHost> =
        Unchecked.defaultof<_>

    /// <summary>
    /// A <c>boolean</c> property. Whether there is currently internet connection.<br/><br/>A return value of <c>false</c> is a pretty strong indicator that the
    /// user won't be able to connect to remote sites. However, a return value of <c>true</c> is inconclusive; even if some
    /// link is up, it is uncertain whether a particular connection attempt to a particular remote site will be successful.
    /// </summary>
    [<Erase>]
    static member val online: bool = Unchecked.defaultof<_> with get

/// <summary>
/// <para>⚠ Process Availability: Main ❌ | Renderer ❌ | Utility ❌ | Exported ❌</para>
/// </summary>
[<Import("IncomingMessage", "electron")>]
type IncomingMessage private () =
    class
    end

    interface EventEmitter

    /// <summary>
    /// The <c>data</c> event is the usual method of transferring response data into applicative code.
    /// </summary>
    [<Emit("$0.on('data', $1)")>]
    member inline _.onData(handler: Buffer -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// The <c>data</c> event is the usual method of transferring response data into applicative code.
    /// </summary>
    [<Emit("$0.once('data', $1)")>]
    member inline _.onceData(handler: Buffer -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// The <c>data</c> event is the usual method of transferring response data into applicative code.
    /// </summary>
    [<Emit("$0.off('data', $1)")>]
    member inline _.offData(handler: Buffer -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Indicates that response body has ended. Must be placed before 'data' event.
    /// </summary>
    [<Emit("$0.on('end', $1)")>]
    member inline _.onEnd(handler: unit -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Indicates that response body has ended. Must be placed before 'data' event.
    /// </summary>
    [<Emit("$0.once('end', $1)")>]
    member inline _.onceEnd(handler: unit -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Indicates that response body has ended. Must be placed before 'data' event.
    /// </summary>
    [<Emit("$0.off('end', $1)")>]
    member inline _.offEnd(handler: unit -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when a request has been canceled during an ongoing HTTP transaction.
    /// </summary>
    [<Emit("$0.on('aborted', $1)")>]
    member inline _.onAborted(handler: unit -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when a request has been canceled during an ongoing HTTP transaction.
    /// </summary>
    [<Emit("$0.once('aborted', $1)")>]
    member inline _.onceAborted(handler: unit -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when a request has been canceled during an ongoing HTTP transaction.
    /// </summary>
    [<Emit("$0.off('aborted', $1)")>]
    member inline _.offAborted(handler: unit -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when an error was encountered while streaming response data events. For instance, if the server closes the underlying while
    /// the response is still streaming, an <c>error</c> event will be emitted on the response object and a <c>close</c> event will
    /// subsequently follow on the request object.
    /// </summary>
    [<Emit("$0.on('error', $1)")>]
    member inline _.onError(handler: Error -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when an error was encountered while streaming response data events. For instance, if the server closes the underlying while
    /// the response is still streaming, an <c>error</c> event will be emitted on the response object and a <c>close</c> event will
    /// subsequently follow on the request object.
    /// </summary>
    [<Emit("$0.once('error', $1)")>]
    member inline _.onceError(handler: Error -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when an error was encountered while streaming response data events. For instance, if the server closes the underlying while
    /// the response is still streaming, an <c>error</c> event will be emitted on the response object and a <c>close</c> event will
    /// subsequently follow on the request object.
    /// </summary>
    [<Emit("$0.off('error', $1)")>]
    member inline _.offError(handler: Error -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// An <c>Integer</c> indicating the HTTP response status code.
    /// </summary>
    [<Erase>]
    member val statusCode: int = Unchecked.defaultof<_> with get, set

    /// <summary>
    /// A <c>string</c> representing the HTTP status message.
    /// </summary>
    [<Erase>]
    member val statusMessage: string = Unchecked.defaultof<_> with get, set

    /// <summary>
    /// A <c>Record&lt;string, string | string[]&gt;</c> representing the HTTP response headers. The <c>headers</c> object is formatted as follows:<br/><br/>* All header names
    /// are lowercased.<br/>* Duplicates of <c>age</c>, <c>authorization</c>, <c>content-length</c>, <c>content-type</c>, <c>etag</c>, <c>expires</c>, <c>from</c>, <c>host</c>, <c>if-modified-since</c>, <c>if-unmodified-since</c>, <c>last-modified</c>, <c>location</c>, <c>max-forwards</c>, <c>proxy-authorization</c>, <c>referer</c>, <c>retry-after</c>,
    /// <c>server</c>, or <c>user-agent</c> are discarded.<br/>* <c>set-cookie</c> is always an array. Duplicates are added to the array.<br/>* For duplicate <c>cookie</c> headers,
    /// the values are joined together with '; '.<br/>* For all other headers, the values are joined together with ', '.
    /// </summary>
    [<Erase>]
    member val headers: Record<string, U2<string, string[]>> = Unchecked.defaultof<_> with get, set

    /// <summary>
    /// A <c>string</c> indicating the HTTP protocol version number. Typical values are '1.0' or '1.1'. Additionally <c>httpVersionMajor</c> and <c>httpVersionMinor</c> are two
    /// Integer-valued readable properties that return respectively the HTTP major and minor version numbers.
    /// </summary>
    [<Erase>]
    member val httpVersion: string = Unchecked.defaultof<_> with get, set

    /// <summary>
    /// An <c>Integer</c> indicating the HTTP protocol major version number.
    /// </summary>
    [<Erase>]
    member val httpVersionMajor: int = Unchecked.defaultof<_> with get, set

    /// <summary>
    /// An <c>Integer</c> indicating the HTTP protocol minor version number.
    /// </summary>
    [<Erase>]
    member val httpVersionMinor: int = Unchecked.defaultof<_> with get, set

    /// <summary>
    /// A <c>string[]</c> containing the raw HTTP response headers exactly as they were received. The keys and values are in the
    /// same list. It is not a list of tuples. So, the even-numbered offsets are key values, and the odd-numbered offsets
    /// are the associated values. Header names are not lowercased, and duplicates are not merged.
    /// </summary>
    [<Erase>]
    member val rawHeaders: string[] = Unchecked.defaultof<_> with get, set

/// <summary>
/// <para>⚠ Process Availability: Main ❌ | Renderer ❌ | Utility ❌ | Exported ❌</para>
/// &gt; Make HTTP/HTTPS requests.<br/><br/>Process: Main, Utility<br/> _This class is not exported from the <c>'electron'</c> module. It is only available as
/// a return value of other methods in the Electron API._<br/><br/><c>ClientRequest</c> implements the Writable Stream interface and is therefore an EventEmitter.
/// </summary>
[<Import("ClientRequest", "electron")>]
type ClientRequest
    /// <param name="options">If <c>options</c> is a string, it is interpreted as the request URL. If it is an object, it is
    /// expected to fully specify an HTTP request via the following properties:</param>
    (options: U2<ClientRequest.Options, string>) =
    class
    end

    interface EventEmitter

    [<Emit("$0.on('response', $1)")>]
    member inline _.onResponse(handler: IncomingMessage -> unit) : unit = Unchecked.defaultof<_>

    [<Emit("$0.once('response', $1)")>]
    member inline _.onceResponse(handler: IncomingMessage -> unit) : unit = Unchecked.defaultof<_>

    [<Emit("$0.off('response', $1)")>]
    member inline _.offResponse(handler: IncomingMessage -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when an authenticating proxy is asking for user credentials.<br/><br/>The <c>callback</c> function is expected to be called back with user
    /// credentials:<br/><br/>* <c>username</c> string<br/>* <c>password</c> string<br/><br/>Providing empty credentials will cancel the request and report an authentication error on the response object:
    /// </summary>
    [<Emit("$0.on('login', $1)")>]
    member inline _.onLogin(handler: ClientRequest.Login.AuthInfo -> ClientRequest.Login.Callback -> unit) : unit =
        Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when an authenticating proxy is asking for user credentials.<br/><br/>The <c>callback</c> function is expected to be called back with user
    /// credentials:<br/><br/>* <c>username</c> string<br/>* <c>password</c> string<br/><br/>Providing empty credentials will cancel the request and report an authentication error on the response object:
    /// </summary>
    [<Emit("$0.on('login', $1)")>]
    member inline _.onLogin(handler: ClientRequest.IOnLogin -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when an authenticating proxy is asking for user credentials.<br/><br/>The <c>callback</c> function is expected to be called back with user
    /// credentials:<br/><br/>* <c>username</c> string<br/>* <c>password</c> string<br/><br/>Providing empty credentials will cancel the request and report an authentication error on the response object:
    /// </summary>
    [<Emit("$0.once('login', $1)")>]
    member inline _.onceLogin(handler: ClientRequest.Login.AuthInfo -> ClientRequest.Login.Callback -> unit) : unit =
        Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when an authenticating proxy is asking for user credentials.<br/><br/>The <c>callback</c> function is expected to be called back with user
    /// credentials:<br/><br/>* <c>username</c> string<br/>* <c>password</c> string<br/><br/>Providing empty credentials will cancel the request and report an authentication error on the response object:
    /// </summary>
    [<Emit("$0.once('login', $1)")>]
    member inline _.onceLogin(handler: ClientRequest.IOnLogin -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when an authenticating proxy is asking for user credentials.<br/><br/>The <c>callback</c> function is expected to be called back with user
    /// credentials:<br/><br/>* <c>username</c> string<br/>* <c>password</c> string<br/><br/>Providing empty credentials will cancel the request and report an authentication error on the response object:
    /// </summary>
    [<Emit("$0.off('login', $1)")>]
    member inline _.offLogin(handler: ClientRequest.Login.AuthInfo -> ClientRequest.Login.Callback -> unit) : unit =
        Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when an authenticating proxy is asking for user credentials.<br/><br/>The <c>callback</c> function is expected to be called back with user
    /// credentials:<br/><br/>* <c>username</c> string<br/>* <c>password</c> string<br/><br/>Providing empty credentials will cancel the request and report an authentication error on the response object:
    /// </summary>
    [<Emit("$0.off('login', $1)")>]
    member inline _.offLogin(handler: ClientRequest.IOnLogin -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted just after the last chunk of the <c>request</c>'s data has been written into the <c>request</c> object.
    /// </summary>
    [<Emit("$0.on('finish', $1)")>]
    member inline _.onFinish(handler: unit -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted just after the last chunk of the <c>request</c>'s data has been written into the <c>request</c> object.
    /// </summary>
    [<Emit("$0.once('finish', $1)")>]
    member inline _.onceFinish(handler: unit -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted just after the last chunk of the <c>request</c>'s data has been written into the <c>request</c> object.
    /// </summary>
    [<Emit("$0.off('finish', $1)")>]
    member inline _.offFinish(handler: unit -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when the <c>request</c> is aborted. The <c>abort</c> event will not be fired if the <c>request</c> is already closed.
    /// </summary>
    [<Emit("$0.on('abort', $1)")>]
    member inline _.onAbort(handler: unit -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when the <c>request</c> is aborted. The <c>abort</c> event will not be fired if the <c>request</c> is already closed.
    /// </summary>
    [<Emit("$0.once('abort', $1)")>]
    member inline _.onceAbort(handler: unit -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when the <c>request</c> is aborted. The <c>abort</c> event will not be fired if the <c>request</c> is already closed.
    /// </summary>
    [<Emit("$0.off('abort', $1)")>]
    member inline _.offAbort(handler: unit -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when the <c>net</c> module fails to issue a network request. Typically when the <c>request</c> object emits an <c>error</c> event,
    /// a <c>close</c> event will subsequently follow and no response object will be provided.
    /// </summary>
    [<Emit("$0.on('error', $1)")>]
    member inline _.onError(handler: Error -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when the <c>net</c> module fails to issue a network request. Typically when the <c>request</c> object emits an <c>error</c> event,
    /// a <c>close</c> event will subsequently follow and no response object will be provided.
    /// </summary>
    [<Emit("$0.once('error', $1)")>]
    member inline _.onceError(handler: Error -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when the <c>net</c> module fails to issue a network request. Typically when the <c>request</c> object emits an <c>error</c> event,
    /// a <c>close</c> event will subsequently follow and no response object will be provided.
    /// </summary>
    [<Emit("$0.off('error', $1)")>]
    member inline _.offError(handler: Error -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted as the last event in the HTTP request-response transaction. The <c>close</c> event indicates that no more events will be
    /// emitted on either the <c>request</c> or <c>response</c> objects.
    /// </summary>
    [<Emit("$0.on('close', $1)")>]
    member inline _.onClose(handler: unit -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted as the last event in the HTTP request-response transaction. The <c>close</c> event indicates that no more events will be
    /// emitted on either the <c>request</c> or <c>response</c> objects.
    /// </summary>
    [<Emit("$0.once('close', $1)")>]
    member inline _.onceClose(handler: unit -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted as the last event in the HTTP request-response transaction. The <c>close</c> event indicates that no more events will be
    /// emitted on either the <c>request</c> or <c>response</c> objects.
    /// </summary>
    [<Emit("$0.off('close', $1)")>]
    member inline _.offClose(handler: unit -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when the server returns a redirect response (e.g. 301 Moved Permanently). Calling <c>request.followRedirect</c> will continue with the redirection.
    /// If this event is handled, <c>request.followRedirect</c> must be called **synchronously**, otherwise the request will be cancelled.
    /// </summary>
    [<Emit("$0.on('redirect', $1)")>]
    member inline _.onRedirect(handler: int -> string -> string -> Record<string, string[]> -> unit) : unit =
        Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when the server returns a redirect response (e.g. 301 Moved Permanently). Calling <c>request.followRedirect</c> will continue with the redirection.
    /// If this event is handled, <c>request.followRedirect</c> must be called **synchronously**, otherwise the request will be cancelled.
    /// </summary>
    [<Emit("$0.on('redirect', $1)")>]
    member inline _.onRedirect(handler: ClientRequest.IOnRedirect -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when the server returns a redirect response (e.g. 301 Moved Permanently). Calling <c>request.followRedirect</c> will continue with the redirection.
    /// If this event is handled, <c>request.followRedirect</c> must be called **synchronously**, otherwise the request will be cancelled.
    /// </summary>
    [<Emit("$0.once('redirect', $1)")>]
    member inline _.onceRedirect(handler: int -> string -> string -> Record<string, string[]> -> unit) : unit =
        Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when the server returns a redirect response (e.g. 301 Moved Permanently). Calling <c>request.followRedirect</c> will continue with the redirection.
    /// If this event is handled, <c>request.followRedirect</c> must be called **synchronously**, otherwise the request will be cancelled.
    /// </summary>
    [<Emit("$0.once('redirect', $1)")>]
    member inline _.onceRedirect(handler: ClientRequest.IOnRedirect -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when the server returns a redirect response (e.g. 301 Moved Permanently). Calling <c>request.followRedirect</c> will continue with the redirection.
    /// If this event is handled, <c>request.followRedirect</c> must be called **synchronously**, otherwise the request will be cancelled.
    /// </summary>
    [<Emit("$0.off('redirect', $1)")>]
    member inline _.offRedirect(handler: int -> string -> string -> Record<string, string[]> -> unit) : unit =
        Unchecked.defaultof<_>

    /// <summary>
    /// Emitted when the server returns a redirect response (e.g. 301 Moved Permanently). Calling <c>request.followRedirect</c> will continue with the redirection.
    /// If this event is handled, <c>request.followRedirect</c> must be called **synchronously**, otherwise the request will be cancelled.
    /// </summary>
    [<Emit("$0.off('redirect', $1)")>]
    member inline _.offRedirect(handler: ClientRequest.IOnRedirect -> unit) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Adds an extra HTTP header. The header name will be issued as-is without lowercasing. It can be called only before
    /// first write. Calling this method after the first write will throw an error. If the passed value is not a
    /// <c>string</c>, its <c>toString()</c> method will be called to obtain the final value.<br/><br/>Certain headers are restricted from being set by apps.
    /// These headers are listed below. More information on restricted headers can be found in Chromium's header utils.<br/><br/>* <c>Content-Length</c><br/>* <c>Host</c><br/>* <c>Trailer</c>
    /// or <c>Te</c><br/>* <c>Upgrade</c><br/>* <c>Cookie2</c><br/>* <c>Keep-Alive</c><br/>* <c>Transfer-Encoding</c><br/><br/>Additionally, setting the <c>Connection</c> header to the value <c>upgrade</c> is also disallowed.
    /// </summary>
    /// <param name="name">An extra HTTP header name.</param>
    /// <param name="value">An extra HTTP header value.</param>
    [<Erase>]
    member inline _.setHeader(name: string, value: string) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// The value of a previously set extra header name.
    /// </summary>
    /// <param name="name">Specify an extra header name.</param>
    [<Erase>]
    member inline _.getHeader(name: string) : string = Unchecked.defaultof<_>

    /// <summary>
    /// Removes a previously set extra header name. This method can be called only before first write. Trying to call it
    /// after the first write will throw an error.
    /// </summary>
    /// <param name="name">Specify an extra header name.</param>
    [<Erase>]
    member inline _.removeHeader(name: string) : unit = Unchecked.defaultof<_>

    /// <summary>
    /// <c>callback</c> is essentially a dummy function introduced in the purpose of keeping similarity with the Node.js API. It is called
    /// asynchronously in the next tick after <c>chunk</c> content have been delivered to the Chromium networking layer. Contrary to the Node.js
    /// implementation, it is not guaranteed that <c>chunk</c> content have been flushed on the wire before <c>callback</c> is called.<br/><br/>Adds a chunk
    /// of data to the request body. The first write operation may cause the request headers to be issued on the
    /// wire. After the first write operation, it is not allowed to add or remove a custom header.
    /// </summary>
    /// <param name="chunk">A chunk of the request body's data. If it is a string, it is converted into a Buffer using
    /// the specified encoding.</param>
    /// <param name="encoding">Used to convert string chunks into Buffer objects. Defaults to 'utf-8'.</param>
    /// <param name="callback">Called after the write operation ends.</param>
    [<Erase>]
    member inline _.write(chunk: U2<string, Buffer>, ?encoding: string, ?callback: unit -> unit) : unit =
        Unchecked.defaultof<_>

    /// <summary>
    /// Sends the last chunk of the request data. Subsequent write or end operations will not be allowed. The <c>finish</c> event
    /// is emitted just after the end operation.
    /// </summary>
    /// <param name="chunk"></param>
    /// <param name="encoding"></param>
    /// <param name="callback"></param>
    [<Erase; CompiledName("end")>]
    member inline this.``end``(?chunk: U2<string, Buffer>, ?encoding: string, ?callback: unit -> unit) = this

    /// <summary>
    /// Cancels an ongoing HTTP transaction. If the request has already emitted the <c>close</c> event, the abort operation will have no
    /// effect. Otherwise an ongoing event will emit <c>abort</c> and <c>close</c> events. Additionally, if there is an ongoing response object,it will
    /// emit the <c>aborted</c> event.
    /// </summary>
    [<Erase>]
    member inline _.abort() : unit = Unchecked.defaultof<_>

    /// <summary>
    /// Continues any pending redirection. Can only be called during a <c>'redirect'</c> event.
    /// </summary>
    [<Erase>]
    member inline _.followRedirect() : unit = Unchecked.defaultof<_>

    /// <summary>
    /// * <c>active</c> boolean - Whether the request is currently active. If this is false no other properties will be set<br/>*
    /// <c>started</c> boolean - Whether the upload has started. If this is false both <c>current</c> and <c>total</c> will be set to
    /// 0.<br/>* <c>current</c> Integer - The number of bytes that have been uploaded so far<br/>* <c>total</c> Integer - The number of
    /// bytes that will be uploaded this request<br/><br/>You can use this method in conjunction with <c>POST</c> requests to get the progress
    /// of a file upload or other data transfer.
    /// </summary>
    [<Erase>]
    member inline _.getUploadProgress() : ClientRequest.GetUploadProgress = Unchecked.defaultof<_>

    /// <summary>
    /// A <c>boolean</c> specifying whether the request will use HTTP chunked transfer encoding or not. Defaults to false. The property is
    /// readable and writable, however it can be set only before the first write operation as the HTTP headers are not
    /// yet put on the wire. Trying to set the <c>chunkedEncoding</c> property after the first write will throw an error.<br/><br/>Using chunked
    /// encoding is strongly recommended if you need to send a large request body as data will be streamed in small
    /// chunks instead of being internally buffered inside Electron process memory.
    /// </summary>
    [<Erase>]
    member val chunkedEncoding: bool = Unchecked.defaultof<_> with get, set

[<Fable.Core.Erase; AutoOpen>]
module Types =
    /// <summary>
    /// Unfortunately, Windows does not offer a way to differentiate between a shutdown and a reboot, meaning the 'shutdown' reason is
    /// triggered in both scenarios. For more details on the <c>WM_ENDSESSION</c> message and its associated reasons, refer to the MSDN documentation.
    /// </summary>
    [<JS.Pojo>]
    type WindowSessionEndEvent
        /// <param name="reasons">List of reasons for shutdown. Can be 'shutdown', 'close-app', 'critical', or 'logoff'.</param>
        (reasons: Enums.Types.WindowSessionEndEvent.Reasons[]) =
        class
        end

        /// <summary>
        /// List of reasons for shutdown. Can be 'shutdown', 'close-app', 'critical', or 'logoff'.
        /// </summary>
        [<Erase>]
        member val reasons: Enums.Types.WindowSessionEndEvent.Reasons[] = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type WindowOpenHandlerResponse
        /// <param name="action">Can be <c>allow</c> or <c>deny</c>. Controls whether new window should be created.</param>
        /// <param name="overrideBrowserWindowOptions">Allows customization of the created window.</param>
        /// <param name="outlivesOpener">By default, child windows are closed when their opener is closed. This can be changed by specifying <c>outlivesOpener: true</c>,
        /// in which case the opened window will not be closed when its opener is closed.</param>
        /// <param name="createWindow">If specified, will be called instead of <c>new BrowserWindow</c> to create the new child window and event <c>did-create-window</c> will
        /// not be emitted. Constructed child window should use passed <c>options</c> object. This can be used for example to have the
        /// new window open as a BrowserView instead of in a separate window.</param>
        (
            action: Enums.Types.WindowOpenHandlerResponse.Action,
            ?overrideBrowserWindowOptions: BrowserWindowConstructorOptions,
            ?outlivesOpener: bool,
            ?createWindow: BrowserWindowConstructorOptions -> WebContents
        ) =
        class
        end

        /// <summary>
        /// Can be <c>allow</c> or <c>deny</c>. Controls whether new window should be created.
        /// </summary>
        [<Erase>]
        member val action: Enums.Types.WindowOpenHandlerResponse.Action = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Allows customization of the created window.
        /// </summary>
        [<Erase>]
        member val overrideBrowserWindowOptions: BrowserWindowConstructorOptions = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// By default, child windows are closed when their opener is closed. This can be changed by specifying <c>outlivesOpener: true</c>, in
        /// which case the opened window will not be closed when its opener is closed.
        /// </summary>
        [<Erase>]
        member val outlivesOpener: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// If specified, will be called instead of <c>new BrowserWindow</c> to create the new child window and event <c>did-create-window</c> will not
        /// be emitted. Constructed child window should use passed <c>options</c> object. This can be used for example to have the new
        /// window open as a BrowserView instead of in a separate window.
        /// </summary>
        [<Erase>]
        member val createWindow: BrowserWindowConstructorOptions -> WebContents = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type WebSource
        /// <param name="code"></param>
        /// <param name="url"></param>
        (code: string, ?url: string) =
        class
        end

        [<Erase>]
        member val code: string = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val url: string = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type WebRequestFilter
        /// <param name="urls">Array of URL patterns used to include requests that match these patterns. Use the pattern <c>&lt;all_urls&gt;</c> to match all
        /// URLs.</param>
        /// <param name="excludeUrls">Array of URL patterns used to exclude requests that match these patterns.</param>
        /// <param name="types">Array of types that will be used to filter out the requests that do not match the types. When
        /// not specified, all types will be matched. Can be <c>mainFrame</c>, <c>subFrame</c>, <c>stylesheet</c>, <c>script</c>, <c>image</c>, <c>font</c>, <c>object</c>, <c>xhr</c>, <c>ping</c>, <c>cspReport</c>, <c>media</c>
        /// or <c>webSocket</c>.</param>
        (urls: string[], ?excludeUrls: string[], ?types: Enums.Types.WebRequestFilter.Types[]) =
        class
        end

        /// <summary>
        /// Array of URL patterns used to include requests that match these patterns. Use the pattern <c>&lt;all_urls&gt;</c> to match all URLs.
        /// </summary>
        [<Erase>]
        member val urls: string[] = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Array of URL patterns used to exclude requests that match these patterns.
        /// </summary>
        [<Erase>]
        member val excludeUrls: string[] = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Array of types that will be used to filter out the requests that do not match the types. When not
        /// specified, all types will be matched. Can be <c>mainFrame</c>, <c>subFrame</c>, <c>stylesheet</c>, <c>script</c>, <c>image</c>, <c>font</c>, <c>object</c>, <c>xhr</c>, <c>ping</c>, <c>cspReport</c>, <c>media</c> or
        /// <c>webSocket</c>.
        /// </summary>
        [<Erase>]
        member val types: Enums.Types.WebRequestFilter.Types[] = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type WebPreferences
        /// <param name="devTools">Whether to enable DevTools. If it is set to <c>false</c>, can not use <c>BrowserWindow.webContents.openDevTools()</c> to open DevTools. Default is
        /// <c>true</c>.</param>
        /// <param name="nodeIntegration">Whether node integration is enabled. Default is <c>false</c>.</param>
        /// <param name="nodeIntegrationInWorker">Whether node integration is enabled in web workers. Default is <c>false</c>. More about this can be found in Multithreading.</param>
        /// <param name="nodeIntegrationInSubFrames">Experimental option for enabling Node.js support in sub-frames such as iframes and child windows. All your preloads will load
        /// for every iframe, you can use <c>process.isMainFrame</c> to determine if you are in the main frame or not.</param>
        /// <param name="preload">Specifies a script that will be loaded before other scripts run in the page. This script will always have
        /// access to node APIs no matter whether node integration is turned on or off. The value should be the absolute
        /// file path to the script. When node integration is turned off, the preload script can reintroduce Node global symbols back
        /// to the global scope. See example here.</param>
        /// <param name="sandbox">If set, this will sandbox the renderer associated with the window, making it compatible with the Chromium OS-level sandbox
        /// and disabling the Node.js engine. This is not the same as the <c>nodeIntegration</c> option and the APIs available to the
        /// preload script are more limited. Default is <c>true</c> since Electron 20. The sandbox will automatically be disabled when <c>nodeIntegration</c> is
        /// set to <c>true</c>. Read more about the option here.</param>
        /// <param name="session">Sets the session used by the page. Instead of passing the Session object directly, you can also choose to
        /// use the <c>partition</c> option instead, which accepts a partition string. When both <c>session</c> and <c>partition</c> are provided, <c>session</c> will be
        /// preferred. Default is the default session.</param>
        /// <param name="partition">Sets the session used by the page according to the session's partition string. If <c>partition</c> starts with <c>persist:</c>, the
        /// page will use a persistent session available to all pages in the app with the same <c>partition</c>. If there is
        /// no <c>persist:</c> prefix, the page will use an in-memory session. By assigning the same <c>partition</c>, multiple pages can share the
        /// same session. Default is the default session.</param>
        /// <param name="zoomFactor">The default zoom factor of the page, <c>3.0</c> represents <c>300%</c>. Default is <c>1.0</c>.</param>
        /// <param name="javascript">Enables JavaScript support. Default is <c>true</c>.</param>
        /// <param name="webSecurity">When <c>false</c>, it will disable the same-origin policy (usually using testing websites by people), and set <c>allowRunningInsecureContent</c> to <c>true</c>
        /// if this options has not been set by user. Default is <c>true</c>.</param>
        /// <param name="allowRunningInsecureContent">Allow an https page to run JavaScript, CSS or plugins from http URLs. Default is <c>false</c>.</param>
        /// <param name="images">Enables image support. Default is <c>true</c>.</param>
        /// <param name="imageAnimationPolicy">Specifies how to run image animations (E.g. GIFs).  Can be <c>animate</c>, <c>animateOnce</c> or <c>noAnimation</c>.  Default is <c>animate</c>.</param>
        /// <param name="textAreasAreResizable">Make TextArea elements resizable. Default is <c>true</c>.</param>
        /// <param name="webgl">Enables WebGL support. Default is <c>true</c>.</param>
        /// <param name="plugins">Whether plugins should be enabled. Default is <c>false</c>.</param>
        /// <param name="experimentalFeatures">Enables Chromium's experimental features. Default is <c>false</c>.</param>
        /// <param name="scrollBounce">⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌ || Enables scroll bounce (rubber
        /// banding) effect on macOS. Default is <c>false</c>.</param>
        /// <param name="enableBlinkFeatures">A list of feature strings separated by <c>,</c>, like <c>CSSVariables,KeyboardEventKey</c> to enable. The full list of supported feature strings
        /// can be found in the RuntimeEnabledFeatures.json5 file.</param>
        /// <param name="disableBlinkFeatures">A list of feature strings separated by <c>,</c>, like <c>CSSVariables,KeyboardEventKey</c> to disable. The full list of supported feature strings
        /// can be found in the RuntimeEnabledFeatures.json5 file.</param>
        /// <param name="defaultFontFamily">Sets the default font for the font-family.</param>
        /// <param name="defaultFontSize">Defaults to <c>16</c>.</param>
        /// <param name="defaultMonospaceFontSize">Defaults to <c>13</c>.</param>
        /// <param name="minimumFontSize">Defaults to <c>0</c>.</param>
        /// <param name="defaultEncoding">Defaults to <c>ISO-8859-1</c>.</param>
        /// <param name="backgroundThrottling">Whether to throttle animations and timers when the page becomes background. This also affects the Page Visibility API. When
        /// at least one webContents displayed in a single browserWindow has disabled <c>backgroundThrottling</c> then frames will be drawn and swapped for
        /// the whole window and other webContents displayed by it. Defaults to <c>true</c>.</param>
        /// <param name="offscreen">Whether to enable offscreen rendering for the browser window. Defaults to <c>false</c>. See the offscreen rendering tutorial for more
        /// details.</param>
        /// <param name="contextIsolation">Whether to run Electron APIs and the specified <c>preload</c> script in a separate JavaScript context. Defaults to <c>true</c>. The
        /// context that the <c>preload</c> script runs in will only have access to its own dedicated <c>document</c> and <c>window</c> globals, as
        /// well as its own set of JavaScript builtins (<c>Array</c>, <c>Object</c>, <c>JSON</c>, etc.), which are all invisible to the loaded content.
        /// The Electron API will only be available in the <c>preload</c> script and not the loaded page. This option should be
        /// used when loading potentially untrusted remote content to ensure the loaded content cannot tamper with the <c>preload</c> script and any
        /// Electron APIs being used.  This option uses the same technique used by Chrome Content Scripts.  You can access
        /// this context in the dev tools by selecting the 'Electron Isolated Context' entry in the combo box at the top
        /// of the Console tab.</param>
        /// <param name="webviewTag">Whether to enable the <c>&lt;webview&gt;</c> tag. Defaults to <c>false</c>. **Note:** The <c>preload</c> script configured for the <c>&lt;webview&gt;</c> will have
        /// node integration enabled when it is executed so you should ensure remote/untrusted content is not able to create a <c>&lt;webview&gt;</c>
        /// tag with a possibly malicious <c>preload</c> script. You can use the <c>will-attach-webview</c> event on webContents to strip away the <c>preload</c>
        /// script and to validate or alter the <c>&lt;webview&gt;</c>'s initial settings.</param>
        /// <param name="additionalArguments">A list of strings that will be appended to <c>process.argv</c> in the renderer process of this app.  Useful
        /// for passing small bits of data down to renderer process preload scripts.</param>
        /// <param name="safeDialogs">Whether to enable browser style consecutive dialog protection. Default is <c>false</c>.</param>
        /// <param name="safeDialogsMessage">The message to display when consecutive dialog protection is triggered. If not defined the default message would be used,
        /// note that currently the default message is in English and not localized.</param>
        /// <param name="disableDialogs">Whether to disable dialogs completely. Overrides <c>safeDialogs</c>. Default is <c>false</c>.</param>
        /// <param name="navigateOnDragDrop">Whether dragging and dropping a file or link onto the page causes a navigation. Default is <c>false</c>.</param>
        /// <param name="autoplayPolicy">Autoplay policy to apply to content in the window, can be <c>no-user-gesture-required</c>, <c>user-gesture-required</c>, <c>document-user-activation-required</c>. Defaults to <c>no-user-gesture-required</c>.</param>
        /// <param name="disableHtmlFullscreenWindowResize">Whether to prevent the window from resizing when entering HTML Fullscreen. Default is <c>false</c>.</param>
        /// <param name="accessibleTitle">An alternative title string provided only to accessibility tools such as screen readers. This string is not directly visible
        /// to users.</param>
        /// <param name="spellcheck">Whether to enable the builtin spellchecker. Default is <c>true</c>.</param>
        /// <param name="enableWebSQL">Whether to enable the WebSQL api. Default is <c>true</c>.</param>
        /// <param name="v8CacheOptions">Enforces the v8 code caching policy used by blink. Accepted values are</param>
        /// <param name="enablePreferredSizeMode">Whether to enable preferred size mode. The preferred size is the minimum size needed to contain the layout of
        /// the document—without requiring scrolling. Enabling this will cause the <c>preferred-size-changed</c> event to be emitted on the <c>WebContents</c> when the preferred
        /// size changes. Default is <c>false</c>.</param>
        /// <param name="transparent">Whether to enable background transparency for the guest page. Default is <c>true</c>. **Note:** The guest page's text and background
        /// colors are derived from the color scheme of its root element. When transparency is enabled, the text color will still
        /// change accordingly but the background will remain transparent.</param>
        /// <param name="enableDeprecatedPaste">Whether to enable the <c>paste</c> execCommand. Default is <c>false</c>.</param>
        (
            ?devTools: bool,
            ?nodeIntegration: bool,
            ?nodeIntegrationInWorker: bool,
            ?nodeIntegrationInSubFrames: bool,
            ?preload: string,
            ?sandbox: bool,
            ?session: Session,
            ?partition: string,
            ?zoomFactor: float,
            ?javascript: bool,
            ?webSecurity: bool,
            ?allowRunningInsecureContent: bool,
            ?images: bool,
            ?imageAnimationPolicy: Enums.Types.WebPreferences.ImageAnimationPolicy,
            ?textAreasAreResizable: bool,
            ?webgl: bool,
            ?plugins: bool,
            ?experimentalFeatures: bool
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
            ,
            ?scrollBounce: bool
            #endif
            ,
            ?enableBlinkFeatures: string,
            ?disableBlinkFeatures: string,
            ?defaultFontFamily: Types.WebPreferences.DefaultFontFamily,
            ?defaultFontSize: int,
            ?defaultMonospaceFontSize: int,
            ?minimumFontSize: int,
            ?defaultEncoding: string,
            ?backgroundThrottling: bool,
            ?offscreen: U2<Types.WebPreferences.Offscreen, bool>,
            ?contextIsolation: bool,
            ?webviewTag: bool,
            ?additionalArguments: string[],
            ?safeDialogs: bool,
            ?safeDialogsMessage: string,
            ?disableDialogs: bool,
            ?navigateOnDragDrop: bool,
            ?autoplayPolicy: Enums.Types.WebPreferences.AutoplayPolicy,
            ?disableHtmlFullscreenWindowResize: bool,
            ?accessibleTitle: string,
            ?spellcheck: bool,
            ?enableWebSQL: bool,
            ?v8CacheOptions: Enums.Types.WebPreferences.V8CacheOptions,
            ?enablePreferredSizeMode: bool,
            ?transparent: bool,
            ?enableDeprecatedPaste: bool
        ) =
        class
        end

        /// <summary>
        /// Whether to enable DevTools. If it is set to <c>false</c>, can not use <c>BrowserWindow.webContents.openDevTools()</c> to open DevTools. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val devTools: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether node integration is enabled. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val nodeIntegration: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether node integration is enabled in web workers. Default is <c>false</c>. More about this can be found in Multithreading.
        /// </summary>
        [<Erase>]
        member val nodeIntegrationInWorker: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Experimental option for enabling Node.js support in sub-frames such as iframes and child windows. All your preloads will load for
        /// every iframe, you can use <c>process.isMainFrame</c> to determine if you are in the main frame or not.
        /// </summary>
        [<Erase>]
        member val nodeIntegrationInSubFrames: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Specifies a script that will be loaded before other scripts run in the page. This script will always have access
        /// to node APIs no matter whether node integration is turned on or off. The value should be the absolute file
        /// path to the script. When node integration is turned off, the preload script can reintroduce Node global symbols back to
        /// the global scope. See example here.
        /// </summary>
        [<Erase>]
        member val preload: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// If set, this will sandbox the renderer associated with the window, making it compatible with the Chromium OS-level sandbox and
        /// disabling the Node.js engine. This is not the same as the <c>nodeIntegration</c> option and the APIs available to the preload
        /// script are more limited. Default is <c>true</c> since Electron 20. The sandbox will automatically be disabled when <c>nodeIntegration</c> is set
        /// to <c>true</c>. Read more about the option here.
        /// </summary>
        [<Erase>]
        member val sandbox: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Sets the session used by the page. Instead of passing the Session object directly, you can also choose to use
        /// the <c>partition</c> option instead, which accepts a partition string. When both <c>session</c> and <c>partition</c> are provided, <c>session</c> will be preferred.
        /// Default is the default session.
        /// </summary>
        [<Erase>]
        member val session: Session = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Sets the session used by the page according to the session's partition string. If <c>partition</c> starts with <c>persist:</c>, the page
        /// will use a persistent session available to all pages in the app with the same <c>partition</c>. If there is no
        /// <c>persist:</c> prefix, the page will use an in-memory session. By assigning the same <c>partition</c>, multiple pages can share the same
        /// session. Default is the default session.
        /// </summary>
        [<Erase>]
        member val partition: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The default zoom factor of the page, <c>3.0</c> represents <c>300%</c>. Default is <c>1.0</c>.
        /// </summary>
        [<Erase>]
        member val zoomFactor: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Enables JavaScript support. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val javascript: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// When <c>false</c>, it will disable the same-origin policy (usually using testing websites by people), and set <c>allowRunningInsecureContent</c> to <c>true</c> if
        /// this options has not been set by user. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val webSecurity: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Allow an https page to run JavaScript, CSS or plugins from http URLs. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val allowRunningInsecureContent: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Enables image support. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val images: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Specifies how to run image animations (E.g. GIFs).  Can be <c>animate</c>, <c>animateOnce</c> or <c>noAnimation</c>.  Default is <c>animate</c>.
        /// </summary>
        [<Erase>]
        member val imageAnimationPolicy: Enums.Types.WebPreferences.ImageAnimationPolicy =
            Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Make TextArea elements resizable. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val textAreasAreResizable: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Enables WebGL support. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val webgl: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether plugins should be enabled. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val plugins: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Enables Chromium's experimental features. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val experimentalFeatures: bool = Unchecked.defaultof<_> with get, set
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Enables scroll bounce (rubber banding) effect on macOS. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val scrollBounce: bool = Unchecked.defaultof<_> with get, set
        #endif


        /// <summary>
        /// A list of feature strings separated by <c>,</c>, like <c>CSSVariables,KeyboardEventKey</c> to enable. The full list of supported feature strings can
        /// be found in the RuntimeEnabledFeatures.json5 file.
        /// </summary>
        [<Erase>]
        member val enableBlinkFeatures: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// A list of feature strings separated by <c>,</c>, like <c>CSSVariables,KeyboardEventKey</c> to disable. The full list of supported feature strings can
        /// be found in the RuntimeEnabledFeatures.json5 file.
        /// </summary>
        [<Erase>]
        member val disableBlinkFeatures: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Sets the default font for the font-family.
        /// </summary>
        [<Erase>]
        member val defaultFontFamily: Types.WebPreferences.DefaultFontFamily = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Defaults to <c>16</c>.
        /// </summary>
        [<Erase>]
        member val defaultFontSize: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Defaults to <c>13</c>.
        /// </summary>
        [<Erase>]
        member val defaultMonospaceFontSize: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Defaults to <c>0</c>.
        /// </summary>
        [<Erase>]
        member val minimumFontSize: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Defaults to <c>ISO-8859-1</c>.
        /// </summary>
        [<Erase>]
        member val defaultEncoding: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether to throttle animations and timers when the page becomes background. This also affects the Page Visibility API. When at
        /// least one webContents displayed in a single browserWindow has disabled <c>backgroundThrottling</c> then frames will be drawn and swapped for the
        /// whole window and other webContents displayed by it. Defaults to <c>true</c>.
        /// </summary>
        [<Erase>]
        member val backgroundThrottling: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether to enable offscreen rendering for the browser window. Defaults to <c>false</c>. See the offscreen rendering tutorial for more details.
        /// </summary>
        [<Erase>]
        member val offscreen: U2<Types.WebPreferences.Offscreen, bool> = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether to run Electron APIs and the specified <c>preload</c> script in a separate JavaScript context. Defaults to <c>true</c>. The context
        /// that the <c>preload</c> script runs in will only have access to its own dedicated <c>document</c> and <c>window</c> globals, as well
        /// as its own set of JavaScript builtins (<c>Array</c>, <c>Object</c>, <c>JSON</c>, etc.), which are all invisible to the loaded content. The
        /// Electron API will only be available in the <c>preload</c> script and not the loaded page. This option should be used
        /// when loading potentially untrusted remote content to ensure the loaded content cannot tamper with the <c>preload</c> script and any Electron
        /// APIs being used.  This option uses the same technique used by Chrome Content Scripts.  You can access this
        /// context in the dev tools by selecting the 'Electron Isolated Context' entry in the combo box at the top of
        /// the Console tab.
        /// </summary>
        [<Erase>]
        member val contextIsolation: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether to enable the <c>&lt;webview&gt;</c> tag. Defaults to <c>false</c>. **Note:** The <c>preload</c> script configured for the <c>&lt;webview&gt;</c> will have node
        /// integration enabled when it is executed so you should ensure remote/untrusted content is not able to create a <c>&lt;webview&gt;</c> tag
        /// with a possibly malicious <c>preload</c> script. You can use the <c>will-attach-webview</c> event on webContents to strip away the <c>preload</c> script
        /// and to validate or alter the <c>&lt;webview&gt;</c>'s initial settings.
        /// </summary>
        [<Erase>]
        member val webviewTag: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// A list of strings that will be appended to <c>process.argv</c> in the renderer process of this app.  Useful for
        /// passing small bits of data down to renderer process preload scripts.
        /// </summary>
        [<Erase>]
        member val additionalArguments: string[] = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether to enable browser style consecutive dialog protection. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val safeDialogs: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The message to display when consecutive dialog protection is triggered. If not defined the default message would be used, note
        /// that currently the default message is in English and not localized.
        /// </summary>
        [<Erase>]
        member val safeDialogsMessage: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether to disable dialogs completely. Overrides <c>safeDialogs</c>. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val disableDialogs: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether dragging and dropping a file or link onto the page causes a navigation. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val navigateOnDragDrop: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Autoplay policy to apply to content in the window, can be <c>no-user-gesture-required</c>, <c>user-gesture-required</c>, <c>document-user-activation-required</c>. Defaults to <c>no-user-gesture-required</c>.
        /// </summary>
        [<Erase>]
        member val autoplayPolicy: Enums.Types.WebPreferences.AutoplayPolicy = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether to prevent the window from resizing when entering HTML Fullscreen. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val disableHtmlFullscreenWindowResize: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// An alternative title string provided only to accessibility tools such as screen readers. This string is not directly visible to
        /// users.
        /// </summary>
        [<Erase>]
        member val accessibleTitle: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether to enable the builtin spellchecker. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val spellcheck: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether to enable the WebSQL api. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val enableWebSQL: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Enforces the v8 code caching policy used by blink. Accepted values are
        /// </summary>
        [<Erase>]
        member val v8CacheOptions: Enums.Types.WebPreferences.V8CacheOptions = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether to enable preferred size mode. The preferred size is the minimum size needed to contain the layout of the
        /// document—without requiring scrolling. Enabling this will cause the <c>preferred-size-changed</c> event to be emitted on the <c>WebContents</c> when the preferred size
        /// changes. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val enablePreferredSizeMode: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether to enable background transparency for the guest page. Default is <c>true</c>. **Note:** The guest page's text and background colors
        /// are derived from the color scheme of its root element. When transparency is enabled, the text color will still change
        /// accordingly but the background will remain transparent.
        /// </summary>
        [<Erase>]
        member val transparent: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether to enable the <c>paste</c> execCommand. Default is <c>false</c>.
        /// </summary>
        [<Erase; System.Obsolete>]
        member val enableDeprecatedPaste: bool = Unchecked.defaultof<_> with get, set

    /// <summary>
    /// This type is a helper alias, no object will ever exist of this type.
    /// </summary>
    [<JS.Pojo>]
    type UserDefaultTypes
        /// <param name="string"></param>
        /// <param name="boolean"></param>
        /// <param name="integer"></param>
        /// <param name="float"></param>
        /// <param name="double"></param>
        /// <param name="url"></param>
        /// <param name="array"></param>
        /// <param name="dictionary"></param>
        (
            string: string,
            boolean: bool,
            integer: float,
            float: float,
            double: float,
            url: string,
            array: obj[],
            dictionary: Record<string, obj>
        ) =
        class
        end

        [<Erase>]
        member val string: string = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val boolean: bool = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val integer: float = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val float: float = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val double: float = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val url: string = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val array: obj[] = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val dictionary: Record<string, obj> = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type USBDevice
        /// <param name="configuration">A USBConfiguration object containing information about the currently selected configuration of a USB device.</param>
        /// <param name="configurations">An array of USBConfiguration interfaces for controlling a paired USB device.</param>
        /// <param name="deviceClass">The device class for the communication interface supported by the device.</param>
        /// <param name="deviceId">Unique identifier for the device.</param>
        /// <param name="deviceProtocol">The device protocol for the communication interface supported by the device.</param>
        /// <param name="deviceSubclass">The device subclass for the communication interface supported by the device.</param>
        /// <param name="deviceVersionMajor">The major version number of the device as defined by the device manufacturer.</param>
        /// <param name="deviceVersionMinor">The minor version number of the device as defined by the device manufacturer.</param>
        /// <param name="deviceVersionSubminor">The subminor version number of the device as defined by the device manufacturer.</param>
        /// <param name="manufacturerName">The manufacturer name of the device.</param>
        /// <param name="productId">The USB product ID.</param>
        /// <param name="productName">Name of the device.</param>
        /// <param name="serialNumber">The USB device serial number.</param>
        /// <param name="usbVersionMajor">The USB protocol major version supported by the device.</param>
        /// <param name="usbVersionMinor">The USB protocol minor version supported by the device.</param>
        /// <param name="usbVersionSubminor">The USB protocol subminor version supported by the device.</param>
        /// <param name="vendorId">The USB vendor ID.</param>
        (
            configurations: obj[],
            deviceClass: int,
            deviceId: string,
            deviceProtocol: int,
            deviceSubclass: int,
            deviceVersionMajor: int,
            deviceVersionMinor: int,
            deviceVersionSubminor: int,
            productId: int,
            usbVersionMajor: int,
            usbVersionMinor: int,
            usbVersionSubminor: int,
            vendorId: int,
            ?configuration: Types.USBDevice.Configuration,
            ?manufacturerName: string,
            ?productName: string,
            ?serialNumber: string
        ) =
        class
        end

        /// <summary>
        /// A USBConfiguration object containing information about the currently selected configuration of a USB device.
        /// </summary>
        [<Erase>]
        member val configuration: Types.USBDevice.Configuration = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// An array of USBConfiguration interfaces for controlling a paired USB device.
        /// </summary>
        [<Erase>]
        member val configurations: obj[] = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The device class for the communication interface supported by the device.
        /// </summary>
        [<Erase>]
        member val deviceClass: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Unique identifier for the device.
        /// </summary>
        [<Erase>]
        member val deviceId: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The device protocol for the communication interface supported by the device.
        /// </summary>
        [<Erase>]
        member val deviceProtocol: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The device subclass for the communication interface supported by the device.
        /// </summary>
        [<Erase>]
        member val deviceSubclass: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The major version number of the device as defined by the device manufacturer.
        /// </summary>
        [<Erase>]
        member val deviceVersionMajor: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The minor version number of the device as defined by the device manufacturer.
        /// </summary>
        [<Erase>]
        member val deviceVersionMinor: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The subminor version number of the device as defined by the device manufacturer.
        /// </summary>
        [<Erase>]
        member val deviceVersionSubminor: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The manufacturer name of the device.
        /// </summary>
        [<Erase>]
        member val manufacturerName: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The USB product ID.
        /// </summary>
        [<Erase>]
        member val productId: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Name of the device.
        /// </summary>
        [<Erase>]
        member val productName: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The USB device serial number.
        /// </summary>
        [<Erase>]
        member val serialNumber: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The USB protocol major version supported by the device.
        /// </summary>
        [<Erase>]
        member val usbVersionMajor: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The USB protocol minor version supported by the device.
        /// </summary>
        [<Erase>]
        member val usbVersionMinor: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The USB protocol subminor version supported by the device.
        /// </summary>
        [<Erase>]
        member val usbVersionSubminor: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The USB vendor ID.
        /// </summary>
        [<Erase>]
        member val vendorId: int = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type UploadRawData
        /// <param name="type"><c>rawData</c>.</param>
        /// <param name="bytes">Data to be uploaded.</param>
        (``type``: string, bytes: Buffer) =
        class
        end

        /// <summary>
        /// <c>rawData</c>.
        /// </summary>
        [<Erase>]
        member val ``type``: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Data to be uploaded.
        /// </summary>
        [<Erase>]
        member val bytes: Buffer = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type UploadFile
        /// <param name="type"><c>file</c>.</param>
        /// <param name="filePath">Path of file to be uploaded.</param>
        /// <param name="offset">Defaults to <c>0</c>.</param>
        /// <param name="length">Number of bytes to read from <c>offset</c>. Defaults to <c>0</c>.</param>
        /// <param name="modificationTime">Last Modification time in number of seconds since the UNIX epoch. Defaults to <c>0</c>.</param>
        (``type``: string, filePath: string, ?offset: int, ?length: int, ?modificationTime: double) =
        class
        end

        /// <summary>
        /// <c>file</c>.
        /// </summary>
        [<Erase>]
        member val ``type``: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Path of file to be uploaded.
        /// </summary>
        [<Erase>]
        member val filePath: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Defaults to <c>0</c>.
        /// </summary>
        [<Erase>]
        member val offset: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Number of bytes to read from <c>offset</c>. Defaults to <c>0</c>.
        /// </summary>
        [<Erase>]
        member val length: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Last Modification time in number of seconds since the UNIX epoch. Defaults to <c>0</c>.
        /// </summary>
        [<Erase>]
        member val modificationTime: double = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type UploadData
        /// <param name="bytes">Content being sent.</param>
        /// <param name="file">Path of file being uploaded.</param>
        /// <param name="blobUUID">UUID of blob data. Use ses.getBlobData method to retrieve the data.</param>
        (bytes: Buffer, ?file: string, ?blobUUID: string) =
        class
        end

        /// <summary>
        /// Content being sent.
        /// </summary>
        [<Erase>]
        member val bytes: Buffer = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Path of file being uploaded.
        /// </summary>
        [<Erase>]
        member val file: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// UUID of blob data. Use ses.getBlobData method to retrieve the data.
        /// </summary>
        [<Erase>]
        member val blobUUID: string = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type Transaction
        /// <param name="transactionIdentifier">A string that uniquely identifies a successful payment transaction.</param>
        /// <param name="transactionDate">The date the transaction was added to the App Store’s payment queue.</param>
        /// <param name="originalTransactionIdentifier">The identifier of the restored transaction by the App Store.</param>
        /// <param name="transactionState">The transaction state, can be <c>purchasing</c>, <c>purchased</c>, <c>failed</c>, <c>restored</c> or <c>deferred</c>.</param>
        /// <param name="errorCode">The error code if an error occurred while processing the transaction.</param>
        /// <param name="errorMessage">The error message if an error occurred while processing the transaction.</param>
        /// <param name="payment"></param>
        (
            transactionIdentifier: string,
            transactionDate: string,
            originalTransactionIdentifier: string,
            transactionState: Enums.Types.Transaction.TransactionState,
            errorCode: int,
            errorMessage: string,
            payment: Types.Transaction.Payment
        ) =
        class
        end

        /// <summary>
        /// A string that uniquely identifies a successful payment transaction.
        /// </summary>
        [<Erase>]
        member val transactionIdentifier: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The date the transaction was added to the App Store’s payment queue.
        /// </summary>
        [<Erase>]
        member val transactionDate: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The identifier of the restored transaction by the App Store.
        /// </summary>
        [<Erase>]
        member val originalTransactionIdentifier: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The transaction state, can be <c>purchasing</c>, <c>purchased</c>, <c>failed</c>, <c>restored</c> or <c>deferred</c>.
        /// </summary>
        [<Erase>]
        member val transactionState: Enums.Types.Transaction.TransactionState = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The error code if an error occurred while processing the transaction.
        /// </summary>
        [<Erase>]
        member val errorCode: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The error message if an error occurred while processing the transaction.
        /// </summary>
        [<Erase>]
        member val errorMessage: string = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val payment: Types.Transaction.Payment = Unchecked.defaultof<_> with get, set

    /// <summary>
    /// An example TraceConfig that roughly matches what Chrome DevTools records:
    /// </summary>
    [<JS.Pojo>]
    type TraceConfig
        /// <param name="recordingMode">Can be <c>record-until-full</c>, <c>record-continuously</c>, <c>record-as-much-as-possible</c> or <c>trace-to-console</c>. Defaults to <c>record-until-full</c>.</param>
        /// <param name="traceBufferSizeInKb">maximum size of the trace recording buffer in kilobytes. Defaults to 100MB.</param>
        /// <param name="traceBufferSizeInEvents">maximum size of the trace recording buffer in events.</param>
        /// <param name="enableArgumentFilter">if true, filter event data according to a specific list of events that have been manually vetted to not
        /// include any PII. See the implementation in Chromium for specifics.</param>
        /// <param name="includedCategories">a list of tracing categories to include. Can include glob-like patterns using <c>*</c> at the end of the category
        /// name. See tracing categories for the list of categories.</param>
        /// <param name="excludedCategories">a list of tracing categories to exclude. Can include glob-like patterns using <c>*</c> at the end of the category
        /// name. See tracing categories for the list of categories.</param>
        /// <param name="includedProcessIds">a list of process IDs to include in the trace. If not specified, trace all processes.</param>
        /// <param name="histogramNames">a list of histogram names to report with the trace.</param>
        /// <param name="memoryDumpConfig">if the <c>disabled-by-default-memory-infra</c> category is enabled, this contains optional additional configuration for data collection. See the Chromium memory-infra docs
        /// for more information.</param>
        (
            ?recordingMode: Enums.Types.TraceConfig.RecordingMode,
            ?traceBufferSizeInKb: float,
            ?traceBufferSizeInEvents: float,
            ?enableArgumentFilter: bool,
            ?includedCategories: string[],
            ?excludedCategories: string[],
            ?includedProcessIds: float[],
            ?histogramNames: string[],
            ?memoryDumpConfig: Record<string, obj>
        ) =
        class
        end

        /// <summary>
        /// Can be <c>record-until-full</c>, <c>record-continuously</c>, <c>record-as-much-as-possible</c> or <c>trace-to-console</c>. Defaults to <c>record-until-full</c>.
        /// </summary>
        [<Erase; Emit("$0.recording_mode{{ = $1 }}")>]
        member _.recordingMode
            with get (): Enums.Types.TraceConfig.RecordingMode = Unchecked.defaultof<_>
            and set (value: Enums.Types.TraceConfig.RecordingMode) = ()

        /// <summary>
        /// maximum size of the trace recording buffer in kilobytes. Defaults to 100MB.
        /// </summary>
        [<Erase; Emit("$0.trace_buffer_size_in_kb{{ = $1 }}")>]
        member _.traceBufferSizeInKb
            with get (): float = Unchecked.defaultof<_>
            and set (value: float) = ()

        /// <summary>
        /// maximum size of the trace recording buffer in events.
        /// </summary>
        [<Erase; Emit("$0.trace_buffer_size_in_events{{ = $1 }}")>]
        member _.traceBufferSizeInEvents
            with get (): float = Unchecked.defaultof<_>
            and set (value: float) = ()

        /// <summary>
        /// if true, filter event data according to a specific list of events that have been manually vetted to not include
        /// any PII. See the implementation in Chromium for specifics.
        /// </summary>
        [<Erase; Emit("$0.enable_argument_filter{{ = $1 }}")>]
        member _.enableArgumentFilter
            with get (): bool = Unchecked.defaultof<_>
            and set (value: bool) = ()

        /// <summary>
        /// a list of tracing categories to include. Can include glob-like patterns using <c>*</c> at the end of the category name.
        /// See tracing categories for the list of categories.
        /// </summary>
        [<Erase; Emit("$0.included_categories{{ = $1 }}")>]
        member _.includedCategories
            with get (): string[] = Unchecked.defaultof<_>
            and set (value: string[]) = ()

        /// <summary>
        /// a list of tracing categories to exclude. Can include glob-like patterns using <c>*</c> at the end of the category name.
        /// See tracing categories for the list of categories.
        /// </summary>
        [<Erase; Emit("$0.excluded_categories{{ = $1 }}")>]
        member _.excludedCategories
            with get (): string[] = Unchecked.defaultof<_>
            and set (value: string[]) = ()

        /// <summary>
        /// a list of process IDs to include in the trace. If not specified, trace all processes.
        /// </summary>
        [<Erase; Emit("$0.included_process_ids{{ = $1 }}")>]
        member _.includedProcessIds
            with get (): float[] = Unchecked.defaultof<_>
            and set (value: float[]) = ()

        /// <summary>
        /// a list of histogram names to report with the trace.
        /// </summary>
        [<Erase; Emit("$0.histogram_names{{ = $1 }}")>]
        member _.histogramNames
            with get (): string[] = Unchecked.defaultof<_>
            and set (value: string[]) = ()

        /// <summary>
        /// if the <c>disabled-by-default-memory-infra</c> category is enabled, this contains optional additional configuration for data collection. See the Chromium memory-infra docs for
        /// more information.
        /// </summary>
        [<Erase; Emit("$0.memory_dump_config{{ = $1 }}")>]
        member _.memoryDumpConfig
            with get (): Record<string, obj> = Unchecked.defaultof<_>
            and set (value: Record<string, obj>) = ()

    [<JS.Pojo>]
    type TraceCategoriesAndOptions
        /// <param name="categoryFilter">A filter to control what category groups should be traced. A filter can have an optional '-' prefix to
        /// exclude category groups that contain a matching category. Having both included and excluded category patterns in the same list is
        /// not supported. Examples: <c>test_MyTest*</c>, <c>test_MyTest*,test_OtherStuff</c>, <c>-excluded_category1,-excluded_category2</c>.</param>
        /// <param name="traceOptions">Controls what kind of tracing is enabled, it is a comma-delimited sequence of the following strings: <c>record-until-full</c>, <c>record-continuously</c>, <c>trace-to-console</c>,
        /// <c>enable-sampling</c>, <c>enable-systrace</c>, e.g. <c>'record-until-full,enable-sampling'</c>. The first 3 options are trace recording modes and hence mutually exclusive. If more than one
        /// trace recording modes appear in the <c>traceOptions</c> string, the last one takes precedence. If none of the trace recording modes
        /// are specified, recording mode is <c>record-until-full</c>. The trace option will first be reset to the default option (<c>record_mode</c> set to
        /// <c>record-until-full</c>, <c>enable_sampling</c> and <c>enable_systrace</c> set to <c>false</c>) before options parsed from <c>traceOptions</c> are applied on it.</param>
        (categoryFilter: string, traceOptions: string) =
        class
        end

        /// <summary>
        /// A filter to control what category groups should be traced. A filter can have an optional '-' prefix to exclude
        /// category groups that contain a matching category. Having both included and excluded category patterns in the same list is not
        /// supported. Examples: <c>test_MyTest*</c>, <c>test_MyTest*,test_OtherStuff</c>, <c>-excluded_category1,-excluded_category2</c>.
        /// </summary>
        [<Erase>]
        member val categoryFilter: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Controls what kind of tracing is enabled, it is a comma-delimited sequence of the following strings: <c>record-until-full</c>, <c>record-continuously</c>, <c>trace-to-console</c>, <c>enable-sampling</c>,
        /// <c>enable-systrace</c>, e.g. <c>'record-until-full,enable-sampling'</c>. The first 3 options are trace recording modes and hence mutually exclusive. If more than one trace
        /// recording modes appear in the <c>traceOptions</c> string, the last one takes precedence. If none of the trace recording modes are
        /// specified, recording mode is <c>record-until-full</c>. The trace option will first be reset to the default option (<c>record_mode</c> set to <c>record-until-full</c>,
        /// <c>enable_sampling</c> and <c>enable_systrace</c> set to <c>false</c>) before options parsed from <c>traceOptions</c> are applied on it.
        /// </summary>
        [<Erase>]
        member val traceOptions: string = Unchecked.defaultof<_> with get, set

    /// <summary>
    /// The <c>flags</c> is an array that can include following <c>string</c>s:<br/><br/>* <c>enabled</c> - The button is active and available to the
    /// user.<br/>* <c>disabled</c> - The button is disabled. It is present, but has a visual state indicating it will not respond
    /// to user action.<br/>* <c>dismissonclick</c> - When the button is clicked, the thumbnail window closes immediately.<br/>* <c>nobackground</c> - Do not draw
    /// a button border, use only the image.<br/>* <c>hidden</c> - The button is not shown to the user.<br/>* <c>noninteractive</c> - The
    /// button is enabled but not interactive; no pressed button state is drawn. This value is intended for instances where the
    /// button is used in a notification.
    /// </summary>
    [<JS.Pojo>]
    type ThumbarButton
        /// <param name="icon">The icon showing in thumbnail toolbar.</param>
        /// <param name="click"></param>
        /// <param name="tooltip">The text of the button's tooltip.</param>
        /// <param name="flags">Control specific states and behaviors of the button. By default, it is <c>['enabled']</c>.</param>
        (icon: NativeImage, click: unit -> unit, ?tooltip: string, ?flags: string[]) =
        class
        end

        /// <summary>
        /// The icon showing in thumbnail toolbar.
        /// </summary>
        [<Erase>]
        member val icon: NativeImage = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val click: unit -> unit = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The text of the button's tooltip.
        /// </summary>
        [<Erase>]
        member val tooltip: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Control specific states and behaviors of the button. By default, it is <c>['enabled']</c>.
        /// </summary>
        [<Erase>]
        member val flags: string[] = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type Task
        /// <param name="program">Path of the program to execute, usually you should specify <c>process.execPath</c> which opens the current program.</param>
        /// <param name="arguments">The command line arguments when <c>program</c> is executed.</param>
        /// <param name="title">The string to be displayed in a JumpList.</param>
        /// <param name="description">Description of this task.</param>
        /// <param name="iconPath">The absolute path to an icon to be displayed in a JumpList, which can be an arbitrary resource file
        /// that contains an icon. You can usually specify <c>process.execPath</c> to show the icon of the program.</param>
        /// <param name="iconIndex">The icon index in the icon file. If an icon file consists of two or more icons, set this
        /// value to identify the icon. If an icon file consists of one icon, this value is 0.</param>
        /// <param name="workingDirectory">The working directory. Default is empty.</param>
        (
            program: string,
            arguments: string,
            title: string,
            description: string,
            iconPath: string,
            iconIndex: float,
            ?workingDirectory: string
        ) =
        class
        end

        /// <summary>
        /// Path of the program to execute, usually you should specify <c>process.execPath</c> which opens the current program.
        /// </summary>
        [<Erase>]
        member val program: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The command line arguments when <c>program</c> is executed.
        /// </summary>
        [<Erase>]
        member val arguments: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The string to be displayed in a JumpList.
        /// </summary>
        [<Erase>]
        member val title: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Description of this task.
        /// </summary>
        [<Erase>]
        member val description: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The absolute path to an icon to be displayed in a JumpList, which can be an arbitrary resource file that
        /// contains an icon. You can usually specify <c>process.execPath</c> to show the icon of the program.
        /// </summary>
        [<Erase>]
        member val iconPath: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The icon index in the icon file. If an icon file consists of two or more icons, set this value
        /// to identify the icon. If an icon file consists of one icon, this value is 0.
        /// </summary>
        [<Erase>]
        member val iconIndex: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The working directory. Default is empty.
        /// </summary>
        [<Erase>]
        member val workingDirectory: string = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type Size
        /// <param name="width"></param>
        /// <param name="height"></param>
        (width: float, height: float) =
        class
        end

        [<Erase>]
        member val width: float = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val height: float = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type ShortcutDetails
        /// <param name="target">The target to launch from this shortcut.</param>
        /// <param name="cwd">The working directory. Default is empty.</param>
        /// <param name="args">The arguments to be applied to <c>target</c> when launching from this shortcut. Default is empty.</param>
        /// <param name="description">The description of the shortcut. Default is empty.</param>
        /// <param name="icon">The path to the icon, can be a DLL or EXE. <c>icon</c> and <c>iconIndex</c> have to be set together.
        /// Default is empty, which uses the target's icon.</param>
        /// <param name="iconIndex">The resource ID of icon when <c>icon</c> is a DLL or EXE. Default is 0.</param>
        /// <param name="appUserModelId">The Application User Model ID. Default is empty.</param>
        /// <param name="toastActivatorClsid">The Application Toast Activator CLSID. Needed for participating in Action Center.</param>
        (
            target: string,
            ?cwd: string,
            ?args: string,
            ?description: string,
            ?icon: string,
            ?iconIndex: float,
            ?appUserModelId: string,
            ?toastActivatorClsid: string
        ) =
        class
        end

        /// <summary>
        /// The target to launch from this shortcut.
        /// </summary>
        [<Erase>]
        member val target: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The working directory. Default is empty.
        /// </summary>
        [<Erase>]
        member val cwd: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The arguments to be applied to <c>target</c> when launching from this shortcut. Default is empty.
        /// </summary>
        [<Erase>]
        member val args: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The description of the shortcut. Default is empty.
        /// </summary>
        [<Erase>]
        member val description: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The path to the icon, can be a DLL or EXE. <c>icon</c> and <c>iconIndex</c> have to be set together. Default
        /// is empty, which uses the target's icon.
        /// </summary>
        [<Erase>]
        member val icon: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The resource ID of icon when <c>icon</c> is a DLL or EXE. Default is 0.
        /// </summary>
        [<Erase>]
        member val iconIndex: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The Application User Model ID. Default is empty.
        /// </summary>
        [<Erase>]
        member val appUserModelId: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The Application Toast Activator CLSID. Needed for participating in Action Center.
        /// </summary>
        [<Erase>]
        member val toastActivatorClsid: string = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type SharingItem
        /// <param name="texts">An array of text to share.</param>
        /// <param name="filePaths">An array of files to share.</param>
        /// <param name="urls">An array of URLs to share.</param>
        (?texts: string[], ?filePaths: string[], ?urls: string[]) =
        class
        end

        /// <summary>
        /// An array of text to share.
        /// </summary>
        [<Erase>]
        member val texts: string[] = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// An array of files to share.
        /// </summary>
        [<Erase>]
        member val filePaths: string[] = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// An array of URLs to share.
        /// </summary>
        [<Erase>]
        member val urls: string[] = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type SharedWorkerInfo
        /// <param name="id">The unique id of the shared worker.</param>
        /// <param name="url">The url of the shared worker.</param>
        (id: string, url: string) =
        class
        end

        /// <summary>
        /// The unique id of the shared worker.
        /// </summary>
        [<Erase>]
        member val id: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The url of the shared worker.
        /// </summary>
        [<Erase>]
        member val url: string = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type SharedTextureHandle
        /// <param name="ntHandle">⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌ || NT HANDLE holds the
        /// shared texture. Note that this NT HANDLE is local to current process.</param>
        /// <param name="ioSurface">⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌ || IOSurfaceRef holds the shared
        /// texture. Note that this IOSurface is local to current process (not global).</param>
        /// <param name="nativePixmap">⚠ OS Compatibility: WIN ❌ | MAC ❌ | LIN ✔ | MAS ❌ || Structure contains planes of
        /// shared texture.</param>
        (
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
            ?ntHandle: Buffer
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
            ,
            ?ioSurface: Buffer
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_LIN
            ,
            ?nativePixmap: Types.SharedTextureHandle.NativePixmap
            #endif

        ) =
        class
        end
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌</para>
        /// NT HANDLE holds the shared texture. Note that this NT HANDLE is local to current process.
        /// </summary>
        [<Erase>]
        member val ntHandle: Buffer = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// IOSurfaceRef holds the shared texture. Note that this IOSurface is local to current process (not global).
        /// </summary>
        [<Erase>]
        member val ioSurface: Buffer = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_LIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ❌ | LIN ✔ | MAS ❌</para>
        /// Structure contains planes of shared texture.
        /// </summary>
        [<Erase>]
        member val nativePixmap: Types.SharedTextureHandle.NativePixmap = Unchecked.defaultof<_> with get, set
        #endif


    [<JS.Pojo>]
    type SharedDictionaryUsageInfo
        /// <param name="frameOrigin">The origin of the frame where the request originates. It’s specific to the individual frame making the request and
        /// is defined by its scheme, host, and port. In practice, will look like a URL.</param>
        /// <param name="topFrameSite">The site of the top-level browsing context (the main frame or tab that contains the request). It’s less granular
        /// than <c>frameOrigin</c> and focuses on the broader "site" scope. In practice, will look like a URL.</param>
        /// <param name="totalSizeBytes">The amount of bytes stored for this shared dictionary information object in Chromium's internal storage (usually Sqlite).</param>
        (frameOrigin: string, topFrameSite: string, totalSizeBytes: float) =
        class
        end

        /// <summary>
        /// The origin of the frame where the request originates. It’s specific to the individual frame making the request and is
        /// defined by its scheme, host, and port. In practice, will look like a URL.
        /// </summary>
        [<Erase>]
        member val frameOrigin: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The site of the top-level browsing context (the main frame or tab that contains the request). It’s less granular than
        /// <c>frameOrigin</c> and focuses on the broader "site" scope. In practice, will look like a URL.
        /// </summary>
        [<Erase>]
        member val topFrameSite: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The amount of bytes stored for this shared dictionary information object in Chromium's internal storage (usually Sqlite).
        /// </summary>
        [<Erase>]
        member val totalSizeBytes: float = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type SharedDictionaryInfo
        /// <param name="match">The matching path pattern for the dictionary which was declared in 'use-as-dictionary' response header's <c>match</c> option.</param>
        /// <param name="matchDestinations">An array of matching destinations for the dictionary which was declared in 'use-as-dictionary' response header's <c>match-dest</c> option.</param>
        /// <param name="id">The Id for the dictionary which was declared in 'use-as-dictionary' response header's <c>id</c> option.</param>
        /// <param name="dictionaryUrl">URL of the dictionary.</param>
        /// <param name="lastFetchTime">The time of when the dictionary was received from the network layer.</param>
        /// <param name="responseTime">The time of when the dictionary was received from the server. For cached responses, this time could be "far"
        /// in the past.</param>
        /// <param name="expirationDuration">The expiration time for the dictionary which was declared in 'use-as-dictionary' response header's <c>expires</c> option in seconds.</param>
        /// <param name="lastUsedTime">The time when the dictionary was last used.</param>
        /// <param name="size">The amount of bytes stored for this shared dictionary information object in Chromium's internal storage (usually Sqlite).</param>
        /// <param name="hash">The sha256 hash of the dictionary binary.</param>
        (
            ``match``: string,
            matchDestinations: string[],
            id: string,
            dictionaryUrl: string,
            lastFetchTime: System.DateTime,
            responseTime: System.DateTime,
            expirationDuration: float,
            lastUsedTime: System.DateTime,
            size: float,
            hash: string
        ) =
        class
        end

        /// <summary>
        /// The matching path pattern for the dictionary which was declared in 'use-as-dictionary' response header's <c>match</c> option.
        /// </summary>
        [<Erase>]
        member val ``match``: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// An array of matching destinations for the dictionary which was declared in 'use-as-dictionary' response header's <c>match-dest</c> option.
        /// </summary>
        [<Erase>]
        member val matchDestinations: string[] = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The Id for the dictionary which was declared in 'use-as-dictionary' response header's <c>id</c> option.
        /// </summary>
        [<Erase>]
        member val id: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// URL of the dictionary.
        /// </summary>
        [<Erase>]
        member val dictionaryUrl: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The time of when the dictionary was received from the network layer.
        /// </summary>
        [<Erase>]
        member val lastFetchTime: System.DateTime = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The time of when the dictionary was received from the server. For cached responses, this time could be "far" in
        /// the past.
        /// </summary>
        [<Erase>]
        member val responseTime: System.DateTime = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The expiration time for the dictionary which was declared in 'use-as-dictionary' response header's <c>expires</c> option in seconds.
        /// </summary>
        [<Erase>]
        member val expirationDuration: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The time when the dictionary was last used.
        /// </summary>
        [<Erase>]
        member val lastUsedTime: System.DateTime = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The amount of bytes stored for this shared dictionary information object in Chromium's internal storage (usually Sqlite).
        /// </summary>
        [<Erase>]
        member val size: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The sha256 hash of the dictionary binary.
        /// </summary>
        [<Erase>]
        member val hash: string = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type ServiceWorkerInfo
        /// <param name="scriptUrl">The full URL to the script that this service worker runs</param>
        /// <param name="scope">The base URL that this service worker is active for.</param>
        /// <param name="renderProcessId">The virtual ID of the process that this service worker is running in.  This is not an OS
        /// level PID.  This aligns with the ID set used for <c>webContents.getProcessId()</c>.</param>
        /// <param name="versionId">ID of the service worker version</param>
        (scriptUrl: string, scope: string, renderProcessId: float, versionId: float) =
        class
        end

        /// <summary>
        /// The full URL to the script that this service worker runs
        /// </summary>
        [<Erase>]
        member val scriptUrl: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The base URL that this service worker is active for.
        /// </summary>
        [<Erase>]
        member val scope: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The virtual ID of the process that this service worker is running in.  This is not an OS level
        /// PID.  This aligns with the ID set used for <c>webContents.getProcessId()</c>.
        /// </summary>
        [<Erase>]
        member val renderProcessId: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// ID of the service worker version
        /// </summary>
        [<Erase>]
        member val versionId: float = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type SerialPort
        /// <param name="portId">Unique identifier for the port.</param>
        /// <param name="portName">Name of the port.</param>
        /// <param name="displayName">A string suitable for display to the user for describing this device.</param>
        /// <param name="vendorId">The USB vendor ID.</param>
        /// <param name="productId">The USB product ID.</param>
        /// <param name="serialNumber">The USB device serial number.</param>
        /// <param name="usbDriverName">⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌ || Represents a single serial
        /// port on macOS can be enumerated by multiple drivers.</param>
        /// <param name="deviceInstanceId">⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌ || A stable identifier on
        /// Windows that can be used for device permissions.</param>
        (
            portId: string,
            portName: string,
            ?displayName: string,
            ?vendorId: string,
            ?productId: string,
            ?serialNumber: string
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
            ,
            ?usbDriverName: string
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
            ,
            ?deviceInstanceId: string
            #endif

        ) =
        class
        end

        /// <summary>
        /// Unique identifier for the port.
        /// </summary>
        [<Erase>]
        member val portId: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Name of the port.
        /// </summary>
        [<Erase>]
        member val portName: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// A string suitable for display to the user for describing this device.
        /// </summary>
        [<Erase>]
        member val displayName: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The USB vendor ID.
        /// </summary>
        [<Erase>]
        member val vendorId: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The USB product ID.
        /// </summary>
        [<Erase>]
        member val productId: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The USB device serial number.
        /// </summary>
        [<Erase>]
        member val serialNumber: string = Unchecked.defaultof<_> with get, set
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Represents a single serial port on macOS can be enumerated by multiple drivers.
        /// </summary>
        [<Erase>]
        member val usbDriverName: string = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌</para>
        /// A stable identifier on Windows that can be used for device permissions.
        /// </summary>
        [<Erase>]
        member val deviceInstanceId: string = Unchecked.defaultof<_> with get, set
        #endif


    [<JS.Pojo>]
    type SegmentedControlSegment
        /// <param name="label">The text to appear in this segment.</param>
        /// <param name="icon">The image to appear in this segment.</param>
        /// <param name="enabled">Whether this segment is selectable. Default: true.</param>
        (?label: string, ?icon: NativeImage, ?enabled: bool) =
        class
        end

        /// <summary>
        /// The text to appear in this segment.
        /// </summary>
        [<Erase>]
        member val label: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The image to appear in this segment.
        /// </summary>
        [<Erase>]
        member val icon: NativeImage = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether this segment is selectable. Default: true.
        /// </summary>
        [<Erase>]
        member val enabled: bool = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type ScrubberItem
        /// <param name="label">The text to appear in this item.</param>
        /// <param name="icon">The image to appear in this item.</param>
        (?label: string, ?icon: NativeImage) =
        class
        end

        /// <summary>
        /// The text to appear in this item.
        /// </summary>
        [<Erase>]
        member val label: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The image to appear in this item.
        /// </summary>
        [<Erase>]
        member val icon: NativeImage = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type ResolvedHost
        /// <param name="endpoints">resolved DNS entries for the hostname</param>
        (endpoints: ResolvedEndpoint[]) =
        class
        end

        /// <summary>
        /// resolved DNS entries for the hostname
        /// </summary>
        [<Erase>]
        member val endpoints: ResolvedEndpoint[] = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type ResolvedEndpoint
        /// <param name="address"></param>
        /// <param name="family">One of the following:</param>
        (address: string, family: Enums.Types.ResolvedEndpoint.Family) =
        class
        end

        [<Erase>]
        member val address: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// One of the following:
        /// </summary>
        [<Erase>]
        member val family: Enums.Types.ResolvedEndpoint.Family = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type RenderProcessGoneDetails
        /// <param name="reason">The reason the render process is gone.  Possible values:</param>
        /// <param name="exitCode">The exit code of the process, unless <c>reason</c> is <c>launch-failed</c>, in which case <c>exitCode</c> will be a platform-specific launch
        /// failure error code.</param>
        (reason: Enums.Types.RenderProcessGoneDetails.Reason, exitCode: int) =
        class
        end

        /// <summary>
        /// The reason the render process is gone.  Possible values:
        /// </summary>
        [<Erase>]
        member val reason: Enums.Types.RenderProcessGoneDetails.Reason = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The exit code of the process, unless <c>reason</c> is <c>launch-failed</c>, in which case <c>exitCode</c> will be a platform-specific launch failure
        /// error code.
        /// </summary>
        [<Erase>]
        member val exitCode: int = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type Referrer
        /// <param name="url">HTTP Referrer URL.</param>
        /// <param name="policy">Can be <c>default</c>, <c>unsafe-url</c>, <c>no-referrer-when-downgrade</c>, <c>no-referrer</c>, <c>origin</c>, <c>strict-origin-when-cross-origin</c>, <c>same-origin</c> or <c>strict-origin</c>. See the Referrer-Policy spec for more details on
        /// the meaning of these values.</param>
        (url: string, policy: Enums.Types.Referrer.Policy) =
        class
        end

        /// <summary>
        /// HTTP Referrer URL.
        /// </summary>
        [<Erase>]
        member val url: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Can be <c>default</c>, <c>unsafe-url</c>, <c>no-referrer-when-downgrade</c>, <c>no-referrer</c>, <c>origin</c>, <c>strict-origin-when-cross-origin</c>, <c>same-origin</c> or <c>strict-origin</c>. See the Referrer-Policy spec for more details on the
        /// meaning of these values.
        /// </summary>
        [<Erase>]
        member val policy: Enums.Types.Referrer.Policy = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type Rectangle
        /// <param name="x">The x coordinate of the origin of the rectangle (must be an integer).</param>
        /// <param name="y">The y coordinate of the origin of the rectangle (must be an integer).</param>
        /// <param name="width">The width of the rectangle (must be an integer).</param>
        /// <param name="height">The height of the rectangle (must be an integer).</param>
        (x: float, y: float, width: float, height: float) =
        class
        end

        /// <summary>
        /// The x coordinate of the origin of the rectangle (must be an integer).
        /// </summary>
        [<Erase>]
        member val x: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The y coordinate of the origin of the rectangle (must be an integer).
        /// </summary>
        [<Erase>]
        member val y: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The width of the rectangle (must be an integer).
        /// </summary>
        [<Erase>]
        member val width: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The height of the rectangle (must be an integer).
        /// </summary>
        [<Erase>]
        member val height: float = Unchecked.defaultof<_> with get, set

    /// <summary>
    /// When <c>mode</c> is unspecified, <c>pacScript</c> and <c>proxyRules</c> are provided together, the <c>proxyRules</c> option is ignored and <c>pacScript</c> configuration is applied.<br/><br/>The
    /// <c>proxyRules</c> has to follow the rules below:<br/><br/>For example:<br/><br/>* <c>http=foopy:80;ftp=foopy2</c> - Use HTTP proxy <c>foopy:80</c> for <c>http://</c> URLs, and HTTP proxy
    /// <c>foopy2:80</c> for <c>ftp://</c> URLs.<br/>* <c>foopy:80</c> - Use HTTP proxy <c>foopy:80</c> for all URLs.<br/>* <c>foopy:80,bar,direct://</c> - Use HTTP proxy <c>foopy:80</c> for
    /// all URLs, failing over to <c>bar</c> if <c>foopy:80</c> is unavailable, and after that using no proxy.<br/>* <c>socks4://foopy</c> - Use SOCKS
    /// v4 proxy <c>foopy:1080</c> for all URLs.<br/>* <c>http=foopy,socks5://bar.com</c> - Use HTTP proxy <c>foopy</c> for http URLs, and fail over to the
    /// SOCKS5 proxy <c>bar.com</c> if <c>foopy</c> is unavailable.<br/>* <c>http=foopy,direct://</c> - Use HTTP proxy <c>foopy</c> for http URLs, and use no proxy
    /// if <c>foopy</c> is unavailable.<br/>* <c>http=foopy;socks=foopy2</c> - Use HTTP proxy <c>foopy</c> for http URLs, and use <c>socks4://foopy2</c> for all other URLs.<br/><br/>The
    /// <c>proxyBypassRules</c> is a comma separated list of rules described below:<br/><br/>* <c>[ URL_SCHEME "://" ] HOSTNAME_PATTERN [ ":" &lt;port&gt; ]</c><br/><br/>Match all
    /// hostnames that match the pattern HOSTNAME_PATTERN.<br/><br/>Examples: "foobar.com", "*foobar.com", "*.foobar.com", "*foobar.com:99", "https://x.*.y.com:99"<br/>* <c>"." HOSTNAME_SUFFIX_PATTERN [ ":" PORT ]</c><br/><br/>Match a particular domain
    /// suffix.<br/><br/>Examples: ".google.com", ".com", "http://.google.com"<br/>* <c>[ SCHEME "://" ] IP_LITERAL [ ":" PORT ]</c><br/><br/>Match URLs which are IP address literals.<br/><br/>Examples: "127.0.1",
    /// "[0:0::1]", "[::1]", "http://[::1]:99"<br/>* <c>IP_LITERAL "/" PREFIX_LENGTH_IN_BITS</c><br/><br/>Match any URL that is to an IP literal that falls between the given range.
    /// IP range is specified using CIDR notation.<br/><br/>Examples: "192.168.1.1/16", "fefe:13::abc/33".<br/>* <c>&lt;local&gt;</c><br/><br/>Match local addresses. The meaning of <c>&lt;local&gt;</c> is whether the host
    /// matches one of: "127.0.0.1", "::1", "localhost".
    /// </summary>
    [<JS.Pojo>]
    type ProxyConfig
        /// <param name="mode">The proxy mode. Should be one of <c>direct</c>, <c>auto_detect</c>, <c>pac_script</c>, <c>fixed_servers</c> or <c>system</c>. Defaults to <c>pac_script</c> proxy mode if
        /// <c>pacScript</c> option is specified otherwise defaults to <c>fixed_servers</c>.</param>
        /// <param name="pacScript">The URL associated with the PAC file.</param>
        /// <param name="proxyRules">Rules indicating which proxies to use.</param>
        /// <param name="proxyBypassRules">Rules indicating which URLs should bypass the proxy settings.</param>
        (?mode: Enums.Types.ProxyConfig.Mode, ?pacScript: string, ?proxyRules: string, ?proxyBypassRules: string) =
        class
        end

        /// <summary>
        /// The proxy mode. Should be one of <c>direct</c>, <c>auto_detect</c>, <c>pac_script</c>, <c>fixed_servers</c> or <c>system</c>. Defaults to <c>pac_script</c> proxy mode if <c>pacScript</c>
        /// option is specified otherwise defaults to <c>fixed_servers</c>.
        /// </summary>
        [<Erase>]
        member val mode: Enums.Types.ProxyConfig.Mode = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The URL associated with the PAC file.
        /// </summary>
        [<Erase>]
        member val pacScript: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Rules indicating which proxies to use.
        /// </summary>
        [<Erase>]
        member val proxyRules: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Rules indicating which URLs should bypass the proxy settings.
        /// </summary>
        [<Erase>]
        member val proxyBypassRules: string = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type ProtocolResponse
        /// <param name="error">When assigned, the <c>request</c> will fail with the <c>error</c> number . For the available error numbers you can use,
        /// please see the net error list.</param>
        /// <param name="statusCode">The HTTP response code, default is 200.</param>
        /// <param name="charset">The charset of response body, default is <c>"utf-8"</c>.</param>
        /// <param name="mimeType">The MIME type of response body, default is <c>"text/html"</c>. Setting <c>mimeType</c> would implicitly set the <c>content-type</c> header in response,
        /// but if <c>content-type</c> is already set in <c>headers</c>, the <c>mimeType</c> would be ignored.</param>
        /// <param name="headers">An object containing the response headers. The keys must be string, and values must be either string or Array
        /// of string.</param>
        /// <param name="data">The response body. When returning stream as response, this is a Node.js readable stream representing the response body. When
        /// returning <c>Buffer</c> as response, this is a <c>Buffer</c>. When returning <c>string</c> as response, this is a <c>string</c>. This is ignored
        /// for other types of responses.</param>
        /// <param name="path">Path to the file which would be sent as response body. This is only used for file responses.</param>
        /// <param name="url">Download the <c>url</c> and pipe the result as response body. This is only used for URL responses.</param>
        /// <param name="referrer">The <c>referrer</c> URL. This is only used for file and URL responses.</param>
        /// <param name="method">The HTTP <c>method</c>. This is only used for file and URL responses.</param>
        /// <param name="session">The session used for requesting URL. The HTTP request will reuse the current session by default.</param>
        /// <param name="uploadData">The data used as upload data. This is only used for URL responses when <c>method</c> is <c>"POST"</c>.</param>
        (
            ?error: int,
            ?statusCode: float,
            ?charset: string,
            ?mimeType: string,
            ?headers: Record<string, U2<string, string[]>>,
            ?data: U3<Buffer, string, Readable<obj>>,
            ?path: string,
            ?url: string,
            ?referrer: string,
            ?method: string,
            ?session: Session,
            ?uploadData: ProtocolResponseUploadData
        ) =
        class
        end

        /// <summary>
        /// When assigned, the <c>request</c> will fail with the <c>error</c> number . For the available error numbers you can use, please
        /// see the net error list.
        /// </summary>
        [<Erase>]
        member val error: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The HTTP response code, default is 200.
        /// </summary>
        [<Erase>]
        member val statusCode: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The charset of response body, default is <c>"utf-8"</c>.
        /// </summary>
        [<Erase>]
        member val charset: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The MIME type of response body, default is <c>"text/html"</c>. Setting <c>mimeType</c> would implicitly set the <c>content-type</c> header in response, but
        /// if <c>content-type</c> is already set in <c>headers</c>, the <c>mimeType</c> would be ignored.
        /// </summary>
        [<Erase>]
        member val mimeType: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// An object containing the response headers. The keys must be string, and values must be either string or Array of
        /// string.
        /// </summary>
        [<Erase>]
        member val headers: Record<string, U2<string, string[]>> = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The response body. When returning stream as response, this is a Node.js readable stream representing the response body. When returning
        /// <c>Buffer</c> as response, this is a <c>Buffer</c>. When returning <c>string</c> as response, this is a <c>string</c>. This is ignored for
        /// other types of responses.
        /// </summary>
        [<Erase>]
        member val data: U3<Buffer, string, Readable<obj>> = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Path to the file which would be sent as response body. This is only used for file responses.
        /// </summary>
        [<Erase>]
        member val path: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Download the <c>url</c> and pipe the result as response body. This is only used for URL responses.
        /// </summary>
        [<Erase>]
        member val url: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The <c>referrer</c> URL. This is only used for file and URL responses.
        /// </summary>
        [<Erase>]
        member val referrer: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The HTTP <c>method</c>. This is only used for file and URL responses.
        /// </summary>
        [<Erase>]
        member val method: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The session used for requesting URL. The HTTP request will reuse the current session by default.
        /// </summary>
        [<Erase>]
        member val session: Session = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The data used as upload data. This is only used for URL responses when <c>method</c> is <c>"POST"</c>.
        /// </summary>
        [<Erase>]
        member val uploadData: ProtocolResponseUploadData = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type ProtocolResponseUploadData
        /// <param name="contentType">MIME type of the content.</param>
        /// <param name="data">Content to be sent.</param>
        (contentType: string, data: U2<string, Buffer>) =
        class
        end

        /// <summary>
        /// MIME type of the content.
        /// </summary>
        [<Erase>]
        member val contentType: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Content to be sent.
        /// </summary>
        [<Erase>]
        member val data: U2<string, Buffer> = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type ProtocolRequest
        /// <param name="url"></param>
        /// <param name="referrer"></param>
        /// <param name="method"></param>
        /// <param name="uploadData"></param>
        /// <param name="headers"></param>
        (url: string, referrer: string, method: string, headers: Record<string, string>, ?uploadData: UploadData[]) =
        class
        end

        [<Erase>]
        member val url: string = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val referrer: string = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val method: string = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val uploadData: UploadData[] = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val headers: Record<string, string> = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type Product
        /// <param name="productIdentifier">The string that identifies the product to the Apple App Store.</param>
        /// <param name="localizedDescription">A description of the product.</param>
        /// <param name="localizedTitle">The name of the product.</param>
        /// <param name="price">The cost of the product in the local currency.</param>
        /// <param name="formattedPrice">The locale formatted price of the product.</param>
        /// <param name="currencyCode">3 character code presenting a product's currency based on the ISO 4217 standard.</param>
        /// <param name="introductoryPrice">The object containing introductory price information for the product. available for the product.</param>
        /// <param name="discounts">An array of discount offers</param>
        /// <param name="subscriptionGroupIdentifier">The identifier of the subscription group to which the subscription belongs.</param>
        /// <param name="subscriptionPeriod">The period details for products that are subscriptions.</param>
        /// <param name="isDownloadable">A boolean value that indicates whether the App Store has downloadable content for this product. <c>true</c> if at least
        /// one file has been associated with the product.</param>
        /// <param name="downloadContentVersion">A string that identifies the version of the content.</param>
        /// <param name="downloadContentLengths">The total size of the content, in bytes.</param>
        (
            productIdentifier: string,
            localizedDescription: string,
            localizedTitle: string,
            price: float,
            formattedPrice: string,
            currencyCode: string,
            discounts: ProductDiscount[],
            subscriptionGroupIdentifier: string,
            isDownloadable: bool,
            downloadContentVersion: string,
            downloadContentLengths: float[],
            ?introductoryPrice: ProductDiscount,
            ?subscriptionPeriod: ProductSubscriptionPeriod
        ) =
        class
        end

        /// <summary>
        /// The string that identifies the product to the Apple App Store.
        /// </summary>
        [<Erase>]
        member val productIdentifier: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// A description of the product.
        /// </summary>
        [<Erase>]
        member val localizedDescription: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The name of the product.
        /// </summary>
        [<Erase>]
        member val localizedTitle: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The cost of the product in the local currency.
        /// </summary>
        [<Erase>]
        member val price: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The locale formatted price of the product.
        /// </summary>
        [<Erase>]
        member val formattedPrice: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// 3 character code presenting a product's currency based on the ISO 4217 standard.
        /// </summary>
        [<Erase>]
        member val currencyCode: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The object containing introductory price information for the product. available for the product.
        /// </summary>
        [<Erase>]
        member val introductoryPrice: ProductDiscount = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// An array of discount offers
        /// </summary>
        [<Erase>]
        member val discounts: ProductDiscount[] = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The identifier of the subscription group to which the subscription belongs.
        /// </summary>
        [<Erase>]
        member val subscriptionGroupIdentifier: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The period details for products that are subscriptions.
        /// </summary>
        [<Erase>]
        member val subscriptionPeriod: ProductSubscriptionPeriod = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// A boolean value that indicates whether the App Store has downloadable content for this product. <c>true</c> if at least one
        /// file has been associated with the product.
        /// </summary>
        [<Erase>]
        member val isDownloadable: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// A string that identifies the version of the content.
        /// </summary>
        [<Erase>]
        member val downloadContentVersion: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The total size of the content, in bytes.
        /// </summary>
        [<Erase>]
        member val downloadContentLengths: float[] = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type ProductSubscriptionPeriod
        /// <param name="numberOfUnits">The number of units per subscription period.</param>
        /// <param name="unit">The increment of time that a subscription period is specified in. Can be <c>day</c>, <c>week</c>, <c>month</c>, <c>year</c>.</param>
        (numberOfUnits: float, unit: Enums.Types.ProductSubscriptionPeriod.Unit) =
        class
        end

        /// <summary>
        /// The number of units per subscription period.
        /// </summary>
        [<Erase>]
        member val numberOfUnits: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The increment of time that a subscription period is specified in. Can be <c>day</c>, <c>week</c>, <c>month</c>, <c>year</c>.
        /// </summary>
        [<Erase>]
        member val unit: Enums.Types.ProductSubscriptionPeriod.Unit = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type ProductDiscount
        /// <param name="identifier">A string used to uniquely identify a discount offer for a product.</param>
        /// <param name="type">The type of discount offer.</param>
        /// <param name="price">The discount price of the product in the local currency.</param>
        /// <param name="priceLocale">The locale used to format the discount price of the product.</param>
        /// <param name="paymentMode">The payment mode for this product discount. Can be <c>payAsYouGo</c>, <c>payUpFront</c>, or <c>freeTrial</c>.</param>
        /// <param name="numberOfPeriods">An integer that indicates the number of periods the product discount is available.</param>
        /// <param name="subscriptionPeriod">An object that defines the period for the product discount.</param>
        (
            identifier: string,
            ``type``: float,
            price: float,
            priceLocale: string,
            paymentMode: Enums.Types.ProductDiscount.PaymentMode,
            numberOfPeriods: float,
            ?subscriptionPeriod: ProductSubscriptionPeriod
        ) =
        class
        end

        /// <summary>
        /// A string used to uniquely identify a discount offer for a product.
        /// </summary>
        [<Erase>]
        member val identifier: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The type of discount offer.
        /// </summary>
        [<Erase>]
        member val ``type``: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The discount price of the product in the local currency.
        /// </summary>
        [<Erase>]
        member val price: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The locale used to format the discount price of the product.
        /// </summary>
        [<Erase>]
        member val priceLocale: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The payment mode for this product discount. Can be <c>payAsYouGo</c>, <c>payUpFront</c>, or <c>freeTrial</c>.
        /// </summary>
        [<Erase>]
        member val paymentMode: Enums.Types.ProductDiscount.PaymentMode = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// An integer that indicates the number of periods the product discount is available.
        /// </summary>
        [<Erase>]
        member val numberOfPeriods: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// An object that defines the period for the product discount.
        /// </summary>
        [<Erase>]
        member val subscriptionPeriod: ProductSubscriptionPeriod = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type ProcessMetric
        /// <param name="pid">Process id of the process.</param>
        /// <param name="type">Process type. One of the following values:</param>
        /// <param name="serviceName">The non-localized name of the process.</param>
        /// <param name="name">The name of the process. Examples for utility: <c>Audio Service</c>, <c>Content Decryption Module Service</c>, <c>Network Service</c>, <c>Video Capture</c>, etc.</param>
        /// <param name="cpu">CPU usage of the process.</param>
        /// <param name="creationTime">Creation time for this process. The time is represented as number of milliseconds since epoch. Since the <c>pid</c> can
        /// be reused after a process dies, it is useful to use both the <c>pid</c> and the <c>creationTime</c> to uniquely identify
        /// a process.</param>
        /// <param name="memory">Memory information for the process.</param>
        /// <param name="sandboxed">⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌ || Whether the process is
        /// sandboxed on OS level.</param>
        /// <param name="integrityLevel">⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌ || One of the following
        /// values:</param>
        (
            pid: int,
            ``type``: Enums.Types.ProcessMetric.Type,
            cpu: CPUUsage,
            creationTime: float,
            memory: MemoryInfo,
            ?serviceName: string,
            ?name: string
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
            ,
            ?sandboxed: bool
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
            ,
            ?integrityLevel: Enums.Types.ProcessMetric.IntegrityLevel
            #endif

        ) =
        class
        end

        /// <summary>
        /// Process id of the process.
        /// </summary>
        [<Erase>]
        member val pid: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Process type. One of the following values:
        /// </summary>
        [<Erase>]
        member val ``type``: Enums.Types.ProcessMetric.Type = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The non-localized name of the process.
        /// </summary>
        [<Erase>]
        member val serviceName: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The name of the process. Examples for utility: <c>Audio Service</c>, <c>Content Decryption Module Service</c>, <c>Network Service</c>, <c>Video Capture</c>, etc.
        /// </summary>
        [<Erase>]
        member val name: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// CPU usage of the process.
        /// </summary>
        [<Erase>]
        member val cpu: CPUUsage = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Creation time for this process. The time is represented as number of milliseconds since epoch. Since the <c>pid</c> can be
        /// reused after a process dies, it is useful to use both the <c>pid</c> and the <c>creationTime</c> to uniquely identify a
        /// process.
        /// </summary>
        [<Erase>]
        member val creationTime: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Memory information for the process.
        /// </summary>
        [<Erase>]
        member val memory: MemoryInfo = Unchecked.defaultof<_> with get, set
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Whether the process is sandboxed on OS level.
        /// </summary>
        [<Erase>]
        member val sandboxed: bool = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌</para>
        /// One of the following values:
        /// </summary>
        [<Erase>]
        member val integrityLevel: Enums.Types.ProcessMetric.IntegrityLevel = Unchecked.defaultof<_> with get, set
        #endif


    [<JS.Pojo>]
    type ProcessMemoryInfo
        /// <param name="residentSet">⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ✔ | MAS ❌ || The amount of memory
        /// currently pinned to actual physical RAM in Kilobytes.</param>
        /// <param name="private">The amount of memory not shared by other processes, such as JS heap or HTML content in Kilobytes.</param>
        /// <param name="shared">The amount of memory shared between processes, typically memory consumed by the Electron code itself in Kilobytes.</param>
        (
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_LIN || ELECTRON_OS_WIN
            residentSet: int
            #endif
            ,
            ``private``: int,
            shared: int
        ) =
        class
        end
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_LIN || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ✔ | MAS ❌</para>
        /// The amount of memory currently pinned to actual physical RAM in Kilobytes.
        /// </summary>
        [<Erase>]
        member val residentSet: int = Unchecked.defaultof<_> with get, set
        #endif


        /// <summary>
        /// The amount of memory not shared by other processes, such as JS heap or HTML content in Kilobytes.
        /// </summary>
        [<Erase>]
        member val ``private``: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The amount of memory shared between processes, typically memory consumed by the Electron code itself in Kilobytes.
        /// </summary>
        [<Erase>]
        member val shared: int = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type PrinterInfo
        /// <param name="name">the name of the printer as understood by the OS.</param>
        /// <param name="displayName">the name of the printer as shown in Print Preview.</param>
        /// <param name="description">a longer description of the printer's type.</param>
        /// <param name="options">an object containing a variable number of platform-specific printer information.</param>
        (name: string, displayName: string, description: string, options: obj) =
        class
        end

        /// <summary>
        /// the name of the printer as understood by the OS.
        /// </summary>
        [<Erase>]
        member val name: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// the name of the printer as shown in Print Preview.
        /// </summary>
        [<Erase>]
        member val displayName: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// a longer description of the printer's type.
        /// </summary>
        [<Erase>]
        member val description: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// an object containing a variable number of platform-specific printer information.
        /// </summary>
        [<Erase>]
        member val options: obj = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type PreloadScript
        /// <param name="type">Context type where the preload script will be executed. Possible values include <c>frame</c> or <c>service-worker</c>.</param>
        /// <param name="id">Unique ID of preload script.</param>
        /// <param name="filePath">Path of the script file. Must be an absolute path.</param>
        (``type``: Enums.Types.PreloadScript.Type, id: string, filePath: string) =
        class
        end

        /// <summary>
        /// Context type where the preload script will be executed. Possible values include <c>frame</c> or <c>service-worker</c>.
        /// </summary>
        [<Erase>]
        member val ``type``: Enums.Types.PreloadScript.Type = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Unique ID of preload script.
        /// </summary>
        [<Erase>]
        member val id: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Path of the script file. Must be an absolute path.
        /// </summary>
        [<Erase>]
        member val filePath: string = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type PreloadScriptRegistration
        /// <param name="type">Context type where the preload script will be executed. Possible values include <c>frame</c> or <c>service-worker</c>.</param>
        /// <param name="id">Unique ID of preload script. Defaults to a random UUID.</param>
        /// <param name="filePath">Path of the script file. Must be an absolute path.</param>
        (``type``: Enums.Types.PreloadScriptRegistration.Type, filePath: string, ?id: string) =
        class
        end

        /// <summary>
        /// Context type where the preload script will be executed. Possible values include <c>frame</c> or <c>service-worker</c>.
        /// </summary>
        [<Erase>]
        member val ``type``: Enums.Types.PreloadScriptRegistration.Type = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Unique ID of preload script. Defaults to a random UUID.
        /// </summary>
        [<Erase>]
        member val id: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Path of the script file. Must be an absolute path.
        /// </summary>
        [<Erase>]
        member val filePath: string = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type PostBody
        /// <param name="data">The post data to be sent to the new window.</param>
        /// <param name="contentType">The <c>content-type</c> header used for the data. One of <c>application/x-www-form-urlencoded</c> or <c>multipart/form-data</c>. Corresponds to the <c>enctype</c> attribute of the
        /// submitted HTML form.</param>
        /// <param name="boundary">The boundary used to separate multiple parts of the message. Only valid when <c>contentType</c> is <c>multipart/form-data</c>.</param>
        (data: U2<UploadRawData, UploadFile>[], contentType: string, ?boundary: string) =
        class
        end

        /// <summary>
        /// The post data to be sent to the new window.
        /// </summary>
        [<Erase>]
        member val data: U2<UploadRawData, UploadFile>[] = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The <c>content-type</c> header used for the data. One of <c>application/x-www-form-urlencoded</c> or <c>multipart/form-data</c>. Corresponds to the <c>enctype</c> attribute of the submitted
        /// HTML form.
        /// </summary>
        [<Erase>]
        member val contentType: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The boundary used to separate multiple parts of the message. Only valid when <c>contentType</c> is <c>multipart/form-data</c>.
        /// </summary>
        [<Erase>]
        member val boundary: string = Unchecked.defaultof<_> with get, set

    /// <summary>
    /// &gt; [!NOTE] Both <c>x</c> and <c>y</c> must be whole integers, when providing a point object as input to an Electron
    /// API we will automatically round your <c>x</c> and <c>y</c> values to the nearest whole integer.
    /// </summary>
    [<JS.Pojo>]
    type Point
        /// <param name="x"></param>
        /// <param name="y"></param>
        (x: float, y: float) =
        class
        end

        [<Erase>]
        member val x: float = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val y: float = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type PermissionRequest
        /// <param name="requestingUrl">The last URL the requesting frame loaded.</param>
        /// <param name="isMainFrame">Whether the frame making the request is the main frame.</param>
        (requestingUrl: string, isMainFrame: bool) =
        class
        end

        /// <summary>
        /// The last URL the requesting frame loaded.
        /// </summary>
        [<Erase>]
        member val requestingUrl: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether the frame making the request is the main frame.
        /// </summary>
        [<Erase>]
        member val isMainFrame: bool = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type PaymentDiscount
        /// <param name="identifier">A string used to uniquely identify a discount offer for a product.</param>
        /// <param name="keyIdentifier">A string that identifies the key used to generate the signature.</param>
        /// <param name="nonce">A universally unique ID (UUID) value that you define.</param>
        /// <param name="signature">A UTF-8 string representing the properties of a specific discount offer, cryptographically signed.</param>
        /// <param name="timestamp">The date and time of the signature's creation in milliseconds, formatted in Unix epoch time.</param>
        (identifier: string, keyIdentifier: string, nonce: string, signature: string, timestamp: float) =
        class
        end

        /// <summary>
        /// A string used to uniquely identify a discount offer for a product.
        /// </summary>
        [<Erase>]
        member val identifier: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// A string that identifies the key used to generate the signature.
        /// </summary>
        [<Erase>]
        member val keyIdentifier: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// A universally unique ID (UUID) value that you define.
        /// </summary>
        [<Erase>]
        member val nonce: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// A UTF-8 string representing the properties of a specific discount offer, cryptographically signed.
        /// </summary>
        [<Erase>]
        member val signature: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The date and time of the signature's creation in milliseconds, formatted in Unix epoch time.
        /// </summary>
        [<Erase>]
        member val timestamp: float = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type OpenExternalPermissionRequest
        /// <param name="externalURL">The url of the <c>openExternal</c> request.</param>
        /// <param name="requestingUrl">The last URL the requesting frame loaded.</param>
        /// <param name="isMainFrame">Whether the frame making the request is the main frame.</param>
        (requestingUrl: string, isMainFrame: bool, ?externalURL: string) =
        class
        end

        /// <summary>
        /// The url of the <c>openExternal</c> request.
        /// </summary>
        [<Erase>]
        member val externalURL: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The last URL the requesting frame loaded.
        /// </summary>
        [<Erase>]
        member val requestingUrl: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether the frame making the request is the main frame.
        /// </summary>
        [<Erase>]
        member val isMainFrame: bool = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type OffscreenSharedTexture
        /// <param name="textureInfo">The shared texture info.</param>
        /// <param name="release">Release the resources. The <c>texture</c> cannot be directly passed to another process, users need to maintain texture lifecycles in
        /// main process, but it is safe to pass the <c>textureInfo</c> to another process. Only a limited number of textures can
        /// exist at the same time, so it's important that you call <c>texture.release()</c> as soon as you're done with the texture.</param>
        (textureInfo: Types.OffscreenSharedTexture.TextureInfo, release: unit -> unit) =
        class
        end

        /// <summary>
        /// The shared texture info.
        /// </summary>
        [<Erase>]
        member val textureInfo: Types.OffscreenSharedTexture.TextureInfo = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Release the resources. The <c>texture</c> cannot be directly passed to another process, users need to maintain texture lifecycles in main
        /// process, but it is safe to pass the <c>textureInfo</c> to another process. Only a limited number of textures can exist
        /// at the same time, so it's important that you call <c>texture.release()</c> as soon as you're done with the texture.
        /// </summary>
        [<Erase>]
        member val release: unit -> unit = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type NotificationResponse
        /// <param name="actionIdentifier">The identifier string of the action that the user selected.</param>
        /// <param name="date">The delivery date of the notification.</param>
        /// <param name="identifier">The unique identifier for this notification request.</param>
        /// <param name="userInfo">A dictionary of custom information associated with the notification.</param>
        /// <param name="userText">The text entered or chosen by the user.</param>
        (actionIdentifier: string, date: float, identifier: string, userInfo: Record<string, obj>, ?userText: string) =
        class
        end

        /// <summary>
        /// The identifier string of the action that the user selected.
        /// </summary>
        [<Erase>]
        member val actionIdentifier: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The delivery date of the notification.
        /// </summary>
        [<Erase>]
        member val date: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The unique identifier for this notification request.
        /// </summary>
        [<Erase>]
        member val identifier: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// A dictionary of custom information associated with the notification.
        /// </summary>
        [<Erase>]
        member val userInfo: Record<string, obj> = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The text entered or chosen by the user.
        /// </summary>
        [<Erase>]
        member val userText: string = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type NotificationAction
        /// <param name="type">The type of action, can be <c>button</c>.</param>
        /// <param name="text">The label for the given action.</param>
        (``type``: Enums.Types.NotificationAction.Type, ?text: string) =
        class
        end

        /// <summary>
        /// The type of action, can be <c>button</c>.
        /// </summary>
        [<Erase>]
        member val ``type``: Enums.Types.NotificationAction.Type = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The label for the given action.
        /// </summary>
        [<Erase>]
        member val text: string = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type NavigationEntry
        /// <param name="url"></param>
        /// <param name="title"></param>
        /// <param name="pageState">A base64 encoded data string containing Chromium page state including information like the current scroll position or form values.
        /// It is committed by Chromium before a navigation event and on a regular interval.</param>
        (url: string, title: string, ?pageState: string) =
        class
        end

        [<Erase>]
        member val url: string = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val title: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// A base64 encoded data string containing Chromium page state including information like the current scroll position or form values. It
        /// is committed by Chromium before a navigation event and on a regular interval.
        /// </summary>
        [<Erase>]
        member val pageState: string = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type MouseWheelInputEvent
        /// <param name="type">The type of the event, can be <c>mouseWheel</c>.</param>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        /// <param name="wheelTicksX"></param>
        /// <param name="wheelTicksY"></param>
        /// <param name="accelerationRatioX"></param>
        /// <param name="accelerationRatioY"></param>
        /// <param name="hasPreciseScrollingDeltas"></param>
        /// <param name="canScroll"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="button">The button pressed, can be <c>left</c>, <c>middle</c>, <c>right</c>.</param>
        /// <param name="globalX"></param>
        /// <param name="globalY"></param>
        /// <param name="movementX"></param>
        /// <param name="movementY"></param>
        /// <param name="clickCount"></param>
        (
            ``type``: Enums.Types.MouseWheelInputEvent.Type,
            x: int,
            y: int,
            ?deltaX: int,
            ?deltaY: int,
            ?wheelTicksX: int,
            ?wheelTicksY: int,
            ?accelerationRatioX: int,
            ?accelerationRatioY: int,
            ?hasPreciseScrollingDeltas: bool,
            ?canScroll: bool,
            ?button: Enums.Types.MouseWheelInputEvent.Button,
            ?globalX: int,
            ?globalY: int,
            ?movementX: int,
            ?movementY: int,
            ?clickCount: int
        ) =
        class
        end

        /// <summary>
        /// The type of the event, can be <c>mouseWheel</c>.
        /// </summary>
        [<Erase>]
        member val ``type``: Enums.Types.MouseWheelInputEvent.Type = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val deltaX: int = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val deltaY: int = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val wheelTicksX: int = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val wheelTicksY: int = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val accelerationRatioX: int = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val accelerationRatioY: int = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val hasPreciseScrollingDeltas: bool = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val canScroll: bool = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val x: int = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val y: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The button pressed, can be <c>left</c>, <c>middle</c>, <c>right</c>.
        /// </summary>
        [<Erase>]
        member val button: Enums.Types.MouseWheelInputEvent.Button = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val globalX: int = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val globalY: int = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val movementX: int = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val movementY: int = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val clickCount: int = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type MouseInputEvent
        /// <param name="type">The type of the event, can be <c>mouseDown</c>, <c>mouseUp</c>, <c>mouseEnter</c>, <c>mouseLeave</c>, <c>contextMenu</c>, <c>mouseWheel</c> or <c>mouseMove</c>.</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="button">The button pressed, can be <c>left</c>, <c>middle</c>, <c>right</c>.</param>
        /// <param name="globalX"></param>
        /// <param name="globalY"></param>
        /// <param name="movementX"></param>
        /// <param name="movementY"></param>
        /// <param name="clickCount"></param>
        /// <param name="modifiers">An array of modifiers of the event, can be <c>shift</c>, <c>control</c>, <c>ctrl</c>, <c>alt</c>, <c>meta</c>, <c>command</c>, <c>cmd</c>, <c>iskeypad</c>, <c>isautorepeat</c>, <c>leftbuttondown</c>,
        /// <c>middlebuttondown</c>, <c>rightbuttondown</c>, <c>capslock</c>, <c>numlock</c>, <c>left</c>, <c>right</c>.</param>
        (
            ``type``: Enums.Types.MouseInputEvent.Type,
            x: int,
            y: int,
            ?button: Enums.Types.MouseInputEvent.Button,
            ?globalX: int,
            ?globalY: int,
            ?movementX: int,
            ?movementY: int,
            ?clickCount: int,
            ?modifiers: Enums.Types.MouseInputEvent.Modifiers[]
        ) =
        class
        end

        /// <summary>
        /// The type of the event, can be <c>mouseDown</c>, <c>mouseUp</c>, <c>mouseEnter</c>, <c>mouseLeave</c>, <c>contextMenu</c>, <c>mouseWheel</c> or <c>mouseMove</c>.
        /// </summary>
        [<Erase>]
        member val ``type``: Enums.Types.MouseInputEvent.Type = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val x: int = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val y: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The button pressed, can be <c>left</c>, <c>middle</c>, <c>right</c>.
        /// </summary>
        [<Erase>]
        member val button: Enums.Types.MouseInputEvent.Button = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val globalX: int = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val globalY: int = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val movementX: int = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val movementY: int = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val clickCount: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// An array of modifiers of the event, can be <c>shift</c>, <c>control</c>, <c>ctrl</c>, <c>alt</c>, <c>meta</c>, <c>command</c>, <c>cmd</c>, <c>iskeypad</c>, <c>isautorepeat</c>, <c>leftbuttondown</c>, <c>middlebuttondown</c>,
        /// <c>rightbuttondown</c>, <c>capslock</c>, <c>numlock</c>, <c>left</c>, <c>right</c>.
        /// </summary>
        [<Erase>]
        member val modifiers: Enums.Types.MouseInputEvent.Modifiers[] = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type MimeTypedBuffer
        /// <param name="mimeType">MIME type of the buffer.</param>
        /// <param name="charset">Charset of the buffer.</param>
        /// <param name="data">The actual Buffer content.</param>
        (data: Buffer, ?mimeType: string, ?charset: string) =
        class
        end

        /// <summary>
        /// MIME type of the buffer.
        /// </summary>
        [<Erase>]
        member val mimeType: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Charset of the buffer.
        /// </summary>
        [<Erase>]
        member val charset: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The actual Buffer content.
        /// </summary>
        [<Erase>]
        member val data: Buffer = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type MemoryUsageDetails
        /// <param name="count"></param>
        /// <param name="size"></param>
        /// <param name="liveSize"></param>
        (count: float, size: float, liveSize: float) =
        class
        end

        [<Erase>]
        member val count: float = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val size: float = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val liveSize: float = Unchecked.defaultof<_> with get, set

    /// <summary>
    /// Note that all statistics are reported in Kilobytes.
    /// </summary>
    [<JS.Pojo>]
    type MemoryInfo
        /// <param name="workingSetSize">The amount of memory currently pinned to actual physical RAM.</param>
        /// <param name="peakWorkingSetSize">The maximum amount of memory that has ever been pinned to actual physical RAM.</param>
        /// <param name="privateBytes">⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌ || The amount of memory
        /// not shared by other processes, such as JS heap or HTML content.</param>
        (
            workingSetSize: int,
            peakWorkingSetSize: int
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
            ,
            ?privateBytes: int
            #endif

        ) =
        class
        end

        /// <summary>
        /// The amount of memory currently pinned to actual physical RAM.
        /// </summary>
        [<Erase>]
        member val workingSetSize: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The maximum amount of memory that has ever been pinned to actual physical RAM.
        /// </summary>
        [<Erase>]
        member val peakWorkingSetSize: int = Unchecked.defaultof<_> with get, set
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌</para>
        /// The amount of memory not shared by other processes, such as JS heap or HTML content.
        /// </summary>
        [<Erase>]
        member val privateBytes: int = Unchecked.defaultof<_> with get, set
        #endif


    [<JS.Pojo>]
    type MediaAccessPermissionRequest
        /// <param name="securityOrigin">The security origin of the request.</param>
        /// <param name="mediaTypes">The types of media access being requested - elements can be <c>video</c> or <c>audio</c>.</param>
        /// <param name="requestingUrl">The last URL the requesting frame loaded.</param>
        /// <param name="isMainFrame">Whether the frame making the request is the main frame.</param>
        (
            requestingUrl: string,
            isMainFrame: bool,
            ?securityOrigin: string,
            ?mediaTypes: Enums.Types.MediaAccessPermissionRequest.MediaTypes[]
        ) =
        class
        end

        /// <summary>
        /// The security origin of the request.
        /// </summary>
        [<Erase>]
        member val securityOrigin: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The types of media access being requested - elements can be <c>video</c> or <c>audio</c>.
        /// </summary>
        [<Erase>]
        member val mediaTypes: Enums.Types.MediaAccessPermissionRequest.MediaTypes[] =
            Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The last URL the requesting frame loaded.
        /// </summary>
        [<Erase>]
        member val requestingUrl: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether the frame making the request is the main frame.
        /// </summary>
        [<Erase>]
        member val isMainFrame: bool = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type KeyboardInputEvent
        /// <param name="type">The type of the event, can be <c>rawKeyDown</c>, <c>keyDown</c>, <c>keyUp</c> or <c>char</c>.</param>
        /// <param name="keyCode">The character that will be sent as the keyboard event. Should only use valid Accelerator key codes.</param>
        /// <param name="modifiers">An array of modifiers of the event, can be <c>shift</c>, <c>control</c>, <c>ctrl</c>, <c>alt</c>, <c>meta</c>, <c>command</c>, <c>cmd</c>, <c>iskeypad</c>, <c>isautorepeat</c>, <c>leftbuttondown</c>,
        /// <c>middlebuttondown</c>, <c>rightbuttondown</c>, <c>capslock</c>, <c>numlock</c>, <c>left</c>, <c>right</c>.</param>
        (
            ``type``: Enums.Types.KeyboardInputEvent.Type,
            keyCode: string,
            ?modifiers: Enums.Types.KeyboardInputEvent.Modifiers[]
        ) =
        class
        end

        /// <summary>
        /// The type of the event, can be <c>rawKeyDown</c>, <c>keyDown</c>, <c>keyUp</c> or <c>char</c>.
        /// </summary>
        [<Erase>]
        member val ``type``: Enums.Types.KeyboardInputEvent.Type = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The character that will be sent as the keyboard event. Should only use valid Accelerator key codes.
        /// </summary>
        [<Erase>]
        member val keyCode: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// An array of modifiers of the event, can be <c>shift</c>, <c>control</c>, <c>ctrl</c>, <c>alt</c>, <c>meta</c>, <c>command</c>, <c>cmd</c>, <c>iskeypad</c>, <c>isautorepeat</c>, <c>leftbuttondown</c>, <c>middlebuttondown</c>,
        /// <c>rightbuttondown</c>, <c>capslock</c>, <c>numlock</c>, <c>left</c>, <c>right</c>.
        /// </summary>
        [<Erase>]
        member val modifiers: Enums.Types.KeyboardInputEvent.Modifiers[] = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type KeyboardEvent
        /// <param name="ctrlKey">whether the Control key was used in an accelerator to trigger the Event</param>
        /// <param name="metaKey">whether a meta key was used in an accelerator to trigger the Event</param>
        /// <param name="shiftKey">whether a Shift key was used in an accelerator to trigger the Event</param>
        /// <param name="altKey">whether an Alt key was used in an accelerator to trigger the Event</param>
        /// <param name="triggeredByAccelerator">whether an accelerator was used to trigger the event as opposed to another user gesture like mouse click</param>
        (?ctrlKey: bool, ?metaKey: bool, ?shiftKey: bool, ?altKey: bool, ?triggeredByAccelerator: bool) =
        class
        end

        /// <summary>
        /// whether the Control key was used in an accelerator to trigger the Event
        /// </summary>
        [<Erase>]
        member val ctrlKey: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// whether a meta key was used in an accelerator to trigger the Event
        /// </summary>
        [<Erase>]
        member val metaKey: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// whether a Shift key was used in an accelerator to trigger the Event
        /// </summary>
        [<Erase>]
        member val shiftKey: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// whether an Alt key was used in an accelerator to trigger the Event
        /// </summary>
        [<Erase>]
        member val altKey: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// whether an accelerator was used to trigger the event as opposed to another user gesture like mouse click
        /// </summary>
        [<Erase>]
        member val triggeredByAccelerator: bool = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type JumpListItem
        /// <param name="type">One of the following:</param>
        /// <param name="path">Path of the file to open, should only be set if <c>type</c> is <c>file</c>.</param>
        /// <param name="program">Path of the program to execute, usually you should specify <c>process.execPath</c> which opens the current program. Should only be
        /// set if <c>type</c> is <c>task</c>.</param>
        /// <param name="args">The command line arguments when <c>program</c> is executed. Should only be set if <c>type</c> is <c>task</c>.</param>
        /// <param name="title">The text to be displayed for the item in the Jump List. Should only be set if <c>type</c> is
        /// <c>task</c>.</param>
        /// <param name="description">Description of the task (displayed in a tooltip). Should only be set if <c>type</c> is <c>task</c>. Maximum length 260
        /// characters.</param>
        /// <param name="iconPath">The absolute path to an icon to be displayed in a Jump List, which can be an arbitrary resource
        /// file that contains an icon (e.g. <c>.ico</c>, <c>.exe</c>, <c>.dll</c>). You can usually specify <c>process.execPath</c> to show the program icon.</param>
        /// <param name="iconIndex">The index of the icon in the resource file. If a resource file contains multiple icons this value can
        /// be used to specify the zero-based index of the icon that should be displayed for this task. If a resource
        /// file contains only one icon, this property should be set to zero.</param>
        /// <param name="workingDirectory">The working directory. Default is empty.</param>
        (
            ?``type``: Enums.Types.JumpListItem.Type,
            ?path: string,
            ?program: string,
            ?args: string,
            ?title: string,
            ?description: string,
            ?iconPath: string,
            ?iconIndex: float,
            ?workingDirectory: string
        ) =
        class
        end

        /// <summary>
        /// One of the following:
        /// </summary>
        [<Erase>]
        member val ``type``: Enums.Types.JumpListItem.Type = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Path of the file to open, should only be set if <c>type</c> is <c>file</c>.
        /// </summary>
        [<Erase>]
        member val path: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Path of the program to execute, usually you should specify <c>process.execPath</c> which opens the current program. Should only be set
        /// if <c>type</c> is <c>task</c>.
        /// </summary>
        [<Erase>]
        member val program: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The command line arguments when <c>program</c> is executed. Should only be set if <c>type</c> is <c>task</c>.
        /// </summary>
        [<Erase>]
        member val args: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The text to be displayed for the item in the Jump List. Should only be set if <c>type</c> is <c>task</c>.
        /// </summary>
        [<Erase>]
        member val title: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Description of the task (displayed in a tooltip). Should only be set if <c>type</c> is <c>task</c>. Maximum length 260 characters.
        /// </summary>
        [<Erase>]
        member val description: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The absolute path to an icon to be displayed in a Jump List, which can be an arbitrary resource file
        /// that contains an icon (e.g. <c>.ico</c>, <c>.exe</c>, <c>.dll</c>). You can usually specify <c>process.execPath</c> to show the program icon.
        /// </summary>
        [<Erase>]
        member val iconPath: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The index of the icon in the resource file. If a resource file contains multiple icons this value can be
        /// used to specify the zero-based index of the icon that should be displayed for this task. If a resource file
        /// contains only one icon, this property should be set to zero.
        /// </summary>
        [<Erase>]
        member val iconIndex: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The working directory. Default is empty.
        /// </summary>
        [<Erase>]
        member val workingDirectory: string = Unchecked.defaultof<_> with get, set

    /// <summary>
    /// &gt; [!NOTE] If a <c>JumpListCategory</c> object has neither the <c>type</c> nor the <c>name</c> property set then its <c>type</c> is assumed
    /// to be <c>tasks</c>. If the <c>name</c> property is set but the <c>type</c> property is omitted then the <c>type</c> is assumed
    /// to be <c>custom</c>.<br/><br/>&gt; [!NOTE] The maximum length of a Jump List item's <c>description</c> property is 260 characters. Beyond this limit,
    /// the item will not be added to the Jump List, nor will it be displayed.
    /// </summary>
    [<JS.Pojo>]
    type JumpListCategory
        /// <param name="type">One of the following:</param>
        /// <param name="name">Must be set if <c>type</c> is <c>custom</c>, otherwise it should be omitted.</param>
        /// <param name="items">Array of <c>JumpListItem</c> objects if <c>type</c> is <c>tasks</c> or <c>custom</c>, otherwise it should be omitted.</param>
        (?``type``: Enums.Types.JumpListCategory.Type, ?name: string, ?items: JumpListItem[]) =
        class
        end

        /// <summary>
        /// One of the following:
        /// </summary>
        [<Erase>]
        member val ``type``: Enums.Types.JumpListCategory.Type = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Must be set if <c>type</c> is <c>custom</c>, otherwise it should be omitted.
        /// </summary>
        [<Erase>]
        member val name: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Array of <c>JumpListItem</c> objects if <c>type</c> is <c>tasks</c> or <c>custom</c>, otherwise it should be omitted.
        /// </summary>
        [<Erase>]
        member val items: JumpListItem[] = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type IpcRendererEvent
        /// <param name="sender">The <c>IpcRenderer</c> instance that emitted the event originally</param>
        /// <param name="ports">A list of MessagePorts that were transferred with this message</param>
        (sender: IpcRenderer, ports: MessagePort[]) =
        class
        end

        /// <summary>
        /// The <c>IpcRenderer</c> instance that emitted the event originally
        /// </summary>
        [<Erase>]
        member val sender: IpcRenderer = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// A list of MessagePorts that were transferred with this message
        /// </summary>
        [<Erase>]
        member val ports: MessagePort[] = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type IpcMainServiceWorkerInvokeEvent
        /// <param name="type">Possible values include <c>service-worker</c>.</param>
        /// <param name="serviceWorker">The service worker that sent this message</param>
        /// <param name="versionId">The service worker version ID.</param>
        /// <param name="session">The <c>Session</c> instance with which the event is associated.</param>
        (
            ``type``: Enums.Types.IpcMainServiceWorkerInvokeEvent.Type,
            serviceWorker: ServiceWorkerMain,
            versionId: float,
            session: Session
        ) =
        class
        end

        /// <summary>
        /// Possible values include <c>service-worker</c>.
        /// </summary>
        [<Erase>]
        member val ``type``: Enums.Types.IpcMainServiceWorkerInvokeEvent.Type = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The service worker that sent this message
        /// </summary>
        [<Erase>]
        member val serviceWorker: ServiceWorkerMain = Unchecked.defaultof<_> with get

        /// <summary>
        /// The service worker version ID.
        /// </summary>
        [<Erase>]
        member val versionId: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The <c>Session</c> instance with which the event is associated.
        /// </summary>
        [<Erase>]
        member val session: Session = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type IpcMainServiceWorkerEvent
        /// <param name="type">Possible values include <c>service-worker</c>.</param>
        /// <param name="serviceWorker">The service worker that sent this message</param>
        /// <param name="versionId">The service worker version ID.</param>
        /// <param name="session">The <c>Session</c> instance with which the event is associated.</param>
        /// <param name="returnValue">Set this to the value to be returned in a synchronous message</param>
        /// <param name="ports">A list of MessagePorts that were transferred with this message</param>
        /// <param name="reply">A function that will send an IPC message to the renderer frame that sent the original message that you
        /// are currently handling.  You should use this method to "reply" to the sent message in order to guarantee the
        /// reply will go to the correct process and frame.</param>
        (
            ``type``: Enums.Types.IpcMainServiceWorkerEvent.Type,
            serviceWorker: ServiceWorkerMain,
            versionId: float,
            session: Session,
            returnValue: obj,
            ports: MessagePortMain[],
            reply: Types.IpcMainServiceWorkerEvent.Reply
        ) =
        class
        end

        /// <summary>
        /// Possible values include <c>service-worker</c>.
        /// </summary>
        [<Erase>]
        member val ``type``: Enums.Types.IpcMainServiceWorkerEvent.Type = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The service worker that sent this message
        /// </summary>
        [<Erase>]
        member val serviceWorker: ServiceWorkerMain = Unchecked.defaultof<_> with get

        /// <summary>
        /// The service worker version ID.
        /// </summary>
        [<Erase>]
        member val versionId: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The <c>Session</c> instance with which the event is associated.
        /// </summary>
        [<Erase>]
        member val session: Session = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Set this to the value to be returned in a synchronous message
        /// </summary>
        [<Erase>]
        member val returnValue: obj = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// A list of MessagePorts that were transferred with this message
        /// </summary>
        [<Erase>]
        member val ports: MessagePortMain[] = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// A function that will send an IPC message to the renderer frame that sent the original message that you are
        /// currently handling.  You should use this method to "reply" to the sent message in order to guarantee the reply
        /// will go to the correct process and frame.
        /// </summary>
        [<Erase>]
        member val reply: Types.IpcMainServiceWorkerEvent.Reply = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type IpcMainInvokeEvent
        /// <param name="type">Possible values include <c>frame</c></param>
        /// <param name="processId">The internal ID of the renderer process that sent this message</param>
        /// <param name="frameId">The ID of the renderer frame that sent this message</param>
        /// <param name="sender">Returns the <c>webContents</c> that sent the message</param>
        /// <param name="senderFrame">The frame that sent this message. May be <c>null</c> if accessed after the frame has either navigated or been
        /// destroyed.</param>
        (
            ``type``: Enums.Types.IpcMainInvokeEvent.Type,
            processId: int,
            frameId: int,
            sender: WebContents,
            senderFrame: Option<WebFrameMain>
        ) =
        class
        end

        /// <summary>
        /// Possible values include <c>frame</c>
        /// </summary>
        [<Erase>]
        member val ``type``: Enums.Types.IpcMainInvokeEvent.Type = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The internal ID of the renderer process that sent this message
        /// </summary>
        [<Erase>]
        member val processId: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The ID of the renderer frame that sent this message
        /// </summary>
        [<Erase>]
        member val frameId: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Returns the <c>webContents</c> that sent the message
        /// </summary>
        [<Erase>]
        member val sender: WebContents = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The frame that sent this message. May be <c>null</c> if accessed after the frame has either navigated or been destroyed.
        /// </summary>
        [<Erase>]
        member val senderFrame: Option<WebFrameMain> = Unchecked.defaultof<_> with get

    [<JS.Pojo>]
    type IpcMainEvent
        /// <param name="type">Possible values include <c>frame</c></param>
        /// <param name="processId">The internal ID of the renderer process that sent this message</param>
        /// <param name="frameId">The ID of the renderer frame that sent this message</param>
        /// <param name="returnValue">Set this to the value to be returned in a synchronous message</param>
        /// <param name="sender">Returns the <c>webContents</c> that sent the message</param>
        /// <param name="senderFrame">The frame that sent this message. May be <c>null</c> if accessed after the frame has either navigated or been
        /// destroyed.</param>
        /// <param name="ports">A list of MessagePorts that were transferred with this message</param>
        /// <param name="reply">A function that will send an IPC message to the renderer frame that sent the original message that you
        /// are currently handling.  You should use this method to "reply" to the sent message in order to guarantee the
        /// reply will go to the correct process and frame.</param>
        (
            ``type``: Enums.Types.IpcMainEvent.Type,
            processId: int,
            frameId: int,
            returnValue: obj,
            sender: WebContents,
            senderFrame: Option<WebFrameMain>,
            ports: MessagePortMain[],
            reply: Types.IpcMainEvent.Reply
        ) =
        class
        end

        /// <summary>
        /// Possible values include <c>frame</c>
        /// </summary>
        [<Erase>]
        member val ``type``: Enums.Types.IpcMainEvent.Type = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The internal ID of the renderer process that sent this message
        /// </summary>
        [<Erase>]
        member val processId: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The ID of the renderer frame that sent this message
        /// </summary>
        [<Erase>]
        member val frameId: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Set this to the value to be returned in a synchronous message
        /// </summary>
        [<Erase>]
        member val returnValue: obj = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Returns the <c>webContents</c> that sent the message
        /// </summary>
        [<Erase>]
        member val sender: WebContents = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The frame that sent this message. May be <c>null</c> if accessed after the frame has either navigated or been destroyed.
        /// </summary>
        [<Erase>]
        member val senderFrame: Option<WebFrameMain> = Unchecked.defaultof<_> with get

        /// <summary>
        /// A list of MessagePorts that were transferred with this message
        /// </summary>
        [<Erase>]
        member val ports: MessagePortMain[] = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// A function that will send an IPC message to the renderer frame that sent the original message that you are
        /// currently handling.  You should use this method to "reply" to the sent message in order to guarantee the reply
        /// will go to the correct process and frame.
        /// </summary>
        [<Erase>]
        member val reply: Types.IpcMainEvent.Reply = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type InputEvent
        /// <param name="type">Can be <c>undefined</c>, <c>mouseDown</c>, <c>mouseUp</c>, <c>mouseMove</c>, <c>mouseEnter</c>, <c>mouseLeave</c>, <c>contextMenu</c>, <c>mouseWheel</c>, <c>rawKeyDown</c>, <c>keyDown</c>, <c>keyUp</c>, <c>char</c>, <c>gestureScrollBegin</c>, <c>gestureScrollEnd</c>, <c>gestureScrollUpdate</c>, <c>gestureFlingStart</c>, <c>gestureFlingCancel</c>,
        /// <c>gesturePinchBegin</c>, <c>gesturePinchEnd</c>, <c>gesturePinchUpdate</c>, <c>gestureTapDown</c>, <c>gestureShowPress</c>, <c>gestureTap</c>, <c>gestureTapCancel</c>, <c>gestureShortPress</c>, <c>gestureLongPress</c>, <c>gestureLongTap</c>, <c>gestureTwoFingerTap</c>, <c>gestureTapUnconfirmed</c>, <c>gestureDoubleTap</c>, <c>touchStart</c>, <c>touchMove</c>, <c>touchEnd</c>, <c>touchCancel</c>, <c>touchScrollStarted</c>, <c>pointerDown</c>, <c>pointerUp</c>,
        /// <c>pointerMove</c>, <c>pointerRawUpdate</c>, <c>pointerCancel</c> or <c>pointerCausedUaAction</c>.</param>
        /// <param name="modifiers">An array of modifiers of the event, can be <c>shift</c>, <c>control</c>, <c>ctrl</c>, <c>alt</c>, <c>meta</c>, <c>command</c>, <c>cmd</c>, <c>iskeypad</c>, <c>isautorepeat</c>, <c>leftbuttondown</c>,
        /// <c>middlebuttondown</c>, <c>rightbuttondown</c>, <c>capslock</c>, <c>numlock</c>, <c>left</c>, <c>right</c>.</param>
        (``type``: Enums.Types.InputEvent.Type, ?modifiers: Enums.Types.InputEvent.Modifiers[]) =
        class
        end

        /// <summary>
        /// Can be <c>undefined</c>, <c>mouseDown</c>, <c>mouseUp</c>, <c>mouseMove</c>, <c>mouseEnter</c>, <c>mouseLeave</c>, <c>contextMenu</c>, <c>mouseWheel</c>, <c>rawKeyDown</c>, <c>keyDown</c>, <c>keyUp</c>, <c>char</c>, <c>gestureScrollBegin</c>, <c>gestureScrollEnd</c>, <c>gestureScrollUpdate</c>, <c>gestureFlingStart</c>, <c>gestureFlingCancel</c>, <c>gesturePinchBegin</c>,
        /// <c>gesturePinchEnd</c>, <c>gesturePinchUpdate</c>, <c>gestureTapDown</c>, <c>gestureShowPress</c>, <c>gestureTap</c>, <c>gestureTapCancel</c>, <c>gestureShortPress</c>, <c>gestureLongPress</c>, <c>gestureLongTap</c>, <c>gestureTwoFingerTap</c>, <c>gestureTapUnconfirmed</c>, <c>gestureDoubleTap</c>, <c>touchStart</c>, <c>touchMove</c>, <c>touchEnd</c>, <c>touchCancel</c>, <c>touchScrollStarted</c>, <c>pointerDown</c>, <c>pointerUp</c>, <c>pointerMove</c>,
        /// <c>pointerRawUpdate</c>, <c>pointerCancel</c> or <c>pointerCausedUaAction</c>.
        /// </summary>
        [<Erase>]
        member val ``type``: Enums.Types.InputEvent.Type = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// An array of modifiers of the event, can be <c>shift</c>, <c>control</c>, <c>ctrl</c>, <c>alt</c>, <c>meta</c>, <c>command</c>, <c>cmd</c>, <c>iskeypad</c>, <c>isautorepeat</c>, <c>leftbuttondown</c>, <c>middlebuttondown</c>,
        /// <c>rightbuttondown</c>, <c>capslock</c>, <c>numlock</c>, <c>left</c>, <c>right</c>.
        /// </summary>
        [<Erase>]
        member val modifiers: Enums.Types.InputEvent.Modifiers[] = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type HIDDevice
        /// <param name="deviceId">Unique identifier for the device.</param>
        /// <param name="name">Name of the device.</param>
        /// <param name="vendorId">The USB vendor ID.</param>
        /// <param name="productId">The USB product ID.</param>
        /// <param name="serialNumber">The USB device serial number.</param>
        /// <param name="guid">Unique identifier for the HID interface.  A device may have multiple HID interfaces.</param>
        /// <param name="collections">an array of report formats. See MDN documentation for more.</param>
        (
            deviceId: string,
            name: string,
            vendorId: int,
            productId: int,
            collections: Types.HIDDevice.Collections[],
            ?serialNumber: string,
            ?guid: string
        ) =
        class
        end

        /// <summary>
        /// Unique identifier for the device.
        /// </summary>
        [<Erase>]
        member val deviceId: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Name of the device.
        /// </summary>
        [<Erase>]
        member val name: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The USB vendor ID.
        /// </summary>
        [<Erase>]
        member val vendorId: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The USB product ID.
        /// </summary>
        [<Erase>]
        member val productId: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The USB device serial number.
        /// </summary>
        [<Erase>]
        member val serialNumber: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Unique identifier for the HID interface.  A device may have multiple HID interfaces.
        /// </summary>
        [<Erase>]
        member val guid: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// an array of report formats. See MDN documentation for more.
        /// </summary>
        [<Erase>]
        member val collections: Types.HIDDevice.Collections[] = Unchecked.defaultof<_> with get, set

    /// <summary>
    /// Possible values:<br/><br/>* <c>disabled_software</c> - Software only. Hardware acceleration disabled (yellow)<br/>* <c>disabled_off</c> - Disabled (red)<br/>* <c>disabled_off_ok</c> - Disabled (yellow)<br/>* <c>unavailable_software</c> -
    /// Software only, hardware acceleration unavailable (yellow)<br/>* <c>unavailable_off</c> - Unavailable (red)<br/>* <c>unavailable_off_ok</c> - Unavailable (yellow)<br/>* <c>enabled_readback</c> - Hardware accelerated but at
    /// reduced performance (yellow)<br/>* <c>enabled_force</c> - Hardware accelerated on all pages (green)<br/>* <c>enabled</c> - Hardware accelerated (green)<br/>* <c>enabled_on</c> - Enabled (green)<br/>*
    /// <c>enabled_force_on</c> - Force enabled (green)
    /// </summary>
    [<JS.Pojo>]
    type GPUFeatureStatus
        /// <param name="2dCanvas">Canvas.</param>
        /// <param name="flash_3d">Flash.</param>
        /// <param name="flashStage3d">Flash Stage3D.</param>
        /// <param name="flashStage3dBaseline">Flash Stage3D Baseline profile.</param>
        /// <param name="gpuCompositing">Compositing.</param>
        /// <param name="multipleRasterThreads">Multiple Raster Threads.</param>
        /// <param name="nativeGpuMemoryBuffers">Native GpuMemoryBuffers.</param>
        /// <param name="rasterization">Rasterization.</param>
        /// <param name="videoDecode">Video Decode.</param>
        /// <param name="videoEncode">Video Encode.</param>
        /// <param name="vpxDecode">VPx Video Decode.</param>
        /// <param name="webgl">WebGL.</param>
        /// <param name="webgl2">WebGL2.</param>
        (
            ``2dCanvas``: string,
            flash_3d: string,
            flashStage3d: string,
            flashStage3dBaseline: string,
            gpuCompositing: string,
            multipleRasterThreads: string,
            nativeGpuMemoryBuffers: string,
            rasterization: string,
            videoDecode: string,
            videoEncode: string,
            vpxDecode: string,
            webgl: string,
            webgl2: string
        ) =
        class
        end

        /// <summary>
        /// Canvas.
        /// </summary>
        [<Erase>]
        member val ``2dCanvas``: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Flash.
        /// </summary>
        [<Erase>]
        member val flash_3d: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Flash Stage3D.
        /// </summary>
        [<Erase; Emit("$0.flash_stage3d{{ = $1 }}")>]
        member _.flashStage3d
            with get (): string = Unchecked.defaultof<_>
            and set (value: string) = ()

        /// <summary>
        /// Flash Stage3D Baseline profile.
        /// </summary>
        [<Erase; Emit("$0.flash_stage3d_baseline{{ = $1 }}")>]
        member _.flashStage3dBaseline
            with get (): string = Unchecked.defaultof<_>
            and set (value: string) = ()

        /// <summary>
        /// Compositing.
        /// </summary>
        [<Erase; Emit("$0.gpu_compositing{{ = $1 }}")>]
        member _.gpuCompositing
            with get (): string = Unchecked.defaultof<_>
            and set (value: string) = ()

        /// <summary>
        /// Multiple Raster Threads.
        /// </summary>
        [<Erase; Emit("$0.multiple_raster_threads{{ = $1 }}")>]
        member _.multipleRasterThreads
            with get (): string = Unchecked.defaultof<_>
            and set (value: string) = ()

        /// <summary>
        /// Native GpuMemoryBuffers.
        /// </summary>
        [<Erase; Emit("$0.native_gpu_memory_buffers{{ = $1 }}")>]
        member _.nativeGpuMemoryBuffers
            with get (): string = Unchecked.defaultof<_>
            and set (value: string) = ()

        /// <summary>
        /// Rasterization.
        /// </summary>
        [<Erase>]
        member val rasterization: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Video Decode.
        /// </summary>
        [<Erase; Emit("$0.video_decode{{ = $1 }}")>]
        member _.videoDecode
            with get (): string = Unchecked.defaultof<_>
            and set (value: string) = ()

        /// <summary>
        /// Video Encode.
        /// </summary>
        [<Erase; Emit("$0.video_encode{{ = $1 }}")>]
        member _.videoEncode
            with get (): string = Unchecked.defaultof<_>
            and set (value: string) = ()

        /// <summary>
        /// VPx Video Decode.
        /// </summary>
        [<Erase; Emit("$0.vpx_decode{{ = $1 }}")>]
        member _.vpxDecode
            with get (): string = Unchecked.defaultof<_>
            and set (value: string) = ()

        /// <summary>
        /// WebGL.
        /// </summary>
        [<Erase>]
        member val webgl: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// WebGL2.
        /// </summary>
        [<Erase>]
        member val webgl2: string = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type FilesystemPermissionRequest
        /// <param name="filePath">The path of the <c>fileSystem</c> request.</param>
        /// <param name="isDirectory">Whether the <c>fileSystem</c> request is a directory.</param>
        /// <param name="fileAccessType">The access type of the <c>fileSystem</c> request. Can be <c>writable</c> or <c>readable</c>.</param>
        /// <param name="requestingUrl">The last URL the requesting frame loaded.</param>
        /// <param name="isMainFrame">Whether the frame making the request is the main frame.</param>
        (
            requestingUrl: string,
            isMainFrame: bool,
            ?filePath: string,
            ?isDirectory: bool,
            ?fileAccessType: Enums.Types.FilesystemPermissionRequest.FileAccessType
        ) =
        class
        end

        /// <summary>
        /// The path of the <c>fileSystem</c> request.
        /// </summary>
        [<Erase>]
        member val filePath: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether the <c>fileSystem</c> request is a directory.
        /// </summary>
        [<Erase>]
        member val isDirectory: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The access type of the <c>fileSystem</c> request. Can be <c>writable</c> or <c>readable</c>.
        /// </summary>
        [<Erase>]
        member val fileAccessType: Enums.Types.FilesystemPermissionRequest.FileAccessType =
            Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The last URL the requesting frame loaded.
        /// </summary>
        [<Erase>]
        member val requestingUrl: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether the frame making the request is the main frame.
        /// </summary>
        [<Erase>]
        member val isMainFrame: bool = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type FilePathWithHeaders
        /// <param name="path">The path to the file to send.</param>
        /// <param name="headers">Additional headers to be sent.</param>
        (path: string, ?headers: Record<string, string>) =
        class
        end

        /// <summary>
        /// The path to the file to send.
        /// </summary>
        [<Erase>]
        member val path: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Additional headers to be sent.
        /// </summary>
        [<Erase>]
        member val headers: Record<string, string> = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type FileFilter
        /// <param name="name"></param>
        /// <param name="extensions"></param>
        (name: string, extensions: string[]) =
        class
        end

        [<Erase>]
        member val name: string = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val extensions: string[] = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type Extension
        /// <param name="id"></param>
        /// <param name="manifest">Copy of the extension's manifest data.</param>
        /// <param name="name"></param>
        /// <param name="path">The extension's file path.</param>
        /// <param name="version"></param>
        /// <param name="url">The extension's <c>chrome-extension://</c> URL.</param>
        (id: string, manifest: obj, name: string, path: string, version: string, url: string) =
        class
        end

        [<Erase>]
        member val id: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Copy of the extension's manifest data.
        /// </summary>
        [<Erase>]
        member val manifest: obj = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val name: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The extension's file path.
        /// </summary>
        [<Erase>]
        member val path: string = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val version: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The extension's <c>chrome-extension://</c> URL.
        /// </summary>
        [<Erase>]
        member val url: string = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type ExtensionInfo
        /// <param name="name"></param>
        /// <param name="version"></param>
        (name: string, version: string) =
        class
        end

        [<Erase>]
        member val name: string = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val version: string = Unchecked.defaultof<_> with get, set

    /// <summary>
    /// The <c>Display</c> object represents a physical display connected to the system. A fake <c>Display</c> may exist on a headless system,
    /// or a <c>Display</c> may correspond to a remote, virtual display.
    /// </summary>
    [<JS.Pojo>]
    type Display
        /// <param name="accelerometerSupport">Can be <c>available</c>, <c>unavailable</c>, <c>unknown</c>.</param>
        /// <param name="bounds">the bounds of the display in DIP points.</param>
        /// <param name="colorDepth">The number of bits per pixel.</param>
        /// <param name="colorSpace"> represent a color space (three-dimensional object which contains all realizable color combinations) for the purpose of color conversions.</param>
        /// <param name="depthPerComponent">The number of bits per color component.</param>
        /// <param name="detected"><c>true</c> if the display is detected by the system.</param>
        /// <param name="displayFrequency">The display refresh rate.</param>
        /// <param name="id">Unique identifier associated with the display. A value of of -1 means the display is invalid or the correct
        /// <c>id</c> is not yet known, and a value of -10 means the display is a virtual display assigned to a
        /// unified desktop.</param>
        /// <param name="internal"><c>true</c> for an internal display and <c>false</c> for an external display.</param>
        /// <param name="label">User-friendly label, determined by the platform.</param>
        /// <param name="maximumCursorSize">Maximum cursor size in native pixels.</param>
        /// <param name="nativeOrigin">Returns the display's origin in pixel coordinates. Only available on windowing systems like X11 that position displays in pixel
        /// coordinates.</param>
        /// <param name="rotation">Can be 0, 90, 180, 270, represents screen rotation in clock-wise degrees.</param>
        /// <param name="scaleFactor">Output device's pixel scale factor.</param>
        /// <param name="touchSupport">Can be <c>available</c>, <c>unavailable</c>, <c>unknown</c>.</param>
        /// <param name="monochrome">Whether or not the display is a monochrome display.</param>
        /// <param name="size"></param>
        /// <param name="workArea">the work area of the display in DIP points.</param>
        /// <param name="workAreaSize">The size of the work area.</param>
        (
            accelerometerSupport: Enums.Types.Display.AccelerometerSupport,
            bounds: Rectangle,
            colorDepth: float,
            colorSpace: string,
            depthPerComponent: float,
            detected: bool,
            displayFrequency: float,
            id: float,
            ``internal``: bool,
            label: string,
            maximumCursorSize: Size,
            nativeOrigin: Point,
            rotation: float,
            scaleFactor: float,
            touchSupport: Enums.Types.Display.TouchSupport,
            monochrome: bool,
            size: Size,
            workArea: Rectangle,
            workAreaSize: Size
        ) =
        class
        end

        /// <summary>
        /// Can be <c>available</c>, <c>unavailable</c>, <c>unknown</c>.
        /// </summary>
        [<Erase>]
        member val accelerometerSupport: Enums.Types.Display.AccelerometerSupport = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// the bounds of the display in DIP points.
        /// </summary>
        [<Erase>]
        member val bounds: Rectangle = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The number of bits per pixel.
        /// </summary>
        [<Erase>]
        member val colorDepth: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        ///  represent a color space (three-dimensional object which contains all realizable color combinations) for the purpose of color conversions.
        /// </summary>
        [<Erase>]
        member val colorSpace: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The number of bits per color component.
        /// </summary>
        [<Erase>]
        member val depthPerComponent: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// <c>true</c> if the display is detected by the system.
        /// </summary>
        [<Erase>]
        member val detected: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The display refresh rate.
        /// </summary>
        [<Erase>]
        member val displayFrequency: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Unique identifier associated with the display. A value of of -1 means the display is invalid or the correct <c>id</c>
        /// is not yet known, and a value of -10 means the display is a virtual display assigned to a unified
        /// desktop.
        /// </summary>
        [<Erase>]
        member val id: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// <c>true</c> for an internal display and <c>false</c> for an external display.
        /// </summary>
        [<Erase>]
        member val ``internal``: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// User-friendly label, determined by the platform.
        /// </summary>
        [<Erase>]
        member val label: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Maximum cursor size in native pixels.
        /// </summary>
        [<Erase>]
        member val maximumCursorSize: Size = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Returns the display's origin in pixel coordinates. Only available on windowing systems like X11 that position displays in pixel coordinates.
        /// </summary>
        [<Erase>]
        member val nativeOrigin: Point = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Can be 0, 90, 180, 270, represents screen rotation in clock-wise degrees.
        /// </summary>
        [<Erase>]
        member val rotation: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Output device's pixel scale factor.
        /// </summary>
        [<Erase>]
        member val scaleFactor: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Can be <c>available</c>, <c>unavailable</c>, <c>unknown</c>.
        /// </summary>
        [<Erase>]
        member val touchSupport: Enums.Types.Display.TouchSupport = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether or not the display is a monochrome display.
        /// </summary>
        [<Erase>]
        member val monochrome: bool = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val size: Size = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// the work area of the display in DIP points.
        /// </summary>
        [<Erase>]
        member val workArea: Rectangle = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The size of the work area.
        /// </summary>
        [<Erase>]
        member val workAreaSize: Size = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type DesktopCapturerSource
        /// <param name="id">The identifier of a window or screen that can be used as a <c>chromeMediaSourceId</c> constraint when calling <c>navigator.getUserMedia</c>. The
        /// format of the identifier will be <c>window:XX:YY</c> or <c>screen:ZZ:0</c>. XX is the windowID/handle. YY is 1 for the current process,
        /// and 0 for all others. ZZ is a sequential number that represents the screen, and it does not equal to
        /// the index in the source's name.</param>
        /// <param name="name">A screen source will be named either <c>Entire Screen</c> or <c>Screen &lt;index&gt;</c>, while the name of a window source
        /// will match the window title.</param>
        /// <param name="thumbnail">A thumbnail image. **Note:** There is no guarantee that the size of the thumbnail is the same as the
        /// <c>thumbnailSize</c> specified in the <c>options</c> passed to <c>desktopCapturer.getSources</c>. The actual size depends on the scale of the screen or window.</param>
        /// <param name="displayId">A unique identifier that will correspond to the <c>id</c> of the matching Display returned by the Screen API. On
        /// some platforms, this is equivalent to the <c>XX</c> portion of the <c>id</c> field above and on others it will differ.
        /// It will be an empty string if not available.</param>
        /// <param name="appIcon">An icon image of the application that owns the window or null if the source has a type screen.
        /// The size of the icon is not known in advance and depends on what the application provides.</param>
        (id: string, name: string, thumbnail: NativeImage, displayId: string, appIcon: NativeImage) =
        class
        end

        /// <summary>
        /// The identifier of a window or screen that can be used as a <c>chromeMediaSourceId</c> constraint when calling <c>navigator.getUserMedia</c>. The format
        /// of the identifier will be <c>window:XX:YY</c> or <c>screen:ZZ:0</c>. XX is the windowID/handle. YY is 1 for the current process, and
        /// 0 for all others. ZZ is a sequential number that represents the screen, and it does not equal to the
        /// index in the source's name.
        /// </summary>
        [<Erase>]
        member val id: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// A screen source will be named either <c>Entire Screen</c> or <c>Screen &lt;index&gt;</c>, while the name of a window source will
        /// match the window title.
        /// </summary>
        [<Erase>]
        member val name: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// A thumbnail image. **Note:** There is no guarantee that the size of the thumbnail is the same as the <c>thumbnailSize</c>
        /// specified in the <c>options</c> passed to <c>desktopCapturer.getSources</c>. The actual size depends on the scale of the screen or window.
        /// </summary>
        [<Erase>]
        member val thumbnail: NativeImage = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// A unique identifier that will correspond to the <c>id</c> of the matching Display returned by the Screen API. On some
        /// platforms, this is equivalent to the <c>XX</c> portion of the <c>id</c> field above and on others it will differ. It
        /// will be an empty string if not available.
        /// </summary>
        [<Erase; Emit("$0.display_id{{ = $1 }}")>]
        member _.displayId
            with get (): string = Unchecked.defaultof<_>
            and set (value: string) = ()

        /// <summary>
        /// An icon image of the application that owns the window or null if the source has a type screen. The
        /// size of the icon is not known in advance and depends on what the application provides.
        /// </summary>
        [<Erase>]
        member val appIcon: NativeImage = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type CustomScheme
        /// <param name="scheme">Custom schemes to be registered with options.</param>
        /// <param name="privileges"></param>
        (scheme: string, ?privileges: Types.CustomScheme.Privileges) =
        class
        end

        /// <summary>
        /// Custom schemes to be registered with options.
        /// </summary>
        [<Erase>]
        member val scheme: string = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val privileges: Types.CustomScheme.Privileges = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type CrashReport
        /// <param name="date"></param>
        /// <param name="id"></param>
        (date: System.DateTime, id: string) =
        class
        end

        [<Erase>]
        member val date: System.DateTime = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val id: string = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type CPUUsage
        /// <param name="percentCPUUsage">Percentage of CPU used since the last call to getCPUUsage. First call returns 0.</param>
        /// <param name="cumulativeCPUUsage">Total seconds of CPU time used since process startup.</param>
        /// <param name="idleWakeupsPerSecond">The number of average idle CPU wakeups per second since the last call to getCPUUsage. First call returns 0.
        /// Will always return 0 on Windows.</param>
        (percentCPUUsage: float, idleWakeupsPerSecond: float, ?cumulativeCPUUsage: float) =
        class
        end

        /// <summary>
        /// Percentage of CPU used since the last call to getCPUUsage. First call returns 0.
        /// </summary>
        [<Erase>]
        member val percentCPUUsage: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Total seconds of CPU time used since process startup.
        /// </summary>
        [<Erase>]
        member val cumulativeCPUUsage: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The number of average idle CPU wakeups per second since the last call to getCPUUsage. First call returns 0. Will
        /// always return 0 on Windows.
        /// </summary>
        [<Erase>]
        member val idleWakeupsPerSecond: float = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type Cookie
        /// <param name="name">The name of the cookie.</param>
        /// <param name="value">The value of the cookie.</param>
        /// <param name="domain">The domain of the cookie; this will be normalized with a preceding dot so that it's also valid for
        /// subdomains.</param>
        /// <param name="hostOnly">Whether the cookie is a host-only cookie; this will only be <c>true</c> if no domain was passed.</param>
        /// <param name="path">The path of the cookie.</param>
        /// <param name="secure">Whether the cookie is marked as secure.</param>
        /// <param name="httpOnly">Whether the cookie is marked as HTTP only.</param>
        /// <param name="session">Whether the cookie is a session cookie or a persistent cookie with an expiration date.</param>
        /// <param name="expirationDate">The expiration date of the cookie as the number of seconds since the UNIX epoch. Not provided for session
        /// cookies.</param>
        /// <param name="sameSite">The Same Site policy applied to this cookie.  Can be <c>unspecified</c>, <c>no_restriction</c>, <c>lax</c> or <c>strict</c>.</param>
        (
            name: string,
            value: string,
            sameSite: Enums.Types.Cookie.SameSite,
            ?domain: string,
            ?hostOnly: bool,
            ?path: string,
            ?secure: bool,
            ?httpOnly: bool,
            ?session: bool,
            ?expirationDate: double
        ) =
        class
        end

        /// <summary>
        /// The name of the cookie.
        /// </summary>
        [<Erase>]
        member val name: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The value of the cookie.
        /// </summary>
        [<Erase>]
        member val value: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The domain of the cookie; this will be normalized with a preceding dot so that it's also valid for subdomains.
        /// </summary>
        [<Erase>]
        member val domain: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether the cookie is a host-only cookie; this will only be <c>true</c> if no domain was passed.
        /// </summary>
        [<Erase>]
        member val hostOnly: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The path of the cookie.
        /// </summary>
        [<Erase>]
        member val path: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether the cookie is marked as secure.
        /// </summary>
        [<Erase>]
        member val secure: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether the cookie is marked as HTTP only.
        /// </summary>
        [<Erase>]
        member val httpOnly: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether the cookie is a session cookie or a persistent cookie with an expiration date.
        /// </summary>
        [<Erase>]
        member val session: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The expiration date of the cookie as the number of seconds since the UNIX epoch. Not provided for session cookies.
        /// </summary>
        [<Erase>]
        member val expirationDate: double = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The Same Site policy applied to this cookie.  Can be <c>unspecified</c>, <c>no_restriction</c>, <c>lax</c> or <c>strict</c>.
        /// </summary>
        [<Erase>]
        member val sameSite: Enums.Types.Cookie.SameSite = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type ColorSpace
        /// <param name="primaries">The color primaries of the color space. Can be one of the following values:</param>
        /// <param name="transfer">The transfer function of the color space. Can be one of the following values:</param>
        /// <param name="matrix">The color matrix of the color space. Can be one of the following values:</param>
        /// <param name="range">The color range of the color space. Can be one of the following values:</param>
        (
            primaries: Enums.Types.ColorSpace.Primaries,
            transfer: Enums.Types.ColorSpace.Transfer,
            matrix: Enums.Types.ColorSpace.Matrix,
            range: Enums.Types.ColorSpace.Range
        ) =
        class
        end

        /// <summary>
        /// The color primaries of the color space. Can be one of the following values:
        /// </summary>
        [<Erase>]
        member val primaries: Enums.Types.ColorSpace.Primaries = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The transfer function of the color space. Can be one of the following values:
        /// </summary>
        [<Erase>]
        member val transfer: Enums.Types.ColorSpace.Transfer = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The color matrix of the color space. Can be one of the following values:
        /// </summary>
        [<Erase>]
        member val matrix: Enums.Types.ColorSpace.Matrix = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The color range of the color space. Can be one of the following values:
        /// </summary>
        [<Erase>]
        member val range: Enums.Types.ColorSpace.Range = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type Certificate
        /// <param name="data">PEM encoded data</param>
        /// <param name="issuer">Issuer principal</param>
        /// <param name="issuerName">Issuer's Common Name</param>
        /// <param name="issuerCert">Issuer certificate (if not self-signed)</param>
        /// <param name="subject">Subject principal</param>
        /// <param name="subjectName">Subject's Common Name</param>
        /// <param name="serialNumber">Hex value represented string</param>
        /// <param name="validStart">Start date of the certificate being valid in seconds</param>
        /// <param name="validExpiry">End date of the certificate being valid in seconds</param>
        /// <param name="fingerprint">Fingerprint of the certificate</param>
        (
            data: string,
            issuer: CertificatePrincipal,
            issuerName: string,
            issuerCert: Certificate,
            subject: CertificatePrincipal,
            subjectName: string,
            serialNumber: string,
            validStart: float,
            validExpiry: float,
            fingerprint: string
        ) =
        class
        end

        /// <summary>
        /// PEM encoded data
        /// </summary>
        [<Erase>]
        member val data: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Issuer principal
        /// </summary>
        [<Erase>]
        member val issuer: CertificatePrincipal = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Issuer's Common Name
        /// </summary>
        [<Erase>]
        member val issuerName: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Issuer certificate (if not self-signed)
        /// </summary>
        [<Erase>]
        member val issuerCert: Certificate = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Subject principal
        /// </summary>
        [<Erase>]
        member val subject: CertificatePrincipal = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Subject's Common Name
        /// </summary>
        [<Erase>]
        member val subjectName: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Hex value represented string
        /// </summary>
        [<Erase>]
        member val serialNumber: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Start date of the certificate being valid in seconds
        /// </summary>
        [<Erase>]
        member val validStart: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// End date of the certificate being valid in seconds
        /// </summary>
        [<Erase>]
        member val validExpiry: float = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Fingerprint of the certificate
        /// </summary>
        [<Erase>]
        member val fingerprint: string = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type CertificatePrincipal
        /// <param name="commonName">Common Name.</param>
        /// <param name="organizations">Organization names.</param>
        /// <param name="organizationUnits">Organization Unit names.</param>
        /// <param name="locality">Locality.</param>
        /// <param name="state">State or province.</param>
        /// <param name="country">Country or region.</param>
        (
            commonName: string,
            organizations: string[],
            organizationUnits: string[],
            locality: string,
            state: string,
            country: string
        ) =
        class
        end

        /// <summary>
        /// Common Name.
        /// </summary>
        [<Erase>]
        member val commonName: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Organization names.
        /// </summary>
        [<Erase>]
        member val organizations: string[] = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Organization Unit names.
        /// </summary>
        [<Erase>]
        member val organizationUnits: string[] = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Locality.
        /// </summary>
        [<Erase>]
        member val locality: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// State or province.
        /// </summary>
        [<Erase>]
        member val state: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Country or region.
        /// </summary>
        [<Erase>]
        member val country: string = Unchecked.defaultof<_> with get, set

    [<JS.Pojo>]
    type BrowserWindowConstructorOptions
        /// <param name="webPreferences">Settings of web page's features.</param>
        /// <param name="paintWhenInitiallyHidden">Whether the renderer should be active when <c>show</c> is <c>false</c> and it has just been created.  In order
        /// for <c>document.visibilityState</c> to work correctly on first load with <c>show: false</c> you should set this to <c>false</c>.  Setting this
        /// to <c>false</c> will cause the <c>ready-to-show</c> event to not fire.  Default is <c>true</c>.</param>
        /// <param name="width">Window's width in pixels. Default is <c>800</c>.</param>
        /// <param name="height">Window's height in pixels. Default is <c>600</c>.</param>
        /// <param name="x">(**required** if y is used) Window's left offset from screen. Default is to center the window.</param>
        /// <param name="y">(**required** if x is used) Window's top offset from screen. Default is to center the window.</param>
        /// <param name="useContentSize">The <c>width</c> and <c>height</c> would be used as web page's size, which means the actual window's size will include
        /// window frame's size and be slightly larger. Default is <c>false</c>.</param>
        /// <param name="center">Show window in the center of the screen. Default is <c>false</c>.</param>
        /// <param name="minWidth">Window's minimum width. Default is <c>0</c>.</param>
        /// <param name="minHeight">Window's minimum height. Default is <c>0</c>.</param>
        /// <param name="maxWidth">Window's maximum width. Default is no limit.</param>
        /// <param name="maxHeight">Window's maximum height. Default is no limit.</param>
        /// <param name="resizable">Whether window is resizable. Default is <c>true</c>.</param>
        /// <param name="movable">⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌ || Whether window is movable.
        /// This is not implemented on Linux. Default is <c>true</c>.</param>
        /// <param name="minimizable">⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌ || Whether window is minimizable.
        /// This is not implemented on Linux. Default is <c>true</c>.</param>
        /// <param name="maximizable">⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌ || Whether window is maximizable.
        /// This is not implemented on Linux. Default is <c>true</c>.</param>
        /// <param name="closable">⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌ || Whether window is closable.
        /// This is not implemented on Linux. Default is <c>true</c>.</param>
        /// <param name="focusable">Whether the window can be focused. Default is <c>true</c>. On Windows setting <c>focusable: false</c> also implies setting <c>skipTaskbar: true</c>.
        /// On Linux setting <c>focusable: false</c> makes the window stop interacting with wm, so the window will always stay on top
        /// in all workspaces.</param>
        /// <param name="alwaysOnTop">Whether the window should always stay on top of other windows. Default is <c>false</c>.</param>
        /// <param name="fullscreen">Whether the window should show in fullscreen. When explicitly set to <c>false</c> the fullscreen button will be hidden or
        /// disabled on macOS. Default is <c>false</c>.</param>
        /// <param name="fullscreenable">Whether the window can be put into fullscreen mode. On macOS, also whether the maximize/zoom button should toggle full
        /// screen mode or maximize window. Default is <c>true</c>.</param>
        /// <param name="simpleFullscreen">⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌ || Use pre-Lion fullscreen on
        /// macOS. Default is <c>false</c>.</param>
        /// <param name="skipTaskbar">⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌ || Whether to show the
        /// window in taskbar. Default is <c>false</c>.</param>
        /// <param name="hiddenInMissionControl">⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌ || Whether window should be
        /// hidden when the user toggles into mission control.</param>
        /// <param name="kiosk">Whether the window is in kiosk mode. Default is <c>false</c>.</param>
        /// <param name="title">Default window title. Default is <c>"Electron"</c>. If the HTML tag <c>&lt;title&gt;</c> is defined in the HTML file loaded by
        /// <c>loadURL()</c>, this property will be ignored.</param>
        /// <param name="icon">The window icon. On Windows it is recommended to use <c>ICO</c> icons to get best visual effects, you can
        /// also leave it undefined so the executable's icon will be used.</param>
        /// <param name="show">Whether window should be shown when created. Default is <c>true</c>.</param>
        /// <param name="frame">Specify <c>false</c> to create a frameless window. Default is <c>true</c>.</param>
        /// <param name="parent">Specify parent window. Default is <c>null</c>.</param>
        /// <param name="modal">Whether this is a modal window. This only works when the window is a child window. Default is <c>false</c>.</param>
        /// <param name="acceptFirstMouse">⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌ || Whether clicking an inactive
        /// window will also click through to the web contents. Default is <c>false</c> on macOS. This option is not configurable on
        /// other platforms.</param>
        /// <param name="disableAutoHideCursor">Whether to hide cursor when typing. Default is <c>false</c>.</param>
        /// <param name="autoHideMenuBar">⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ✔ | MAS ❌ || Auto hide the menu
        /// bar unless the <c>Alt</c> key is pressed. Default is <c>false</c>.</param>
        /// <param name="enableLargerThanScreen">⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌ || Enable the window to
        /// be resized larger than screen. Only relevant for macOS, as other OSes allow larger-than-screen windows by default. Default is <c>false</c>.</param>
        /// <param name="backgroundColor">The window's background color in Hex, RGB, RGBA, HSL, HSLA or named CSS color format. Alpha in #AARRGGBB format
        /// is supported if <c>transparent</c> is set to <c>true</c>. Default is <c>#FFF</c> (white). See win.setBackgroundColor for more information.</param>
        /// <param name="hasShadow">Whether window should have a shadow. Default is <c>true</c>.</param>
        /// <param name="opacity">⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌ || Set the initial opacity
        /// of the window, between 0.0 (fully transparent) and 1.0 (fully opaque). This is only implemented on Windows and macOS.</param>
        /// <param name="darkTheme">Forces using dark theme for the window, only works on some GTK+3 desktop environments. Default is <c>false</c>.</param>
        /// <param name="transparent">Makes the window transparent. Default is <c>false</c>. On Windows, does not work unless the window is frameless.</param>
        /// <param name="type">The type of window, default is normal window. See more about this below.</param>
        /// <param name="visualEffectState">⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌ || Specify how the material
        /// appearance should reflect window activity state on macOS. Must be used with the <c>vibrancy</c> property. Possible values are:</param>
        /// <param name="titleBarStyle">The style of window title bar. Default is <c>default</c>. Possible values are:</param>
        /// <param name="titleBarOverlay"> When using a frameless window in conjunction with <c>win.setWindowButtonVisibility(true)</c> on macOS or using a <c>titleBarStyle</c> so that the
        /// standard window controls ("traffic lights" on macOS) are visible, this property enables the Window Controls Overlay JavaScript APIs and CSS
        /// Environment Variables. Specifying <c>true</c> will result in an overlay with default system colors. Default is <c>false</c>.</param>
        /// <param name="accentColor">⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌ || The accent color for
        /// the window. By default, follows user preference in System Settings. Set to <c>false</c> to explicitly disable, or set the color
        /// in Hex, RGB, RGBA, HSL, HSLA or named CSS color format. Alpha values will be ignored.</param>
        /// <param name="trafficLightPosition">⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌ || Set a custom position
        /// for the traffic light buttons in frameless windows.</param>
        /// <param name="roundedCorners">⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌ || Whether frameless window should
        /// have rounded corners. Default is <c>true</c>. Setting this property to <c>false</c> will prevent the window from being fullscreenable on macOS.
        /// On Windows versions older than Windows 11 Build 22000 this property has no effect, and frameless windows will not have
        /// rounded corners.</param>
        /// <param name="thickFrame">⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌ || Use <c>WS_THICKFRAME</c> style for
        /// frameless windows on Windows, which adds the standard window frame. Setting it to <c>false</c> will remove window shadow and window
        /// animations, and disable window resizing via dragging the window edges. Default is <c>true</c>.</param>
        /// <param name="vibrancy">⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌ || Add a type of
        /// vibrancy effect to the window, only on macOS. Can be <c>appearance-based</c>, <c>titlebar</c>, <c>selection</c>, <c>menu</c>, <c>popover</c>, <c>sidebar</c>, <c>header</c>, <c>sheet</c>, <c>window</c>, <c>hud</c>,
        /// <c>fullscreen-ui</c>, <c>tooltip</c>, <c>content</c>, <c>under-window</c>, or <c>under-page</c>.</param>
        /// <param name="backgroundMaterial">⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌ || Set the window's system-drawn
        /// background material, including behind the non-client area. Can be <c>auto</c>, <c>none</c>, <c>mica</c>, <c>acrylic</c> or <c>tabbed</c>. See win.setBackgroundMaterial for more information.</param>
        /// <param name="zoomToPageWidth">⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌ || Controls the behavior on
        /// macOS when option-clicking the green stoplight button on the toolbar or by clicking the Window &gt; Zoom menu item. If
        /// <c>true</c>, the window will grow to the preferred width of the web page when zoomed, <c>false</c> will cause it to
        /// zoom to the width of the screen. This will also affect the behavior when calling <c>maximize()</c> directly. Default is <c>false</c>.</param>
        /// <param name="tabbingIdentifier">⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌ || Tab group name, allows
        /// opening the window as a native tab. Windows with the same tabbing identifier will be grouped together. This also adds
        /// a native new tab button to your window's tab bar and allows your <c>app</c> and window to receive the <c>new-window-for-tab</c>
        /// event.</param>
        (
            ?webPreferences: WebPreferences,
            ?paintWhenInitiallyHidden: bool,
            ?width: int,
            ?height: int,
            ?x: int,
            ?y: int,
            ?useContentSize: bool,
            ?center: bool,
            ?minWidth: int,
            ?minHeight: int,
            ?maxWidth: int,
            ?maxHeight: int,
            ?resizable: bool
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
            ,
            ?movable: bool
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
            ,
            ?minimizable: bool
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
            ,
            ?maximizable: bool
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
            ,
            ?closable: bool
            #endif
            ,
            ?focusable: bool,
            ?alwaysOnTop: bool,
            ?fullscreen: bool,
            ?fullscreenable: bool
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
            ,
            ?simpleFullscreen: bool
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
            ,
            ?skipTaskbar: bool
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
            ,
            ?hiddenInMissionControl: bool
            #endif
            ,
            ?kiosk: bool,
            ?title: string,
            ?icon: U2<NativeImage, string>,
            ?show: bool,
            ?frame: bool,
            ?parent: BaseWindow,
            ?modal: bool
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
            ,
            ?acceptFirstMouse: bool
            #endif
            ,
            ?disableAutoHideCursor: bool
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_LIN || ELECTRON_OS_WIN
            ,
            ?autoHideMenuBar: bool
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
            ,
            ?enableLargerThanScreen: bool
            #endif
            ,
            ?backgroundColor: string,
            ?hasShadow: bool
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
            ,
            ?opacity: float
            #endif
            ,
            ?darkTheme: bool,
            ?transparent: bool,
            ?``type``: string
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
            ,
            ?visualEffectState: Enums.Types.BrowserWindowConstructorOptions.VisualEffectState
            #endif
            ,
            ?titleBarStyle: Enums.Types.BrowserWindowConstructorOptions.TitleBarStyle,
            ?titleBarOverlay: U2<Types.BrowserWindowConstructorOptions.TitleBarOverlay, bool>
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
            ,
            ?accentColor: U2<bool, string>
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
            ,
            ?trafficLightPosition: Point
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
            ,
            ?roundedCorners: bool
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
            ,
            ?thickFrame: bool
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
            ,
            ?vibrancy: Enums.Types.BrowserWindowConstructorOptions.Vibrancy
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
            ,
            ?backgroundMaterial: Enums.Types.BrowserWindowConstructorOptions.BackgroundMaterial
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
            ,
            ?zoomToPageWidth: bool
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
            ,
            ?tabbingIdentifier: string
            #endif

        ) =
        class
        end

        /// <summary>
        /// Settings of web page's features.
        /// </summary>
        [<Erase>]
        member val webPreferences: WebPreferences = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether the renderer should be active when <c>show</c> is <c>false</c> and it has just been created.  In order for
        /// <c>document.visibilityState</c> to work correctly on first load with <c>show: false</c> you should set this to <c>false</c>.  Setting this to
        /// <c>false</c> will cause the <c>ready-to-show</c> event to not fire.  Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val paintWhenInitiallyHidden: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Window's width in pixels. Default is <c>800</c>.
        /// </summary>
        [<Erase>]
        member val width: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Window's height in pixels. Default is <c>600</c>.
        /// </summary>
        [<Erase>]
        member val height: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// (**required** if y is used) Window's left offset from screen. Default is to center the window.
        /// </summary>
        [<Erase>]
        member val x: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// (**required** if x is used) Window's top offset from screen. Default is to center the window.
        /// </summary>
        [<Erase>]
        member val y: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The <c>width</c> and <c>height</c> would be used as web page's size, which means the actual window's size will include window
        /// frame's size and be slightly larger. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val useContentSize: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Show window in the center of the screen. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val center: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Window's minimum width. Default is <c>0</c>.
        /// </summary>
        [<Erase>]
        member val minWidth: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Window's minimum height. Default is <c>0</c>.
        /// </summary>
        [<Erase>]
        member val minHeight: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Window's maximum width. Default is no limit.
        /// </summary>
        [<Erase>]
        member val maxWidth: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Window's maximum height. Default is no limit.
        /// </summary>
        [<Erase>]
        member val maxHeight: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether window is resizable. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val resizable: bool = Unchecked.defaultof<_> with get, set
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Whether window is movable. This is not implemented on Linux. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val movable: bool = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Whether window is minimizable. This is not implemented on Linux. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val minimizable: bool = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Whether window is maximizable. This is not implemented on Linux. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val maximizable: bool = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Whether window is closable. This is not implemented on Linux. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val closable: bool = Unchecked.defaultof<_> with get, set
        #endif


        /// <summary>
        /// Whether the window can be focused. Default is <c>true</c>. On Windows setting <c>focusable: false</c> also implies setting <c>skipTaskbar: true</c>. On
        /// Linux setting <c>focusable: false</c> makes the window stop interacting with wm, so the window will always stay on top in
        /// all workspaces.
        /// </summary>
        [<Erase>]
        member val focusable: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether the window should always stay on top of other windows. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val alwaysOnTop: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether the window should show in fullscreen. When explicitly set to <c>false</c> the fullscreen button will be hidden or disabled
        /// on macOS. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val fullscreen: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether the window can be put into fullscreen mode. On macOS, also whether the maximize/zoom button should toggle full screen
        /// mode or maximize window. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val fullscreenable: bool = Unchecked.defaultof<_> with get, set
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Use pre-Lion fullscreen on macOS. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val simpleFullscreen: bool = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Whether to show the window in taskbar. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val skipTaskbar: bool = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Whether window should be hidden when the user toggles into mission control.
        /// </summary>
        [<Erase>]
        member val hiddenInMissionControl: bool = Unchecked.defaultof<_> with get, set
        #endif


        /// <summary>
        /// Whether the window is in kiosk mode. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val kiosk: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Default window title. Default is <c>"Electron"</c>. If the HTML tag <c>&lt;title&gt;</c> is defined in the HTML file loaded by <c>loadURL()</c>,
        /// this property will be ignored.
        /// </summary>
        [<Erase>]
        member val title: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The window icon. On Windows it is recommended to use <c>ICO</c> icons to get best visual effects, you can also
        /// leave it undefined so the executable's icon will be used.
        /// </summary>
        [<Erase>]
        member val icon: U2<NativeImage, string> = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether window should be shown when created. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val show: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Specify <c>false</c> to create a frameless window. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val frame: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Specify parent window. Default is <c>null</c>.
        /// </summary>
        [<Erase>]
        member val parent: BaseWindow = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether this is a modal window. This only works when the window is a child window. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val modal: bool = Unchecked.defaultof<_> with get, set
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Whether clicking an inactive window will also click through to the web contents. Default is <c>false</c> on macOS. This option
        /// is not configurable on other platforms.
        /// </summary>
        [<Erase>]
        member val acceptFirstMouse: bool = Unchecked.defaultof<_> with get, set
        #endif


        /// <summary>
        /// Whether to hide cursor when typing. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val disableAutoHideCursor: bool = Unchecked.defaultof<_> with get, set
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_LIN || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ✔ | MAS ❌</para>
        /// Auto hide the menu bar unless the <c>Alt</c> key is pressed. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val autoHideMenuBar: bool = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Enable the window to be resized larger than screen. Only relevant for macOS, as other OSes allow larger-than-screen windows by
        /// default. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val enableLargerThanScreen: bool = Unchecked.defaultof<_> with get, set
        #endif


        /// <summary>
        /// The window's background color in Hex, RGB, RGBA, HSL, HSLA or named CSS color format. Alpha in #AARRGGBB format is
        /// supported if <c>transparent</c> is set to <c>true</c>. Default is <c>#FFF</c> (white). See win.setBackgroundColor for more information.
        /// </summary>
        [<Erase>]
        member val backgroundColor: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether window should have a shadow. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val hasShadow: bool = Unchecked.defaultof<_> with get, set
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Set the initial opacity of the window, between 0.0 (fully transparent) and 1.0 (fully opaque). This is only implemented on
        /// Windows and macOS.
        /// </summary>
        [<Erase>]
        member val opacity: float = Unchecked.defaultof<_> with get, set
        #endif


        /// <summary>
        /// Forces using dark theme for the window, only works on some GTK+3 desktop environments. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val darkTheme: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Makes the window transparent. Default is <c>false</c>. On Windows, does not work unless the window is frameless.
        /// </summary>
        [<Erase>]
        member val transparent: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The type of window, default is normal window. See more about this below.
        /// </summary>
        [<Erase>]
        member val ``type``: string = Unchecked.defaultof<_> with get, set
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Specify how the material appearance should reflect window activity state on macOS. Must be used with the <c>vibrancy</c> property. Possible
        /// values are:
        /// </summary>
        [<Erase>]
        member val visualEffectState: Enums.Types.BrowserWindowConstructorOptions.VisualEffectState =
            Unchecked.defaultof<_> with get, set
        #endif


        /// <summary>
        /// The style of window title bar. Default is <c>default</c>. Possible values are:
        /// </summary>
        [<Erase>]
        member val titleBarStyle: Enums.Types.BrowserWindowConstructorOptions.TitleBarStyle =
            Unchecked.defaultof<_> with get, set

        /// <summary>
        ///  When using a frameless window in conjunction with <c>win.setWindowButtonVisibility(true)</c> on macOS or using a <c>titleBarStyle</c> so that the standard
        /// window controls ("traffic lights" on macOS) are visible, this property enables the Window Controls Overlay JavaScript APIs and CSS Environment
        /// Variables. Specifying <c>true</c> will result in an overlay with default system colors. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val titleBarOverlay: U2<Types.BrowserWindowConstructorOptions.TitleBarOverlay, bool> =
            Unchecked.defaultof<_> with get, set
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌</para>
        /// The accent color for the window. By default, follows user preference in System Settings. Set to <c>false</c> to explicitly disable,
        /// or set the color in Hex, RGB, RGBA, HSL, HSLA or named CSS color format. Alpha values will be ignored.
        /// </summary>
        [<Erase>]
        member val accentColor: U2<bool, string> = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Set a custom position for the traffic light buttons in frameless windows.
        /// </summary>
        [<Erase>]
        member val trafficLightPosition: Point = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Whether frameless window should have rounded corners. Default is <c>true</c>. Setting this property to <c>false</c> will prevent the window from
        /// being fullscreenable on macOS. On Windows versions older than Windows 11 Build 22000 this property has no effect, and frameless
        /// windows will not have rounded corners.
        /// </summary>
        [<Erase>]
        member val roundedCorners: bool = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌</para>
        /// Use <c>WS_THICKFRAME</c> style for frameless windows on Windows, which adds the standard window frame. Setting it to <c>false</c> will remove
        /// window shadow and window animations, and disable window resizing via dragging the window edges. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val thickFrame: bool = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Add a type of vibrancy effect to the window, only on macOS. Can be <c>appearance-based</c>, <c>titlebar</c>, <c>selection</c>, <c>menu</c>, <c>popover</c>, <c>sidebar</c>,
        /// <c>header</c>, <c>sheet</c>, <c>window</c>, <c>hud</c>, <c>fullscreen-ui</c>, <c>tooltip</c>, <c>content</c>, <c>under-window</c>, or <c>under-page</c>.
        /// </summary>
        [<Erase>]
        member val vibrancy: Enums.Types.BrowserWindowConstructorOptions.Vibrancy = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌</para>
        /// Set the window's system-drawn background material, including behind the non-client area. Can be <c>auto</c>, <c>none</c>, <c>mica</c>, <c>acrylic</c> or <c>tabbed</c>. See
        /// win.setBackgroundMaterial for more information.
        /// </summary>
        [<Erase>]
        member val backgroundMaterial: Enums.Types.BrowserWindowConstructorOptions.BackgroundMaterial =
            Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Controls the behavior on macOS when option-clicking the green stoplight button on the toolbar or by clicking the Window &gt;
        /// Zoom menu item. If <c>true</c>, the window will grow to the preferred width of the web page when zoomed, <c>false</c>
        /// will cause it to zoom to the width of the screen. This will also affect the behavior when calling <c>maximize()</c>
        /// directly. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val zoomToPageWidth: bool = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Tab group name, allows opening the window as a native tab. Windows with the same tabbing identifier will be grouped
        /// together. This also adds a native new tab button to your window's tab bar and allows your <c>app</c> and window
        /// to receive the <c>new-window-for-tab</c> event.
        /// </summary>
        [<Erase>]
        member val tabbingIdentifier: string = Unchecked.defaultof<_> with get, set
        #endif


    [<JS.Pojo>]
    type BluetoothDevice
        /// <param name="deviceName"></param>
        /// <param name="deviceId"></param>
        (deviceName: string, deviceId: string) =
        class
        end

        [<Erase>]
        member val deviceName: string = Unchecked.defaultof<_> with get, set

        [<Erase>]
        member val deviceId: string = Unchecked.defaultof<_> with get, set

    /// <summary>
    /// When setting minimum or maximum window size with <c>minWidth</c>/<c>maxWidth</c>/ <c>minHeight</c>/<c>maxHeight</c>, it only constrains the users. It won't prevent you from
    /// passing a size that does not follow size constraints to <c>setBounds</c>/<c>setSize</c> or to the constructor of <c>BrowserWindow</c>.<br/><br/>The possible values and
    /// behaviors of the <c>type</c> option are platform dependent. Possible values are:<br/><br/>* On Linux, possible types are <c>desktop</c>, <c>dock</c>, <c>toolbar</c>, <c>splash</c>,
    /// <c>notification</c>.<br/>  * The <c>desktop</c> type places the window at the desktop background window level (kCGDesktopWindowLevel - 1). However, note
    /// that a desktop window will not receive focus, keyboard, or mouse events. You can still use globalShortcut to receive input
    /// sparingly.<br/>  * The <c>dock</c> type creates a dock-like window behavior.<br/>  * The <c>toolbar</c> type creates a window with
    /// a toolbar appearance.<br/>  * The <c>splash</c> type behaves in a specific way. It is not draggable, even if the
    /// CSS styling of the window's body contains -webkit-app-region: drag. This type is commonly used for splash screens.<br/>  * The
    /// <c>notification</c> type creates a window that behaves like a system notification.<br/>* On macOS, possible types are <c>desktop</c>, <c>textured</c>, <c>panel</c>.<br/>
    /// * The <c>textured</c> type adds metal gradient appearance. This option is **deprecated**.<br/>  * The <c>desktop</c> type places the window
    /// at the desktop background window level (<c>kCGDesktopWindowLevel - 1</c>). Note that desktop window will not receive focus, keyboard or mouse
    /// events, but you can use <c>globalShortcut</c> to receive input sparingly.<br/>  * The <c>panel</c> type enables the window to float
    /// on top of full-screened apps by adding the <c>NSWindowStyleMaskNonactivatingPanel</c> style mask, normally reserved for NSPanel, at runtime. Also, the window
    /// will appear on all spaces (desktops).<br/>* On Windows, possible type is <c>toolbar</c>.
    /// </summary>
    [<JS.Pojo>]
    type BaseWindowConstructorOptions
        /// <param name="width">Window's width in pixels. Default is <c>800</c>.</param>
        /// <param name="height">Window's height in pixels. Default is <c>600</c>.</param>
        /// <param name="x">(**required** if y is used) Window's left offset from screen. Default is to center the window.</param>
        /// <param name="y">(**required** if x is used) Window's top offset from screen. Default is to center the window.</param>
        /// <param name="useContentSize">The <c>width</c> and <c>height</c> would be used as web page's size, which means the actual window's size will include
        /// window frame's size and be slightly larger. Default is <c>false</c>.</param>
        /// <param name="center">Show window in the center of the screen. Default is <c>false</c>.</param>
        /// <param name="minWidth">Window's minimum width. Default is <c>0</c>.</param>
        /// <param name="minHeight">Window's minimum height. Default is <c>0</c>.</param>
        /// <param name="maxWidth">Window's maximum width. Default is no limit.</param>
        /// <param name="maxHeight">Window's maximum height. Default is no limit.</param>
        /// <param name="resizable">Whether window is resizable. Default is <c>true</c>.</param>
        /// <param name="movable">⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌ || Whether window is movable.
        /// This is not implemented on Linux. Default is <c>true</c>.</param>
        /// <param name="minimizable">⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌ || Whether window is minimizable.
        /// This is not implemented on Linux. Default is <c>true</c>.</param>
        /// <param name="maximizable">⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌ || Whether window is maximizable.
        /// This is not implemented on Linux. Default is <c>true</c>.</param>
        /// <param name="closable">⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌ || Whether window is closable.
        /// This is not implemented on Linux. Default is <c>true</c>.</param>
        /// <param name="focusable">Whether the window can be focused. Default is <c>true</c>. On Windows setting <c>focusable: false</c> also implies setting <c>skipTaskbar: true</c>.
        /// On Linux setting <c>focusable: false</c> makes the window stop interacting with wm, so the window will always stay on top
        /// in all workspaces.</param>
        /// <param name="alwaysOnTop">Whether the window should always stay on top of other windows. Default is <c>false</c>.</param>
        /// <param name="fullscreen">Whether the window should show in fullscreen. When explicitly set to <c>false</c> the fullscreen button will be hidden or
        /// disabled on macOS. Default is <c>false</c>.</param>
        /// <param name="fullscreenable">Whether the window can be put into fullscreen mode. On macOS, also whether the maximize/zoom button should toggle full
        /// screen mode or maximize window. Default is <c>true</c>.</param>
        /// <param name="simpleFullscreen">⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌ || Use pre-Lion fullscreen on
        /// macOS. Default is <c>false</c>.</param>
        /// <param name="skipTaskbar">⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌ || Whether to show the
        /// window in taskbar. Default is <c>false</c>.</param>
        /// <param name="hiddenInMissionControl">⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌ || Whether window should be
        /// hidden when the user toggles into mission control.</param>
        /// <param name="kiosk">Whether the window is in kiosk mode. Default is <c>false</c>.</param>
        /// <param name="title">Default window title. Default is <c>"Electron"</c>. If the HTML tag <c>&lt;title&gt;</c> is defined in the HTML file loaded by
        /// <c>loadURL()</c>, this property will be ignored.</param>
        /// <param name="icon">The window icon. On Windows it is recommended to use <c>ICO</c> icons to get best visual effects, you can
        /// also leave it undefined so the executable's icon will be used.</param>
        /// <param name="show">Whether window should be shown when created. Default is <c>true</c>.</param>
        /// <param name="frame">Specify <c>false</c> to create a frameless window. Default is <c>true</c>.</param>
        /// <param name="parent">Specify parent window. Default is <c>null</c>.</param>
        /// <param name="modal">Whether this is a modal window. This only works when the window is a child window. Default is <c>false</c>.</param>
        /// <param name="acceptFirstMouse">⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌ || Whether clicking an inactive
        /// window will also click through to the web contents. Default is <c>false</c> on macOS. This option is not configurable on
        /// other platforms.</param>
        /// <param name="disableAutoHideCursor">Whether to hide cursor when typing. Default is <c>false</c>.</param>
        /// <param name="autoHideMenuBar">⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ✔ | MAS ❌ || Auto hide the menu
        /// bar unless the <c>Alt</c> key is pressed. Default is <c>false</c>.</param>
        /// <param name="enableLargerThanScreen">⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌ || Enable the window to
        /// be resized larger than screen. Only relevant for macOS, as other OSes allow larger-than-screen windows by default. Default is <c>false</c>.</param>
        /// <param name="backgroundColor">The window's background color in Hex, RGB, RGBA, HSL, HSLA or named CSS color format. Alpha in #AARRGGBB format
        /// is supported if <c>transparent</c> is set to <c>true</c>. Default is <c>#FFF</c> (white). See win.setBackgroundColor for more information.</param>
        /// <param name="hasShadow">Whether window should have a shadow. Default is <c>true</c>.</param>
        /// <param name="opacity">⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌ || Set the initial opacity
        /// of the window, between 0.0 (fully transparent) and 1.0 (fully opaque). This is only implemented on Windows and macOS.</param>
        /// <param name="darkTheme">Forces using dark theme for the window, only works on some GTK+3 desktop environments. Default is <c>false</c>.</param>
        /// <param name="transparent">Makes the window transparent. Default is <c>false</c>. On Windows, does not work unless the window is frameless.</param>
        /// <param name="type">The type of window, default is normal window. See more about this below.</param>
        /// <param name="visualEffectState">⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌ || Specify how the material
        /// appearance should reflect window activity state on macOS. Must be used with the <c>vibrancy</c> property. Possible values are:</param>
        /// <param name="titleBarStyle">The style of window title bar. Default is <c>default</c>. Possible values are:</param>
        /// <param name="titleBarOverlay"> When using a frameless window in conjunction with <c>win.setWindowButtonVisibility(true)</c> on macOS or using a <c>titleBarStyle</c> so that the
        /// standard window controls ("traffic lights" on macOS) are visible, this property enables the Window Controls Overlay JavaScript APIs and CSS
        /// Environment Variables. Specifying <c>true</c> will result in an overlay with default system colors. Default is <c>false</c>.</param>
        /// <param name="accentColor">⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌ || The accent color for
        /// the window. By default, follows user preference in System Settings. Set to <c>false</c> to explicitly disable, or set the color
        /// in Hex, RGB, RGBA, HSL, HSLA or named CSS color format. Alpha values will be ignored.</param>
        /// <param name="trafficLightPosition">⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌ || Set a custom position
        /// for the traffic light buttons in frameless windows.</param>
        /// <param name="roundedCorners">⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌ || Whether frameless window should
        /// have rounded corners. Default is <c>true</c>. Setting this property to <c>false</c> will prevent the window from being fullscreenable on macOS.
        /// On Windows versions older than Windows 11 Build 22000 this property has no effect, and frameless windows will not have
        /// rounded corners.</param>
        /// <param name="thickFrame">⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌ || Use <c>WS_THICKFRAME</c> style for
        /// frameless windows on Windows, which adds the standard window frame. Setting it to <c>false</c> will remove window shadow and window
        /// animations, and disable window resizing via dragging the window edges. Default is <c>true</c>.</param>
        /// <param name="vibrancy">⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌ || Add a type of
        /// vibrancy effect to the window, only on macOS. Can be <c>appearance-based</c>, <c>titlebar</c>, <c>selection</c>, <c>menu</c>, <c>popover</c>, <c>sidebar</c>, <c>header</c>, <c>sheet</c>, <c>window</c>, <c>hud</c>,
        /// <c>fullscreen-ui</c>, <c>tooltip</c>, <c>content</c>, <c>under-window</c>, or <c>under-page</c>.</param>
        /// <param name="backgroundMaterial">⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌ || Set the window's system-drawn
        /// background material, including behind the non-client area. Can be <c>auto</c>, <c>none</c>, <c>mica</c>, <c>acrylic</c> or <c>tabbed</c>. See win.setBackgroundMaterial for more information.</param>
        /// <param name="zoomToPageWidth">⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌ || Controls the behavior on
        /// macOS when option-clicking the green stoplight button on the toolbar or by clicking the Window &gt; Zoom menu item. If
        /// <c>true</c>, the window will grow to the preferred width of the web page when zoomed, <c>false</c> will cause it to
        /// zoom to the width of the screen. This will also affect the behavior when calling <c>maximize()</c> directly. Default is <c>false</c>.</param>
        /// <param name="tabbingIdentifier">⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌ || Tab group name, allows
        /// opening the window as a native tab. Windows with the same tabbing identifier will be grouped together. This also adds
        /// a native new tab button to your window's tab bar and allows your <c>app</c> and window to receive the <c>new-window-for-tab</c>
        /// event.</param>
        (
            ?width: int,
            ?height: int,
            ?x: int,
            ?y: int,
            ?useContentSize: bool,
            ?center: bool,
            ?minWidth: int,
            ?minHeight: int,
            ?maxWidth: int,
            ?maxHeight: int,
            ?resizable: bool
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
            ,
            ?movable: bool
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
            ,
            ?minimizable: bool
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
            ,
            ?maximizable: bool
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
            ,
            ?closable: bool
            #endif
            ,
            ?focusable: bool,
            ?alwaysOnTop: bool,
            ?fullscreen: bool,
            ?fullscreenable: bool
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
            ,
            ?simpleFullscreen: bool
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
            ,
            ?skipTaskbar: bool
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
            ,
            ?hiddenInMissionControl: bool
            #endif
            ,
            ?kiosk: bool,
            ?title: string,
            ?icon: U2<NativeImage, string>,
            ?show: bool,
            ?frame: bool,
            ?parent: BaseWindow,
            ?modal: bool
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
            ,
            ?acceptFirstMouse: bool
            #endif
            ,
            ?disableAutoHideCursor: bool
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_LIN || ELECTRON_OS_WIN
            ,
            ?autoHideMenuBar: bool
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
            ,
            ?enableLargerThanScreen: bool
            #endif
            ,
            ?backgroundColor: string,
            ?hasShadow: bool
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
            ,
            ?opacity: float
            #endif
            ,
            ?darkTheme: bool,
            ?transparent: bool,
            ?``type``: string
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
            ,
            ?visualEffectState: Enums.Types.BaseWindowConstructorOptions.VisualEffectState
            #endif
            ,
            ?titleBarStyle: Enums.Types.BaseWindowConstructorOptions.TitleBarStyle,
            ?titleBarOverlay: U2<Types.BaseWindowConstructorOptions.TitleBarOverlay, bool>
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
            ,
            ?accentColor: U2<bool, string>
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
            ,
            ?trafficLightPosition: Point
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
            ,
            ?roundedCorners: bool
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
            ,
            ?thickFrame: bool
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
            ,
            ?vibrancy: Enums.Types.BaseWindowConstructorOptions.Vibrancy
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
            ,
            ?backgroundMaterial: Enums.Types.BaseWindowConstructorOptions.BackgroundMaterial
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
            ,
            ?zoomToPageWidth: bool
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
            ,
            ?tabbingIdentifier: string
            #endif

        ) =
        class
        end

        /// <summary>
        /// Window's width in pixels. Default is <c>800</c>.
        /// </summary>
        [<Erase>]
        member val width: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Window's height in pixels. Default is <c>600</c>.
        /// </summary>
        [<Erase>]
        member val height: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// (**required** if y is used) Window's left offset from screen. Default is to center the window.
        /// </summary>
        [<Erase>]
        member val x: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// (**required** if x is used) Window's top offset from screen. Default is to center the window.
        /// </summary>
        [<Erase>]
        member val y: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The <c>width</c> and <c>height</c> would be used as web page's size, which means the actual window's size will include window
        /// frame's size and be slightly larger. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val useContentSize: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Show window in the center of the screen. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val center: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Window's minimum width. Default is <c>0</c>.
        /// </summary>
        [<Erase>]
        member val minWidth: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Window's minimum height. Default is <c>0</c>.
        /// </summary>
        [<Erase>]
        member val minHeight: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Window's maximum width. Default is no limit.
        /// </summary>
        [<Erase>]
        member val maxWidth: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Window's maximum height. Default is no limit.
        /// </summary>
        [<Erase>]
        member val maxHeight: int = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether window is resizable. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val resizable: bool = Unchecked.defaultof<_> with get, set
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Whether window is movable. This is not implemented on Linux. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val movable: bool = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Whether window is minimizable. This is not implemented on Linux. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val minimizable: bool = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Whether window is maximizable. This is not implemented on Linux. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val maximizable: bool = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Whether window is closable. This is not implemented on Linux. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val closable: bool = Unchecked.defaultof<_> with get, set
        #endif


        /// <summary>
        /// Whether the window can be focused. Default is <c>true</c>. On Windows setting <c>focusable: false</c> also implies setting <c>skipTaskbar: true</c>. On
        /// Linux setting <c>focusable: false</c> makes the window stop interacting with wm, so the window will always stay on top in
        /// all workspaces.
        /// </summary>
        [<Erase>]
        member val focusable: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether the window should always stay on top of other windows. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val alwaysOnTop: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether the window should show in fullscreen. When explicitly set to <c>false</c> the fullscreen button will be hidden or disabled
        /// on macOS. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val fullscreen: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether the window can be put into fullscreen mode. On macOS, also whether the maximize/zoom button should toggle full screen
        /// mode or maximize window. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val fullscreenable: bool = Unchecked.defaultof<_> with get, set
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Use pre-Lion fullscreen on macOS. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val simpleFullscreen: bool = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Whether to show the window in taskbar. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val skipTaskbar: bool = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Whether window should be hidden when the user toggles into mission control.
        /// </summary>
        [<Erase>]
        member val hiddenInMissionControl: bool = Unchecked.defaultof<_> with get, set
        #endif


        /// <summary>
        /// Whether the window is in kiosk mode. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val kiosk: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Default window title. Default is <c>"Electron"</c>. If the HTML tag <c>&lt;title&gt;</c> is defined in the HTML file loaded by <c>loadURL()</c>,
        /// this property will be ignored.
        /// </summary>
        [<Erase>]
        member val title: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The window icon. On Windows it is recommended to use <c>ICO</c> icons to get best visual effects, you can also
        /// leave it undefined so the executable's icon will be used.
        /// </summary>
        [<Erase>]
        member val icon: U2<NativeImage, string> = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether window should be shown when created. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val show: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Specify <c>false</c> to create a frameless window. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val frame: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Specify parent window. Default is <c>null</c>.
        /// </summary>
        [<Erase>]
        member val parent: BaseWindow = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether this is a modal window. This only works when the window is a child window. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val modal: bool = Unchecked.defaultof<_> with get, set
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Whether clicking an inactive window will also click through to the web contents. Default is <c>false</c> on macOS. This option
        /// is not configurable on other platforms.
        /// </summary>
        [<Erase>]
        member val acceptFirstMouse: bool = Unchecked.defaultof<_> with get, set
        #endif


        /// <summary>
        /// Whether to hide cursor when typing. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val disableAutoHideCursor: bool = Unchecked.defaultof<_> with get, set
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_LIN || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ✔ | MAS ❌</para>
        /// Auto hide the menu bar unless the <c>Alt</c> key is pressed. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val autoHideMenuBar: bool = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Enable the window to be resized larger than screen. Only relevant for macOS, as other OSes allow larger-than-screen windows by
        /// default. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val enableLargerThanScreen: bool = Unchecked.defaultof<_> with get, set
        #endif


        /// <summary>
        /// The window's background color in Hex, RGB, RGBA, HSL, HSLA or named CSS color format. Alpha in #AARRGGBB format is
        /// supported if <c>transparent</c> is set to <c>true</c>. Default is <c>#FFF</c> (white). See win.setBackgroundColor for more information.
        /// </summary>
        [<Erase>]
        member val backgroundColor: string = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Whether window should have a shadow. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val hasShadow: bool = Unchecked.defaultof<_> with get, set
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Set the initial opacity of the window, between 0.0 (fully transparent) and 1.0 (fully opaque). This is only implemented on
        /// Windows and macOS.
        /// </summary>
        [<Erase>]
        member val opacity: float = Unchecked.defaultof<_> with get, set
        #endif


        /// <summary>
        /// Forces using dark theme for the window, only works on some GTK+3 desktop environments. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val darkTheme: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// Makes the window transparent. Default is <c>false</c>. On Windows, does not work unless the window is frameless.
        /// </summary>
        [<Erase>]
        member val transparent: bool = Unchecked.defaultof<_> with get, set

        /// <summary>
        /// The type of window, default is normal window. See more about this below.
        /// </summary>
        [<Erase>]
        member val ``type``: string = Unchecked.defaultof<_> with get, set
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Specify how the material appearance should reflect window activity state on macOS. Must be used with the <c>vibrancy</c> property. Possible
        /// values are:
        /// </summary>
        [<Erase>]
        member val visualEffectState: Enums.Types.BaseWindowConstructorOptions.VisualEffectState =
            Unchecked.defaultof<_> with get, set
        #endif


        /// <summary>
        /// The style of window title bar. Default is <c>default</c>. Possible values are:
        /// </summary>
        [<Erase>]
        member val titleBarStyle: Enums.Types.BaseWindowConstructorOptions.TitleBarStyle =
            Unchecked.defaultof<_> with get, set

        /// <summary>
        ///  When using a frameless window in conjunction with <c>win.setWindowButtonVisibility(true)</c> on macOS or using a <c>titleBarStyle</c> so that the standard
        /// window controls ("traffic lights" on macOS) are visible, this property enables the Window Controls Overlay JavaScript APIs and CSS Environment
        /// Variables. Specifying <c>true</c> will result in an overlay with default system colors. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val titleBarOverlay: U2<Types.BaseWindowConstructorOptions.TitleBarOverlay, bool> =
            Unchecked.defaultof<_> with get, set
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌</para>
        /// The accent color for the window. By default, follows user preference in System Settings. Set to <c>false</c> to explicitly disable,
        /// or set the color in Hex, RGB, RGBA, HSL, HSLA or named CSS color format. Alpha values will be ignored.
        /// </summary>
        [<Erase>]
        member val accentColor: U2<bool, string> = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Set a custom position for the traffic light buttons in frameless windows.
        /// </summary>
        [<Erase>]
        member val trafficLightPosition: Point = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Whether frameless window should have rounded corners. Default is <c>true</c>. Setting this property to <c>false</c> will prevent the window from
        /// being fullscreenable on macOS. On Windows versions older than Windows 11 Build 22000 this property has no effect, and frameless
        /// windows will not have rounded corners.
        /// </summary>
        [<Erase>]
        member val roundedCorners: bool = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌</para>
        /// Use <c>WS_THICKFRAME</c> style for frameless windows on Windows, which adds the standard window frame. Setting it to <c>false</c> will remove
        /// window shadow and window animations, and disable window resizing via dragging the window edges. Default is <c>true</c>.
        /// </summary>
        [<Erase>]
        member val thickFrame: bool = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Add a type of vibrancy effect to the window, only on macOS. Can be <c>appearance-based</c>, <c>titlebar</c>, <c>selection</c>, <c>menu</c>, <c>popover</c>, <c>sidebar</c>,
        /// <c>header</c>, <c>sheet</c>, <c>window</c>, <c>hud</c>, <c>fullscreen-ui</c>, <c>tooltip</c>, <c>content</c>, <c>under-window</c>, or <c>under-page</c>.
        /// </summary>
        [<Erase>]
        member val vibrancy: Enums.Types.BaseWindowConstructorOptions.Vibrancy = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌</para>
        /// Set the window's system-drawn background material, including behind the non-client area. Can be <c>auto</c>, <c>none</c>, <c>mica</c>, <c>acrylic</c> or <c>tabbed</c>. See
        /// win.setBackgroundMaterial for more information.
        /// </summary>
        [<Erase>]
        member val backgroundMaterial: Enums.Types.BaseWindowConstructorOptions.BackgroundMaterial =
            Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Controls the behavior on macOS when option-clicking the green stoplight button on the toolbar or by clicking the Window &gt;
        /// Zoom menu item. If <c>true</c>, the window will grow to the preferred width of the web page when zoomed, <c>false</c>
        /// will cause it to zoom to the width of the screen. This will also affect the behavior when calling <c>maximize()</c>
        /// directly. Default is <c>false</c>.
        /// </summary>
        [<Erase>]
        member val zoomToPageWidth: bool = Unchecked.defaultof<_> with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Tab group name, allows opening the window as a native tab. Windows with the same tabbing identifier will be grouped
        /// together. This also adds a native new tab button to your window's tab bar and allows your <c>app</c> and window
        /// to receive the <c>new-window-for-tab</c> event.
        /// </summary>
        [<Erase>]
        member val tabbingIdentifier: string = Unchecked.defaultof<_> with get, set
        #endif


[<Fable.Core.Erase; AutoOpen>]
module Enums =
    begin end

module Renderer =
    module UtilityProcess =
        /// <summary>
        /// Emitted when the child process needs to terminate due to non continuable error from V8.<br/><br/>No matter if you listen to
        /// the <c>error</c> event, the <c>exit</c> event will be emitted after the child process terminates.
        /// </summary>
        [<Experimental("Indicated to be Experimental by Electron");
          System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnError =
            /// <summary>
            /// Type of error. One of the following values:
            /// </summary>
            [<Emit("$0[0]")>]
            abstract member ``type``: Renderer.Enums.UtilityProcess.Error.Type with get, set

            /// <summary>
            /// Source location from where the error originated.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member location: string with get, set

            /// <summary>
            /// <c>Node.js diagnostic report</c>.
            /// </summary>
            [<Emit("$0[2]")>]
            abstract member report: string with get, set

    module WebviewTag =
        /// <summary>
        /// Emitted when 'Search' is selected for text in its context menu.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDevtoolsSearchQuery =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// text to query for.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member query: string with get, set

        /// <summary>
        /// Fired when the guest page has sent an asynchronous message to embedder page.<br/><br/>With <c>sendToHost</c> method and <c>ipc-message</c> event you can
        /// communicate between guest page and embedder page:
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnIpcMessage =
            /// <summary>
            /// pair of <c>[processId, frameId]</c>.
            /// </summary>
            [<Emit("$0[0]")>]
            abstract member frameId: float * float with get, set

            [<Emit("$0[1]")>]
            abstract member channel: string with get, set

            [<Emit("$0[2]")>]
            abstract member args: obj[] with get, set

        /// <summary>
        /// Emitted when an in-page navigation happened.<br/><br/>When in-page navigation happens, the page URL changes but does not cause navigation outside of
        /// the page. Examples of this occurring are when anchor links are clicked or when the DOM <c>hashchange</c> event is triggered.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDidNavigateInPage =
            [<Emit("$0[0]")>]
            abstract member isMainFrame: bool with get, set

            [<Emit("$0[1]")>]
            abstract member url: string with get, set

        /// <summary>
        /// Emitted when any frame navigation is done.<br/><br/>This event is not emitted for in-page navigations, such as clicking anchor links or
        /// updating the <c>window.location.hash</c>. Use <c>did-navigate-in-page</c> event for this purpose.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDidFrameNavigate =
            [<Emit("$0[0]")>]
            abstract member url: string with get, set

            /// <summary>
            /// -1 for non HTTP navigations
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member httpResponseCode: int with get, set

            /// <summary>
            /// empty for non HTTP navigations,
            /// </summary>
            [<Emit("$0[2]")>]
            abstract member httpStatusText: string with get, set

            [<Emit("$0[3]")>]
            abstract member isMainFrame: bool with get, set

            [<Emit("$0[4]")>]
            abstract member frameProcessId: int with get, set

            [<Emit("$0[5]")>]
            abstract member frameRoutingId: int with get, set

        /// <summary>
        /// Emitted after a server side redirect occurs during navigation. For example a 302 redirect.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDidRedirectNavigation =
            [<Emit("$0[0]")>]
            abstract member url: string with get, set

            [<Emit("$0[1]")>]
            abstract member isInPlace: bool with get, set

            [<Emit("$0[2]")>]
            abstract member isMainFrame: bool with get, set

            [<Emit("$0[3]")>]
            abstract member frameProcessId: int with get, set

            [<Emit("$0[4]")>]
            abstract member frameRoutingId: int with get, set

        /// <summary>
        /// Emitted when any frame (including main) starts navigating. <c>isInPlace</c> will be <c>true</c> for in-page navigations.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDidStartNavigation =
            [<Emit("$0[0]")>]
            abstract member url: string with get, set

            [<Emit("$0[1]")>]
            abstract member isInPlace: bool with get, set

            [<Emit("$0[2]")>]
            abstract member isMainFrame: bool with get, set

            [<Emit("$0[3]")>]
            abstract member frameProcessId: int with get, set

            [<Emit("$0[4]")>]
            abstract member frameRoutingId: int with get, set

        /// <summary>
        /// Emitted when a user or the page wants to start navigation anywhere in the <c>&lt;webview&gt;</c> or any frames embedded within.
        /// It can happen when the <c>window.location</c> object is changed or a user clicks a link in the page.<br/><br/>This event will
        /// not emit when the navigation is started programmatically with APIs like <c>&lt;webview&gt;.loadURL</c> and <c>&lt;webview&gt;.back</c>.<br/><br/>It is also not emitted during in-page
        /// navigation, such as clicking anchor links or updating the <c>window.location.hash</c>. Use <c>did-navigate-in-page</c> event for this purpose.<br/><br/>Calling <c>event.preventDefault()</c> does **NOT** have
        /// any effect.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnWillFrameNavigate =
            [<Emit("$0[0]")>]
            abstract member url: string with get, set

            [<Emit("$0[1]")>]
            abstract member isMainFrame: bool with get, set

            [<Emit("$0[2]")>]
            abstract member frameProcessId: int with get, set

            [<Emit("$0[3]")>]
            abstract member frameRoutingId: int with get, set

        /// <summary>
        /// Fired when the guest window logs a console message.<br/><br/>The following example code forwards all log messages to the embedder's console
        /// without regard for log level or other properties.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnConsoleMessage =
            /// <summary>
            /// The log level, from 0 to 3. In order it matches <c>verbose</c>, <c>info</c>, <c>warning</c> and <c>error</c>.
            /// </summary>
            [<Emit("$0[0]")>]
            abstract member level: int with get, set

            /// <summary>
            /// The actual console message
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member message: string with get, set

            /// <summary>
            /// The line number of the source that triggered this console message
            /// </summary>
            [<Emit("$0[2]")>]
            abstract member line: int with get, set

            [<Emit("$0[3]")>]
            abstract member sourceId: string with get, set

        /// <summary>
        /// Fired when page title is set during navigation. <c>explicitSet</c> is false when title is synthesized from file url.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnPageTitleUpdated =
            [<Emit("$0[0]")>]
            abstract member title: string with get, set

            [<Emit("$0[1]")>]
            abstract member explicitSet: bool with get, set

        /// <summary>
        /// This event is like <c>did-finish-load</c>, but fired when the load failed or was cancelled, e.g. <c>window.stop()</c> is invoked.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDidFailLoad =
            [<Emit("$0[0]")>]
            abstract member errorCode: int with get, set

            [<Emit("$0[1]")>]
            abstract member errorDescription: string with get, set

            [<Emit("$0[2]")>]
            abstract member validatedURL: string with get, set

            [<Emit("$0[3]")>]
            abstract member isMainFrame: bool with get, set

        /// <summary>
        /// Fired when a load has committed. This includes navigation within the current document as well as subframe document-level loads, but
        /// does not include asynchronous resource loads.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnLoadCommit =
            [<Emit("$0[0]")>]
            abstract member url: string with get, set

            [<Emit("$0[1]")>]
            abstract member isMainFrame: bool with get, set

module Utility =
    module WebviewTag =
        /// <summary>
        /// Emitted when 'Search' is selected for text in its context menu.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDevtoolsSearchQuery =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// text to query for.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member query: string with get, set

        /// <summary>
        /// Fired when the guest page has sent an asynchronous message to embedder page.<br/><br/>With <c>sendToHost</c> method and <c>ipc-message</c> event you can
        /// communicate between guest page and embedder page:
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnIpcMessage =
            /// <summary>
            /// pair of <c>[processId, frameId]</c>.
            /// </summary>
            [<Emit("$0[0]")>]
            abstract member frameId: float * float with get, set

            [<Emit("$0[1]")>]
            abstract member channel: string with get, set

            [<Emit("$0[2]")>]
            abstract member args: obj[] with get, set

        /// <summary>
        /// Emitted when an in-page navigation happened.<br/><br/>When in-page navigation happens, the page URL changes but does not cause navigation outside of
        /// the page. Examples of this occurring are when anchor links are clicked or when the DOM <c>hashchange</c> event is triggered.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDidNavigateInPage =
            [<Emit("$0[0]")>]
            abstract member isMainFrame: bool with get, set

            [<Emit("$0[1]")>]
            abstract member url: string with get, set

        /// <summary>
        /// Emitted when any frame navigation is done.<br/><br/>This event is not emitted for in-page navigations, such as clicking anchor links or
        /// updating the <c>window.location.hash</c>. Use <c>did-navigate-in-page</c> event for this purpose.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDidFrameNavigate =
            [<Emit("$0[0]")>]
            abstract member url: string with get, set

            /// <summary>
            /// -1 for non HTTP navigations
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member httpResponseCode: int with get, set

            /// <summary>
            /// empty for non HTTP navigations,
            /// </summary>
            [<Emit("$0[2]")>]
            abstract member httpStatusText: string with get, set

            [<Emit("$0[3]")>]
            abstract member isMainFrame: bool with get, set

            [<Emit("$0[4]")>]
            abstract member frameProcessId: int with get, set

            [<Emit("$0[5]")>]
            abstract member frameRoutingId: int with get, set

        /// <summary>
        /// Emitted after a server side redirect occurs during navigation. For example a 302 redirect.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDidRedirectNavigation =
            [<Emit("$0[0]")>]
            abstract member url: string with get, set

            [<Emit("$0[1]")>]
            abstract member isInPlace: bool with get, set

            [<Emit("$0[2]")>]
            abstract member isMainFrame: bool with get, set

            [<Emit("$0[3]")>]
            abstract member frameProcessId: int with get, set

            [<Emit("$0[4]")>]
            abstract member frameRoutingId: int with get, set

        /// <summary>
        /// Emitted when any frame (including main) starts navigating. <c>isInPlace</c> will be <c>true</c> for in-page navigations.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDidStartNavigation =
            [<Emit("$0[0]")>]
            abstract member url: string with get, set

            [<Emit("$0[1]")>]
            abstract member isInPlace: bool with get, set

            [<Emit("$0[2]")>]
            abstract member isMainFrame: bool with get, set

            [<Emit("$0[3]")>]
            abstract member frameProcessId: int with get, set

            [<Emit("$0[4]")>]
            abstract member frameRoutingId: int with get, set

        /// <summary>
        /// Emitted when a user or the page wants to start navigation anywhere in the <c>&lt;webview&gt;</c> or any frames embedded within.
        /// It can happen when the <c>window.location</c> object is changed or a user clicks a link in the page.<br/><br/>This event will
        /// not emit when the navigation is started programmatically with APIs like <c>&lt;webview&gt;.loadURL</c> and <c>&lt;webview&gt;.back</c>.<br/><br/>It is also not emitted during in-page
        /// navigation, such as clicking anchor links or updating the <c>window.location.hash</c>. Use <c>did-navigate-in-page</c> event for this purpose.<br/><br/>Calling <c>event.preventDefault()</c> does **NOT** have
        /// any effect.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnWillFrameNavigate =
            [<Emit("$0[0]")>]
            abstract member url: string with get, set

            [<Emit("$0[1]")>]
            abstract member isMainFrame: bool with get, set

            [<Emit("$0[2]")>]
            abstract member frameProcessId: int with get, set

            [<Emit("$0[3]")>]
            abstract member frameRoutingId: int with get, set

        /// <summary>
        /// Fired when the guest window logs a console message.<br/><br/>The following example code forwards all log messages to the embedder's console
        /// without regard for log level or other properties.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnConsoleMessage =
            /// <summary>
            /// The log level, from 0 to 3. In order it matches <c>verbose</c>, <c>info</c>, <c>warning</c> and <c>error</c>.
            /// </summary>
            [<Emit("$0[0]")>]
            abstract member level: int with get, set

            /// <summary>
            /// The actual console message
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member message: string with get, set

            /// <summary>
            /// The line number of the source that triggered this console message
            /// </summary>
            [<Emit("$0[2]")>]
            abstract member line: int with get, set

            [<Emit("$0[3]")>]
            abstract member sourceId: string with get, set

        /// <summary>
        /// Fired when page title is set during navigation. <c>explicitSet</c> is false when title is synthesized from file url.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnPageTitleUpdated =
            [<Emit("$0[0]")>]
            abstract member title: string with get, set

            [<Emit("$0[1]")>]
            abstract member explicitSet: bool with get, set

        /// <summary>
        /// This event is like <c>did-finish-load</c>, but fired when the load failed or was cancelled, e.g. <c>window.stop()</c> is invoked.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDidFailLoad =
            [<Emit("$0[0]")>]
            abstract member errorCode: int with get, set

            [<Emit("$0[1]")>]
            abstract member errorDescription: string with get, set

            [<Emit("$0[2]")>]
            abstract member validatedURL: string with get, set

            [<Emit("$0[3]")>]
            abstract member isMainFrame: bool with get, set

        /// <summary>
        /// Fired when a load has committed. This includes navigation within the current document as well as subframe document-level loads, but
        /// does not include asynchronous resource loads.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnLoadCommit =
            [<Emit("$0[0]")>]
            abstract member url: string with get, set

            [<Emit("$0[1]")>]
            abstract member isMainFrame: bool with get, set

    module ClientRequest =
        /// <summary>
        /// Emitted when the server returns a redirect response (e.g. 301 Moved Permanently). Calling <c>request.followRedirect</c> will continue with the redirection.
        /// If this event is handled, <c>request.followRedirect</c> must be called **synchronously**, otherwise the request will be cancelled.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnRedirect =
            [<Emit("$0[0]")>]
            abstract member statusCode: int with get, set

            [<Emit("$0[1]")>]
            abstract member method: string with get, set

            [<Emit("$0[2]")>]
            abstract member redirectUrl: string with get, set

            [<Emit("$0[3]")>]
            abstract member responseHeaders: Record<string, string[]> with get, set

        /// <summary>
        /// Emitted when an authenticating proxy is asking for user credentials.<br/><br/>The <c>callback</c> function is expected to be called back with user
        /// credentials:<br/><br/>* <c>username</c> string<br/>* <c>password</c> string<br/><br/>Providing empty credentials will cancel the request and report an authentication error on the response object:
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnLogin =
            [<Emit("$0[0]")>]
            abstract member authInfo: Utility.ClientRequest.Login.AuthInfo with get, set

            [<Emit("$0[1]")>]
            abstract member callback: Utility.ClientRequest.Login.Callback with get, set

    module SystemPreferences =
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_LIN || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ✔ | MAS ❌</para>
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnAccentColorChanged =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// The new RGBA color the user assigned to be their system accent color.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member newColor: string with get, set
        #endif


    module UtilityProcess =
        /// <summary>
        /// Emitted when the child process needs to terminate due to non continuable error from V8.<br/><br/>No matter if you listen to
        /// the <c>error</c> event, the <c>exit</c> event will be emitted after the child process terminates.
        /// </summary>
        [<Experimental("Indicated to be Experimental by Electron");
          System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnError =
            /// <summary>
            /// Type of error. One of the following values:
            /// </summary>
            [<Emit("$0[0]")>]
            abstract member ``type``: Utility.Enums.UtilityProcess.Error.Type with get, set

            /// <summary>
            /// Source location from where the error originated.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member location: string with get, set

            /// <summary>
            /// <c>Node.js diagnostic report</c>.
            /// </summary>
            [<Emit("$0[2]")>]
            abstract member report: string with get, set

module Main =
    [<Erase>]
    type TouchBarItem =
        | Button of TouchBarButton
        | ColorPicker of TouchBarColorPicker
        | Group of TouchBarGroup
        | Label of TouchBarLabel
        | Popover of TouchBarPopover
        | Scrubber of TouchBarScrubber
        | SegmentedControl of TouchBarSegmentedControl
        | Slider of TouchBarSlider
        | Spacer of TouchBarSpacer

    module App =
        /// <summary>
        /// This event will be emitted inside the primary instance of your application when a second instance has been executed and
        /// calls <c>app.requestSingleInstanceLock()</c>.<br/><br/><c>argv</c> is an Array of the second instance's command line arguments, and <c>workingDirectory</c> is its current working directory. Usually
        /// applications respond to this by making their primary window focused and non-minimized.<br/><br/>&gt; [!NOTE] <c>argv</c> will not be exactly the same
        /// list of arguments as those passed to the second instance. The order might change and additional arguments might be appended.
        /// If you need to maintain the exact same arguments, it's advised to use <c>additionalData</c> instead.<br/><br/>&gt; [!NOTE] If the second instance
        /// is started by a different user than the first, the <c>argv</c> array will not include the arguments.<br/><br/>This event is guaranteed
        /// to be emitted after the <c>ready</c> event of <c>app</c> gets emitted.<br/><br/>&gt; [!NOTE] Extra command line arguments might be added by
        /// Chromium, such as <c>--original-process-start-time</c>.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnSecondInstance =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// An array of the second instance's command line arguments
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member argv: string[] with get, set

            /// <summary>
            /// The second instance's working directory
            /// </summary>
            [<Emit("$0[2]")>]
            abstract member workingDirectory: string with get, set

            /// <summary>
            /// A JSON object of additional data passed from the second instance
            /// </summary>
            [<Emit("$0[3]")>]
            abstract member additionalData: obj with get, set
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted when Chrome's accessibility support changes. This event fires when assistive technologies, such as screen readers, are enabled or disabled.
        /// See https://www.chromium.org/developers/design-documents/accessibility for more details.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnAccessibilitySupportChanged =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// <c>true</c> when Chrome's accessibility support is enabled, <c>false</c> otherwise.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member accessibilitySupportEnabled: bool with get, set
        #endif


        /// <summary>
        /// Emitted when the child process unexpectedly disappears. This is normally because it was crashed or killed. It does not include
        /// renderer processes.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnChildProcessGone =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member details: Main.App.ChildProcessGone.Details with get, set

        /// <summary>
        /// Emitted when the renderer process unexpectedly disappears.  This is normally because it was crashed or killed.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnRenderProcessGone =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member webContents: WebContents with get, set

            [<Emit("$0[2]")>]
            abstract member details: RenderProcessGoneDetails with get, set

        /// <summary>
        /// Emitted when <c>webContents</c> or Utility process wants to do basic auth.<br/><br/>The default behavior is to cancel all authentications. To override
        /// this you should prevent the default behavior with <c>event.preventDefault()</c> and call <c>callback(username, password)</c> with the credentials.<br/><br/>If <c>callback</c> is called without
        /// a username or password, the authentication request will be cancelled and the authentication error will be returned to the page.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnLogin =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member webContents: WebContents with get, set

            [<Emit("$0[2]")>]
            abstract member authenticationResponseDetails: Main.App.Login.AuthenticationResponseDetails with get, set

            [<Emit("$0[3]")>]
            abstract member authInfo: Main.App.Login.AuthInfo with get, set

            [<Emit("$0[4]")>]
            abstract member callback: Main.App.Login.Callback with get, set

        /// <summary>
        /// Emitted when a client certificate is requested.<br/><br/>The <c>url</c> corresponds to the navigation entry requesting the client certificate and <c>callback</c> can
        /// be called with an entry filtered from the list. Using <c>event.preventDefault()</c> prevents the application from using the first certificate from
        /// the store.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnSelectClientCertificate =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member webContents: WebContents with get, set

            [<Emit("$0[2]")>]
            abstract member url: URL with get, set

            [<Emit("$0[3]")>]
            abstract member certificateList: Certificate[] with get, set

            [<Emit("$0[4]")>]
            abstract member callback: Option<Certificate> -> unit with get, set

        /// <summary>
        /// Emitted when failed to verify the <c>certificate</c> for <c>url</c>, to trust the certificate you should prevent the default behavior with
        /// <c>event.preventDefault()</c> and call <c>callback(true)</c>.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnCertificateError =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member webContents: WebContents with get, set

            [<Emit("$0[2]")>]
            abstract member url: string with get, set

            /// <summary>
            /// The error code
            /// </summary>
            [<Emit("$0[3]")>]
            abstract member error: string with get, set

            [<Emit("$0[4]")>]
            abstract member certificate: Certificate with get, set

            [<Emit("$0[5]")>]
            abstract member callback: bool -> unit with get, set

            [<Emit("$0[6]")>]
            abstract member isMainFrame: bool with get, set

        /// <summary>
        /// Emitted when a new webContents is created.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnWebContentsCreated =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member webContents: WebContents with get, set

        /// <summary>
        /// Emitted when a new browserWindow is created.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnBrowserWindowCreated =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member window: BrowserWindow with get, set

        /// <summary>
        /// Emitted when a browserWindow gets focused.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnBrowserWindowFocus =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member window: BrowserWindow with get, set

        /// <summary>
        /// Emitted when a browserWindow gets blurred.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnBrowserWindowBlur =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member window: BrowserWindow with get, set
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted when Handoff is about to be resumed on another device. If you need to update the state to be
        /// transferred, you should call <c>event.preventDefault()</c> immediately, construct a new <c>userInfo</c> dictionary and call <c>app.updateCurrentActivity()</c> in a timely manner. Otherwise, the
        /// operation will fail and <c>continue-activity-error</c> will be called.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnUpdateActivityState =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// A string identifying the activity. Maps to <c>NSUserActivity.activityType</c>.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member ``type``: string with get, set

            /// <summary>
            /// Contains app-specific state stored by the activity.
            /// </summary>
            [<Emit("$0[2]")>]
            abstract member userInfo: obj with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted during Handoff after an activity from this device was successfully resumed on another one.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnActivityWasContinued =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// A string identifying the activity. Maps to <c>NSUserActivity.activityType</c>.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member ``type``: string with get, set

            /// <summary>
            /// Contains app-specific state stored by the activity.
            /// </summary>
            [<Emit("$0[2]")>]
            abstract member userInfo: obj with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted during Handoff when an activity from a different device fails to be resumed.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnContinueActivityError =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// A string identifying the activity. Maps to <c>NSUserActivity.activityType</c>.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member ``type``: string with get, set

            /// <summary>
            /// A string with the error's localized description.
            /// </summary>
            [<Emit("$0[2]")>]
            abstract member error: string with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted during Handoff before an activity from a different device wants to be resumed. You should call <c>event.preventDefault()</c> if you
        /// want to handle this event.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnWillContinueActivity =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// A string identifying the activity. Maps to <c>NSUserActivity.activityType</c>.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member ``type``: string with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted during Handoff when an activity from a different device wants to be resumed. You should call <c>event.preventDefault()</c> if you
        /// want to handle this event.<br/><br/>A user activity can be continued only in an app that has the same developer Team
        /// ID as the activity's source app and that supports the activity's type. Supported activity types are specified in the app's
        /// <c>Info.plist</c> under the <c>NSUserActivityTypes</c> key.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnContinueActivity =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// A string identifying the activity. Maps to <c>NSUserActivity.activityType</c>.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member ``type``: string with get, set

            /// <summary>
            /// Contains app-specific state stored by the activity on another device.
            /// </summary>
            [<Emit("$0[2]")>]
            abstract member userInfo: obj with get, set

            [<Emit("$0[3]")>]
            abstract member details: Main.App.ContinueActivity.Details with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted when the application is activated. Various actions can trigger this event, such as launching the application for the first
        /// time, attempting to re-launch the application when it's already running, or clicking on the application's dock or taskbar icon.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnActivate =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member hasVisibleWindows: bool with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted when the user wants to open a URL with the application. Your application's <c>Info.plist</c> file must define the URL
        /// scheme within the <c>CFBundleURLTypes</c> key, and set <c>NSPrincipalClass</c> to <c>AtomApplication</c>.<br/><br/>As with the <c>open-file</c> event, be sure to register a listener
        /// for the <c>open-url</c> event early in your application startup to detect if the application is being opened to handle a
        /// URL. If you register the listener in response to a <c>ready</c> event, you'll miss URLs that trigger the launch of
        /// your application.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnOpenUrl =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member url: string with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted when the user wants to open a file with the application. The <c>open-file</c> event is usually emitted when the
        /// application is already open and the OS wants to reuse the application to open the file. <c>open-file</c> is also emitted
        /// when a file is dropped onto the dock and the application is not yet running. Make sure to listen for
        /// the <c>open-file</c> event very early in your application startup to handle this case (even before the <c>ready</c> event is emitted).<br/><br/>You
        /// should call <c>event.preventDefault()</c> if you want to handle this event.<br/><br/>On Windows, you have to parse <c>process.argv</c> (in the main process)
        /// to get the filepath.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnOpenFile =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member path: string with get, set
        #endif


        /// <summary>
        /// Emitted when the application is quitting.<br/><br/>&gt; [!NOTE] On Windows, this event will not be emitted if the app is closed
        /// due to a shutdown/restart of the system or a user logout.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnQuit =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member exitCode: int with get, set

        /// <summary>
        /// Emitted once, when Electron has finished initializing. On macOS, <c>launchInfo</c> holds the <c>userInfo</c> of the <c>NSUserNotification</c> or information from <c>UNNotificationResponse</c>
        /// that was used to open the application, if it was launched from Notification Center. You can also call <c>app.isReady()</c> to
        /// check if this event has already fired and <c>app.whenReady()</c> to get a Promise that is fulfilled when Electron is initialized.<br/><br/>&gt;
        /// [!NOTE] The <c>ready</c> event is only fired after the main process has finished running the first tick of the event
        /// loop. If an Electron API needs to be called before the <c>ready</c> event, ensure that it is called synchronously in
        /// the top-level context of the main process.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnReady =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member launchInfo: U2<Record<string, obj>, NotificationResponse> with get, set

    module AutoUpdater =
        /// <summary>
        /// Emitted when an update has been downloaded.<br/><br/>On Windows only <c>releaseName</c> is available.<br/><br/>&gt; [!NOTE] It is not strictly necessary to handle
        /// this event. A successfully downloaded update will still be applied the next time the application starts.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnUpdateDownloaded =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member releaseNotes: string with get, set

            [<Emit("$0[2]")>]
            abstract member releaseName: string with get, set

            [<Emit("$0[3]")>]
            abstract member releaseDate: System.DateTime with get, set

            [<Emit("$0[4]")>]
            abstract member updateURL: string with get, set

    module BaseWindow =
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_LIN || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ✔ | MAS ❌</para>
        /// Emitted when the system context menu is triggered on the window, this is normally only triggered when the user right
        /// clicks on the non-client area of your window.  This is the window titlebar or any area you have declared
        /// as <c>-webkit-app-region: drag</c> in a frameless window.<br/><br/>Calling <c>event.preventDefault()</c> will prevent the menu from being displayed.<br/><br/>To convert <c>point</c> to DIP, use
        /// <c>screen.screenToDipPoint(point)</c>.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnSystemContextMenu =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// The screen coordinates where the context menu was triggered.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member point: Point with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted on trackpad rotation gesture. Continually emitted until rotation gesture is ended. The <c>rotation</c> value on each emission is the
        /// angle in degrees rotated since the last emission. The last emitted event upon a rotation gesture will always be of
        /// value <c>0</c>. Counter-clockwise rotation values are positive, while clockwise ones are negative.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnRotateGesture =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member rotation: float with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted on 3-finger swipe. Possible directions are <c>up</c>, <c>right</c>, <c>down</c>, <c>left</c>.<br/><br/>The method underlying this event is built to handle older
        /// macOS-style trackpad swiping, where the content on the screen doesn't move with the swipe. Most macOS trackpads are not configured
        /// to allow this kind of swiping anymore, so in order for it to emit properly the 'Swipe between pages' preference
        /// in <c>System Preferences &gt; Trackpad &gt; More Gestures</c> must be set to 'Swipe with two or three fingers'.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnSwipe =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member direction: string with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_LIN || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ✔ | MAS ❌</para>
        /// Emitted when an App Command is invoked. These are typically related to keyboard media keys or browser commands, as well
        /// as the "Back" button built into some mice on Windows.<br/><br/>Commands are lowercased, underscores are replaced with hyphens, and the <c>APPCOMMAND_</c>
        /// prefix is stripped off. e.g. <c>APPCOMMAND_BROWSER_BACKWARD</c> is emitted as <c>browser-backward</c>.<br/><br/>The following app commands are explicitly supported on Linux:<br/><br/>* <c>browser-backward</c><br/>* <c>browser-forward</c>
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnAppCommand =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member command: string with get, set
        #endif


        /// <summary>
        /// Emitted when the window is set or unset to show always on top of other windows.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnAlwaysOnTopChanged =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member isAlwaysOnTop: bool with get, set
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted before the window is moved. On Windows, calling <c>event.preventDefault()</c> will prevent the window from being moved.<br/><br/>Note that this is
        /// only emitted when the window is being moved manually. Moving the window with <c>setPosition</c>/<c>setBounds</c>/<c>center</c> will not emit this event.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnWillMove =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// Location the window is being moved to.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member newBounds: Rectangle with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted before the window is resized. Calling <c>event.preventDefault()</c> will prevent the window from being resized.<br/><br/>Note that this is only emitted
        /// when the window is being resized manually. Resizing the window with <c>setBounds</c>/<c>setSize</c> will not emit this event.<br/><br/>The possible values and
        /// behaviors of the <c>edge</c> option are platform dependent. Possible values are:<br/><br/>* On Windows, possible values are <c>bottom</c>, <c>top</c>, <c>left</c>, <c>right</c>,
        /// <c>top-left</c>, <c>top-right</c>, <c>bottom-left</c>, <c>bottom-right</c>.<br/>* On macOS, possible values are <c>bottom</c> and <c>right</c>.<br/>  * The value <c>bottom</c> is used to
        /// denote vertical resizing.<br/>  * The value <c>right</c> is used to denote horizontal resizing.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnWillResize =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// Size the window is being resized to.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member newBounds: Rectangle with get, set

            [<Emit("$0[2]")>]
            abstract member details: Main.BaseWindow.WillResize.Details with get, set
        #endif


    module BrowserWindow =
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_LIN || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ✔ | MAS ❌</para>
        /// Emitted when the system context menu is triggered on the window, this is normally only triggered when the user right
        /// clicks on the non-client area of your window.  This is the window titlebar or any area you have declared
        /// as <c>-webkit-app-region: drag</c> in a frameless window.<br/><br/>Calling <c>event.preventDefault()</c> will prevent the menu from being displayed.<br/><br/>To convert <c>point</c> to DIP, use
        /// <c>screen.screenToDipPoint(point)</c>.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnSystemContextMenu =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// The screen coordinates where the context menu was triggered.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member point: Point with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted on trackpad rotation gesture. Continually emitted until rotation gesture is ended. The <c>rotation</c> value on each emission is the
        /// angle in degrees rotated since the last emission. The last emitted event upon a rotation gesture will always be of
        /// value <c>0</c>. Counter-clockwise rotation values are positive, while clockwise ones are negative.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnRotateGesture =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member rotation: float with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted on 3-finger swipe. Possible directions are <c>up</c>, <c>right</c>, <c>down</c>, <c>left</c>.<br/><br/>The method underlying this event is built to handle older
        /// macOS-style trackpad swiping, where the content on the screen doesn't move with the swipe. Most macOS trackpads are not configured
        /// to allow this kind of swiping anymore, so in order for it to emit properly the 'Swipe between pages' preference
        /// in <c>System Preferences &gt; Trackpad &gt; More Gestures</c> must be set to 'Swipe with two or three fingers'.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnSwipe =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member direction: string with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_LIN || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ✔ | MAS ❌</para>
        /// Emitted when an App Command is invoked. These are typically related to keyboard media keys or browser commands, as well
        /// as the "Back" button built into some mice on Windows.<br/><br/>Commands are lowercased, underscores are replaced with hyphens, and the <c>APPCOMMAND_</c>
        /// prefix is stripped off. e.g. <c>APPCOMMAND_BROWSER_BACKWARD</c> is emitted as <c>browser-backward</c>.<br/><br/>The following app commands are explicitly supported on Linux:<br/><br/>* <c>browser-backward</c><br/>* <c>browser-forward</c>
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnAppCommand =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member command: string with get, set
        #endif


        /// <summary>
        /// Emitted when the window is set or unset to show always on top of other windows.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnAlwaysOnTopChanged =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member isAlwaysOnTop: bool with get, set
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted before the window is moved. On Windows, calling <c>event.preventDefault()</c> will prevent the window from being moved.<br/><br/>Note that this is
        /// only emitted when the window is being moved manually. Moving the window with <c>setPosition</c>/<c>setBounds</c>/<c>center</c> will not emit this event.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnWillMove =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// Location the window is being moved to.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member newBounds: Rectangle with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted before the window is resized. Calling <c>event.preventDefault()</c> will prevent the window from being resized.<br/><br/>Note that this is only emitted
        /// when the window is being resized manually. Resizing the window with <c>setBounds</c>/<c>setSize</c> will not emit this event.<br/><br/>The possible values and
        /// behaviors of the <c>edge</c> option are platform dependent. Possible values are:<br/><br/>* On Windows, possible values are <c>bottom</c>, <c>top</c>, <c>left</c>, <c>right</c>,
        /// <c>top-left</c>, <c>top-right</c>, <c>bottom-left</c>, <c>bottom-right</c>.<br/>* On macOS, possible values are <c>bottom</c> and <c>right</c>.<br/>  * The value <c>bottom</c> is used to
        /// denote vertical resizing.<br/>  * The value <c>right</c> is used to denote horizontal resizing.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnWillResize =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// Size the window is being resized to.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member newBounds: Rectangle with get, set

            [<Emit("$0[2]")>]
            abstract member details: Main.BrowserWindow.WillResize.Details with get, set
        #endif


        /// <summary>
        /// Emitted when the document changed its title, calling <c>event.preventDefault()</c> will prevent the native window's title from changing. <c>explicitSet</c> is false
        /// when title is synthesized from file URL.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnPageTitleUpdated =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member title: string with get, set

            [<Emit("$0[2]")>]
            abstract member explicitSet: bool with get, set

    module ClientRequest =
        /// <summary>
        /// Emitted when the server returns a redirect response (e.g. 301 Moved Permanently). Calling <c>request.followRedirect</c> will continue with the redirection.
        /// If this event is handled, <c>request.followRedirect</c> must be called **synchronously**, otherwise the request will be cancelled.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnRedirect =
            [<Emit("$0[0]")>]
            abstract member statusCode: int with get, set

            [<Emit("$0[1]")>]
            abstract member method: string with get, set

            [<Emit("$0[2]")>]
            abstract member redirectUrl: string with get, set

            [<Emit("$0[3]")>]
            abstract member responseHeaders: Record<string, string[]> with get, set

        /// <summary>
        /// Emitted when an authenticating proxy is asking for user credentials.<br/><br/>The <c>callback</c> function is expected to be called back with user
        /// credentials:<br/><br/>* <c>username</c> string<br/>* <c>password</c> string<br/><br/>Providing empty credentials will cancel the request and report an authentication error on the response object:
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnLogin =
            [<Emit("$0[0]")>]
            abstract member authInfo: Main.ClientRequest.Login.AuthInfo with get, set

            [<Emit("$0[1]")>]
            abstract member callback: Main.ClientRequest.Login.Callback with get, set

    module Cookies =
        /// <summary>
        /// Emitted when a cookie is changed because it was added, edited, removed, or expired.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnChanged =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// The cookie that was changed.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member cookie: Cookie with get, set

            /// <summary>
            /// The cause of the change with one of the following values:
            /// </summary>
            [<Emit("$0[2]")>]
            abstract member cause: Main.Enums.Cookies.Changed.Cause with get, set

            /// <summary>
            /// <c>true</c> if the cookie was removed, <c>false</c> otherwise.
            /// </summary>
            [<Emit("$0[3]")>]
            abstract member removed: bool with get, set

    module Debugger =
        /// <summary>
        /// Emitted whenever the debugging target issues an instrumentation event.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnMessage =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// Method name.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member method: string with get, set

            /// <summary>
            /// Event parameters defined by the 'parameters' attribute in the remote debugging protocol.
            /// </summary>
            [<Emit("$0[2]")>]
            abstract member ``params``: obj with get, set

            /// <summary>
            /// Unique identifier of attached debugging session, will match the value sent from <c>debugger.sendCommand</c>.
            /// </summary>
            [<Emit("$0[3]")>]
            abstract member sessionId: string with get, set

        /// <summary>
        /// Emitted when the debugging session is terminated. This happens either when <c>webContents</c> is closed or devtools is invoked for the
        /// attached <c>webContents</c>.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDetach =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// Reason for detaching debugger.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member reason: string with get, set

    module DownloadItem =
        /// <summary>
        /// Emitted when the download is in a terminal state. This includes a completed download, a cancelled download (via <c>downloadItem.cancel()</c>), and
        /// interrupted download that can't be resumed.<br/><br/>The <c>state</c> can be one of following:<br/><br/>* <c>completed</c> - The download completed successfully.<br/>* <c>cancelled</c> -
        /// The download has been cancelled.<br/>* <c>interrupted</c> - The download has interrupted and can not resume.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDone =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// Can be <c>completed</c>, <c>cancelled</c> or <c>interrupted</c>.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member state: Main.Enums.DownloadItem.Done.State with get, set

        /// <summary>
        /// Emitted when the download has been updated and is not done.<br/><br/>The <c>state</c> can be one of following:<br/><br/>* <c>progressing</c> - The
        /// download is in-progress.<br/>* <c>interrupted</c> - The download has interrupted and can be resumed.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnUpdated =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// Can be <c>progressing</c> or <c>interrupted</c>.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member state: Main.Enums.DownloadItem.Updated.State with get, set

    module Extensions =
        /// <summary>
        /// Emitted after an extension is loaded and all necessary browser state is initialized to support the start of the extension's
        /// background page.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnExtensionReady =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member extension: Extension with get, set

        /// <summary>
        /// Emitted after an extension is unloaded. This occurs when <c>Session.removeExtension</c> is called.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnExtensionUnloaded =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member extension: Extension with get, set

        /// <summary>
        /// Emitted after an extension is loaded. This occurs whenever an extension is added to the "enabled" set of extensions. This
        /// includes:<br/><br/>* Extensions being loaded from <c>Extensions.loadExtension</c>.<br/>* Extensions being reloaded:<br/>  * from a crash.<br/>  * if the extension requested
        /// it (<c>chrome.runtime.reload()</c>).
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnExtensionLoaded =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member extension: Extension with get, set

    module InAppPurchase =
        /// <summary>
        /// Emitted when one or more transactions have been updated.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnTransactionsUpdated =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// Array of <c>Transaction</c> objects.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member transactions: Transaction[] with get, set

    module Notification =
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌</para>
        /// Emitted when an error is encountered while creating and showing the native notification.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnFailed =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// The error encountered during execution of the <c>show()</c> method.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member error: string with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnAction =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// The index of the action that was activated.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member index: float with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted when the user clicks the "Reply" button on a notification with <c>hasReply: true</c>.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnReply =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// The string the user entered into the inline reply field.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member reply: string with get, set
        #endif


    module PushNotifications =
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted when the app receives a remote notification while running. See: https://developer.apple.com/documentation/appkit/nsapplicationdelegate/1428430-application?language=objc
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnReceivedApnsNotification =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member userInfo: Record<string, obj> with get, set
        #endif


    module Screen =
        /// <summary>
        /// Emitted when one or more metrics change in a <c>display</c>. The <c>changedMetrics</c> is an array of strings that describe the
        /// changes. Possible changes are <c>bounds</c>, <c>workArea</c>, <c>scaleFactor</c> and <c>rotation</c>.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDisplayMetricsChanged =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member display: Display with get, set

            [<Emit("$0[2]")>]
            abstract member changedMetrics: string[] with get, set

        /// <summary>
        /// Emitted when <c>oldDisplay</c> has been removed.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDisplayRemoved =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member oldDisplay: Display with get, set

        /// <summary>
        /// Emitted when <c>newDisplay</c> has been added.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDisplayAdded =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member newDisplay: Display with get, set

    module ServiceWorkers =
        /// <summary>
        /// Emitted when a service worker has been registered. Can occur after a call to <c>navigator.serviceWorker.register('/sw.js')</c> successfully resolves or when a
        /// Chrome extension is loaded.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnRegistrationCompleted =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// Information about the registered service worker
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member details: Main.ServiceWorkers.RegistrationCompleted.Details with get, set

        /// <summary>
        /// Emitted when a service worker logs something to the console.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnConsoleMessage =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// Information about the console message
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member messageDetails: Main.ServiceWorkers.ConsoleMessage.MessageDetails with get, set

    module Session =
        /// <summary>
        /// Emitted after <c>USBDevice.forget()</c> has been called.  This event can be used to help maintain persistent storage of permissions when
        /// <c>setDevicePermissionHandler</c> is used.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnUsbDeviceRevoked =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member details: Main.Session.UsbDeviceRevoked.Details with get, set

        /// <summary>
        /// Emitted after <c>navigator.usb.requestDevice</c> has been called and <c>select-usb-device</c> has fired if a device has been removed before the callback from
        /// <c>select-usb-device</c> is called.  This event is intended for use when using a UI to ask users to pick a
        /// device so that the UI can be updated to remove the specified device.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnUsbDeviceRemoved =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member device: USBDevice with get, set

            [<Emit("$0[2]")>]
            abstract member webContents: WebContents with get, set

        /// <summary>
        /// Emitted after <c>navigator.usb.requestDevice</c> has been called and <c>select-usb-device</c> has fired if a new device becomes available before the callback from
        /// <c>select-usb-device</c> is called.  This event is intended for use when using a UI to ask users to pick a
        /// device so that the UI can be updated with the newly added device.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnUsbDeviceAdded =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member device: USBDevice with get, set

            [<Emit("$0[2]")>]
            abstract member webContents: WebContents with get, set

        /// <summary>
        /// Emitted when a USB device needs to be selected when a call to <c>navigator.usb.requestDevice</c> is made. <c>callback</c> should be called
        /// with <c>deviceId</c> to be selected; passing no arguments to <c>callback</c> will cancel the request.  Additionally, permissioning on <c>navigator.usb</c> can
        /// be further managed by using <c>ses.setPermissionCheckHandler(handler)</c> and <c>ses.setDevicePermissionHandler(handler)</c>.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnSelectUsbDevice =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member details: Main.Session.SelectUsbDevice.Details with get, set

            [<Emit("$0[2]")>]
            abstract member callback: Option<string> -> unit with get, set

        /// <summary>
        /// Emitted after <c>SerialPort.forget()</c> has been called.  This event can be used to help maintain persistent storage of permissions when
        /// <c>setDevicePermissionHandler</c> is used.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnSerialPortRevoked =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member details: Main.Session.SerialPortRevoked.Details with get, set

        /// <summary>
        /// Emitted after <c>navigator.serial.requestPort</c> has been called and <c>select-serial-port</c> has fired if a serial port has been removed before the callback
        /// from <c>select-serial-port</c> is called.  This event is intended for use when using a UI to ask users to pick
        /// a port so that the UI can be updated to remove the specified port.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnSerialPortRemoved =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member port: SerialPort with get, set

            [<Emit("$0[2]")>]
            abstract member webContents: WebContents with get, set

        /// <summary>
        /// Emitted after <c>navigator.serial.requestPort</c> has been called and <c>select-serial-port</c> has fired if a new serial port becomes available before the callback
        /// from <c>select-serial-port</c> is called.  This event is intended for use when using a UI to ask users to pick
        /// a port so that the UI can be updated with the newly added port.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnSerialPortAdded =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member port: SerialPort with get, set

            [<Emit("$0[2]")>]
            abstract member webContents: WebContents with get, set

        /// <summary>
        /// Emitted when a serial port needs to be selected when a call to <c>navigator.serial.requestPort</c> is made. <c>callback</c> should be called
        /// with <c>portId</c> to be selected, passing an empty string to <c>callback</c> will cancel the request.  Additionally, permissioning on <c>navigator.serial</c>
        /// can be managed by using ses.setPermissionCheckHandler(handler) with the <c>serial</c> permission.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnSelectSerialPort =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member portList: SerialPort[] with get, set

            [<Emit("$0[2]")>]
            abstract member webContents: WebContents with get, set

            [<Emit("$0[3]")>]
            abstract member callback: string -> unit with get, set

        /// <summary>
        /// Emitted after <c>HIDDevice.forget()</c> has been called.  This event can be used to help maintain persistent storage of permissions when
        /// <c>setDevicePermissionHandler</c> is used.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnHidDeviceRevoked =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member details: Main.Session.HidDeviceRevoked.Details with get, set

        /// <summary>
        /// Emitted after <c>navigator.hid.requestDevice</c> has been called and <c>select-hid-device</c> has fired if a device has been removed before the callback from
        /// <c>select-hid-device</c> is called.  This event is intended for use when using a UI to ask users to pick a
        /// device so that the UI can be updated to remove the specified device.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnHidDeviceRemoved =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member details: Main.Session.HidDeviceRemoved.Details with get, set

        /// <summary>
        /// Emitted after <c>navigator.hid.requestDevice</c> has been called and <c>select-hid-device</c> has fired if a new device becomes available before the callback from
        /// <c>select-hid-device</c> is called.  This event is intended for use when using a UI to ask users to pick a
        /// device so that the UI can be updated with the newly added device.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnHidDeviceAdded =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member details: Main.Session.HidDeviceAdded.Details with get, set

        /// <summary>
        /// Emitted when a HID device needs to be selected when a call to <c>navigator.hid.requestDevice</c> is made. <c>callback</c> should be called
        /// with <c>deviceId</c> to be selected; passing no arguments to <c>callback</c> will cancel the request.  Additionally, permissioning on <c>navigator.hid</c> can
        /// be further managed by using <c>ses.setPermissionCheckHandler(handler)</c> and <c>ses.setDevicePermissionHandler(handler)</c>.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnSelectHidDevice =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member details: Main.Session.SelectHidDevice.Details with get, set

            [<Emit("$0[2]")>]
            abstract member callback: Option<Option<string>> -> unit with get, set

        /// <summary>
        /// Emitted when a hunspell dictionary file download fails.  For details on the failure you should collect a netlog and
        /// inspect the download request.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnSpellcheckDictionaryDownloadFailure =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// The language code of the dictionary file
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member languageCode: string with get, set

        /// <summary>
        /// Emitted when a hunspell dictionary file has been successfully downloaded
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnSpellcheckDictionaryDownloadSuccess =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// The language code of the dictionary file
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member languageCode: string with get, set

        /// <summary>
        /// Emitted when a hunspell dictionary file starts downloading
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnSpellcheckDictionaryDownloadBegin =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// The language code of the dictionary file
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member languageCode: string with get, set

        /// <summary>
        /// Emitted when a hunspell dictionary file has been successfully initialized. This occurs after the file has been downloaded.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnSpellcheckDictionaryInitialized =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// The language code of the dictionary file
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member languageCode: string with get, set

        /// <summary>
        /// Emitted when a render process requests preconnection to a URL, generally due to a resource hint.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnPreconnect =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// The URL being requested for preconnection by the renderer.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member preconnectUrl: string with get, set

            /// <summary>
            /// True if the renderer is requesting that the connection include credentials (see the spec for more details.)
            /// </summary>
            [<Emit("$0[2]")>]
            abstract member allowCredentials: bool with get, set

        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnFileSystemAccessRestricted =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member details: Main.Session.FileSystemAccessRestricted.Details with get, set

            [<Emit("$0[2]")>]
            abstract member callback: Main.Enums.Session.FileSystemAccessRestricted.Callback.Action -> unit with get, set

        /// <summary>
        /// Emitted after an extension is loaded and all necessary browser state is initialized to support the start of the extension's
        /// background page.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnExtensionReady =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member extension: Extension with get, set

        /// <summary>
        /// Emitted after an extension is unloaded. This occurs when <c>Session.removeExtension</c> is called.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnExtensionUnloaded =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member extension: Extension with get, set

        /// <summary>
        /// Emitted after an extension is loaded. This occurs whenever an extension is added to the "enabled" set of extensions. This
        /// includes:<br/><br/>* Extensions being loaded from <c>Session.loadExtension</c>.<br/>* Extensions being reloaded:<br/>  * from a crash.<br/>  * if the extension requested
        /// it (<c>chrome.runtime.reload()</c>).
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnExtensionLoaded =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member extension: Extension with get, set

        /// <summary>
        /// Emitted when Electron is about to download <c>item</c> in <c>webContents</c>.<br/><br/>Calling <c>event.preventDefault()</c> will cancel the download and <c>item</c> will not be
        /// available from next tick of the process.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnWillDownload =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member item: DownloadItem with get, set

            [<Emit("$0[2]")>]
            abstract member webContents: WebContents with get, set

    module SystemPreferences =
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_LIN || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ✔ | MAS ❌</para>
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnAccentColorChanged =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// The new RGBA color the user assigned to be their system accent color.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member newColor: string with get, set
        #endif


    module Tray =
        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted when the mouse moves in the tray icon.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnMouseMove =
            [<Emit("$0[0]")>]
            abstract member event: KeyboardEvent with get, set

            /// <summary>
            /// The position of the event.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member position: Point with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted when the mouse exits the tray icon.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnMouseLeave =
            [<Emit("$0[0]")>]
            abstract member event: KeyboardEvent with get, set

            /// <summary>
            /// The position of the event.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member position: Point with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted when the mouse enters the tray icon.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnMouseEnter =
            [<Emit("$0[0]")>]
            abstract member event: KeyboardEvent with get, set

            /// <summary>
            /// The position of the event.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member position: Point with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted when the mouse clicks the tray icon.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnMouseDown =
            [<Emit("$0[0]")>]
            abstract member event: KeyboardEvent with get, set

            /// <summary>
            /// The position of the event.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member position: Point with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted when the mouse is released from clicking the tray icon.<br/><br/>&gt; [!NOTE] This will not be emitted if you have
        /// set a context menu for your Tray using <c>tray.setContextMenu</c>, as a result of macOS-level constraints.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnMouseUp =
            [<Emit("$0[0]")>]
            abstract member event: KeyboardEvent with get, set

            /// <summary>
            /// The position of the event.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member position: Point with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted when dragged text is dropped in the tray icon.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDropText =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// the dropped text string.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member text: string with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ❌ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted when dragged files are dropped in the tray icon.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDropFiles =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// The paths of the dropped files.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member files: string[] with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ❌ | LIN ❌ | MAS ❌</para>
        /// Emitted when the tray icon is middle clicked.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnMiddleClick =
            [<Emit("$0[0]")>]
            abstract member event: KeyboardEvent with get, set

            /// <summary>
            /// The bounds of tray icon.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member bounds: Rectangle with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted when the tray icon is double clicked.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDoubleClick =
            [<Emit("$0[0]")>]
            abstract member event: KeyboardEvent with get, set

            /// <summary>
            /// The bounds of tray icon.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member bounds: Rectangle with get, set
        #endif

        #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_MAC || ELECTRON_OS_WIN
        /// <summary>
        /// <para>⚠ OS Compatibility: WIN ✔ | MAC ✔ | LIN ❌ | MAS ❌</para>
        /// Emitted when the tray icon is right clicked.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnRightClick =
            [<Emit("$0[0]")>]
            abstract member event: KeyboardEvent with get, set

            /// <summary>
            /// The bounds of tray icon.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member bounds: Rectangle with get, set
        #endif


        /// <summary>
        /// Emitted when the tray icon is clicked.<br/><br/>Note that on Linux this event is emitted when the tray icon receives an
        /// activation, which might not necessarily be left mouse click.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnClick =
            [<Emit("$0[0]")>]
            abstract member event: KeyboardEvent with get, set

            /// <summary>
            /// The bounds of tray icon.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member bounds: Rectangle with get, set

            /// <summary>
            /// The position of the event.
            /// </summary>
            [<Emit("$0[2]")>]
            abstract member position: Point with get, set

    module UtilityProcess =
        /// <summary>
        /// Emitted when the child process needs to terminate due to non continuable error from V8.<br/><br/>No matter if you listen to
        /// the <c>error</c> event, the <c>exit</c> event will be emitted after the child process terminates.
        /// </summary>
        [<Experimental("Indicated to be Experimental by Electron");
          System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnError =
            /// <summary>
            /// Type of error. One of the following values:
            /// </summary>
            [<Emit("$0[0]")>]
            abstract member ``type``: Main.Enums.UtilityProcess.Error.Type with get, set

            /// <summary>
            /// Source location from where the error originated.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member location: string with get, set

            /// <summary>
            /// <c>Node.js diagnostic report</c>.
            /// </summary>
            [<Emit("$0[2]")>]
            abstract member report: string with get, set

    module WebContents =
        /// <summary>
        /// Emitted when the mainFrame, an <c>&lt;iframe&gt;</c>, or a nested <c>&lt;iframe&gt;</c> is loaded within the page.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnFrameCreated =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member details: Main.WebContents.FrameCreated.Details with get, set

        /// <summary>
        /// Emitted when the <c>WebContents</c> preferred size has changed.<br/><br/>This event will only be emitted when <c>enablePreferredSizeMode</c> is set to <c>true</c> in
        /// <c>webPreferences</c>.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnPreferredSizeChanged =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// The minimum size needed to contain the layout of the document—without requiring scrolling.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member preferredSize: Size with get, set

        /// <summary>
        /// Emitted when the renderer process sends a synchronous message via <c>ipcRenderer.sendSync()</c>.<br/><br/>See also <c>webContents.ipc</c>, which provides an <c>IpcMain</c>-like interface for responding
        /// to IPC messages specifically from this WebContents.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnIpcMessageSync =
            [<Emit("$0[0]")>]
            abstract member event: IpcMainEvent with get, set

            [<Emit("$0[1]")>]
            abstract member channel: string with get, set

            [<Emit("$0.slice(2)")>]
            abstract member args: obj[] with get, set

        /// <summary>
        /// Emitted when the renderer process sends an asynchronous message via <c>ipcRenderer.send()</c>.<br/><br/>See also <c>webContents.ipc</c>, which provides an <c>IpcMain</c>-like interface for responding
        /// to IPC messages specifically from this WebContents.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnIpcMessage =
            [<Emit("$0[0]")>]
            abstract member event: IpcMainEvent with get, set

            [<Emit("$0[1]")>]
            abstract member channel: string with get, set

            [<Emit("$0.slice(2)")>]
            abstract member args: obj[] with get, set

        /// <summary>
        /// Emitted when the preload script <c>preloadPath</c> throws an unhandled exception <c>error</c>.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnPreloadError =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member preloadPath: string with get, set

            [<Emit("$0[2]")>]
            abstract member error: Error with get, set

        /// <summary>
        /// Emitted when the associated window logs a console message.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnConsoleMessage =
            [<Emit("$0[0]")>]
            abstract member details: Main.Details with get, set

            /// <summary>
            /// The log level, from 0 to 3. In order it matches <c>verbose</c>, <c>info</c>, <c>warning</c> and <c>error</c>.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member level: int with get, set

            /// <summary>
            /// The actual console message
            /// </summary>
            [<Emit("$0[2]")>]
            abstract member message: string with get, set

            /// <summary>
            /// The line number of the source that triggered this console message
            /// </summary>
            [<Emit("$0[3]")>]
            abstract member line: int with get, set

            [<Emit("$0[4]")>]
            abstract member sourceId: string with get, set

        /// <summary>
        /// Emitted when a <c>&lt;webview&gt;</c> has been attached to this web contents.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDidAttachWebview =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// The guest web contents that is used by the <c>&lt;webview&gt;</c>.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member webContents: WebContents with get, set

        /// <summary>
        /// Emitted when a <c>&lt;webview&gt;</c>'s web contents is being attached to this web contents. Calling <c>event.preventDefault()</c> will destroy the guest page.<br/><br/>This
        /// event can be used to configure <c>webPreferences</c> for the <c>webContents</c> of a <c>&lt;webview&gt;</c> before it's loaded, and provides the ability
        /// to set settings that can't be set via <c>&lt;webview&gt;</c> attributes.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnWillAttachWebview =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// The web preferences that will be used by the guest page. This object can be modified to adjust the preferences
            /// for the guest page.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member webPreferences: WebPreferences with get, set

            /// <summary>
            /// The other <c>&lt;webview&gt;</c> parameters such as the <c>src</c> URL. This object can be modified to adjust the parameters of the
            /// guest page.
            /// </summary>
            [<Emit("$0[2]")>]
            abstract member ``params``: Record<string, string> with get, set

        /// <summary>
        /// Emitted when a new frame is generated. Only the dirty area is passed in the buffer.<br/><br/>When using shared texture (set
        /// <c>webPreferences.offscreen.useSharedTexture</c> to <c>true</c>) feature, you can pass the texture handle to external rendering pipeline without the overhead of copying data
        /// between CPU and GPU memory, with Chromium's hardware acceleration support. This feature is helpful for high-performance rendering scenarios.<br/><br/>Only a limited
        /// number of textures can exist at the same time, so it's important that you call <c>texture.release()</c> as soon as you're
        /// done with the texture. By managing the texture lifecycle by yourself, you can safely pass the <c>texture.textureInfo</c> to other processes
        /// through IPC.<br/><br/>More details can be found in the offscreen rendering tutorial. To learn about how to handle the texture in
        /// native code, refer to offscreen rendering's code documentation..
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnPaint =
            [<Emit("$0[0]")>]
            abstract member details: Main.Details with get, set

            [<Emit("$0[1]")>]
            abstract member dirtyRect: Rectangle with get, set

            /// <summary>
            /// The image data of the whole frame.
            /// </summary>
            [<Emit("$0[2]")>]
            abstract member image: NativeImage with get, set

        /// <summary>
        /// Emitted when a bluetooth device needs to be selected when a call to <c>navigator.bluetooth.requestDevice</c> is made. <c>callback</c> should be called
        /// with the <c>deviceId</c> of the device to be selected.  Passing an empty string to <c>callback</c> will cancel the request.<br/><br/>If
        /// no event listener is added for this event, all bluetooth requests will be cancelled.<br/><br/>If <c>event.preventDefault</c> is not called when handling
        /// this event, the first available device will be automatically selected.<br/><br/>Due to the nature of bluetooth, scanning for devices when <c>navigator.bluetooth.requestDevice</c>
        /// is called may take time and will cause <c>select-bluetooth-device</c> to fire multiple times until <c>callback</c> is called with either a
        /// device id or an empty string to cancel the request.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnSelectBluetoothDevice =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member devices: BluetoothDevice[] with get, set

            [<Emit("$0[2]")>]
            abstract member callback: string -> unit with get, set

        /// <summary>
        /// Emitted when there is a new context menu that needs to be handled.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnContextMenu =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member ``params``: Main.WebContents.ContextMenu.Params with get, set

        /// <summary>
        /// Emitted when the cursor's type changes. The <c>type</c> parameter can be <c>pointer</c>, <c>crosshair</c>, <c>hand</c>, <c>text</c>, <c>wait</c>, <c>help</c>, <c>e-resize</c>, <c>n-resize</c>, <c>ne-resize</c>,
        /// <c>nw-resize</c>, <c>s-resize</c>, <c>se-resize</c>, <c>sw-resize</c>, <c>w-resize</c>, <c>ns-resize</c>, <c>ew-resize</c>, <c>nesw-resize</c>, <c>nwse-resize</c>, <c>col-resize</c>, <c>row-resize</c>, <c>m-panning</c>, <c>m-panning-vertical</c>, <c>m-panning-horizontal</c>, <c>e-panning</c>, <c>n-panning</c>, <c>ne-panning</c>, <c>nw-panning</c>, <c>s-panning</c>, <c>se-panning</c>,
        /// <c>sw-panning</c>, <c>w-panning</c>, <c>move</c>, <c>vertical-text</c>, <c>cell</c>, <c>context-menu</c>, <c>alias</c>, <c>progress</c>, <c>nodrop</c>, <c>copy</c>, <c>none</c>, <c>not-allowed</c>, <c>zoom-in</c>, <c>zoom-out</c>, <c>grab</c>, <c>grabbing</c>, <c>custom</c>, <c>null</c>, <c>drag-drop-none</c>, <c>drag-drop-move</c>,
        /// <c>drag-drop-copy</c>, <c>drag-drop-link</c>, <c>ns-no-resize</c>, <c>ew-no-resize</c>, <c>nesw-no-resize</c>, <c>nwse-no-resize</c>, or <c>default</c>.<br/><br/>If the <c>type</c> parameter is <c>custom</c>, the <c>image</c> parameter will hold the custom
        /// cursor image in a <c>NativeImage</c>, and <c>scale</c>, <c>size</c> and <c>hotspot</c> will hold additional information about the custom cursor.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnCursorChanged =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member ``type``: string with get, set

            [<Emit("$0[2]")>]
            abstract member image: NativeImage with get, set

            /// <summary>
            /// scaling factor for the custom cursor.
            /// </summary>
            [<Emit("$0[3]")>]
            abstract member scale: float with get, set

            /// <summary>
            /// the size of the <c>image</c>.
            /// </summary>
            [<Emit("$0[4]")>]
            abstract member size: Size with get, set

            /// <summary>
            /// coordinates of the custom cursor's hotspot.
            /// </summary>
            [<Emit("$0[5]")>]
            abstract member hotspot: Point with get, set

        /// <summary>
        /// Emitted when mouse moves over a link or the keyboard moves the focus to a link.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnUpdateTargetUrl =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member url: string with get, set

        /// <summary>
        /// Emitted when a page's theme color changes. This is usually due to encountering a meta tag:
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDidChangeThemeColor =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// Theme color is in format of '#rrggbb'. It is <c>null</c> when no theme color is set.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member color: Option<string> with get, set

        /// <summary>
        /// Emitted when a result is available for <c>webContents.findInPage</c> request.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnFoundInPage =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member result: Main.WebContents.FoundInPage.Result with get, set

        /// <summary>
        /// Emitted when <c>webContents</c> wants to do basic auth.<br/><br/>The usage is the same with the <c>login</c> event of <c>app</c>.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnLogin =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member authenticationResponseDetails: Main.WebContents.Login.AuthenticationResponseDetails with get, set

            [<Emit("$0[2]")>]
            abstract member authInfo: Main.WebContents.Login.AuthInfo with get, set

            [<Emit("$0[3]")>]
            abstract member callback: Main.WebContents.Login.Callback with get, set

        /// <summary>
        /// Emitted when a client certificate is requested.<br/><br/>The usage is the same with the <c>select-client-certificate</c> event of <c>app</c>.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnSelectClientCertificate =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member url: URL with get, set

            [<Emit("$0[2]")>]
            abstract member certificateList: Certificate[] with get, set

            [<Emit("$0[3]")>]
            abstract member callback: Certificate -> unit with get, set

        /// <summary>
        /// Emitted when failed to verify the <c>certificate</c> for <c>url</c>.<br/><br/>The usage is the same with the <c>certificate-error</c> event of <c>app</c>.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnCertificateError =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member url: string with get, set

            /// <summary>
            /// The error code.
            /// </summary>
            [<Emit("$0[2]")>]
            abstract member error: string with get, set

            [<Emit("$0[3]")>]
            abstract member certificate: Certificate with get, set

            [<Emit("$0[4]")>]
            abstract member callback: bool -> unit with get, set

            [<Emit("$0[5]")>]
            abstract member isMainFrame: bool with get, set

        /// <summary>
        /// Emitted when 'Search' is selected for text in its context menu.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDevtoolsSearchQuery =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// text to query for.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member query: string with get, set

        /// <summary>
        /// Emitted when a link is clicked in DevTools or 'Open in new tab' is selected for a link in its
        /// context menu.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDevtoolsOpenUrl =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// URL of the link that was clicked or selected.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member url: string with get, set

        /// <summary>
        /// Emitted when the user is requesting to change the zoom level using the mouse wheel.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnZoomChanged =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// Can be <c>in</c> or <c>out</c>.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member zoomDirection: Main.Enums.WebContents.ZoomChanged.ZoomDirection with get, set

        /// <summary>
        /// Emitted before dispatching mouse events in the page.<br/><br/>Calling <c>event.preventDefault</c> will prevent the page mouse events.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnBeforeMouseEvent =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member mouse: MouseInputEvent with get, set

        /// <summary>
        /// Emitted before dispatching the <c>keydown</c> and <c>keyup</c> events in the page. Calling <c>event.preventDefault</c> will prevent the page <c>keydown</c>/<c>keyup</c> events and
        /// the menu shortcuts.<br/><br/>To only prevent the menu shortcuts, use <c>setIgnoreMenuShortcuts</c>:
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnBeforeInputEvent =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// Input properties.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member input: Main.WebContents.BeforeInputEvent.Input with get, set

        /// <summary>
        /// Emitted when an input event is sent to the WebContents. See InputEvent for details.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnInputEvent =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member inputEvent: InputEvent with get, set

        /// <summary>
        /// Emitted when the renderer process unexpectedly disappears.  This is normally because it was crashed or killed.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnRenderProcessGone =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member details: RenderProcessGoneDetails with get, set

        /// <summary>
        /// Emitted when an in-page navigation happened in any frame.<br/><br/>When in-page navigation happens, the page URL changes but does not cause
        /// navigation outside of the page. Examples of this occurring are when anchor links are clicked or when the DOM <c>hashchange</c>
        /// event is triggered.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDidNavigateInPage =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member url: string with get, set

            [<Emit("$0[2]")>]
            abstract member isMainFrame: bool with get, set

            [<Emit("$0[3]")>]
            abstract member frameProcessId: int with get, set

            [<Emit("$0[4]")>]
            abstract member frameRoutingId: int with get, set

        /// <summary>
        /// Emitted when any frame navigation is done.<br/><br/>This event is not emitted for in-page navigations, such as clicking anchor links or
        /// updating the <c>window.location.hash</c>. Use <c>did-navigate-in-page</c> event for this purpose.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDidFrameNavigate =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member url: string with get, set

            /// <summary>
            /// -1 for non HTTP navigations
            /// </summary>
            [<Emit("$0[2]")>]
            abstract member httpResponseCode: int with get, set

            /// <summary>
            /// empty for non HTTP navigations,
            /// </summary>
            [<Emit("$0[3]")>]
            abstract member httpStatusText: string with get, set

            [<Emit("$0[4]")>]
            abstract member isMainFrame: bool with get, set

            [<Emit("$0[5]")>]
            abstract member frameProcessId: int with get, set

            [<Emit("$0[6]")>]
            abstract member frameRoutingId: int with get, set

        /// <summary>
        /// Emitted when a main frame navigation is done.<br/><br/>This event is not emitted for in-page navigations, such as clicking anchor links
        /// or updating the <c>window.location.hash</c>. Use <c>did-navigate-in-page</c> event for this purpose.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDidNavigate =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member url: string with get, set

            /// <summary>
            /// -1 for non HTTP navigations
            /// </summary>
            [<Emit("$0[2]")>]
            abstract member httpResponseCode: int with get, set

            /// <summary>
            /// empty for non HTTP navigations
            /// </summary>
            [<Emit("$0[3]")>]
            abstract member httpStatusText: string with get, set

        /// <summary>
        /// Emitted after a server side redirect occurs during navigation.  For example a 302 redirect.<br/><br/>This event cannot be prevented, if
        /// you want to prevent redirects you should checkout out the <c>will-redirect</c> event above.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDidRedirectNavigation =
            [<Emit("$0[0]")>]
            abstract member details: Main.Details with get, set

            [<Emit("$0[1]")>]
            abstract member url: string with get, set

            [<Emit("$0[2]")>]
            abstract member isInPlace: bool with get, set

            [<Emit("$0[3]")>]
            abstract member isMainFrame: bool with get, set

            [<Emit("$0[4]")>]
            abstract member frameProcessId: int with get, set

            [<Emit("$0[5]")>]
            abstract member frameRoutingId: int with get, set

        /// <summary>
        /// Emitted when a server side redirect occurs during navigation.  For example a 302 redirect.<br/><br/>This event will be emitted after
        /// <c>did-start-navigation</c> and always before the <c>did-redirect-navigation</c> event for the same navigation.<br/><br/>Calling <c>event.preventDefault()</c> will prevent the navigation (not just the redirect).
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnWillRedirect =
            [<Emit("$0[0]")>]
            abstract member details: Main.Details with get, set

            [<Emit("$0[1]")>]
            abstract member url: string with get, set

            [<Emit("$0[2]")>]
            abstract member isInPlace: bool with get, set

            [<Emit("$0[3]")>]
            abstract member isMainFrame: bool with get, set

            [<Emit("$0[4]")>]
            abstract member frameProcessId: int with get, set

            [<Emit("$0[5]")>]
            abstract member frameRoutingId: int with get, set

        /// <summary>
        /// Emitted when any frame (including main) starts navigating.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDidStartNavigation =
            [<Emit("$0[0]")>]
            abstract member details: Main.Details with get, set

            [<Emit("$0[1]")>]
            abstract member url: string with get, set

            [<Emit("$0[2]")>]
            abstract member isInPlace: bool with get, set

            [<Emit("$0[3]")>]
            abstract member isMainFrame: bool with get, set

            [<Emit("$0[4]")>]
            abstract member frameProcessId: int with get, set

            [<Emit("$0[5]")>]
            abstract member frameRoutingId: int with get, set

        /// <summary>
        /// Emitted when a user or the page wants to start navigation on the main frame. It can happen when the
        /// <c>window.location</c> object is changed or a user clicks a link in the page.<br/><br/>This event will not emit when the navigation
        /// is started programmatically with APIs like <c>webContents.loadURL</c> and <c>webContents.back</c>.<br/><br/>It is also not emitted for in-page navigations, such as clicking anchor
        /// links or updating the <c>window.location.hash</c>. Use <c>did-navigate-in-page</c> event for this purpose.<br/><br/>Calling <c>event.preventDefault()</c> will prevent the navigation.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnWillNavigate =
            [<Emit("$0[0]")>]
            abstract member details: Main.Details with get, set

            [<Emit("$0[1]")>]
            abstract member url: string with get, set

            [<Emit("$0[2]")>]
            abstract member isInPlace: bool with get, set

            [<Emit("$0[3]")>]
            abstract member isMainFrame: bool with get, set

            [<Emit("$0[4]")>]
            abstract member frameProcessId: int with get, set

            [<Emit("$0[5]")>]
            abstract member frameRoutingId: int with get, set

        /// <summary>
        /// Emitted _after_ successful creation of a window via <c>window.open</c> in the renderer. Not emitted if the creation of the window
        /// is canceled from <c>webContents.setWindowOpenHandler</c>.<br/><br/>See <c>window.open()</c> for more details and how to use this in conjunction with <c>webContents.setWindowOpenHandler</c>.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDidCreateWindow =
            [<Emit("$0[0]")>]
            abstract member window: BrowserWindow with get, set

            [<Emit("$0[1]")>]
            abstract member details: Main.WebContents.DidCreateWindow.Details with get, set

        /// <summary>
        /// Emitted when the page calls <c>window.moveTo</c>, <c>window.resizeTo</c> or related APIs.<br/><br/>By default, this will move the window. To prevent that behavior,
        /// call <c>event.preventDefault()</c>.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnContentBoundsUpdated =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// requested new content bounds
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member bounds: Rectangle with get, set

        /// <summary>
        /// Emitted when page receives favicon urls.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnPageFaviconUpdated =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            /// <summary>
            /// Array of URLs.
            /// </summary>
            [<Emit("$0[1]")>]
            abstract member favicons: string[] with get, set

        /// <summary>
        /// Fired when page title is set during navigation. <c>explicitSet</c> is false when title is synthesized from file url.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnPageTitleUpdated =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member title: string with get, set

            [<Emit("$0[2]")>]
            abstract member explicitSet: bool with get, set

        /// <summary>
        /// Emitted when a frame has done navigation.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDidFrameFinishLoad =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member isMainFrame: bool with get, set

            [<Emit("$0[2]")>]
            abstract member frameProcessId: int with get, set

            [<Emit("$0[3]")>]
            abstract member frameRoutingId: int with get, set

        /// <summary>
        /// This event is like <c>did-fail-load</c> but emitted when the load was cancelled (e.g. <c>window.stop()</c> was invoked).
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDidFailProvisionalLoad =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member errorCode: int with get, set

            [<Emit("$0[2]")>]
            abstract member errorDescription: string with get, set

            [<Emit("$0[3]")>]
            abstract member validatedURL: string with get, set

            [<Emit("$0[4]")>]
            abstract member isMainFrame: bool with get, set

            [<Emit("$0[5]")>]
            abstract member frameProcessId: int with get, set

            [<Emit("$0[6]")>]
            abstract member frameRoutingId: int with get, set

        /// <summary>
        /// This event is like <c>did-finish-load</c> but emitted when the load failed. The full list of error codes and their meaning
        /// is available here.
        /// </summary>
        [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never);
          AllowNullLiteral;
          Interface>]
        type IOnDidFailLoad =
            [<Emit("$0[0]")>]
            abstract member event: Event with get, set

            [<Emit("$0[1]")>]
            abstract member errorCode: int with get, set

            [<Emit("$0[2]")>]
            abstract member errorDescription: string with get, set

            [<Emit("$0[3]")>]
            abstract member validatedURL: string with get, set

            [<Emit("$0[4]")>]
            abstract member isMainFrame: bool with get, set

            [<Emit("$0[5]")>]
            abstract member frameProcessId: int with get, set

            [<Emit("$0[6]")>]
            abstract member frameRoutingId: int with get, set
