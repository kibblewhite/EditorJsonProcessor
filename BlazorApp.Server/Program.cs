using System.Collections.Concurrent;
using System.Text.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient("tiles", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "EditorJsonProcessor-Demo");
    client.Timeout = TimeSpan.FromSeconds(10);
});
WebApplication app = builder.Build();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

// Tile proxy — bypasses CORS for the protomaps tile server.
// Caches tiles in memory so repeated requests (pan back, zoom in/out) are instant.
ConcurrentDictionary<string, byte[]> tile_cache = new();

app.MapGet("/tiles/{z:int}/{x:int}/{y:int}.mvt", async (int z, int x, int y, HttpContext http_context, IHttpClientFactory http_factory) =>
{
    string cache_key = $"{z}/{x}/{y}";
    http_context.Response.Headers.CacheControl = "public, max-age=86400";
    http_context.Response.ContentType = "application/vnd.mapbox-vector-tile";

    if (tile_cache.TryGetValue(cache_key, out byte[]? cached))
    {
        await http_context.Response.Body.WriteAsync(cached);
        return;
    }

    try
    {
        HttpClient client = http_factory.CreateClient("tiles");
        byte[] content = await client.GetByteArrayAsync($"https://pull-pmtiles.fullevent.io/tiles/{z}/{x}/{y}.mvt");
        tile_cache.TryAdd(cache_key, content);
        await http_context.Response.Body.WriteAsync(content);
    }
    catch (Exception)
    {
        http_context.Response.StatusCode = 504;
    }
});

// -------------------------------------------------------------------------
// Mock API endpoints matching the viewer's fetch.js contract.
// GUIDs must be real Guid-parseable values (the HTML renderer's
// FilterValidGuids uses Guid.TryParse to validate data-* attribute values).
// -------------------------------------------------------------------------

JsonSerializerOptions json_options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
ILogger logger = app.Logger;

Dictionary<string, object> venues = new()
{
    ["00000001-0000-0000-0000-000000000001"] = new
    {
        venueGuid = new { value = "00000001-0000-0000-0000-000000000001" },
        name = "Tower of London",
        address = new { addressLine1 = "Tower of London", locality = "London", postalCode = "EC3N 4AB", country = "United Kingdom", latitude = 51.5081, longitude = -0.0759 },
        geometry = """{"type":"Polygon","coordinates":[[[-0.07848,51.50950],[-0.07595,51.50970],[-0.07465,51.50887],[-0.07443,51.50834],[-0.07503,51.50780],[-0.07560,51.50737],[-0.07612,51.50715],[-0.07717,51.50721],[-0.07821,51.50762],[-0.07876,51.50815],[-0.07888,51.50878],[-0.07848,51.50950]]]}""",
        borderColor = "#7c3aed", fillColor = "#7c3aed", borderWeight = 2, fillOpacity = 0.08
    },
    ["00000001-0000-0000-0000-000000000002"] = new
    {
        venueGuid = new { value = "00000001-0000-0000-0000-000000000002" },
        name = "Tower Bridge",
        address = new { addressLine1 = "Tower Bridge Road", locality = "London", postalCode = "SE1 2UP", country = "United Kingdom", latitude = 51.5055, longitude = -0.0753 },
        geometry = """{"type":"MultiPolygon","coordinates":[[[[-0.07568,51.50582],[-0.07544,51.50582],[-0.07544,51.50548],[-0.07568,51.50548],[-0.07568,51.50582]]],[[[-0.07518,51.50582],[-0.07494,51.50582],[-0.07494,51.50548],[-0.07518,51.50548],[-0.07518,51.50582]]]]}""",
        borderColor = "#dc2626", fillColor = "#dc2626", borderWeight = 1, fillOpacity = 0.15
    },
    ["00000001-0000-0000-0000-000000000003"] = new
    {
        venueGuid = new { value = "00000001-0000-0000-0000-000000000003" },
        name = "The Shard",
        address = new { addressLine1 = "32 London Bridge Street", locality = "London", postalCode = "SE1 9SG", country = "United Kingdom", latitude = 51.5045, longitude = -0.0865 }
    }
};

