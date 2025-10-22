# PathKey

While creating the parser and subsequent source generator, I came across the issue of tracking the 'path' of a
binding.

> The path is essentially the breadcrumbs it took to arrive at a named binding. This is relevant when generating the source, as we may nest a type within a series of modules to replicate this path, as is done in Glutinum.

I'm not sure how this problem is approached in Glutinum, but I decidedly did not want to just add a list
of strings to create the path.

It may be relevant whether a path is derivative of a type, and if so, what the name of the type is.

What I've done is create a 'Path' type, which tracks whether a pathkey is found within the root namespace, or is derivative of a module.

A pathkey is either a combination of the path and the name of the binding, or a combination of another pathkey and name.

For instance, a parameter would be a combination of a method pathkey and a name. A method would be a combination of either a path or a type and a name.

Unfortunately, this methodology doesn't provide us static errors. But it does make guarantees with our domain, as we can instantly fail if we try to make a method pathkey for another method (why would we have a method deriving from another method?).

## Motivation

Honestly this has turned into a pile of spaghetti that is most certainly unnecessary to solve a 'what if' that doesn't occur with such a structured API.

If there is any need to remove code, this can probably go first.
