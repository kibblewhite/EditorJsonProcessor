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
        string expected_html = EjsRenderFragmentTestsHelpers.EditorHtmlValue;

        // Act
        string result = await _ejs_html_renderer.ParseAsync(json_value);

        // Assert
        Assert.IsTrue(result.Contains(expected_html));
    }

    [TestMethod]
    public async Task ParseAsync_ReturnsEmptyHtml_ForEmptyJson()
    {
        ArgumentNullException.ThrowIfNull(_ejs_html_renderer, nameof(_ejs_html_renderer));

        // Arrange
        string json_value = EjsRenderFragmentTestsHelpers.EditorJsonEmpty;

        // Act
        string result = await _ejs_html_renderer.ParseAsync(json_value);

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public async Task ParseAsync_ThrowsJsonException_ForInvalidJson()
    {
        ArgumentNullException.ThrowIfNull(_ejs_html_renderer, nameof(_ejs_html_renderer));

        // Arrange
        string invalid_json_value = "Invalid JSON";

        // Act
        await _ejs_html_renderer.ParseAsync(invalid_json_value);

        // Assert is handled by ExpectedException
    }

    [TestMethod]
    public async Task ParseAsync_ThrowsException_ForInvalidJson()
    {
        ArgumentNullException.ThrowIfNull(_ejs_html_renderer, nameof(_ejs_html_renderer));

        // Arrange
        string invalidJsonValue = "Invalid JSON";

        try
        {
            // Act
            await _ejs_html_renderer.ParseAsync(invalidJsonValue);

            // Fail the test if no exception is thrown
            Assert.Fail("Expected a JsonException to be thrown, but no exception was thrown.");
        }
        catch (JsonException ex)
        {
            TestContext.WriteLine("Exception caught:");
            TestContext.WriteLine($"Message: {ex.Message}");
            TestContext.WriteLine($"Path: {ex.Path}");
            TestContext.WriteLine($"Line number: {ex.LineNumber}");
            TestContext.WriteLine($"Byte position in line: {ex.BytePositionInLine}");

            // Assert
            Assert.IsNotNull(ex, "Exception should not be null.");
            Assert.AreEqual("'I' is an invalid start of a value. Path: $ | LineNumber: 0 | BytePositionInLine: 0.", ex.Message);
        }
    }
}