Dictionary<string, object> spaces = new()
{
    ["00000002-0000-0000-0000-000000000001"] = new
    {
        spaceGuid = new { value = "00000002-0000-0000-0000-000000000001" }, venueGuid = new { value = "00000001-0000-0000-0000-000000000001" },
        name = "White Tower", areaM2 = 530,
        location = new { latitude = 51.50843, longitude = -0.07614, level = 0 },
        geometry = """{"type":"Polygon","coordinates":[[[-0.07660,51.50870],[-0.07580,51.50870],[-0.07565,51.50855],[-0.07565,51.50820],[-0.07580,51.50810],[-0.07660,51.50810],[-0.07675,51.50820],[-0.07675,51.50855],[-0.07660,51.50870]]]}""",
        borderColor = "#0284c7", fillColor = "#0ea5e9", borderWeight = 1, fillOpacity = 0.15
    },
    ["00000002-0000-0000-0000-000000000002"] = new
    {
        spaceGuid = new { value = "00000002-0000-0000-0000-000000000002" }, venueGuid = new { value = "00000001-0000-0000-0000-000000000001" },
        name = "Chapel Royal of St Peter ad Vincula", areaM2 = 180,
        location = new { latitude = 51.50920, longitude = -0.07655, level = 0 },
        geometry = """{"type":"Polygon","coordinates":[[[-0.07710,51.50940],[-0.07610,51.50940],[-0.07610,51.50905],[-0.07710,51.50905],[-0.07710,51.50940]]]}"""
    },
    ["00000002-0000-0000-0000-000000000003"] = new
    {
        spaceGuid = new { value = "00000002-0000-0000-0000-000000000003" }, venueGuid = new { value = "00000001-0000-0000-0000-000000000002" },
        name = "Tower Bridge High-Level Walkway", areaM2 = 90,
        location = new { latitude = 51.50565, longitude = -0.07531, level = 2 }
    },
    ["00000002-0000-0000-0000-000000000004"] = new
    {
        spaceGuid = new { value = "00000002-0000-0000-0000-000000000004" }, venueGuid = new { value = "00000001-0000-0000-0000-000000000001" },
        name = "Waterloo Barracks \u2014 Crown Jewels", areaM2 = 350,
        location = new { latitude = 51.50890, longitude = -0.07540, level = 0 },
        geometry = """{"type":"Polygon","coordinates":[[[-0.07600,51.50910],[-0.07490,51.50910],[-0.07490,51.50870],[-0.07600,51.50870],[-0.07600,51.50910]]]}"""
    }
};

object[] pois =
[
    new { poiGuid = new { value = "00000005-0000-0000-0000-000000000001" }, name = "Ticket Office", icon = "\uD83C\uDFAB", location = new { latitude = 51.50770, longitude = -0.07630, level = 0 } },
    new { poiGuid = new { value = "00000005-0000-0000-0000-000000000002" }, name = "New Armouries Cafe", icon = "\u2615", location = new { latitude = 51.50815, longitude = -0.07480, level = 0 } },
    new { poiGuid = new { value = "00000005-0000-0000-0000-000000000003" }, name = "Toilets (West)", icon = "\uD83D\uDEBB", location = new { latitude = 51.50860, longitude = -0.07750, level = 0 } },
    new { poiGuid = new { value = "00000005-0000-0000-0000-000000000004" }, name = "Information Point", icon = "\u2139\uFE0F", location = new { latitude = 51.50780, longitude = -0.07580, level = 0 } },
    new { poiGuid = new { value = "00000005-0000-0000-0000-000000000005" }, name = "First Aid Station", icon = "\uD83C\uDFE5", location = new { latitude = 51.50900, longitude = -0.07500, level = 0 } },
    new { poiGuid = new { value = "00000005-0000-0000-0000-000000000006" }, name = "Gift Shop", icon = "\uD83D\uDECD\uFE0F", location = new { latitude = 51.50850, longitude = -0.07440, level = 0 } },
    new { poiGuid = new { value = "00000005-0000-0000-0000-000000000007" }, name = "Raven Kiosk", icon = "\uD83E\uDD44", location = new { latitude = 51.50835, longitude = -0.07680, level = 0 } }
];

