# EditorJsonToHtmlConverter

[![NuGet](https://img.shields.io/nuget/v/EditorJsonToHtmlConverter.svg)](https://www.nuget.org/packages/EditorJsonToHtmlConverter)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/kibblewhite/EditorJsonProcessor/blob/main/EditorJsonToHtmlConverter/LICENSE)

A .NET 10 library that turns [Editor.js](https://editorjs.io/) JSON into HTML, either as a string on the server or as a Blazor component in a Razor page.

- **`EjsHtmlRenderer`** — renders Editor.js JSON to an HTML string (or stripped plain text) for server-side use: emails, feeds, static pages, search indexing.
- **`EjsRenderFragment`** — a Blazor component that renders Editor.js JSON in place in a Razor page.

Both share the same twelve block renderers, an optional CSS class map for styling each block type, and machine-readable documentation of every block that can be read at runtime — for example, to teach a language model how to author valid Editor.js JSON.

## Contents

- [Installation](#installation)
- [Quick start](#quick-start)
- [Input requirements](#input-requirements)
- [Supported blocks](#supported-blocks)
- [Compatible Editor.js tools](#compatible-editorjs-tools)
- [Embed services](#embed-services)
- [Styling map](#styling-map)
- [Map block](#map-block)
- [API reference](#api-reference)
- [Block documentation at runtime](#block-documentation-at-runtime)
- [Demo application](#demo-application)
- [Adding a block renderer](#adding-a-block-renderer)
- [License](#license)

## Installation

```bash
dotnet add package EditorJsonToHtmlConverter
```

Requires .NET 10.

## Quick start

Register the services once:

```csharp
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddScopedEditorJsonProcessorServices();
```

This registers `Microsoft.AspNetCore.Components.Web.HtmlRenderer` and `EjsHtmlRenderer` as scoped services.

Render to a string:

```csharp
public sealed class ArticleService(EjsHtmlRenderer renderer)
{
    public Task<string> ToHtmlAsync(string editor_json) => renderer.ParseAsync(editor_json);

    public Task<string> ToPlainTextAsync(string editor_json) => renderer.ParseAsync(editor_json, strip_html: true);
}
```

Or render in a Razor page, with `ChildContent` shown until the markup is ready:

```razor
<EjsRenderFragment Value="@editor_json" StylingMap="[]" DataRetrievalMode="DataRetrievalMode.Embedded">
    <span>Loading...</span>
</EjsRenderFragment>
```

## Input requirements

The input is a complete Editor.js document:

```json
{
  "time": 1727136000000,
  "blocks": [
    { "id": "b1c2d3e4f5", "type": "paragraph", "data": { "text": "Doors open at <b>7pm</b>." } }
  ],
  "version": "2.31.5"
}
```

- The envelope's `time` (a number), `blocks` and `version` (a string) are all required.
- Every block needs an `id`, a `type` and a `data` object. Current Editor.js versions write an `id` on every block, but JSON from older versions, or written by hand, may lack one and must be given one before rendering.
- Field types must match: a header's `level` is a JSON number, so `"level": "2"` is invalid.

**Error handling.** A document that breaks these rules fails to deserialise; the error is logged and rethrown, and nothing is rendered. A block whose `type` has no renderer is skipped silently. A styling map that is not valid JSON is logged and ignored. Blocks render in array order.

## Supported blocks

Each block's full contract — when to choose it, every `data` field it reads, what happens when a field is absent, and a complete example — is in the renderer's XML documentation (see [Block documentation at runtime](#block-documentation-at-runtime)). That documentation is authoritative; this table is a summary.

| `type` | `data` fields read | HTML output |
|---|---|---|
| `paragraph` | `text` | `<p>` |
| `header` | `text`, `level` (1–6; otherwise 2) | `<h1>` – `<h6>` |
| `list` | `style` (`"ordered"` or bulleted), `items[]` of `{ content, items[] }` — nests to any depth | `<ol>` / `<ul>` |
| `checklist` | `items[]` of `{ text, checked }` | `<ul>` of disabled checkboxes |
| `quote` | `text`, `caption`, `alignment` | `<blockquote>` with a `<footer>` caption |
| `table` | `content` (rows of cell strings), `withHeadings` | `<table>`, with `<thead>` when `withHeadings` is true; nothing when there are no rows |
| `image` | `url`, `caption`, `withBorder`, `withBackground`, `stretched` | `<img>`, with the caption as `alt` text and as a line beneath |
| `delimiter` | — | `<hr>` |
| `warning` | `title`, `message` | `<div>` with a `<strong>` title and a `<p>` message |
| `embed` | `service`, `source`, `width`, `height`, `caption` | the provider's `<iframe>` (see [Embed services](#embed-services)) |
| `text` | `text` | the inline markup itself, with no wrapper element |
| `map` | see [Map block](#map-block) | `<div data-block-type="map">` |

Note the field names: a **list** item's text is in `content`, while a **checklist** item's text is in `text`. Text-bearing fields keep inline HTML such as `<b>`, `<i>`, `<a href>` and `<mark>`.

## Compatible Editor.js tools

A block's `type` is the key its tool is registered under in the Editor.js `tools` configuration, so each tool must be registered under the name in the first column. These are the tools and versions the documented block contracts were verified against, by loading each documented example into the tool and checking that its `save()` output keeps every field:

| `type` | Editor.js tool | Verified version |
|---|---|---|
| `paragraph` | [`@editorjs/paragraph`](https://github.com/editor-js/paragraph) (built into Editor.js) | 2.11.7 |
| `header` | [`@editorjs/header`](https://github.com/editor-js/header) | 2.8.8 |
| `list` | [`@editorjs/nested-list`](https://github.com/editor-js/nested-list) | 1.4.3 |
| `checklist` | [`@editorjs/checklist`](https://github.com/editor-js/checklist) | 1.6.0 |
| `quote` | [`@editorjs/quote`](https://github.com/editor-js/quote) | 2.7.6 |
| `table` | [`@editorjs/table`](https://github.com/editor-js/table) | 2.4.5 |
| `image` | [`@editorjs/simple-image`](https://github.com/editor-js/simple-image) | 1.6.0 |
| `delimiter` | [`@editorjs/delimiter`](https://github.com/editor-js/delimiter) | 1.4.2 |
| `warning` | [`@editorjs/warning`](https://github.com/editor-js/warning) | 1.4.1 |
| `embed` | [`@editorjs/embed`](https://github.com/editor-js/embed) | 2.8.0 |
| `text` | [`editorjs-text`](https://github.com/kibblewhite/editorjs-text) | 1.0.3 |
| `map` | [`editorjs-leaflet`](https://byteloch-shared.gitlab.io/libraries/editorjs-leaflet/) | 0.0.15 |

Verified with Editor.js 2.31.5. Things to know when choosing tools:

- **`@editorjs/list` 2.x** saves the same `content` / `items` shape, so its ordered and unordered lists render; its `"checklist"` style renders as a plain bulleted list, and its `meta` (start number, counter type) is not read. Use the `checklist` block for tick boxes.
- **`@editorjs/image`** stores its URL at `data.file.url` rather than `data.url`, so it is not supported; use `@editorjs/simple-image`.
- The editor tools are stricter than the renderer in places, and a document authored outside the editor should respect them or it will change the next time it is saved: blank paragraphs, headings and checklist items are discarded; a quote's `alignment` is only `left` or `center`; every table row must have as many cells as the first; checklists cannot be nested. Each block's XML documentation lists these.

## Embed services

The `embed` block builds the provider's frame itself from `source` — the original page URL, such as `https://www.youtube.com/watch?v=…` — so `source` is required. `embed`, the provider's embed URL, is what `@editorjs/embed` uses for its preview and is not read by the renderer. Supply `width` and `height`: `youtube` falls back to 560 × 315, and `pinterest`, `github` and the Yandex.Music services use a fixed size, but every other service draws a frame with no height when they are absent.

| `service` | Notes |
|---|---|
| `youtube`, `vimeo`, `coub`, `facebook`, `instagram`, `twitter`, `twitch-channel`, `twitch-video`, `miro`, `gfycat`, `imgur`, `vine`, `aparat`, `codepen`, `pinterest`, `github`, `yandex-music-album`, `yandex-music-track`, `yandex-music-playlist` | The `@editorjs/embed` service keys. |
| `gist.github`, `music.yandex.album`, `music.yandex.track`, `music.yandex.playlist` | Older names for the gist and Yandex.Music services, still rendered for documents saved with them. |
| `google-maps` | Renders a Google Maps embed URL (`…/maps/embed?pb=…`) given as `source`. `@editorjs/embed` has no such service, so such a block cannot be edited with it. |

Any other `service` value renders an empty container. `@editorjs/embed`'s `reddit`, `whimsical` and `figma` services have no renderer.

## Styling map

The styling map is a JSON array assigning CSS classes to rendered blocks. Pass `"[]"` for none.

```json
[
    { "type": "paragraph", "style": "lead" },
    { "type": "paragraph", "style": "lead fw-bold", "id": "NaTtEbbeRT" },
    { "type": "header", "level": 2, "style": "h4 mt-4" },
    { "type": "header", "level": 1, "style": "display-6", "id": "KgrM3aNM-n" },
    { "type": "list", "style": "list-group list-group-flush", "item-style": "list-group-item" },
    { "type": "checklist", "style": "list-group", "item-style": "list-group-item" },
    { "type": "quote", "style": "blockquote", "footer-style": "blockquote-footer" },
    { "type": "table", "style": "table table-hover" },
    { "type": "image", "style": "img-fluid" },
    { "type": "map", "style": "map-container" }
]
```

| Property | Applies to | Meaning |
|---|---|---|
| `type` | all | The block type the entry styles (required). |
| `style` | all | Classes for the block's element (required). |
| `id` | all | Restricts the entry to the block with this `id`. |
| `level` | `header` | The heading level the entry styles. Header entries must give one — a header entry without `level` never matches. |
| `item-style` | `list`, `checklist` | Classes for each top-level `<li>`. |
| `footer-style` | `quote` | Classes for the caption `<footer>`. |

For each block, an entry naming its `id` wins over one that does not; header entries must also match the block's `level`. The `text` block has no element of its own and is not styled.

## Map block

The `map` block renders a container for the [editorjs-leaflet](https://byteloch-shared.gitlab.io/libraries/editorjs-leaflet/) viewer. The block does not describe geography itself: it references venue, space, typology and activity records by identifier, and the consuming application supplies those records — either resolved ahead of rendering or fetched by the viewer. The records' shape is defined by the viewer, not by this library.

### Block data

| Property | Type | Description |
|---|---|---|
| `center` | `{ lat, lng }` | Starting map centre. |
| `zoom` | `int` | Starting zoom level. |
| `height` | `int` | Container height in pixels. |
| `venueGuids` | `string[]` | Venue identifiers. |
| `spaceGuids` | `string[]` | Space identifiers. |
| `typologyGuids` | `string[]` | Typology identifiers. |
| `activityGuids` | `{ activityGuid, spaceGuid }[]` | Activity references, each with the space captured when the block was saved. |
| `venues`, `spaces`, `typologies`, `pois`, `activities` | `object[]` | Resolved records, supplied by the consuming application for embedded mode. |

### Tile URL

The tile URL is configuration, not block data: `editorjs-leaflet` 0.0.15 and later no longer save a `tileUrl` on the block. Configure it once:

```csharp
builder.Services.AddScopedEditorJsonProcessorServices(options =>
{
    // Standard Leaflet placeholders: {z}, {x}, {y}
    options.TileUrlTemplate = "https://your-cdn.example/tiles/{z}/{x}/{y}.mvt";
});
```

The configured value reaches every map block, whether `EjsHtmlRenderer` is resolved from DI or constructed manually, and when `EjsRenderFragment` is used directly in a page. An explicit `tile_url_template` constructor argument on `EjsHtmlRenderer`, or `TileUrlTemplate` parameter on `EjsRenderFragment`, wins over the configured value. When neither supplies one, the tile URL is omitted and the viewer reports it missing.

A `tileUrl` on previously saved JSON is ignored, so a stale address in old content never reaches the page. `EditorJsBlockData.TileUrl` is `[Obsolete]` and kept only so that older JSON still deserialises.

### Rendering modes

`DataRetrievalMode` controls what a map block writes.

**`Embedded`** — the consuming application resolves every identifier before rendering, and the whole payload, plus the injected `tileUrl`, is written into a child `<script type="application/json">`. The viewer draws the map immediately with no further requests.

```html
<div id="m1a2b3c4d5" data-block-type="map">
  <script type="application/json">
    {"center":{"lat":51.505,"lng":-0.09},"zoom":16,"tileUrl":"/tiles/{z}/{x}/{y}.mvt","height":600,"venueGuids":[...],"venues":[...],"spaces":[...],"pois":[...],"activities":[...]}
  </script>
</div>
```

**`Reference`** — the identifiers and configuration are written as `data-*` attributes, and the viewer fetches the records itself, using `data-locale` when a `Locale` is supplied. Identifiers that are empty, malformed or all-zero are dropped, and an activity reference is dropped unless both of its identifiers are valid.

```html
<div id="m1a2b3c4d5"
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

### Client-side viewer

The renderer writes no `<script>` or `<link>` tags. Add the viewer's stylesheet and script to the page once:

```html
<link rel="stylesheet" href="https://byteloch-shared.gitlab.io/libraries/editorjs-leaflet/dist/viewer/0.0.15/leaflet-map-viewer.min.css" />
<script src="https://byteloch-shared.gitlab.io/libraries/editorjs-leaflet/dist/viewer/0.0.15/leaflet-map-viewer.min.js" data-api-base="/api"></script>
```

- On load the viewer finds every `[data-block-type="map"]` container and draws it; a `MutationObserver` picks up containers added later, so it works unchanged with Blazor Server, Blazor WebAssembly and other frameworks that render dynamically.
- Each container is marked `data-map-initialised` once drawn, so repeated discovery is safe.
- Do not also call the viewer from `OnAfterRenderAsync` or other JavaScript interop: it is redundant with the observer and can run before the script has loaded.
- `data-api-base` is where the viewer fetches records for reference-mode containers.
- Keep the stylesheet and script at the same version.

## API reference

### `AddScopedEditorJsonProcessorServices`

| Overload | Description |
|---|---|
| `AddScopedEditorJsonProcessorServices()` | Registers `HtmlRenderer` and `EjsHtmlRenderer` as scoped services. |
| `AddScopedEditorJsonProcessorServices(Action<EditorJsonProcessorOptions>? configure)` | As above, and configures `EditorJsonProcessorOptions` (currently `TileUrlTemplate`). |

### `EjsHtmlRenderer`

```csharp
// Resolved from DI, or constructed directly:
EjsHtmlRenderer renderer = new(html_renderer);
string html = await renderer.ParseAsync(editor_json);
string text = await renderer.ParseAsync(editor_json, strip_html: true);
string styled = await renderer.ParseAsync(editor_json, styling_map: styling_json);
HtmlRootComponent root = await renderer.ParseAsHtmlRootComponentAsync(editor_json);

// Reference mode with a locale, an explicit tile URL and a completion callback:
EjsHtmlRenderer reference_renderer = new(
    html_renderer,
    DataRetrievalMode.Reference,
    on_render_completed: args =>
    {
        logger.LogInformation("Render {Id} took {Ms} ms", args.CorrelationIdentifier, args.Elapsed.TotalMilliseconds);
        return Task.CompletedTask;
    },
    locale: new CultureInfo("en-GB"),
    tile_url_template: "https://your-cdn.example/tiles/{z}/{x}/{y}.mvt");
string reference_html = await reference_renderer.ParseAsync(editor_json, correlation_identifier: Guid.CreateVersion7());
```

**Constructor**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `html_renderer` | `HtmlRenderer` | *(required)* | The Blazor `HtmlRenderer` used to render the component. |
| `data_retrieval_mode` | `DataRetrievalMode` | `Embedded` | How map blocks are rendered. |
| `on_render_completed` | `Func<EjsRenderCompletedEventArgs, Task>?` | `null` | Called after each parse with the correlation identifier and the elapsed time. |
| `locale` | `CultureInfo?` | `null` | Made available to block renderers; map blocks write it as `data-locale` in reference mode. |
| `tile_url_template` | `string?` | `null` | Map tile URL template; overrides `EditorJsonProcessorOptions.TileUrlTemplate`. |

**Methods**

| Method | Description |
|---|---|
| `ParseAsync(string value, Guid correlation_identifier = default, bool strip_html = false, string? styling_map = "[]")` | Renders to an HTML string. With `strip_html`, tags are removed and entities decoded to give plain text. |
| `ParseAsHtmlRootComponentAsync(string value, string? styling_map = "[]", Guid correlation_identifier = default)` | Renders to an `HtmlRootComponent`. |

### `EjsRenderFragment`

```razor
<EjsRenderFragment Value="@editor_json"
                   StylingMap="@styling_json"
                   DataRetrievalMode="DataRetrievalMode.Reference"
                   Locale="@(new CultureInfo("en-GB"))"
                   TileUrlTemplate="https://your-cdn.example/tiles/{z}/{x}/{y}.mvt"
                   CorrelationIdentifier="@_correlation_identifier"
                   RenderCompleted="OnRenderCompletedAsync">
    <span>Loading...</span>
</EjsRenderFragment>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Value` | `string` | *(required)* | The Editor.js JSON. Nothing is rendered, and `ChildContent` stays, while it is empty. |
| `StylingMap` | `string` | *(required)* | The [styling map](#styling-map); `"[]"` for none. |
| `DataRetrievalMode` | `DataRetrievalMode` | *(required)* | How map blocks are rendered. |
| `ChildContent` | `RenderFragment` | *(required)* | Placeholder shown until the rendered blocks replace it. |
| `Locale` | `CultureInfo?` | `null` | As for `EjsHtmlRenderer`. |
| `TileUrlTemplate` | `string?` | `null` | Map tile URL template; overrides `EditorJsonProcessorOptions.TileUrlTemplate`. |
| `CorrelationIdentifier` | `Guid` | `Guid.Empty` | Echoed back on `RenderCompleted`. Create it once per component instance — binding `@Guid.CreateVersion7()` inline gives a new value, and a parameter change, on every parent render. |
| `RenderCompleted` | `EventCallback<EjsRenderCompletedEventArgs>` | — | Fires once, after the first successful render, with the correlation identifier and the elapsed time. |

The component builds its content once, from the first non-empty `Value`. After that, parent re-renders neither rebuild it nor bring the placeholder back, and a later change to `Value` is not picked up — to show a different document, render a new component instance (for example with a `@key` tied to the document).

### `EditorJsBlocksExtensions`

Helpers for building documents in code:

```csharp
string empty_document = EditorJsBlocksExtensions.EmptyEditorJsString;  // {"time":0,"blocks":[],"version":"0.0.0"}
JsonObject empty_object = EditorJsBlocksExtensions.EmptyEditorJsObject;

EditorJsBlocks document = EditorJsBlocks.Empty
    .AddBlock(new EditorJsBlock { Id = "b1c2d3e4f5", Type = "paragraph", Data = new EditorJsBlockData { Text = "Hello" } });
```

## Block documentation at runtime

Every renderer carries XML documentation for its block — a one-sentence summary, when to choose the block, every `data` field it reads and what happens when one is absent, and a complete example block. Each renderer also declares the block type it serves through the static `IBlockRenderer.BlockType` property, so the supported blocks can be discovered by reflecting over `IBlockRenderer` implementations rather than kept in a separate list.

That documentation is useful beyond IntelliSense — for example, to describe the supported blocks to a language model or other tooling that authors Editor.js JSON. To read it at runtime, the XML file has to be beside your application, and NuGet never copies package documentation into build output on its own. Opt in with:

```xml
<PropertyGroup>
    <IncludeEditorJsDocumentationFile>true</IncludeEditorJsDocumentationFile>
</PropertyGroup>
```

`EditorJsonToHtmlConverter.xml` is then copied to the build and publish output. Set the property on the project that produces the executable; the copy also flows through a library that references this package, so the application needs no direct reference of its own.

This works in container builds too. The official `dotnet/sdk` images set `NUGET_XMLDOC_MODE=skip`, which discards the documentation file that normally sits beside the assembly, so the package carries a second copy that restore leaves in place.

## Demo application

The repository's `BlazorApp.Server` / `BlazorApp.Client` pair demonstrates both map rendering modes against mock data:

- **`/embedded`** — a map block with fully resolved data, rendered by `EjsRenderFragment` in embedded mode.
- **`/reference`** — a map block in reference mode; the viewer fetches venue, space, point-of-interest and activity data from mock endpoints served by `BlazorApp.Server`.

Neither page calls the viewer from Blazor; the viewer's script tag in `wwwroot/index.html` is the only integration point.

```bash
dotnet run --project BlazorApp.Server
```

The server hosts the WebAssembly client, proxies tile requests to avoid cross-origin issues, and serves the mock endpoints the reference-mode viewer reads:

- `GET /api/venue-details/{guid}/locale/{locale}`
- `GET /api/space-details/{guid}/locale/{locale}`
- `GET /api/typologies/locale/{locale}`
- `POST /api/pois-by-typology-guids/locale/{locale}`
- `POST /api/activities-by-activity-guids/locale/{locale}`

## Adding a block renderer

1. Add a member to `SupportedRenderers`. A block's `type` string is matched to it case-insensitively.
2. Add a `public sealed class Render{Type} : IBlockRenderer` under `EditorJsonToHtmlConverter/Renderers/`, declaring `BlockType` and implementing `Render`.
3. Document the class to the contract on `IBlockRenderer`: a one-sentence `<summary>`; `<remarks>` covering when to choose the block and every `data` field it reads; and an `<example>` holding one complete, valid block, including its `id`. Use `<para>` and `<c>` for structure. Check the example against the Editor.js tool that authors the block, so that it survives that tool's `save()`.
4. Add the dispatch case in `EjsRenderFragment`.

The new block then appears in the runtime documentation with no further change.

## License

[MIT](https://github.com/kibblewhite/EditorJsonProcessor/blob/main/EditorJsonToHtmlConverter/LICENSE)
