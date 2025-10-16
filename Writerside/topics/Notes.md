# Notes

With regards to source generation, we want to consider a few things.

### Namespace Pollution {collapsible="true"}

There are a wide swathe of APIs and structures available, especially in a 'global' context.

Personally, it is favourable to make pollution of your global namespace opt-in rather than by default.

<note>
We could have two namespaces available, such as <code>Fable.Electron</code> to
open access to modules of access such as <code>Fable.Electron.Structures</code>.

The second namespace might be <code>Fable.Electron.Core</code> which uses aliased modules and <code>AutoOpen</code> to open the core globally accessible API structures and methods.
</note>

### OS Compatabilities {collapsible="true"}

When presenting API that have OS specificity, we want to consider how to provide the API in a type safe manner, which is resolved at Compile Time.

<note>
We could have a module per OS, which reference an editor hidden common module to present their respective APIs.

This would only be for specific OS methods. Generalised methods are globally accessible.
</note>

<note>
We could use conditional compiler directives in the project to make OS specific methods and APIs only available in the source when compiling with a specific Definition set.
</note>

<note>
We could do a combination of both the above.

Any API with OS specificity will be implemented in a specific, commonly shared module, which is editor hidden.

They will be generated into the source code of the 'global' module/namespace, using conditional directives. They will also be generated into modules which acts as aliases to the common source implementations.

In this manner, one can choose whether they want the OS specific methods to be available by default, or explicitly accessed.
</note>

Since the source is being entirely generated, I don't see the harm in opting for the third method.

### Enums/Unions {collapsible="true"}

When it comes to generating, naming, and referencing Enums, I prefer things to have a common access point.

A methodology I like, which is available to us as we are source generating, is to have an <code>AutoOpen</code> <code>Enums</code> module. All Enums are contained within via modules which follow their access.

For example, if we have an Enum for the Renderer Class BrowserWindow for the set-rect method, we would locate this in <code>Enums.BrowserWindow.SetRect.#NameOfParameter#</code>.

The primary reason I prefer this, is because you can explore unions/enums within your editor using intellisense, without other classes/types polluting the suggestions.
