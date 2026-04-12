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
EjsHtmlRenderer reference_renderer = new(htmlRenderer, DataRetrievalMode.Reference, locale: locale);
string reference_html = await reference_renderer.ParseAsync(editor_json);

// Reference mode with a render-completed callback for correlation/timing
EjsHtmlRenderer instrumented_renderer = new(
    htmlRenderer,
    DataRetrievalMode.Reference,
    on_render_completed: args =>
    {
        logger.LogInformation("Render completed (correlation: {Id}) in {Ms} ms",
            args.CorrelationIdentifier, args.Elapsed.TotalMilliseconds);
        return Task.CompletedTask;
    },
    locale: locale);
string instrumented_html = await instrumented_renderer.ParseAsync(editor_json, correlation_identifier: Guid.CreateVersion7());

// HtmlRootComponent
HtmlRootComponent root = await renderer.ParseAsHtmlRootComponentAsync(editor_json);
```

**Constructor parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `html_renderer` | `HtmlRenderer` | *(required)* | Blazor HtmlRenderer instance |
| `data_retrieval_mode` | `DataRetrievalMode` | `Embedded` | Controls map block rendering mode |
| `on_render_completed` | `Func<EjsRenderCompletedEventArgs, Task>?` | `null` | Async callback invoked after each successful parse. Receives the caller-supplied correlation identifier and the wall-clock render duration. |
| `locale` | `CultureInfo?` | `null` | Locale for rendering. Available to block renderers for locale-aware output (e.g. `data-locale` attributes). Invalid cultures are silently ignored. |

**`ParseAsync` parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `value` | `string` | *(required)* | Editor.js JSON output to convert |
| `correlation_identifier` | `Guid` | `Guid.Empty` | Optional identifier echoed back on the `on_render_completed` callback so a render can be correlated with an outer request or element. |
| `strip_html` | `bool` | `false` | When `true`, strips HTML tags from the result and returns plain text. |
| `styling_map` | `string?` | `"[]"` | JSON array defining CSS class mappings per block type. |

### EjsRenderFragment (Razor Component)

```razor
<EjsRenderFragment Value="@editor_json"
                   StylingMap="[]"
                   DataRetrievalMode="DataRetrievalMode.Embedded">
    <span>Loading...</span>
</EjsRenderFragment>

<!-- With locale, correlation identifier, and render-completed callback -->
<EjsRenderFragment Value="@editor_json"
                   StylingMap="@styling_json"
                   DataRetrievalMode="DataRetrievalMode.Reference"
                   Locale="@(new CultureInfo("en-GB"))"
                   CorrelationIdentifier="@_correlation_identifier"
                   RenderCompleted="OnRenderCompletedAsync">
    <span>Loading...</span>
</EjsRenderFragment>
```

`ChildContent` is shown as a placeholder until the JSON has been parsed and the converted block markup is ready to replace it. Once the fragment has been built, parent re-renders will not revert back to the placeholder — the built content is held in component-private state.

**Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Value` | `string` | *(required)* | Editor.js JSON output |
| `StylingMap` | `string` | *(required)* | JSON array defining CSS class mappings per block type. Pass `"[]"` for no custom styling. |
| `DataRetrievalMode` | `DataRetrievalMode` | *(required)* | Controls map block rendering mode |
| `ChildContent` | `RenderFragment` | *(required)* | Placeholder shown until the JSON has been parsed and the converted block markup replaces it (typically a "Loading..." element). |
| `Locale` | `CultureInfo?` | `null` | Locale for rendering. Available to block renderers for locale-aware output. |
| `CorrelationIdentifier` | `Guid` | `Guid.Empty` | Optional caller-supplied identifier echoed back on `RenderCompleted`. Useful for correlating a render with an outer request or element. **Generate once per component instance** — do not bind `@Guid.CreateVersion7()` inline in the markup, as that produces a new value on every parent re-render, which causes Blazor to push a parameter change into the component on every parent render. |
| `RenderCompleted` | `EventCallback<EjsRenderCompletedEventArgs>` | *(none)* | Fires once after the first successful build of the render fragment. Receives the correlation identifier and the wall-clock time taken. |

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

### Client-Side Hydration

Hydrating rendered map containers in the browser requires two tags — a stylesheet `<link>` for the shared marker styling and a `<script>` for the [editorjs-leaflet](https://byteloch-shared.gitlab.io/libraries/editorjs-leaflet/) viewer. The viewer handles discovery and rendering automatically from there:

```html
<link rel="stylesheet" href="//byteloch-shared.gitlab.io/libraries/editorjs-leaflet/dist/viewer/0.0.12/leaflet-map-viewer.min.css" />
<script src="//byteloch-shared.gitlab.io/libraries/editorjs-leaflet/dist/viewer/0.0.12/leaflet-map-viewer.min.js" data-api-base="/api"></script>
```

- **The CSS file** ships the POI icon reset (`.ce-leaflet-map-poi-icon`) so emoji markers render cleanly without borders or backgrounds. Load it in `<head>` alongside any other stylesheets — overriding is straightforward via the normal CSS cascade from a later stylesheet.
- **On page load** the viewer scans for existing `[data-block-type="map"]` elements and hydrates them.
- **A `MutationObserver`** watches the document for containers added after initial load, so the viewer works transparently with Blazor WASM, Blazor Server, React, Vue, and any other framework that mounts content dynamically. No lifecycle hooks or interop calls are required from the consumer.
- **Discovery is idempotent** — each container is marked with `data-map-initialised` after hydration, so repeated scans are safe.
- **Do not** call `JSRuntime.InvokeVoidAsync("MapViewer.initialise")` from `OnAfterRenderAsync` (or equivalent). It is redundant with the observer and can race the script's load, producing cryptic `'MapViewer' was undefined` errors. Omit the interop call entirely and let the observer do its job.

The `data-api-base` attribute tells the viewer where to fetch venue/space/POI/activity data for reference-mode containers. **Keep the CSS and JS version numbers in sync** — bump both when upgrading.

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

- **`/embedded`** -- Map block with fully resolved data rendered via `EjsRenderFragment` in embedded mode. The viewer reads the inline `<script type="application/json">` and hydrates the container with venue polygons, space markers, POI emoji icons, and activity markers.
- **`/reference`** -- Map block rendered via `EjsRenderFragment` in reference mode. The viewer reads the `data-*` attributes and fetches venue/space/POI/activity data from mock API endpoints served by `BlazorApp.Server`.

Neither page invokes the viewer from Blazor — the `leaflet-map-viewer.min.js` script tag in `wwwroot/index.html` is the only integration point, and the viewer's `MutationObserver` picks up the containers as Blazor renders them.

Run with:

```bash
dotnet run --project BlazorApp.Server
```

The server hosts the WASM client, proxies tile requests to avoid CORS issues with the protomaps tile server, and serves the mock API endpoints that reference-mode hydration reads from:

- `GET /api/venue-details/{guid}/locale/{locale}`
- `GET /api/space-details/{guid}/locale/{locale}`
- `GET /api/typologies/locale/{locale}`
- `POST /api/pois-by-typology-guids/locale/{locale}`
- `POST /api/activities-by-activity-guids/locale/{locale}`

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
