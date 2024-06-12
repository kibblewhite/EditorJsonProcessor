using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EditorJsonToHtmlConverter.Tests;

[TestClass]
public class EjsRenderFragmentTests : Bunit.TestContext
{
    private ILogger<EjsRenderFragment> _logger = default!;

    [TestInitialize]
    public void TestInitialise()
    {
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

        ILoggerFactory factory = serviceProvider.GetRequiredService<ILoggerFactory>();

        _logger = factory.CreateLogger<EjsRenderFragment>();
    }

    [TestMethod]
    public void BasicTest()
    {
        // Arrange
        string styling_map = "[]";
        _ = _logger;

        // "does not resolve to a public property on the component" <- Google returnes zero results on this, so for now now unit testing until the produce matures a bit more.
        IRenderedComponent<EjsRenderFragment> cut = RenderComponent<EjsRenderFragment>(parameters => parameters
            .Add(p => p.Value, EjsRenderFragmentTestsHelpers.EditorJsonValue)
            .Add(p => p.StylingMap, styling_map));

        // Act
        string rendered_html = cut.Markup;

        // Assert
        // Add appropriate assertions to verify the HTML output
        // For example:
        // cut.Find("p").MarkupMatches("<p>Hello World</p>");
    }
}
