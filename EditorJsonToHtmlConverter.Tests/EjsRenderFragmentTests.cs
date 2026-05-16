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

    // ---------------------------------------------------------------------
    // Tile URL injection tests
    //
    // After the editorjs-leaflet migration, tileUrl is no longer persisted in
    // block data. The rendering layer is the source of truth: it injects the
    // value via EditorJsonProcessorOptions / EjsRenderFragment.TileUrlTemplate
    // and the renderer writes it into data-tile-url (reference mode) or the
    // embedded JSON (embedded mode).
    // ---------------------------------------------------------------------

    [TestMethod]
    public void ReferenceMode_TileUrl_EmittedFromInjectedTemplate()
    {
        IRenderedComponent<EjsRenderFragment> cut = Render<EjsRenderFragment>(parameters => parameters
            .Add(p => p.Value, EjsRenderFragmentTestsHelpers.EditorJsonMapBlockReference)
            .Add(p => p.StylingMap, "[]")
            .Add(p => p.DataRetrievalMode, DataRetrievalMode.Reference)
            .Add(p => p.TileUrlTemplate, "https://configured.example/{z}/{x}/{y}.mvt")
            .AddChildContent("<span>Loading...</span>"));

        IElement container = cut.Find("div[data-block-type='map']");
        Assert.IsTrue(container.HasAttribute("data-tile-url"), "data-tile-url should be present when TileUrlTemplate is supplied.");
        Assert.AreEqual("https://configured.example/{z}/{x}/{y}.mvt", container.GetAttribute("data-tile-url"));
    }

    [TestMethod]
    public void ReferenceMode_TileUrl_OmittedWhenTemplateIsNull()
    {
        // Block data fixture no longer carries tileUrl, and no template is supplied
        // by the component. The renderer must NOT emit data-tile-url — letting the
        // viewer's loud-failure guard surface the misconfiguration cleanly.
        IRenderedComponent<EjsRenderFragment> cut = Render<EjsRenderFragment>(parameters => parameters
            .Add(p => p.Value, EjsRenderFragmentTestsHelpers.EditorJsonMapBlockReference)
            .Add(p => p.StylingMap, "[]")
            .Add(p => p.DataRetrievalMode, DataRetrievalMode.Reference)
            .AddChildContent("<span>Loading...</span>"));

        IElement container = cut.Find("div[data-block-type='map']");
        Assert.IsFalse(container.HasAttribute("data-tile-url"), "data-tile-url must be omitted when no TileUrlTemplate is configured.");
    }

    [TestMethod]
    public void ReferenceMode_TileUrl_LegacyBlockDataValueIsIgnored()
    {
        // Pre-migration block JSON with tileUrl on data — must NOT influence
        // the rendered data-tile-url. The injected template wins; legacy data
        // is silently ignored.
        const string legacy_json = """
            {
              "time": 1,
              "version": "2.31.5",
              "blocks": [
                {
                  "id": "ref_map_legacy",
                  "type": "map",
                  "data": {
                    "center": { "lat": 0, "lng": 0 },
                    "zoom": 1,
                    "tileUrl": "https://STALE.legacy.example/{z}/{x}/{y}.mvt",
                    "height": 400,
                    "venueGuids": ["00000001-0000-0000-0000-000000000001"]
                  }
                }
              ]
            }
            """;

        IRenderedComponent<EjsRenderFragment> cut = Render<EjsRenderFragment>(parameters => parameters
            .Add(p => p.Value, legacy_json)
            .Add(p => p.StylingMap, "[]")
            .Add(p => p.DataRetrievalMode, DataRetrievalMode.Reference)
            .Add(p => p.TileUrlTemplate, "https://current.example/{z}/{x}/{y}.mvt")
            .AddChildContent("<span>Loading...</span>"));

        IElement container = cut.Find("div[data-block-type='map']");
        Assert.AreEqual("https://current.example/{z}/{x}/{y}.mvt", container.GetAttribute("data-tile-url"),
            "Injected TileUrlTemplate must win — legacy data.tileUrl must not leak through.");
    }

    [TestMethod]
    public void EmbeddedMode_TileUrl_FlowsIntoInlineJson()
    {
        IRenderedComponent<EjsRenderFragment> cut = Render<EjsRenderFragment>(parameters => parameters
            .Add(p => p.Value, EjsRenderFragmentTestsHelpers.EditorJsonMapBlockEmbedded)
            .Add(p => p.StylingMap, "[]")
            .Add(p => p.DataRetrievalMode, DataRetrievalMode.Embedded)
            .Add(p => p.TileUrlTemplate, "https://configured.example/{z}/{x}/{y}.mvt")
            .AddChildContent("<span>Loading...</span>"));

        IElement container = cut.Find("div[data-block-type='map']");
        IElement? inline_json = container.QuerySelector("script[type='application/json']");
        Assert.IsNotNull(inline_json, "Embedded mode should emit child <script type='application/json'>.");
        Assert.Contains("\"tileUrl\":\"https://configured.example/{z}/{x}/{y}.mvt\"", inline_json.TextContent,
            "Injected TileUrlTemplate should appear in the embedded JSON payload.");
    }

    [TestMethod]
    public void EmbeddedMode_TileUrl_LegacyBlockDataValueIsIgnored()
    {
        const string legacy_json = """
            {
              "time": 1,
              "version": "2.31.5",
              "blocks": [
                {
                  "id": "emb_map_legacy",
                  "type": "map",
                  "data": {
                    "center": { "lat": 0, "lng": 0 },
                    "zoom": 1,
                    "tileUrl": "https://STALE.legacy.example/{z}/{x}/{y}.mvt",
                    "height": 400,
                    "venueGuids": ["00000001-0000-0000-0000-000000000001"]
                  }
                }
              ]
            }
            """;

        IRenderedComponent<EjsRenderFragment> cut = Render<EjsRenderFragment>(parameters => parameters
            .Add(p => p.Value, legacy_json)
            .Add(p => p.StylingMap, "[]")
            .Add(p => p.DataRetrievalMode, DataRetrievalMode.Embedded)
            .Add(p => p.TileUrlTemplate, "https://current.example/{z}/{x}/{y}.mvt")
            .AddChildContent("<span>Loading...</span>"));

        IElement container = cut.Find("div[data-block-type='map']");
        IElement? inline_json = container.QuerySelector("script[type='application/json']");
        Assert.IsNotNull(inline_json);
        string json_text = inline_json.TextContent;

        Assert.Contains("https://current.example/", json_text, "Injected TileUrlTemplate must appear.");
        Assert.DoesNotContain("STALE.legacy.example", json_text,
            "Legacy block.Data.TileUrl must not leak into the embedded JSON output.");
    }
}
