# Bindings

<tldr>
<p>Schema is derived from <code>@electron/docs-parser</code></p>
<p>Parsing done with <code>Thoth.Json.Net</code></p>
<p>A script is available that you can run to see the parsed AST result.</p>
</tldr>

Achieving parity with Type-Script in type-safety is certainly a goal.

<procedure>

Electron generates their `.d.ts` from their docs directly.
<step>The docs are parsed using <code>@electron/docs-parser</code></step>
<step>A <tooltip term="electron-api">json api</tooltip> is generated.</step>
<step>The json api is then utilised by a separate package to generate the type definitions</step>
</procedure>

Our <tooltip term="parser">parser</tooltip> uses the same source of truth to generate our bindings.

<procedure>
<step>We first parse the json into strong types based off the source</step>
<step>We then map this to a more idiomatic FSharp representation.</step>
<step>We can then either choose to further map this to an intermediary AST such as <code>Glutinum</code> to generate the source, or generate our own source using <code>Fantomas</code></step>
</procedure>

## Parser Types

The types are crafted from the <code>@electron/docs-parser/ParsedDocumentation.d.ts</code> file which you can download <resource src="ParsedDocumentation.d.ts"/> to view.

<note>
Contributors should download the npm package directly if the binding must be updated.
</note>

My methodology to craft the types is to use Discriminated Unions to distinguish between types delineated by a tagged field such as <code>type</code>.

<tabs>
<code-block lang="ts" collapsible="true">
export declare type DetailedStringType = {
    type: 'String';
    possibleValues: PossibleStringValue[] | null;
};
export declare type DetailedObjectType = {
    type: 'Object';
    properties: PropertyDocumentationBlock[];
};
export declare type DetailedEventType = {
    type: 'Event';
    eventProperties: PropertyDocumentationBlock[];
};
export declare type DetailedEventReferenceType = {
    type: 'Event';
    eventPropertiesReference: TypeInformation;
};
export declare type DetailedFunctionType = {
    type: 'Function';
    parameters: MethodParameterDocumentation[];
    returns: TypeInformation | null;
};
export declare type DetailedType = ({
    type: TypeInformation[];
} | DetailedFunctionType | DetailedObjectType | DetailedEventType | DetailedEventReferenceType | DetailedStringType | {
    type: string;
}) & {
    innerTypes?: TypeInformation[];
};

</code-block>
<code-block lang="f#" collapsible="true">
<![CDATA[
[<RequireQualifiedAccess>]
module TypeInformationKind =
    type InfoArray = {
        Collection: bool
        Type: TypeInformation[]
    }
    type InfoString = {
        Collection: bool
        Type: string
    }
    type String = {
        Collection: bool
        PossibleValues: PossibleStringValue[] option
    }
    type Object = {
        Collection: bool
        Properties: PropertyDocumentationBlock[]
    }
    type Event = {
        Collection: bool
        EventProperties: PropertyDocumentationBlock[]
    }
    type EventRef = {
        Collection: bool
        EventPropertiesReference: TypeInformation
    }
    type Function = {
        Collection: bool
        Parameters: MethodParameterDocumentation[]
        Returns: TypeInformation option
    }
type TypeInformationKind =
    | InfoArray of TypeInformationKind.InfoArray
    | InfoString of TypeInformationKind.InfoString
    | String of TypeInformationKind.String
    | Object of TypeInformationKind.Object
    | Event of TypeInformationKind.Event
    | EventRef of TypeInformationKind.EventRef
    | Function of TypeInformationKind.Function
type TypeInformation =
    | WithInnerTypes of kind: TypeInformationKind * innerTypes: TypeInformation[]
    | TypeInformation of kind: TypeInformationKind


]]>
</code-block>
</tabs>

Where relevant, I would 'embed' inherited fields using a named field.

<tabs>
<code-block lang="ts" >
export declare type MethodParameterDocumentation = {
    name: string;
    description: string;
    required: boolean;
} & TypeInformation;

</code-block>
<code-block lang="f#">
<![CDATA[
type MethodParameterDocumentation = {
    Name: string
    Description: string
    Required: bool
    // Embedded
    TypeInformation: TypeInformation
}

]]>
</code-block>
</tabs>

Immediately following the type definition, I would write the `Thoth.Json.Net` decoder.

<code-block lang="f#" collapsible="true">
<![CDATA[
type PropertyDocumentationBlock = {
    // Embedded
    DocumentationBlock: DocumentationBlock
    Required: bool
    // Embedded
    TypeInformation: TypeInformation
}

module PropertyDocumentationBlock =
    let decode: Decoder<PropertyDocumentationBlock> =
        Decode.object (fun get ->
            {
                DocumentationBlock = get.Required.Raw DocumentationBlock.decode
                Required = get.Required.Field "required" Decode.bool
                TypeInformation = get.Required.Raw TypeInformation.decode
            })

]]>
</code-block>

<tip>
In my view, it is better to parse the JSON as close to the source as possible, and then map that within F# to
our own representation/domain instead of parsing the JSON directly into our domain.

This should prevent data-loss, and also pick up any changes in the api source generation for electron.
</tip>

<note>
Glutinum FSharp AST is good, but I'm not a fan of having to use fable at this step.
Will likely use the FSharp AST as inspiration for the 'glue' ast and then use fantomas for the source generation.
</note>
