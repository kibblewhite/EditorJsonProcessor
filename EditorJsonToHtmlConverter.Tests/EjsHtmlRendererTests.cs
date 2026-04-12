using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace EditorJsonToHtmlConverter.Tests;

[TestClass]
public class EjsHtmlRendererTests
{
    private HtmlRenderer? _html_renderer;
    private EjsHtmlRenderer? _ejs_html_renderer;
    public required TestContext TestContext { get; set; }

    [TestInitialize]
    public void TestInitialize()
    {
        ServiceProvider services = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

        _html_renderer = new HtmlRenderer(services, NullLoggerFactory.Instance);
        _ejs_html_renderer = new EjsHtmlRenderer(_html_renderer);
    }

    [TestMethod]
    public async Task ParseAsync_ReturnsCorrectHtml_ForValidJson()
    {
        ArgumentNullException.ThrowIfNull(_ejs_html_renderer, nameof(_ejs_html_renderer));

        // Arrange
        string json_value = EjsRenderFragmentTestsHelpers.EditorJsonValue;
        string expected_html = EjsRenderFragmentTestsHelpers.ExpectedEditorHtmlValue;

        // Act
        string result = await _ejs_html_renderer.ParseAsync(json_value);

        // Assert
        Assert.Contains(expected_html, result);
    }

    [TestMethod]
    public async Task ParseAsync_ReturnsCorrectHtml_WithStylingMap_ForValidJson()
    {
        ArgumentNullException.ThrowIfNull(_ejs_html_renderer, nameof(_ejs_html_renderer));

        // Arrange
        string json_value = EjsRenderFragmentTestsHelpers.EditorJsonValue;
        string expected_html = EjsRenderFragmentTestsHelpers.ExpectedEditorHtmlWithStylingsValue;

        // Act
        string result = await _ejs_html_renderer.ParseAsync(json_value, strip_html: false, styling_map: EjsRenderFragmentTestsHelpers.StylingMap);

        // Assert
        Assert.Contains(expected_html, result);
    }

