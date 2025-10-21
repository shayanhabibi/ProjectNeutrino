# FSharp Domain

When 'parsing' or 'reading' from our directly parsed electron api, we want to prepare the model for source generation.

This means ensuring that all information that is required to do so, is guaranteed within the model as it is read.

Currently, this is not done.

There are lots of types which have optional name nodes, mostly because they are in-line lambdas, or string enums, or options. In these cases, we have to manage them appropriately, and either provide context to provide a name, or change our methodology entirely.

## Inline Options

When we have an object of options in a signature, such as with a method, we have an opportunity to use `ParamObjects` to inline the object. In this case, we essentially inline the object options into the method signature.

This requires the generator to first check whether it has only ONE inlined object to generate for, and that all the object fields are optional.

If that is not the case, then we have to extract the object options into a pojo.

In this case, the name of the Pojo will be the name of the method with `Options` appended to the end.
