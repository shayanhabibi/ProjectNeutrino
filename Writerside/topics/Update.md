# Update

> Thought I should update and catalogue my thoughts on the progression of this project.

An **important* aspect of the project was for it to be maintainable. It is certainly a concept that I perhaps have
thought, more recently, is difficult to evaluate.

As I write the project, I certainly feel it is maintainable in some aspect. It seems simplistic to me. I am simple:
1. Parsing the api
2. Modifying the parsed api into a more workable format
3. Mapping it to FSharp types with some helpers
4. Caching types that need to be lifted when doing said mappings
5. Printing output

But realistically, it's looking a lot more gross than that.

I've found a lot of the conversions might have unique elements to consider, so I haven't abstracted a lot of the
source generation, leaving a lot of bloat. But it is easier to manage changing any one elements output.

## Output

There are a lot of thoughts I have on output, because I want to generate strong, and easy to use bindings.

The general JS method of using `.on` for emitters with the event name following and the handler after is not a vibe.

Previously (in Fable.Electron from 6 years ago), curried handlers were used.

To be honest, I'm not a fan of this. When interoping with JS, we lose information when abstracting away handlers using
curried functions. That information is the names of the arguments. It's not the same as when we spin up our own curried
functions which are *usually* more intuitive through their name and types (and docs).

It's issues like this which lead me down rabbitholes of consideration and scope creep.

--- 

At the moment I have multiple caches for types which lift expressions into types or other helpers for source generation.
For instance, I have TWO caches for `Event` types. A `Constant` cache which tracks the paths and names of events, to
generate F# literals for use in manual event handling, or perhaps message sending; and a `Interface` cache, which
stores the `Event` types and creates interfaces from the parameters.

The interface creation is how I plan on solving the 'curried' handler issue. Where there is a handler that takes
more than one argument, I create two overloads for registering a handler. One provides for a single argument handler
which provides the arguments via an interface which emits indexed accessors. The other is the curried handler. Now
we have the freedom to choose between the two. If you know your game, you can just use the curried ones. If you need
to explore the options via intellisense, then the interfaces would be best.

A big concern of mine in these things is always `namespace` pollution. To avoid namespace pollution, I have utilised
the `System.ComponentModel.EditorBrowsable` attribute which is supported by most editors/IDEs for dotnet. We don't
ever need to see the interfaces as suggestions in our intellisense. When we work with the interfaces, we have an instance
with which we can access its properties.

