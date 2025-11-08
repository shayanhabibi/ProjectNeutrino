# Overview

<warning>
The codebase was iterated over several times as I went through the process of creating the generator which is reflected
by the less than ideal state of the codebase.

This will be improved when the testing is complete.
</warning>

There are two key dependencies that we utilise to generate the bindings

## Fantomas

Fantomas is used to generate the source code; it is the same technology used to format F# source code, and is well
tested and feature rich for source production as opposed to printing with strings.

## Thoth.Net.Json

Thoth is used to decode the <tooltip term="electron-api"><code>electron-api.json</code></tooltip> file; the schema
used to create the types for the decoding were derived from the electron docs-parser, which is responsible for creating
the file in the first place.

---

The source generation follows a fairly trivial procedure

<procedure>
<step>Decode the json directly to F# with a typed structure using unions and records</step>
<step>Prepare the json for source generation by further encoding/decoding the JSON information into internal
types such as <code>Path.PathKey</code></step>
<step>Simultaneously cache and track information for producing POJO type definitions, and other helpful types/interfaces</step>
<step>Convert the majority of our internal types and information into a singular generator helper (a single type
that defines the static and instance members, attributes, and documentation of a common class or POJO type).</step>
<step>Produce the source using the information in the generator helper</step>
</procedure>

We believe the most integral aspect of the procedure is to ensure the clean separation of the API Decoder types
from further transformations or manipulations.

<note>It is probably advisable to consider separating the transformer/generation process into a separate project.</note>

## Common Types

Here are some common types used in the procedure described above (further discussion in the relevant source file topic)

<procedure title="Name">
<code-block lang="F#">
type Name =
  | Source of string
  | Modified of source: string * modified: string
</code-block>

A simple Discriminated Union which persists the original name of a type, method, parameter or property from the source,
along side the intended 'transformed' name.

The <code>Name.Source</code> case implies that the name of the binding will be identical to the source. 
<code>Name.Modified</code> provides the source string, along with the modified string.

> There are a number of reasons to modify a string name for source generation; for instance, we will 
<tooltip term="camel-case">use camel casing</tooltip> instead of <tooltip term="kebab-case">kebab casing</tooltip>, and
we will prefer string enums to be in <tooltip term="pascal-case">pascal casing</tooltip> instead of camel or kebab.

Creating, and accessing Name values is almost entirely abstracted away via functions such as <code>Name.createPascal</code>
or methods such as <code>\_.ValueOrModified</code> and <code>\_.ValueOrSource</code> which perform as described by
their names.
</procedure>

<procedure title="Paths and PathKeys">
<code-block lang="F#">
// Example Path type definition:
type Module of parent: Choice&lt;Module, Root&gt; * name: Name
// PathKey definition:
type PathKey =
  | Binding of Binding
  | Type of Type
  | Property of Property
  | Method of Method
  | Event of Event
  | Parameter of Parameter
  | InlineOptions of InlineOptions
  | InlineLambda of InlineLambda
  | Module of Module
  | StringEnum of StringEnum
</code-block>

<code>Path</code> types in combination with the wrapping DU <code>PathKey</code> are essentially similar to tracking
the _access_ path of a parameter (module -> type -> name of parameter) using a list of strings.

The Path mechanism provides more contextual information when/where required, such as determining whether a property
is derived from a JS Module or a class/type.

There are helpers provided to recursively navigate or access the path such as <code>Path.tracePathOfEntry</code>, or
<code>_.Name</code>.

Exceptions are raised when an _unnatural_ path is created (such as a property deriving from another property), since this
is impossible to reflect in the type definitions of the underlying Path types.
</procedure>

> I find this implementation lacking. I was undergoing a refactor, but I was frustrated with the progress when I had
> already succeeded in source generation in this iteration, so reverted.

<procedure title="GeneratorContainer">
<code-block lang="F#">
type GeneratorContainer = {
    PathKey: Path.PathKey
    Constructor: Parameter list option
    InstanceProperties: Property list
    InstanceMethods: Method list
    InstanceEvents: Event list
    StaticProperties: Property list
    StaticMethods: Method list
    StaticEvents: Event list
    TypeAttributes: string list
    Description: string voption
    Process: Decoder.ProcessBlock option
    Compatibility: Compatibility option
    Extends: string voption
}
</code-block>

The generalist type generator.
</procedure>

<procedure title="GeneratorGrouper">
<code-block lang="F#">
type GeneratorGroupChild =
    | Nested of GeneratorGrouper
    | Child of GeneratorContainer
    | StringEnumType of StringEnum
    | Delegate of FuncOrMethod
    | EventInterface of pathKey: Path.PathKey * node: TypeDefn
    | EventConstant of pathKey: Path.PathKey
and GeneratorGrouper = {
    PathKey: Path.PathKey voption
    Description: string list voption
    Opens: OpenModule list
    Children: GeneratorGroupChild list
    Attributes: string list
}
</code-block>
The item that is operated on the most in the final steps; these records are the abstraction reflecting a module in
F# as opposed to in JavaScript.

Where possible, I used the <code>GeneratorContainer</code>; however, either due to older working implementations, or simply
lack of feasible reason to use such a large record, there are other child types introduced.
</procedure>
