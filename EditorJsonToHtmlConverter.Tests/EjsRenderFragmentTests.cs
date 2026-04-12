using AngleSharp.Dom;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EditorJsonToHtmlConverter.Tests;

[TestClass]
public class EjsRenderFragmentTests : Bunit.BunitContext
{
    private ILogger<EjsRenderFragment> _logger = default!;

    [TestInitialize]
    public void TestInitialise()
    {
        // note: registers ILoggerFactory on BUnit's service provider so EjsRenderFragment
        // can resolve its injected ILogger<EjsRenderFragment> dependency at render time.
        Services.AddLogging();

        ILoggerFactory factory = Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
        _logger = factory.CreateLogger<EjsRenderFragment>();
    }

    [TestMethod]
    public void BasicTest()
    {
        // Arrange
        string styling_map = "[]";
        _ = _logger;

        // "does not resolve to a public property on the component" <- Google returnes zero results on this, so for now now unit testing until the produce matures a bit more.
        IRenderedComponent<EjsRenderFragment> cut = Render<EjsRenderFragment>(parameters => parameters
            .Add(p => p.Value, EjsRenderFragmentTestsHelpers.EditorJsonValue)
            .Add(p => p.StylingMap, styling_map));

        // Act
        string rendered_html = cut.Markup;

        // Assert
        // Add appropriate assertions to verify the HTML output
        // For example:
        // cut.Find("p").MarkupMatches("<p>Hello World</p>");
    }

    [TestMethod]
    public void EmbeddedMode_MapBlock_EmitsContainerWithInlineJson()
    {
        // Arrange + Act
        IRenderedComponent<EjsRenderFragment> cut = Render<EjsRenderFragment>(parameters => parameters
            .Add(p => p.Value, EjsRenderFragmentTestsHelpers.EditorJsonMapBlockEmbedded)
            .Add(p => p.StylingMap, "[]")
            .Add(p => p.DataRetrievalMode, DataRetrievalMode.Embedded)
            .AddChildContent("<span>Loading...</span>"));

        // Assert
        IElement container = cut.Find("div[data-block-type='map']");
        IElement? inline_json = container.QuerySelector("script[type='application/json']");
        Assert.IsNotNull(inline_json, "Embedded mode should emit a child <script type='application/json'> carrying the full block data.");
        Assert.DoesNotContain("Loading...", cut.Markup, "The built render fragment should have replaced the ChildContent placeholder.");
    }

    [TestMethod]
    public void ReferenceMode_MapBlock_EmitsContainerWithDataAttributes()
    {
        // Arrange + Act
        IRenderedComponent<EjsRenderFragment> cut = Render<EjsRenderFragment>(parameters => parameters
            .Add(p => p.Value, EjsRenderFragmentTestsHelpers.EditorJsonMapBlockReference)
            .Add(p => p.StylingMap, "[]")
            .Add(p => p.DataRetrievalMode, DataRetrievalMode.Reference)
            .AddChildContent("<span>Loading...</span>"));

        // Assert
        IElement container = cut.Find("div[data-block-type='map']");
        Assert.IsTrue(container.HasAttribute("data-center"), "Reference mode should emit data-center.");
        Assert.IsTrue(container.HasAttribute("data-zoom"), "Reference mode should emit data-zoom.");
        Assert.IsTrue(container.HasAttribute("data-venue-guids"), "Reference mode should emit data-venue-guids for the GUID list.");
        Assert.IsNull(container.QuerySelector("script[type='application/json']"), "Reference mode should not inline block data as JSON — the viewer fetches via data-* attributes.");
    }

    [TestMethod]
    public void ParentRerender_PreservesBuiltRenderFragment()
    {
        // Arrange — first render builds the fragment and swaps in the converted block markup.
        IRenderedComponent<EjsRenderFragment> cut = Render<EjsRenderFragment>(parameters => parameters
            .Add(p => p.Value, EjsRenderFragmentTestsHelpers.EditorJsonMapBlockEmbedded)
            .Add(p => p.StylingMap, "[]")
            .Add(p => p.DataRetrievalMode, DataRetrievalMode.Embedded)
            .AddChildContent("<span>Loading...</span>"));

        cut.Find("div[data-block-type='map']");
        Assert.DoesNotContain("Loading...", cut.Markup, "Initial render should have swapped in the built content.");

        // Act — simulate a parent re-render pushing fresh parameters (same values). This is
        // the regression guard for the bug where ChildContent was mutated as internal state
        // and therefore wiped out by the next SetParametersAsync call.
        cut.Render(parameters => parameters
            .Add(p => p.Value, EjsRenderFragmentTestsHelpers.EditorJsonMapBlockEmbedded)
            .Add(p => p.StylingMap, "[]")
            .Add(p => p.DataRetrievalMode, DataRetrievalMode.Embedded)
            .AddChildContent("<span>Loading...</span>"));

        // Assert — built content survives the re-render.
        cut.Find("div[data-block-type='map']");
        Assert.DoesNotContain("Loading...", cut.Markup, "After parent re-render, the ChildContent placeholder must not come back.");
    }
}
