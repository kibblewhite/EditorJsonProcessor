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
| `data_retrieval_mode` | `DataRetrievalMode` | `Embedded` | Controls map block rendering mode |
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
| `DataRetrievalMode` | `DataRetrievalMode` | `Embedded` | Controls map block rendering mode |
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
| `map` | `<div>` | Two rendering modes: Embedded or Reference (see below) |

### Supported Embed Services

Vimeo, YouTube, Coub, Facebook, Instagram, Twitter, Twitch (channel & video), Miro, Gfycat, Imgur, Vine, Aparat, CodePen, Pinterest, GitHub Gist, Yandex Music (album, track, playlist), and Google Maps.

## Map Block

The `map` block renders an interactive map container for the [editorjs-leaflet](https://byteloch-shared.gitlab.io/libraries/editorjs-leaflet/) plugin. The block data includes map configuration (centre, zoom, tile URL, height), GUID references to venues, spaces, typologies, and activities, and optionally resolved data arrays for embedded mode.

### DataRetrievalMode

Controls how `map` blocks render their data:

- **`Embedded`** -- Outputs resolved data as a `<script type="application/json">` child element inside the map div. The consuming application resolves all GUID references to full data objects before rendering. The client reads the self-contained JSON and renders immediately without further API calls.
- **`Reference`** -- Outputs `data-*` attributes with GUID references and configuration on the container element, deferring data resolution to client-side JavaScript. Block renderers may use the `Locale` to output a `data-locale` attribute for locale-aware API calls. When `Locale` is null, locale-dependent attributes are omitted.

### Embedded Mode Output

```html
<div id="block-abc123" data-block-type="map">
  <script type="application/json">
  {
    "center": { "lat": 51.505, "lng": -0.09 },
    "zoom": 16,
    "tileUrl": "/tiles/{z}/{x}/{y}.mvt",
    "height": 600,
    "venues": [{ "venueGuid": { "value": "..." }, "name": "...", ... }],
    "spaces": [{ "spaceGuid": { "value": "..." }, "name": "...", ... }],
    "pois": [{ "poiGuid": { "value": "..." }, "name": "...", "icon": "...", ... }],
    "activities": [{ "activityGuid": { "value": "..." }, "displayName": "...", ... }]
  }
  </script>
</div>
```

### Reference Mode Output

```html
<div id="block-abc123"
     data-block-type="map"
     data-locale="en-GB"
     data-center='{"lat":51.505,"lng":-0.09}'
     data-zoom="16"
     data-tile-url="/tiles/{z}/{x}/{y}.mvt"
     data-height="600"
     data-venue-guids="00000001-0000-0000-0000-000000000001,00000001-0000-0000-0000-000000000002"
     data-space-guids="00000002-0000-0000-0000-000000000001"
     data-typology-guids="00000003-0000-0000-0000-000000000001"
     data-activity-guids='[{"activityGuid":"00000004-0000-0000-0000-000000000001","spaceGuid":"00000002-0000-0000-0000-000000000001"}]'>
</div>
```

### GUID Validation

In reference mode, GUID strings are validated using `Guid.TryParse` before being output as `data-*` attributes. Invalid values (null, empty, whitespace, `Guid.Empty`) are filtered out. Activity GUID objects require both `activityGuid` and `spaceGuid` to be valid.

### Map Block Data Properties

| Property | Type | Description |
|---|---|---|
| `center` | `{ lat, lng }` | Map centre coordinates |
| `zoom` | `int` | Zoom level |
| `tileUrl` | `string` | Tile layer URL template |
| `height` | `int` | Container height in pixels |
| `venueGuids` | `string[]` | Venue GUID references |
| `spaceGuids` | `string[]` | Space GUID references |
| `typologyGuids` | `string[]` | Typology GUID references |
| `activityGuids` | `object[]` | Activity references (`{ activityGuid, spaceGuid }`) |
| `venues` | `object[]` | Resolved venue data (embedded mode) |
| `spaces` | `object[]` | Resolved space data (embedded mode) |
| `typologies` | `object[]` | Resolved typology data (embedded mode) |
| `pois` | `object[]` | Resolved POI data (embedded mode) |
| `activities` | `object[]` | Resolved activity data (embedded mode) |

## BlazorApp Demo

The solution includes a `BlazorApp.Client` / `BlazorApp.Server` project pair that demonstrates both rendering modes with interactive Leaflet maps:

- **`/embedded`** -- Map block with fully resolved data rendered via `EjsRenderFragment` in embedded mode. The JS map renderer reads the inline `<script type="application/json">` and creates the Leaflet map with venue polygons, space markers, POI emoji icons, and activity markers.
- **`/reference`** -- Map block rendered via `EjsRenderFragment` in reference mode. The JS map renderer reads `data-*` attributes and fetches venue/space/POI/activity data from mock API endpoints served by `BlazorApp.Server`.

Run with:

```bash
dotnet run --project BlazorApp.Server
```

The server hosts the WASM client, serves mock API endpoints (`/api/premises/...`, `/api/schedules/...`), and proxies tile requests to avoid CORS issues with the protomaps tile server.

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
    },
    {
        "type": "map",
        "style": "map-container"
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
