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
        string result = await _ejs_html_renderer.ParseAsync(json_value, false, EjsRenderFragmentTestsHelpers.StylingMap);

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
        string result = await _ejs_html_renderer.ParseAsync(json_value, true);

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
        Assert.IsTrue(result.Contains("data-block-type=\"map\"") is true, "Expected data-block-type=\"map\" attribute on container div");
        Assert.IsTrue(result.Contains("id=\"map_embed_001\"") is true, "Expected id attribute matching block id");

        // Assert — embedded mode outputs a child <script type="application/json"> with the data
        Assert.IsTrue(result.Contains("""<script type="application/json">""") is true, "Expected child script element with application/json type");
        Assert.IsTrue(result.Contains("Tower of London") is true, "Expected resolved venue name in embedded JSON");
        Assert.IsTrue(result.Contains("White Tower") is true, "Expected resolved space name in embedded JSON");

        // Assert — no data-* GUID attributes in embedded mode
        Assert.IsTrue(result.Contains("data-venue-guids") is false, "Embedded mode should not output data-venue-guids attribute");
    }

    [TestMethod]
    public async Task ParseAsync_MapBlock_ReferenceMode_ContainsDataAttributes()
    {
        ArgumentNullException.ThrowIfNull(_html_renderer, nameof(_html_renderer));

        // Arrange
        EjsHtmlRenderer reference_renderer = new(_html_renderer, DataRetrievalMode.Reference, System.Globalization.CultureInfo.GetCultureInfo("en-GB"));
        string json_value = EjsRenderFragmentTestsHelpers.EditorJsonMapBlockReference;

        // Act
        string result = await reference_renderer.ParseAsync(json_value);

        // Assert — container div with data-block-type="map"
        Assert.IsTrue(result.Contains("data-block-type=\"map\"") is true, "Expected data-block-type=\"map\" attribute");
        Assert.IsTrue(result.Contains("id=\"map_ref_001\"") is true, "Expected id attribute matching block id");

        // Assert — reference mode outputs data-* attributes
        Assert.IsTrue(result.Contains("data-locale=\"en-GB\"") is true, "Expected data-locale attribute");
        Assert.IsTrue(result.Contains("data-zoom=\"16\"") is true, "Expected data-zoom attribute");
        Assert.IsTrue(result.Contains("data-height=\"600\"") is true, "Expected data-height attribute");
        Assert.IsTrue(result.Contains("data-tile-url=\"/tiles/{z}/{x}/{y}.mvt\"") is true, "Expected data-tile-url attribute");

        // Assert — GUID lists present as comma-separated data-* attributes
        Assert.IsTrue(result.Contains("data-venue-guids=\"00000001-0000-0000-0000-000000000001,00000001-0000-0000-0000-000000000002\"") is true, "Expected data-venue-guids with both GUIDs");
        Assert.IsTrue(result.Contains("data-space-guids=\"00000002-0000-0000-0000-000000000001\"") is true, "Expected data-space-guids");
        Assert.IsTrue(result.Contains("data-typology-guids=\"00000003-0000-0000-0000-000000000001\"") is true, "Expected data-typology-guids");
        Assert.IsTrue(result.Contains("data-activity-guids=") is true, "Expected data-activity-guids");

        // Assert — no embedded script in reference mode
        Assert.IsTrue(result.Contains("""<script type="application/json">""") is false, "Reference mode should not output embedded script element");
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
        Assert.IsTrue(result.Contains("data-venue-guids=\"00000001-0000-0000-0000-000000000001\"") is true, "Expected only the valid GUID, empty GUID filtered");
        Assert.IsTrue(result.Contains("00000000-0000-0000-0000-000000000000") is false, "Guid.Empty should be filtered out");
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