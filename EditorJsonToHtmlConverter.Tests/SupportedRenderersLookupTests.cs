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
    [DataRow("code", SupportedRenderers.Code)]
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

    [TestMethod]
    [DataRow(SupportedRenderers.Paragraph, "paragraph")]
    [DataRow(SupportedRenderers.Checklist, "checklist")]
    [DataRow(SupportedRenderers.Text, "text")]
    [DataRow(SupportedRenderers.Map, "map")]
    [DataRow(SupportedRenderers.Code, "code")]
    public void A_renderer_writes_the_type_its_editor_js_tool_is_registered_under(SupportedRenderers renderer, string expected_block_type)
        => Assert.AreEqual(expected_block_type, renderer.ToBlockType());

    [TestMethod]
    public void Every_renderer_block_type_is_lower_case_and_resolves_back_to_its_renderer()
    {
        foreach (SupportedRenderers renderer in Enum.GetValues<SupportedRenderers>().Where(renderer => renderer != SupportedRenderers.Empty))
        {
            string block_type = renderer.ToBlockType();

            Assert.AreEqual(renderer.ToString().ToLowerInvariant(), block_type);
            Assert.IsTrue(SupportedRenderersLookup.TryGetBlockType(block_type, out SupportedRenderers resolved));
            Assert.AreEqual(renderer, resolved);
        }
    }

    [TestMethod]
    public void The_empty_block_has_no_block_type()
        => Assert.AreEqual(string.Empty, SupportedRenderers.Empty.ToBlockType());
}