    [TestMethod]
    public async Task ParseAsync_ReturnsEmptyHtml_ForEmptyJson()
    {
        ArgumentNullException.ThrowIfNull(_ejs_html_renderer, nameof(_ejs_html_renderer));

        // Arrange
        string json_value = EjsRenderFragmentTestsHelpers.EditorJsonEmpty;

        // Act
        string result = await _ejs_html_renderer.ParseAsync(json_value, strip_html: true);

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    //[TestMethod]
    //[ExpectedException(typeof(JsonException))]
    //public async Task ParseAsync_ThrowsJsonException_ForInvalidJson()
    //{
    //    ArgumentNullException.ThrowIfNull(_ejs_html_renderer, nameof(_ejs_html_renderer));

    //    // Arrange
    //    string invalid_json_value = "Invalid JSON";

    //    // Act
    //    await _ejs_html_renderer.ParseAsync(invalid_json_value);

    //    // Assert is handled by ExpectedException
    //}

    [TestMethod]
    public async Task ParseAsync_MapBlock_EmbeddedMode_ContainsDataBlockTypeAndScript()
    {
        ArgumentNullException.ThrowIfNull(_ejs_html_renderer, nameof(_ejs_html_renderer));

        // Arrange
        string json_value = EjsRenderFragmentTestsHelpers.EditorJsonMapBlockEmbedded;

        // Act
        string result = await _ejs_html_renderer.ParseAsync(json_value);

        // Assert — container div with data-block-type="map"
        Assert.Contains("data-block-type=\"map\"", result, "Expected data-block-type=\"map\" attribute on container div");
        Assert.Contains("id=\"map_embed_001\"", result, "Expected id attribute matching block id");

        // Assert — embedded mode outputs a child <script type="application/json"> with the data
        Assert.Contains("""<script type="application/json">""", result, "Expected child script element with application/json type");
        Assert.Contains("Tower of London", result, "Expected resolved venue name in embedded JSON");
        Assert.Contains("White Tower", result, "Expected resolved space name in embedded JSON");

        // Assert — no data-* GUID attributes in embedded mode
        Assert.DoesNotContain("data-venue-guids", result, "Embedded mode should not output data-venue-guids attribute");
    }

    [TestMethod]
    public async Task ParseAsync_MapBlock_ReferenceMode_ContainsDataAttributes()
    {
        ArgumentNullException.ThrowIfNull(_html_renderer, nameof(_html_renderer));

        // Arrange
        EjsHtmlRenderer reference_renderer = new(_html_renderer, DataRetrievalMode.Reference, locale: System.Globalization.CultureInfo.GetCultureInfo("en-GB"));
        string json_value = EjsRenderFragmentTestsHelpers.EditorJsonMapBlockReference;

        // Act
        string result = await reference_renderer.ParseAsync(json_value);

        // Assert — container div with data-block-type="map"
        Assert.Contains("data-block-type=\"map\"", result, "Expected data-block-type=\"map\" attribute");
        Assert.Contains("id=\"map_ref_001\"", result, "Expected id attribute matching block id");

        // Assert — reference mode outputs data-* attributes
        Assert.Contains("data-locale=\"en-GB\"", result, "Expected data-locale attribute");
        Assert.Contains("data-zoom=\"16\"", result, "Expected data-zoom attribute");
        Assert.Contains("data-height=\"600\"", result, "Expected data-height attribute");
        Assert.Contains("data-tile-url=\"/tiles/{z}/{x}/{y}.mvt\"", result, "Expected data-tile-url attribute");

        // Assert — GUID lists present as comma-separated data-* attributes
        Assert.Contains("data-venue-guids=\"00000001-0000-0000-0000-000000000001,00000001-0000-0000-0000-000000000002\"", result, "Expected data-venue-guids with both GUIDs");
        Assert.Contains("data-space-guids=\"00000002-0000-0000-0000-000000000001\"", result, "Expected data-space-guids");
        Assert.Contains("data-typology-guids=\"00000003-0000-0000-0000-000000000001\"", result, "Expected data-typology-guids");
        Assert.Contains("data-activity-guids=", result, "Expected data-activity-guids");

        // Assert — no embedded script in reference mode
        Assert.DoesNotContain("""<script type="application/json">""", result, "Reference mode should not output embedded script element");
    }

    [TestMethod]
    public async Task ParseAsync_MapBlock_ReferenceMode_FiltersInvalidGuids()
    {
        ArgumentNullException.ThrowIfNull(_html_renderer, nameof(_html_renderer));

        // Arrange — JSON with a mix of valid GUIDs and Guid.Empty
        string json_with_empty_guids = """
            {
              "time": 1717207275445,
              "version": "2.31.5",
              "blocks": [
                {
                  "id": "map_filter_001",
                  "type": "map",
                  "data": {
                    "center": { "lat": 0, "lng": 0 },
                    "zoom": 2,
                    "venueGuids": ["00000001-0000-0000-0000-000000000001", "00000000-0000-0000-0000-000000000000"],
                    "spaceGuids": [],
                    "typologyGuids": [],
                    "activityGuids": []
                  }
                }
              ]
            }
            """;

        EjsHtmlRenderer reference_renderer = new(_html_renderer, DataRetrievalMode.Reference);

        // Act
        string result = await reference_renderer.ParseAsync(json_with_empty_guids);

        // Assert — only the valid GUID should appear, Guid.Empty filtered out
        Assert.Contains("data-venue-guids=\"00000001-0000-0000-0000-000000000001\"", result, "Expected only the valid GUID, empty GUID filtered");
        Assert.DoesNotContain("00000000-0000-0000-0000-000000000000", result, "Guid.Empty should be filtered out");
    }

    [TestMethod]
    public async Task ParseAsync_ThrowsException_ForInvalidJson()
    {
        ArgumentNullException.ThrowIfNull(_ejs_html_renderer, nameof(_ejs_html_renderer));

        // Arrange
        string invalidJsonValue = "Invalid JSON";

        // Act & Assert
        JsonException ex = await Assert.ThrowsExactlyAsync<JsonException>(
            () => _ejs_html_renderer.ParseAsync(invalidJsonValue));

        TestContext.WriteLine("Exception caught:");
        TestContext.WriteLine($"Message: {ex.Message}");
        TestContext.WriteLine($"Path: {ex.Path}");
        TestContext.WriteLine($"Line number: {ex.LineNumber}");
        TestContext.WriteLine($"Byte position in line: {ex.BytePositionInLine}");

        Assert.AreEqual("'I' is an invalid start of a value. Path: $ | LineNumber: 0 | BytePositionInLine: 0.", ex.Message);
    }
}