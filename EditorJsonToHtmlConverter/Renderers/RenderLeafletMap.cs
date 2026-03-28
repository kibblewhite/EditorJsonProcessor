namespace EditorJsonToHtmlConverter.Renderers;

/// <summary>
/// Renders a leaflet-map block as an HTML container element.
/// In embedded mode, outputs a child script element with fully resolved JSON data.
/// In reference mode, outputs data-* attributes with GUID references for client-side resolution.
/// Does not inject any script or link tags — that is the developer's responsibility.
/// </summary>
public sealed class RenderLeafletMap : IBlockRenderer
{
    private static readonly Guid EmptyGuid = Guid.Empty;
    private static readonly JsonSerializerOptions SerialiserOptions = new() { WriteIndented = false };

    public static void Render(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {
        string id = block.Id;

        render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "div");
        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "id", id);
        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "data-block-type", "leaflet-map");

        // CSS styling lookup
        EditorJsStylingMap? css = render_tree_builder.StylingMap
            .FirstOrDefault(item => item.Type == SupportedRenderers.LeafletMap && item.Id == id);
        css ??= render_tree_builder.StylingMap
            .FirstOrDefault(item => item.Type == SupportedRenderers.LeafletMap && item.Id == null);

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
    /// Renders a child script element containing the full resolved data as JSON (RL-02).
    /// </summary>
    private static void RenderEmbeddedMode(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {
        Dictionary<string, object> embedded_data = new();

        if (block.Data.Center is not null)
        {
            embedded_data["center"] = new { lat = block.Data.Center.Lat, lng = block.Data.Center.Lng };
        }

        if (block.Data.Zoom.HasValue)
        {
            embedded_data["zoom"] = block.Data.Zoom.Value;
        }

        if (string.IsNullOrWhiteSpace(block.Data.TileUrl) is false)
        {
            embedded_data["tileUrl"] = block.Data.TileUrl;
        }

        if (block.Data.Height.HasValue)
        {
            embedded_data["height"] = block.Data.Height.Value;
        }

        // Include resolved data if available (populated by MW-13), filtering out null/empty entries (RL-05)
        if (block.Data.Venues is not null && block.Data.Venues.Count > 0)
        {
            embedded_data["venues"] = block.Data.Venues;
        }

        if (block.Data.Spaces is not null && block.Data.Spaces.Count > 0)
        {
            embedded_data["spaces"] = block.Data.Spaces;
        }

        if (block.Data.Typologies is not null && block.Data.Typologies.Count > 0)
        {
            embedded_data["typologies"] = block.Data.Typologies;
        }

        if (block.Data.Pois is not null && block.Data.Pois.Count > 0)
        {
            embedded_data["pois"] = block.Data.Pois;
        }

        if (block.Data.Activities is not null && block.Data.Activities.Count > 0)
        {
            embedded_data["activities"] = block.Data.Activities;
        }

        string json = JsonSerializer.Serialize(embedded_data, SerialiserOptions);

        render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "script");
        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "type", "application/json");
        render_tree_builder.Builder.AddContent(render_tree_builder.SequenceCounter, json);
        render_tree_builder.Builder.CloseElement();
    }

    /// <summary>
    /// Renders data-* attributes on the container div with GUID references for client-side resolution (RL-03).
    /// </summary>
    private static void RenderReferenceMode(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {
        if (block.Data.Center is not null)
        {
            string center_json = JsonSerializer.Serialize(new { lat = block.Data.Center.Lat, lng = block.Data.Center.Lng }, SerialiserOptions);
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "data-center", center_json);
        }

        if (block.Data.Zoom.HasValue)
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "data-zoom", block.Data.Zoom.Value.ToString());
        }

        if (string.IsNullOrWhiteSpace(block.Data.TileUrl) is false)
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "data-tile-url", block.Data.TileUrl);
        }

        if (block.Data.Height.HasValue)
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "data-height", block.Data.Height.Value.ToString());
        }

        // GUID lists — filter out null, empty, whitespace, and Guid.Empty values (RL-05)
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

        // Activity GUIDs — filter out entries with invalid activity or space GUIDs (RL-05)
        if (block.Data.ActivityGuids is not null)
        {
            List<EditorJsMapActivityReference> valid_activities = block.Data.ActivityGuids
                .Where(a => IsValidGuid(a.ActivityGuid) is true && IsValidGuid(a.SpaceGuid) is true)
                .ToList();

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
    private static List<string> FilterValidGuids(List<string>? guids)
    {
        if (guids is null)
        {
            return [];
        }

        return guids.Where(g => IsValidGuid(g) is true).ToList();
    }

    /// <summary>
    /// Checks whether a GUID string is valid (not null, not empty, not whitespace, not Guid.Empty).
    /// </summary>
    private static bool IsValidGuid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (Guid.TryParse(value, out Guid parsed) is false)
        {
            return false;
        }

        return parsed.Equals(EmptyGuid) is false;
    }
}
