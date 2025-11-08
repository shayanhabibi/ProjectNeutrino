# Spec

<tldr>Contains constants or fields that are globally applicable, such as the root namespace of Fable.Electron</tldr>

This file contains constants and more significant 'prebaked' definitions that have to be injected into our
source code generation (as opposed to being included in the supplementary <code>Types.fs</code>).

More _Specification_ like things will be moved here; ideally thinks like prebaked remappings etc will 
be co-located here, so that there is an easy single source of truth to alter to change these 
types of 'exceptions' to the most common rules.
