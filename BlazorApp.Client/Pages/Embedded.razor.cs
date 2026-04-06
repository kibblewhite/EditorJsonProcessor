using EditorJsonToHtmlConverter;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorApp.Client.Pages;

public partial class Embedded : ComponentBase
{
    [Inject] public required EjsHtmlRenderer EjsHtmlRenderer { get; init; }
    [Inject] public required IJSRuntime JSRuntime { get; init; }

    protected string RenderedHtml { get; set; } = string.Empty;

    protected const string StylingMap = "[]";

    protected const string EditorJsJson = """
        {
          "time": 1717207275445,
          "version": "2.31.5",
          "blocks": [
            {
              "id": "demo_h_001",
              "type": "header",
              "data": { "text": "Tower of London — Embedded Map Demo", "level": 2 }
            },
            {
              "id": "demo_p_001",
              "type": "paragraph",
              "data": { "text": "The map block below contains fully resolved venue, space, POI, and activity data. In embedded mode the renderer serialises the entire block data object as a child script element." }
            },
            {
              "id": "demo_map_001",
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
                ],
                "venues": [
                  {
                    "venueGuid": { "value": "00000001-0000-0000-0000-000000000001" },
                    "name": "Tower of London",
                    "address": { "addressLine1": "Tower of London", "locality": "London", "postalCode": "EC3N 4AB", "country": "United Kingdom", "latitude": 51.5081, "longitude": -0.0759 },
                    "geometry": "{\"type\":\"Polygon\",\"coordinates\":[[[-0.07848,51.50950],[-0.07595,51.50970],[-0.07465,51.50887],[-0.07443,51.50834],[-0.07503,51.50780],[-0.07560,51.50737],[-0.07612,51.50715],[-0.07717,51.50721],[-0.07821,51.50762],[-0.07876,51.50815],[-0.07888,51.50878],[-0.07848,51.50950]]]}",
                    "borderColor": "#7c3aed", "fillColor": "#7c3aed", "borderWeight": 2, "fillOpacity": 0.08
                  },
                  {
                    "venueGuid": { "value": "00000001-0000-0000-0000-000000000002" },
                    "name": "Tower Bridge",
                    "address": { "addressLine1": "Tower Bridge Road", "locality": "London", "postalCode": "SE1 2UP", "country": "United Kingdom", "latitude": 51.5055, "longitude": -0.0753 },
                    "geometry": "{\"type\":\"MultiPolygon\",\"coordinates\":[[[[-0.07568,51.50582],[-0.07544,51.50582],[-0.07544,51.50548],[-0.07568,51.50548],[-0.07568,51.50582]]],[[[-0.07518,51.50582],[-0.07494,51.50582],[-0.07494,51.50548],[-0.07518,51.50548],[-0.07518,51.50582]]]]}",
                    "borderColor": "#dc2626", "fillColor": "#dc2626", "borderWeight": 1, "fillOpacity": 0.15
                  },
                  {
                    "venueGuid": { "value": "00000001-0000-0000-0000-000000000003" },
                    "name": "The Shard",
                    "address": { "addressLine1": "32 London Bridge Street", "locality": "London", "postalCode": "SE1 9SG", "country": "United Kingdom", "latitude": 51.5045, "longitude": -0.0865 }
                  }
                ],
                "spaces": [
                  {
                    "spaceGuid": { "value": "00000002-0000-0000-0000-000000000001" }, "venueGuid": { "value": "00000001-0000-0000-0000-000000000001" },
                    "name": "White Tower", "areaM2": 530,
                    "location": { "latitude": 51.50843, "longitude": -0.07614, "level": 0 },
                    "geometry": "{\"type\":\"Polygon\",\"coordinates\":[[[-0.07660,51.50870],[-0.07580,51.50870],[-0.07565,51.50855],[-0.07565,51.50820],[-0.07580,51.50810],[-0.07660,51.50810],[-0.07675,51.50820],[-0.07675,51.50855],[-0.07660,51.50870]]]}",
                    "borderColor": "#0284c7", "fillColor": "#0ea5e9", "borderWeight": 1, "fillOpacity": 0.15
                  },
                  {
                    "spaceGuid": { "value": "00000002-0000-0000-0000-000000000002" }, "venueGuid": { "value": "00000001-0000-0000-0000-000000000001" },
                    "name": "Chapel Royal of St Peter ad Vincula", "areaM2": 180,
                    "location": { "latitude": 51.50920, "longitude": -0.07655, "level": 0 },
                    "geometry": "{\"type\":\"Polygon\",\"coordinates\":[[[-0.07710,51.50940],[-0.07610,51.50940],[-0.07610,51.50905],[-0.07710,51.50905],[-0.07710,51.50940]]]}"
                  },
                  {
                    "spaceGuid": { "value": "00000002-0000-0000-0000-000000000003" }, "venueGuid": { "value": "00000001-0000-0000-0000-000000000002" },
                    "name": "Tower Bridge High-Level Walkway", "areaM2": 90,
                    "location": { "latitude": 51.50565, "longitude": -0.07531, "level": 2 }
                  },
                  {
                    "spaceGuid": { "value": "00000002-0000-0000-0000-000000000004" }, "venueGuid": { "value": "00000001-0000-0000-0000-000000000001" },
                    "name": "Waterloo Barracks \u2014 Crown Jewels", "areaM2": 350,
                    "location": { "latitude": 51.50890, "longitude": -0.07540, "level": 0 },
                    "geometry": "{\"type\":\"Polygon\",\"coordinates\":[[[-0.07600,51.50910],[-0.07490,51.50910],[-0.07490,51.50870],[-0.07600,51.50870],[-0.07600,51.50910]]]}"
                  }
                ],
                "typologies": [
                  { "typologyGuid": { "value": "00000003-0000-0000-0000-000000000001" }, "name": "Visitor Services" },
                  { "typologyGuid": { "value": "00000003-0000-0000-0000-000000000002" }, "name": "Food & Drink" }
                ],
                "pois": [
                  { "poiGuid": { "value": "00000005-0000-0000-0000-000000000001" }, "name": "Ticket Office", "icon": "\uD83C\uDFAB", "location": { "latitude": 51.50770, "longitude": -0.07630, "level": 0 } },
                  { "poiGuid": { "value": "00000005-0000-0000-0000-000000000002" }, "name": "New Armouries Cafe", "icon": "\u2615", "location": { "latitude": 51.50815, "longitude": -0.07480, "level": 0 } },
                  { "poiGuid": { "value": "00000005-0000-0000-0000-000000000003" }, "name": "Toilets (West)", "icon": "\uD83D\uDEBB", "location": { "latitude": 51.50860, "longitude": -0.07750, "level": 0 } },
                  { "poiGuid": { "value": "00000005-0000-0000-0000-000000000004" }, "name": "Information Point", "icon": "\u2139\uFE0F", "location": { "latitude": 51.50780, "longitude": -0.07580, "level": 0 } },
                  { "poiGuid": { "value": "00000005-0000-0000-0000-000000000005" }, "name": "First Aid Station", "icon": "\uD83C\uDFE5", "location": { "latitude": 51.50900, "longitude": -0.07500, "level": 0 } },
                  { "poiGuid": { "value": "00000005-0000-0000-0000-000000000006" }, "name": "Gift Shop", "icon": "\uD83D\uDECD\uFE0F", "location": { "latitude": 51.50850, "longitude": -0.07440, "level": 0 } },
                  { "poiGuid": { "value": "00000005-0000-0000-0000-000000000007" }, "name": "Raven Kiosk", "icon": "\uD83E\uDD44", "location": { "latitude": 51.50835, "longitude": -0.07680, "level": 0 } }
                ],
                "activities": [
                  { "activityGuid": { "value": "00000004-0000-0000-0000-000000000001" }, "displayName": "Crown Jewels Exhibition", "spaceGuid": { "value": "00000002-0000-0000-0000-000000000004" }, "location": { "latitude": 51.50890, "longitude": -0.07540, "level": 0 } },
                  { "activityGuid": { "value": "00000004-0000-0000-0000-000000000002" }, "displayName": "Chapel Service", "spaceGuid": { "value": "00000002-0000-0000-0000-000000000002" }, "location": { "latitude": 51.50920, "longitude": -0.07655, "level": 0 } },
                  { "activityGuid": { "value": "00000004-0000-0000-0000-000000000003" }, "displayName": "Bridge Walkway Tour", "spaceGuid": { "value": "00000002-0000-0000-0000-000000000003" }, "location": { "latitude": 51.50565, "longitude": -0.07531, "level": 0 } },
                  { "activityGuid": { "value": "00000004-0000-0000-0000-000000000004" }, "displayName": "Armoury & Weapons Display", "spaceGuid": { "value": "00000002-0000-0000-0000-000000000001" }, "location": { "latitude": 51.50843, "longitude": -0.07614, "level": 0 } }
                ]
              }
            },
            {
              "id": "demo_p_002",
              "type": "paragraph",
              "data": { "text": "The map container above has <code>data-block-type=\"map\"</code> and a child <code>&lt;script type=\"application/json\"&gt;</code> containing the full resolved data. A client-side JS viewer reads this and renders the interactive map." }
            }
          ]
        }
        """;

    protected override async Task OnInitializedAsync()
    {
        RenderedHtml = await EjsHtmlRenderer.ParseAsync(EditorJsJson);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JSRuntime.InvokeVoidAsync("MapViewer.initialize");
        }
    }
}
