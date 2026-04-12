using EditorJsonToHtmlConverter;
using EditorJsonToHtmlConverter.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;

namespace BlazorApp.Client.Pages;

public partial class Reference : ComponentBase
{
    [Inject] public required HtmlRenderer HtmlRenderer { get; init; }
    [Inject] public required ILogger<Reference> Logger { get; init; }

    protected string RenderedHtml { get; set; } = string.Empty;

    // note: stable per component instance. Generating this inline in the razor markup via
    // Guid.CreateVersion7() creates a new value on every parent render, which causes Blazor
    // to push a parameter change into EjsRenderFragment on every re-render.
    protected Guid CorrelationIdentifier { get; } = Guid.CreateVersion7();

    protected static CultureInfo Locale => CultureInfo.GetCultureInfo("en-GB");

    protected const string StylingMap = "[]";

    /// <summary>
    /// Reference mode JSON — GUIDs match the mock API endpoints in BlazorApp.Server.
    /// No resolved data arrays — the viewer JS fetches them at runtime.
    /// </summary>
    protected const string EditorJsJson = """
        {
          "time": 1717207275445,
          "version": "2.31.5",
          "blocks": [
            {
              "id": "ref_h_001",
              "type": "header",
              "data": { "text": "Tower of London — Reference Mode", "level": 2 }
            },
            {
              "id": "ref_p_001",
              "type": "paragraph",
              "data": { "text": "The map below is rendered by the leaflet-map-viewer.min.js script. It discovers the container, reads the data-* attributes, and fetches venue/space/POI/activity data from the mock API endpoints." }
            },
            {
              "id": "ref_map_001",
              "type": "map",
              "data": {
                "center": { "lat": 51.5065, "lng": -0.0760 },
                "zoom": 16,
                "tileUrl": "/tiles/{z}/{x}/{y}.mvt",
                "height": 600,
                "venueGuids": ["00000001-0000-0000-0000-000000000001", "00000001-0000-0000-0000-000000000002", "00000001-0000-0000-0000-000000000003"],
                "spaceGuids": ["00000002-0000-0000-0000-000000000001", "00000002-0000-0000-0000-000000000002", "00000002-0000-0000-0000-000000000003", "00000002-0000-0000-0000-000000000004"],
                "typologyGuids": ["00000003-0000-0000-0000-000000000001", "00000003-0000-0000-0000-000000000002"],
                "activityGuids": [
                  { "activityGuid": "00000004-0000-0000-0000-000000000001", "spaceGuid": "00000002-0000-0000-0000-000000000001" },
                  { "activityGuid": "00000004-0000-0000-0000-000000000002", "spaceGuid": "00000002-0000-0000-0000-000000000002" },
                  { "activityGuid": "00000004-0000-0000-0000-000000000003", "spaceGuid": "00000002-0000-0000-0000-000000000003" },
                  { "activityGuid": "00000004-0000-0000-0000-000000000004", "spaceGuid": "00000002-0000-0000-0000-000000000004" }
                ]
              }
            },
            {
              "id": "ref_p_002",
              "type": "paragraph",
              "data": { "text": "The container above was rendered server-side with data-* attributes. The viewer script hydrated it into an interactive map by fetching from /api/premises/... and /api/schedules/... endpoints." }
            }
          ]
        }
        """;

    protected override async Task OnInitializedAsync()
    {
        EjsHtmlRenderer reference_renderer = new(HtmlRenderer, DataRetrievalMode.Reference, OnRenderCompletedAsync, Locale);
        RenderedHtml = await reference_renderer.ParseAsync(EditorJsJson, Guid.CreateVersion7());
    }

    private Task OnRenderCompletedAsync(EjsRenderCompletedEventArgs args)
    {
        Logger.LogInformation(
            "EjsHtmlRenderer completed (correlation: {CorrelationIdentifier}) in {ElapsedMs} ms",
            args.CorrelationIdentifier,
            args.Elapsed.TotalMilliseconds);
        return Task.CompletedTask;
    }
}
