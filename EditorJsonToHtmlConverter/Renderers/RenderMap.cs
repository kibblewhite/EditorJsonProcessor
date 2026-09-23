namespace EditorJsonToHtmlConverter.Renderers;

/// <summary>
/// Renders an interactive map of places the consuming application already holds, referenced by identifier.
/// </summary>
/// <remarks>
/// <para>
/// A map block does not describe geography of its own: it names existing venue, space, typology, point-of-interest
/// and activity records, and the viewer draws them. There is no freehand drawing, no arbitrary marker and no
/// pin at a bare coordinate — a place must already exist as a record before a map can show it. The centre and
/// zoom only frame the view; they do not add anything to it.
/// </para>
/// <para>
/// <c>venueGuids</c>, <c>spaceGuids</c>, <c>typologyGuids</c> (all optional) — identifier lists naming what the
/// map shows. <c>activityGuids</c> (optional) — activity references, each pairing the activity identifier with
/// the space identifier captured when the block was saved. Entries that are empty or all-zero identifiers are
/// discarded, so a block naming nothing renders an empty map.
/// </para>
/// <para>
/// <c>center</c> (optional) — the starting centre as <c>lat</c> and <c>lng</c>. <c>zoom</c> (optional) — the
/// starting zoom level. <c>height</c> (optional) — the map's height in pixels.
/// </para>
/// <para>
/// <b>Two rendering modes.</b> In <i>embedded</i> mode the consuming application resolves every identifier to a
/// full localised record before the renderer runs, and the whole payload is written into a child
/// <c>&lt;script type="application/json"&gt;</c> element, so the client draws the map immediately with no further
/// calls. In <i>reference</i> mode the block still holds only identifiers and configuration; these are written as
/// <c>data-*</c> attributes on the container, and a client-side viewer fetches the records itself using the locale
/// in <c>data-locale</c>. Authoring is identical either way — the mode is the consumer's choice, not the author's.
/// </para>
/// <para>
/// <b>Tile URL source of truth.</b> The <c>editorjs-leaflet</c> plugin no longer persists a <c>tileUrl</c> on block
/// data. The rendering layer is authoritative: the injected <see cref="CustomRenderTreeBuilder.TileUrlTemplate"/>
/// is the only tile URL used, and any legacy <c>tileUrl</c> on incoming data is ignored rather than silently
/// honouring a stale CDN address from older saved content.
/// </para>
/// <para>
/// The renderer injects no <c>&lt;script&gt;</c> or <c>&lt;link&gt;</c> tags; supplying the viewer assets is the
/// consuming application's responsibility.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// {
///   "id": "m1a2b3c4d5",
///   "type": "map",
///   "data": {
///     "venueGuids": [ "6f9619ff-8b86-d011-b42d-00cf4fc964ff" ],
///     "spaceGuids": [ ],
///     "typologyGuids": [ ],
///     "activityGuids": [ ],
///     "center": { "lat": 51.5072, "lng": -0.1276 },
///     "zoom": 14,
///     "height": 400
///   }
/// }
/// </code>
/// </example>
public sealed class RenderMap : IBlockRenderer
{
    /// <inheritdoc />
    public static SupportedRenderers BlockType => SupportedRenderers.Map;

    private static readonly Guid EmptyGuid = Guid.Empty;
    private static readonly JsonSerializerOptions SerialiserOptions = new() { WriteIndented = false };

