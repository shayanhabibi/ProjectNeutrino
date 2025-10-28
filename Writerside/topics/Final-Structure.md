# Final Structure

Now to consider the final structure and api.

We have a variety of parameters, properties, classes, and methods which differ in compatibility between operating
systems. We also have differences in process accessibility (between renderer, main and utility).

There are a few different ways to present these API differences.

1. Dump all the source in one file, with comments for users to know which process they can/can't something from
2. Dump the source into a file, with submodules for the different processes.
  1. Where there exists two process compatibilities, then we will either
     1. Copy the item between the processes
     2. Have a shared type which is exported by the process modules
3. Have a separate package for each process
4. Have a separate package for each OS
5. Use project defines and conditionally compilation to either allow accessibility to an OS api or not?

Looking at it from a macro perspective, we'd end up with something like:

Fable.Electron
|- `File: PreludeTypes` - Fable.Electron |> Injections
|- `File: Types` - Fable.Electron |> Constants ||| Structures
|- `File: Main` - Fable.Electron.Main |> Main process bindings
|- `File: Renderer` - Fable.Electron.Renderer |> Renderer process bindings
|- `File: Utility` - Fable.Electron.Utility |> Utility process bindings

For conditional compiler directives, the mannerisms would need to specifically permit all code when nothing is defined,
and then filter out non-compatible api when something is defined.

```fsharp
#if OS1 || !(!OS1 || !OS2 || !OS3)
```

The above directive should match if we either don't specify any OS, or we have OS1 defined.