object[] typologies =
[
    new { typologyGuid = new { value = "00000003-0000-0000-0000-000000000001" }, name = "Visitor Services" },
    new { typologyGuid = new { value = "00000003-0000-0000-0000-000000000002" }, name = "Food & Drink" }
];

object[] activities =
[
    new { activityGuid = new { value = "00000004-0000-0000-0000-000000000001" }, displayName = "Crown Jewels Exhibition", spaceGuid = new { value = "00000002-0000-0000-0000-000000000004" }, location = new { latitude = 51.50890, longitude = -0.07540, level = 0 } },
    new { activityGuid = new { value = "00000004-0000-0000-0000-000000000002" }, displayName = "Chapel Service", spaceGuid = new { value = "00000002-0000-0000-0000-000000000002" }, location = new { latitude = 51.50920, longitude = -0.07655, level = 0 } },
    new { activityGuid = new { value = "00000004-0000-0000-0000-000000000003" }, displayName = "Bridge Walkway Tour", spaceGuid = new { value = "00000002-0000-0000-0000-000000000003" }, location = new { latitude = 51.50565, longitude = -0.07531, level = 0 } },
    new { activityGuid = new { value = "00000004-0000-0000-0000-000000000004" }, displayName = "Armoury & Weapons Display", spaceGuid = new { value = "00000002-0000-0000-0000-000000000001" }, location = new { latitude = 51.50843, longitude = -0.07614, level = 0 } }
];

// GET /venue-details/{guid}/locale/{locale}
app.MapGet("/api/venue-details/{guid}/locale/{locale}", (string guid, string locale) =>
{
    logger.LogInformation("[mock-api] GET venue-details guid={Guid} locale={Locale}", guid, locale);
    if (venues.TryGetValue(guid, out object? venue) is false)
    {
        logger.LogWarning("[mock-api] Venue not found: {Guid}", guid);
        return Results.NotFound();
    }
    return Results.Json(venue, json_options);
});

// GET /space-details/{guid}/locale/{locale}
app.MapGet("/api/space-details/{guid}/locale/{locale}", (string guid, string locale) =>
{
    logger.LogInformation("[mock-api] GET space-details guid={Guid} locale={Locale}", guid, locale);
    if (spaces.TryGetValue(guid, out object? space) is false)
    {
        logger.LogWarning("[mock-api] Space not found: {Guid}", guid);
        return Results.NotFound();
    }
    return Results.Json(space, json_options);
});

// POST /pois-by-typology-guids/locale/{locale}
app.MapPost("/api/pois-by-typology-guids/locale/{locale}", (string locale) =>
{
    logger.LogInformation("[mock-api] POST pois-by-typology-guids locale={Locale}, returning {Count} POIs", locale, pois.Length);
    return Results.Json(pois, json_options);
});

// GET /typologies/locale/{locale}
app.MapGet("/api/typologies/locale/{locale}", (string locale) =>
{
    logger.LogInformation("[mock-api] GET typologies locale={Locale}, returning {Count} typologies", locale, typologies.Length);
    return Results.Json(typologies, json_options);
});

// POST /activities-by-activity-guids/locale/{locale}
app.MapPost("/api/activities-by-activity-guids/locale/{locale}", (string locale) =>
{
    logger.LogInformation("[mock-api] POST activities-by-activity-guids locale={Locale}, returning {Count} activities", locale, activities.Length);
    return Results.Json(activities, json_options);
});

// Fallback to WASM index.html
app.MapFallbackToFile("index.html");

app.Run();
