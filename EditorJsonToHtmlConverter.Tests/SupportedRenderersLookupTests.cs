namespace EditorJsonToHtmlConverter.Tests;

/// <summary>
/// Pins the public block-type lookup: a renderer's name resolves, in any case; anything <see cref="Enum.TryParse{TEnum}(string?, bool, out TEnum)"/>
/// would also accept — a number, padding, a comma list — and the empty block never do.
/// </summary>
[TestClass]
public sealed class SupportedRenderersLookupTests
{
    [TestMethod]
    [DataRow("paragraph", SupportedRenderers.Paragraph)]
    [DataRow("HEADER", SupportedRenderers.Header)]
    [DataRow("Map", SupportedRenderers.Map)]
    public void A_renderer_name_resolves_in_any_case(string block_type, SupportedRenderers expected_renderer)
    {
        Assert.IsTrue(SupportedRenderersLookup.TryGetBlockType(block_type, out SupportedRenderers renderer));
        Assert.AreEqual(expected_renderer, renderer);
    }

    [TestMethod]
    [DataRow("1")]
    [DataRow(" paragraph ")]
    [DataRow("paragraph, header")]
    [DataRow("empty")]
    [DataRow("")]
    [DataRow(null)]
    [DataRow("no-such-tool")]
    public void Anything_but_a_renderer_name_does_not_resolve(string? block_type)
    {
        Assert.IsFalse(SupportedRenderersLookup.TryGetBlockType(block_type, out SupportedRenderers renderer));
        Assert.AreEqual(SupportedRenderers.Empty, renderer);
    }
}
