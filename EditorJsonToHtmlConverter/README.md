# EditorJsonToHtmlConverter

A .NET 10.0 Blazor library that converts [Editor.js](https://editorjs.io/) JSON output into HTML. It provides two rendering approaches:

- **EjsHtmlRenderer** -- Converts Editor.js JSON into a plain HTML string (or stripped plain text) for server-side use.
- **EjsRenderFragment** -- A Blazor component that renders Editor.js JSON as a `RenderFragment` within Razor pages.

## Getting Started

Register the required services in your DI container:

```csharp
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddScopedEditorJsonProcessorServices();
```

This registers both `Microsoft.AspNetCore.Components.Web.HtmlRenderer` and `EjsHtmlRenderer` as scoped services.

## Usage

### EjsHtmlRenderer (Server-Side)

Inject `HtmlRenderer` and construct `EjsHtmlRenderer` with the desired mode and locale:

```csharp
string editor_json = "{ ... }";

// Default: Embedded mode, no locale
EjsHtmlRenderer renderer = new(htmlRenderer);
string html = await renderer.ParseAsync(editor_json);

// Plain text (HTML tags stripped)
string plain_text = await renderer.ParseAsync(editor_json, strip_html: true);

// Custom styling
string styled_html = await renderer.ParseAsync(editor_json, styling_map: styling_json);

// Reference mode with locale — block renderers can use the locale for
// locale-aware output (e.g. data-locale attributes for client-side hydration)
CultureInfo locale = new("en-GB");
EjsHtmlRenderer reference_renderer = new(htmlRenderer, DataRetrievalMode.Reference, locale);
string reference_html = await reference_renderer.ParseAsync(editor_json);

// HtmlRootComponent
HtmlRootComponent root = await renderer.ParseAsHtmlRootComponentAsync(editor_json);
```

**Constructor parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `html_renderer` | `HtmlRenderer` | *(required)* | Blazor HtmlRenderer instance |
| `data_retrieval_mode` | `DataRetrievalMode` | `Embedded` | Controls leaflet-map rendering mode |
| `locale` | `CultureInfo?` | `null` | Locale for rendering. Available to block renderers for locale-aware output (e.g. `data-locale` attributes). Invalid cultures are silently ignored. |

### EjsRenderFragment (Razor Component)

```razor
<EjsRenderFragment Value="@editor_json" />

<!-- With optional styling, data retrieval mode, and locale -->
<EjsRenderFragment Value="@editor_json"
                   StylingMap="@styling_json"
                   DataRetrievalMode="DataRetrievalMode.Reference"
                   Locale="@(new CultureInfo("en-GB"))" />
```

**Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Value` | `string` | *(required)* | Editor.js JSON output |
| `StylingMap` | `string` | `"[]"` | JSON array defining CSS class mappings per block type |
| `DataRetrievalMode` | `DataRetrievalMode` | `Embedded` | Controls leaflet-map rendering mode |
| `Locale` | `CultureInfo?` | `null` | Locale for rendering. Available to block renderers for locale-aware output. |

## Supported Block Types

| Block Type | HTML Output | Key Features |
|---|---|---|
| `paragraph` | `<p>` | Rich text with markup content |
| `header` | `<h1>` -- `<h6>` | Level-based heading (1--6) |
| `list` | `<ol>` / `<ul>` | Ordered/unordered with nested list support |
| `quote` | `<blockquote>` | Text, caption, alignment (left/center/right) |
| `checklist` | `<ul>` with checkboxes | Disabled checkboxes with checked state |
| `table` | `<table>` | Optional header row (`withHeadings`), cell markup |
| `image` | `<img>` | URL, caption, border/background/stretched flags |
| `delimiter` | `<hr>` | Horizontal rule |
| `warning` | `<div>` | Bold title and message paragraph |
| `embed` | `<iframe>` / service-specific | 20 embed services (see below) |
| `text` | Raw markup | Direct content without a wrapper element |
| `leaflet-map` | `<div>` | Two rendering modes: Embedded or Reference |

### Supported Embed Services

Vimeo, YouTube, Coub, Facebook, Instagram, Twitter, Twitch (channel & video), Miro, Gfycat, Imgur, Vine, Aparat, CodePen, Pinterest, GitHub Gist, Yandex Music (album, track, playlist), and Google Maps.

## DataRetrievalMode

Controls how `leaflet-map` blocks render their data:

- **`Embedded`** -- Outputs resolved data as a `<script type="application/json">` child element inside the map div. The consuming application resolves all GUID references to full data objects before rendering. The client reads the self-contained JSON and renders immediately without further API calls.
- **`Reference`** -- Outputs `data-*` attributes with GUID references and configuration on the container element, deferring data resolution to client-side JavaScript. Block renderers may use the `Locale` to output a `data-locale` attribute for locale-aware API calls. When `Locale` is null, locale-dependent attributes are omitted.

## CSS Styling Map

The styling map is a JSON array that maps CSS classes to block types. Styles are matched with the following priority (first match wins):

1. Type + Level + ID (most specific)
2. Type + Level
3. Type + ID
4. Type only (fallback)

### Styling Map Sample

```json
[
    {
        "type": "header",
        "level": 1,
        "style": "specific-style",
        "id": "KgrM3aNM-n"
    },
    {
        "type": "header",
        "level": 3,
        "style": "general-style"
    },
    {
        "type": "paragraph",
        "style": "specific-style",
        "id": "NaTtEbbeRT"
    },
    {
        "type": "paragraph",
        "style": "general-style"
    },
    {
        "type": "list",
        "style": "list-group list-group-flush",
        "item-style": "list-group-item"
    },
    {
        "type": "checklist",
        "style": "list-group",
        "item-style": "list-group-item"
    },
    {
        "type": "quote",
        "style": "blockquote",
        "footer-style": "blockquote-footer"
    },
    {
        "type": "table",
        "style": "table table-hover"
    },
    {
        "type": "table",
        "style": "table table-striped",
        "id": "zOGIbPv7kl"
    },
    {
        "type": "image",
        "style": "img-fluid"
    }
]
```

Additional style properties per block type:
- **list / checklist**: `item-style` for individual list items
- **quote**: `footer-style` for the caption/footer element

## Extension Methods

`EditorJsBlocksExtensions` provides helpers for programmatic block construction:

```csharp
// Empty Editor.js JSON string
string empty = EditorJsBlocksExtensions.EmptyEditorJsString;

// Fluent block building
EditorJsBlocks blocks = EditorJsBlocks.Empty
    .AddBlock(new EditorJsBlock { Id = "1", Type = "paragraph", Data = ... })
    .AddEmptyBlock();
```

## Caveat

All blocks **must** include an `id` field. Editor.js sometimes omits `id` on certain block types (e.g., delimiter). The following will fail to deserialise:

```json
{
    "blocks": [
        {
            "type": "delimiter",
            "data": {}
        }
    ]
}
```

Ensure every block has an `id` before passing JSON to the converter.

## Editor JSON Sample

```json
{
    "time": 1707325917682,
    "blocks": [
    {"id": "uM3Adn6C9n", "data": {"text": "!!! <i>He</i>y<b>lo, W</b>orld! <a href=\"https://google.com\">Link</a> !!!", "wrap": "title"}, "type": "text"},
    {
        "id": "KgrM3aNM-n",
        "type": "header",
        "data": {
        "text": "<mark class=\"cdx-marker\"><a href=\"http://google.com\">Heylo</a></mark>",
        "level": 1
        }
    },
    {
        "id": "NaTtEbbeRT",
        "type": "paragraph",
        "data": {
        "text": "Heylo World"
        }
    },
    {
        "id": "KgrM3aNM-n",
        "type": "header",
        "data": {
        "text": "Second header",
        "level": 3
        }
    },
    {
        "id": "QdqCFpKBAm",
        "type": "list",
        "data": {
        "style": "ordered",
        "items": [
            {
            "content": "A: One",
            "items": [
                {
                "content": "B: Two",
                "items": []
                }
            ]
            },
            {
            "content": "A: Three",
            "items": [
                {
                "content": "B: Four",
                "items": [
                    {
                    "content": "C: Five",
                    "items": []
                    }
                ]
                },
                {
                "content": "B: Six",
                "items": []
                },
                {
                "content": "B: Seven",
                "items": []
                }
            ]
            }
        ]
        }
    },
    {
        "id": "m-onbmz6BZ",
        "type": "quote",
        "data": {
        "text": "Ohhh interesting...",
        "caption": "by Me!",
        "alignment": "left"
        }
    },
    {
        "id": "ZatOSzA754",
        "type": "paragraph",
        "data": {
        "text": "dsf<i>sfa</i><b>sfasdfs</b>dffasd"
        }
    },
    {
        "id": "SWrBNzvp6A",
        "type": "list",
        "data": {
        "style": "unordered",
        "items": [
            {
            "content": "dwdw",
            "items": []
            },
            {
            "content": "wedwed",
            "items": []
            },
            {
            "content": "wedw",
            "items": []
            }
        ]
        }
    },

    {
        "id": "yD5ZHUxF1N",
        "type": "checklist",
        "data": {
        "items": [
            {
            "text": "Check List Item One",
            "checked": false
            },
            {
            "text": "Check List Item Two",
            "checked": true
            },
            {
            "text": "Check List Item Three",
            "checked": false
            }
        ]
        }
    },
    {
        "id": "J5I_aD9c8j",
        "type": "delimiter",
        "data": {}
    },
    {
        "id": "J-7FqxXppm",
        "type": "table",
        "data": {
        "withHeadings": true,
        "content": [
            [
            "Header 1",
            "Header 2",
            "Header 3"
            ],
            [
            "qwerty",
            "as<b>dfg</b>h",
            "zxc<mark class=\"cdx-marker\">vbn</mark>"
            ],
            [
            "AAA",
            "<a href=\"https://google.com/\">BBB</a>",
            "<code class=\"inline-code\">CCC</code>"
            ]
        ]
        }
    },
    {
        "id": "zOGIbPv7kl",
        "type": "table",
        "data": {
        "withHeadings": false,
        "content": [
            [
            "A1",
            "B1"
            ],
            [
            "A2",
            "B2"
            ]
        ]
        }
    },
    {
        "id": "zOGADPv7kl",
        "type": "image",
        "data": {
        "url": "https://www.tesla.com/tesla_theme/assets/img/_vehicle_redesign/roadster_and_semi/roadster/hero.jpg",
        "caption": "Roadster // tesla.com",
        "withBorder": true,
        "withBackground": false,
        "stretched": true
        }
    },
    {
        "id": "zOGFDv7kl",
        "type" : "delimiter",
        "data" : {}
    },
    {
        "id": "zOUIDPv7kl",
        "type": "warning",
        "data": {
        "title": "Note:",
        "message": "Avoid using this method just for lulz. It can be very dangerous opposite your daily fun stuff."
        }
    },
    {
        "id": "zOJKDPv7kl",
        "type" : "delimiter",
        "data" : {}
    },
    {
        "id": "zUKNDPv7kl",
        "type" : "embed",
        "data" : {
        "service" : "coub",
        "source" : "https://coub.com/view/1czcdf",
        "embed" : "https://coub.com/embed/1czcdf",
        "width" : 580,
        "height" : 320,
        "caption" : "My Life"
        }
    }
    ],
    "version": "2.28.2"
}
```


Notes:

`_content/EditorJsonToHtmlConverter/EditorJsonToHtmlConverter.EjsRenderFragment.razor.js`

For the component to self-initialise, update the `EjsRenderFragment.razor.cs`

```csharp
[Inject] private IJSRuntime JS { get; set; }
private IJSObjectReference? _jsModule;

protected override async Task OnAfterRenderAsync(bool firstRender)
{ 
    if (firstRender)
    {
        _jsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./_content/EditorJsonToHtmlConverter/EditorJsonToHtmlConv  erter.EjsRenderFragment.razor.js");
    }
}
```
