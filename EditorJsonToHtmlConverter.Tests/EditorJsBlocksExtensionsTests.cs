using EditorJsonToHtmlConverter.Extensions;
using EditorJsonToHtmlConverter.Models;
using System.Text.Json;

namespace EditorJsonToHtmlConverter.Tests;

/// <summary>
/// Pins the document helpers: a block identifier is ten hex characters drawn from the random end of a v7 GUID, so blocks
/// created together do not repeat; a document is recognised at the level asked for — its envelope, its structure as
/// Editor.js writes it, or everything the renderer needs — each level refusing all the earlier ones do; and a built document is one
/// JSON-escaped block — a <c>text</c> block carrying only its text and wrap, or a <c>paragraph</c> carrying only its text.
/// </summary>
[TestClass]
public sealed class EditorJsBlocksExtensionsTests
{
    [TestMethod]
    public void A_block_identifier_is_ten_hex_characters_and_does_not_repeat_within_a_burst()
    {
        string[] identifiers = [.. Enumerable.Range(0, 1000).Select(_ => EditorJsBlock.NewId())];

        Assert.IsTrue(identifiers.All(identifier => identifier.Length == 10 && identifier.All(char.IsAsciiHexDigitLower)));
        Assert.HasCount(identifiers.Length, identifiers.Distinct());
    }