    public static void Render(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {
        string id = block.Id;

        render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "div");
        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "id", id);
        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "data-block-type", "map");

        // CSS styling lookup
        EditorJsStylingMap? css = render_tree_builder.StylingMap
            .FirstOrDefault(item => item.Type == SupportedRenderers.Map && item.Id == id);
        css ??= render_tree_builder.StylingMap
            .FirstOrDefault(item => item.Type == SupportedRenderers.Map && item.Id == null);

        if (css is not null)
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "class", css.Style);
        }

        switch (render_tree_builder.DataRetrievalMode)
        {
            case DataRetrievalMode.Embedded:
                RenderEmbeddedMode(render_tree_builder, block);
                break;
            case DataRetrievalMode.Reference:
                RenderReferenceMode(render_tree_builder, block);
                break;
        }

        render_tree_builder.Builder.CloseElement();
    }

    /// <summary>
    /// Renders a child script element containing the complete block data as JSON
    /// plus the injected <c>tileUrl</c>. The GUID resolver resolves all GUIDs to
    /// full objects before rendering, so block.Data contains map configuration,
    /// resolved venues, spaces, typologies, POIs, and activities with all localised
    /// fields. The client JS reads this and has everything needed to render the
    /// map immediately without further API calls.
    /// </summary>
    private static void RenderEmbeddedMode(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {
        // Build the embedded JSON from a wrapper object rather than serialising
        // block.Data directly. This lets the renderer inject the authoritative
        // tileUrl (from CustomRenderTreeBuilder.TileUrlTemplate) without mutating
        // the input model, and explicitly excludes the obsolete TileUrl field
        // from the persisted block data shape.
        var payload = new
        {
            center = block.Data.Center,
            zoom = block.Data.Zoom,
            tileUrl = render_tree_builder.TileUrlTemplate,
            height = block.Data.Height,
            venueGuids = block.Data.VenueGuids,
            spaceGuids = block.Data.SpaceGuids,
            typologyGuids = block.Data.TypologyGuids,
            activityGuids = block.Data.ActivityGuids,
            venues = block.Data.Venues,
            spaces = block.Data.Spaces,
            typologies = block.Data.Typologies,
            pois = block.Data.Pois,
            activities = block.Data.Activities
        };

        string json = JsonSerializer.Serialize(payload, SerialiserOptions);

        render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "script");
        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "type", "application/json");
        render_tree_builder.Builder.AddContent(render_tree_builder.SequenceCounter, json);
        render_tree_builder.Builder.CloseElement();
    }

    /// <summary>
    /// Reference mode: outputs GUID references, map configuration, and locale as <c>data-*</c>
    /// attributes on the container div. No resolved data is included — the block data at this
    /// point contains only flat GUID lists as stored by the EditorJS plugin. A client-side JS
    /// viewer discovers these containers, reads the attributes, and fetches full venue, space,
    /// typology, POI, and activity details from API endpoints using the <c>data-locale</c> value.
    /// The <c>data-tile-url</c> attribute is sourced from the injected
    /// <see cref="CustomRenderTreeBuilder.TileUrlTemplate"/>, never from block data.
    /// </summary>
    private static void RenderReferenceMode(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {
        if (render_tree_builder.Locale is not null)
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "data-locale", render_tree_builder.Locale.Name);
        }

        if (block.Data.Center is not null)
        {
            string center_json = JsonSerializer.Serialize(new { lat = block.Data.Center.Lat, lng = block.Data.Center.Lng }, SerialiserOptions);
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "data-center", center_json);
        }

        if (block.Data.Zoom.HasValue)
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "data-zoom", block.Data.Zoom.Value.ToString());
        }

        // Tile URL is sourced from the renderer config (CustomRenderTreeBuilder.TileUrlTemplate),
        // NOT from block.Data.TileUrl — any value persisted there is silently ignored so old
        // saved content can't leak stale CDN URLs.
        if (!string.IsNullOrWhiteSpace(render_tree_builder.TileUrlTemplate))
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "data-tile-url", render_tree_builder.TileUrlTemplate);
        }

        if (block.Data.Height.HasValue)
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "data-height", block.Data.Height.Value.ToString());
        }

        // GUID lists — filter out null, empty, whitespace, and Guid.Empty values
        List<string> venue_guids = FilterValidGuids(block.Data.VenueGuids);
        if (venue_guids.Count > 0)
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "data-venue-guids", string.Join(",", venue_guids));
        }

        List<string> space_guids = FilterValidGuids(block.Data.SpaceGuids);
        if (space_guids.Count > 0)
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "data-space-guids", string.Join(",", space_guids));
        }

        List<string> typology_guids = FilterValidGuids(block.Data.TypologyGuids);
        if (typology_guids.Count > 0)
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "data-typology-guids", string.Join(",", typology_guids));
        }

        // Activity GUIDs — filter out entries with invalid activity or space GUIDs
        if (block.Data.ActivityGuids is not null)
        {
            List<EditorJsMapActivityReference> valid_activities
                = [.. block.Data.ActivityGuids.Where(a => IsValidGuid(a.ActivityGuid) && IsValidGuid(a.SpaceGuid))];

            if (valid_activities.Count > 0)
            {
                string activity_guids_json = JsonSerializer.Serialize(valid_activities, SerialiserOptions);
                render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "data-activity-guids", activity_guids_json);
            }
        }
    }

    /// <summary>
    /// Filters a list of GUID strings, excluding null, empty, whitespace, and Guid.Empty values.
    /// </summary>
    private static List<string> FilterValidGuids(List<string>? guids) =>
        guids?.Where(IsValidGuid).ToList() ?? [];

    /// <summary>
    /// Checks whether a GUID string is valid (not null, not empty, not whitespace, not Guid.Empty).
    /// </summary>
    private static bool IsValidGuid(string? value) =>
        string.IsNullOrWhiteSpace(value) is false
        && Guid.TryParse(value, out Guid parsed)
        && parsed.Equals(EmptyGuid) is false;
}
