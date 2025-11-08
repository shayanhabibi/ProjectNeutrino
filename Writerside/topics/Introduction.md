# Introduction

<tldr>This module parses the <code>electron-api.json</code> file to produce Fable binding source code.</tldr>

To produce the Fable.Electron bindings, we utilise the same methodology and resources that are used by the electron 
community to produce their TypeScript definitions.

<procedure>
<step>Every release of Electron generates an <code>electron-api.json</code> file from the documentation.</step>
<step>The <code>json</code> file is used to generate type definitions and provide a resource for other features such 
as IDE integration.</step>
We utilise the same resource to produce the Fable.Electron bindings.
</procedure>

The <code>ElectronApi.Json.Parser</code>, as it currently stands, does more than the name implies. It decodes the 
<code>electron-api.json</code> file into <code>Records</code> using <tooltip term="thoth-json">Thoth</tooltip>.

While applying transformations to create more idiomatic names and types/signatures for F# Fable, we go through a series 
of steps; a number of which are redundant (will undergo refactor; priorities at the moment lie with testing)

To help understand the current codebase, you can see the relevant topic.