    [TestMethod]
    [DataRow("""{"time":0,"blocks":[],"version":"0.0.0"}""")]
    [DataRow("""{"blocks":[{"id":"a","type":"paragraph","data":{"text":"x"}}]}""")]
    public void A_json_object_with_a_blocks_array_is_a_document(string value)
        => Assert.IsTrue(EditorJsBlocksExtensions.IsEditorJsDocument(value));

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("plain prose")]
    [DataRow("""{"time":0,"version":"0.0.0"}""")]
    [DataRow("""{"blocks":{}}""")]
    [DataRow("""[{"blocks":[]}]""")]
    [DataRow("""{"blocks":[""")]
    public void Anything_else_is_not_a_document_at_any_level(string value)
    {
        Assert.IsFalse(EditorJsBlocksExtensions.IsEditorJsDocument(value, EditorJsDocumentCheck.Envelope));
        Assert.IsFalse(EditorJsBlocksExtensions.IsEditorJsDocument(value, EditorJsDocumentCheck.Structure));
        Assert.IsFalse(EditorJsBlocksExtensions.IsEditorJsDocument(value, EditorJsDocumentCheck.Renderable));
    }

    [TestMethod]
    public void A_document_is_checked_by_its_envelope_unless_asked_otherwise()
    {
        string unrenderable = """{"blocks":[{"type":"no-such-tool","data":{}}, 7]}""";

        Assert.IsTrue(EditorJsBlocksExtensions.IsEditorJsDocument(unrenderable));
        Assert.IsFalse(EditorJsBlocksExtensions.IsEditorJsDocument(unrenderable, EditorJsDocumentCheck.Structure));
    }

    [TestMethod]
    public void A_check_level_that_is_not_defined_is_refused()
        => Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => EditorJsBlocksExtensions.IsEditorJsDocument("""{"blocks":[]}""", (EditorJsDocumentCheck)3));

    [TestMethod]
    [DataRow("""{"blocks":[]}""")]
    [DataRow("""{"blocks":[{"type":"paragraph","data":{"text":"x"}}]}""")]
    [DataRow("""{"blocks":[{"type":"no-such-tool","data":{}}]}""")]
    [DataRow("""{"time":1.5,"blocks":[{"id":"a","type":"paragraph","data":{},"tunes":{"alignment":{"alignment":"left"}}}],"version":"2.31.0"}""")]
    public void The_structure_keeps_the_fields_editor_js_makes_optional_optional(string value)
        => Assert.IsTrue(EditorJsBlocksExtensions.IsEditorJsDocument(value, EditorJsDocumentCheck.Structure));

    [TestMethod]
    [DataRow("""{"time":"0","blocks":[]}""", DisplayName = "time is not a number")]
    [DataRow("""{"blocks":[],"version":2}""", DisplayName = "version is not a string")]
    [DataRow("""{"blocks":[null]}""", DisplayName = "block is null")]
    [DataRow("""{"blocks":["paragraph"]}""", DisplayName = "block is not an object")]
    [DataRow("""{"blocks":[{"data":{}}]}""", DisplayName = "type is missing")]
    [DataRow("""{"blocks":[{"type":"","data":{}}]}""", DisplayName = "type is empty")]
    [DataRow("""{"blocks":[{"type":1,"data":{}}]}""", DisplayName = "type is not a string")]
    [DataRow("""{"blocks":[{"type":"paragraph"}]}""", DisplayName = "data is missing")]
    [DataRow("""{"blocks":[{"type":"paragraph","data":null}]}""", DisplayName = "data is null")]
    [DataRow("""{"blocks":[{"type":"paragraph","data":[]}]}""", DisplayName = "data is not an object")]
    [DataRow("""{"blocks":[{"id":"","type":"paragraph","data":{}}]}""", DisplayName = "id is empty")]
    [DataRow("""{"blocks":[{"id":null,"type":"paragraph","data":{}}]}""", DisplayName = "id is null")]
    [DataRow("""{"blocks":[{"id":7,"type":"paragraph","data":{}}]}""", DisplayName = "id is not a string")]
    [DataRow("""{"blocks":[{"type":"paragraph","data":{},"tunes":[]}]}""", DisplayName = "tunes is not an object")]
    public void A_field_of_the_wrong_kind_fails_the_structure(string value)
    {
        Assert.IsTrue(EditorJsBlocksExtensions.IsEditorJsDocument(value, EditorJsDocumentCheck.Envelope));
        Assert.IsFalse(EditorJsBlocksExtensions.IsEditorJsDocument(value, EditorJsDocumentCheck.Structure));
        Assert.IsFalse(EditorJsBlocksExtensions.IsEditorJsDocument(value, EditorJsDocumentCheck.Renderable));
    }

    [TestMethod]
    public void A_document_as_the_editor_saves_it_is_renderable()
    {
        string value = """
            {"time":1751560168320,"blocks":[
                {"id":"h1a2b3c4d5","type":"header","data":{"text":"Welcome","level":2}},
                {"id":"p-_AZaz09x","type":"PARAGRAPH","data":{"text":"A <b>bold</b> line"},"tunes":{"alignment":{"alignment":"left"}}},
                {"id":"t1a2b3c4d5","type":"table","data":{"withHeadings":true,"content":[["Ticket","Price"],["Standard","£25"]]}}
            ],"version":"2.31.0"}
            """;

        Assert.IsTrue(EditorJsBlocksExtensions.IsEditorJsDocument(value, EditorJsDocumentCheck.Renderable));
    }

    [TestMethod]
    public void The_documents_this_library_builds_are_renderable()
    {
        Assert.IsTrue(EditorJsBlocksExtensions.IsEditorJsDocument(EditorJsBlocksExtensions.EmptyEditorJsString, EditorJsDocumentCheck.Renderable));
        Assert.IsTrue(EditorJsBlocksExtensions.IsEditorJsDocument(EditorJsBlocksExtensions.TextDocument("A title", "title"), EditorJsDocumentCheck.Renderable));
        Assert.IsTrue(EditorJsBlocksExtensions.IsEditorJsDocument(EditorJsBlocksExtensions.ParagraphDocument("A note"), EditorJsDocumentCheck.Renderable));
    }

    [TestMethod]
    [DataRow("""{"blocks":[],"version":"0.0.0"}""", DisplayName = "time is missing")]
    [DataRow("""{"time":1.5,"blocks":[],"version":"0.0.0"}""", DisplayName = "time is not whole milliseconds")]
    [DataRow("""{"time":0,"blocks":[]}""", DisplayName = "version is missing")]
    [DataRow("""{"time":0,"blocks":[{"type":"paragraph","data":{"text":"x"}}],"version":"0.0.0"}""", DisplayName = "id is missing")]
    [DataRow("""{"time":0,"blocks":[{"id":"a","type":"paragraph","data":{"text":"x"}},{"id":"a","type":"paragraph","data":{"text":"y"}}],"version":"0.0.0"}""", DisplayName = "ids repeat")]
    [DataRow("""{"time":0,"blocks":[{"id":"a","type":"no-such-tool","data":{}}],"version":"0.0.0"}""", DisplayName = "type has no renderer")]
    [DataRow("""{"time":0,"blocks":[{"id":"a","type":"empty","data":{}}],"version":"0.0.0"}""", DisplayName = "type is the empty block")]
    [DataRow("""{"time":0,"blocks":[{"id":"a","type":"1","data":{"text":"x"}}],"version":"0.0.0"}""", DisplayName = "type is a renderer's number")]
    [DataRow("""{"time":0,"blocks":[{"id":"a","type":"paragraph, header","data":{"text":"x"}}],"version":"0.0.0"}""", DisplayName = "type is a list of renderers")]
    [DataRow("""{"time":0,"blocks":[{"id":"a","type":"header","data":{"text":"x","level":"two"}}],"version":"0.0.0"}""", DisplayName = "data field has the wrong type")]
    public void What_the_renderer_cannot_draw_passes_the_structure_but_is_not_renderable(string value)
    {
        Assert.IsTrue(EditorJsBlocksExtensions.IsEditorJsDocument(value, EditorJsDocumentCheck.Structure));
        Assert.IsFalse(EditorJsBlocksExtensions.IsEditorJsDocument(value, EditorJsDocumentCheck.Renderable));
    }

    [TestMethod]
    public void A_text_document_holds_one_escaped_text_block_with_only_its_text_and_wrap()
    {
        string text = """Say "hello" to <b>everyone</b>""";

        string document = EditorJsBlocksExtensions.TextDocument(text, "title");

        Assert.IsTrue(EditorJsBlocksExtensions.IsEditorJsDocument(document));
        EditorJsBlocks? blocks = JsonSerializer.Deserialize<EditorJsBlocks>(document);
        Assert.IsNotNull(blocks);
        Assert.AreEqual(EditorJsBlocks.EmptyVersion, blocks.Version);
        EditorJsBlock block = blocks.Blocks.Single();
        Assert.AreEqual("text", block.Type);
        Assert.AreEqual(10, block.Id.Length);
        Assert.AreEqual(text, block.Data.Text);
        Assert.AreEqual("title", block.Data.Wrap);

        using JsonDocument json_document = JsonDocument.Parse(document);
        JsonElement data = json_document.RootElement.GetProperty("blocks")[0].GetProperty("data");
        Assert.HasCount(2, data.EnumerateObject());
    }

    [TestMethod]
    public void A_paragraph_document_holds_one_escaped_paragraph_block_with_only_its_text()
    {
        string text = """Bring a "plus one" &amp; <i>dancing shoes</i>""";

        string document = EditorJsBlocksExtensions.ParagraphDocument(text);

        Assert.IsTrue(EditorJsBlocksExtensions.IsEditorJsDocument(document));
        EditorJsBlocks? blocks = JsonSerializer.Deserialize<EditorJsBlocks>(document);
        Assert.IsNotNull(blocks);
        Assert.AreEqual(EditorJsBlocks.EmptyVersion, blocks.Version);
        EditorJsBlock block = blocks.Blocks.Single();
        Assert.AreEqual("paragraph", block.Type);
        Assert.AreEqual(10, block.Id.Length);
        Assert.AreEqual(text, block.Data.Text);

        using JsonDocument json_document = JsonDocument.Parse(document);
        JsonElement data = json_document.RootElement.GetProperty("blocks")[0].GetProperty("data");
        Assert.HasCount(1, data.EnumerateObject());
    }
}
