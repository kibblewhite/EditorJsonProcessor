using EditorJsonToHtmlConverter.Extensions;
using EditorJsonToHtmlConverter.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

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
    [DataRow(TextWrapType.Text, "text")]
    [DataRow(TextWrapType.Custom, "custom")]
    [DataRow(TextWrapType.Title, "title")]
    [DataRow(TextWrapType.Synopsis, "synopsis")]
    public void A_text_document_built_from_a_wrap_type_carries_that_wrap_tag(TextWrapType wrap, string expected_wrap)
    {
        EditorJsBlocks? blocks = JsonSerializer.Deserialize<EditorJsBlocks>(EditorJsBlocksExtensions.TextDocument("A line", wrap));

        Assert.IsNotNull(blocks);
        EditorJsBlock block = blocks.Blocks.Single();
        Assert.AreEqual("text", block.Type);
        Assert.AreEqual(expected_wrap, block.Data.Wrap);
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

    [TestMethod]
    public void A_document_built_as_authored_content_carries_the_content_version()
    {
        EditorJsBlocks? text = JsonSerializer.Deserialize<EditorJsBlocks>(EditorJsBlocksExtensions.TextDocument("A title", TextWrapType.Title, is_authored_content: true));
        EditorJsBlocks? paragraph = JsonSerializer.Deserialize<EditorJsBlocks>(EditorJsBlocksExtensions.ParagraphDocument("A note", is_authored_content: true));

        Assert.IsNotNull(text);
        Assert.IsNotNull(paragraph);
        Assert.AreEqual(EditorJsBlocks.ContentVersion, text.Version);
        Assert.AreEqual(EditorJsBlocks.ContentVersion, paragraph.Version);
    }

    [TestMethod]
    [DataRow(null, DisplayName = "null")]
    [DataRow("", DisplayName = "empty string")]
    [DataRow("   ", DisplayName = "whitespace")]
    [DataRow("""{"time":0,"blocks":[],"version":"0.0.0"}""", DisplayName = "the empty document")]
    [DataRow("""{"time":5,"blocks":[{"id":"a","type":"paragraph","data":{"text":"Draft: Gala"}}],"version":"0.0.0"}""", DisplayName = "placeholder content")]
    [DataRow("""{"time":1,"blocks":[]}""", DisplayName = "version is missing")]
    [DataRow("""{"time":1,"blocks":[{"id":"a","type":"paragraph","data":{"text":"x"}}]}""", DisplayName = "content with no version")]
    [DataRow("""{"time":1,"blocks":[],"version":null}""", DisplayName = "version is null")]
    [DataRow("""{"time":1,"blocks":[],"version":""}""", DisplayName = "version is blank")]
    [DataRow("""{"time":1,"blocks":[],"version":0}""", DisplayName = "version is not a string")]
    public void A_blank_value_a_missing_version_or_the_template_version_is_a_template(string? value)
        => Assert.IsTrue(EditorJsBlocksExtensions.IsEmptyTemplate(value));

    [TestMethod]
    [DataRow("""{"time":1,"blocks":[{"id":"a","type":"paragraph","data":{"text":"x"}}],"version":"2.31.0"}""", DisplayName = "saved by the editor")]
    [DataRow("""{"time":1,"blocks":[{"id":"a","type":"paragraph","data":{"text":"x"}}],"version":"1.0.0"}""", DisplayName = "built as authored content")]
    [DataRow("""["not","an","object"]""", DisplayName = "an array")]
    [DataRow("Plain text", DisplayName = "not JSON")]
    public void Anything_else_is_not_a_template(string value)
        => Assert.IsFalse(EditorJsBlocksExtensions.IsEmptyTemplate(value));

    [TestMethod]
    public void The_builders_make_templates_unless_asked_for_authored_content()
    {
        Assert.IsTrue(EditorJsBlocksExtensions.IsEmptyTemplate(EditorJsBlocksExtensions.EmptyEditorJsString));
        Assert.IsTrue(EditorJsBlocksExtensions.IsEmptyTemplate(EditorJsBlocksExtensions.TextDocument("Draft: Gala", TextWrapType.Title)));
        Assert.IsTrue(EditorJsBlocksExtensions.IsEmptyTemplate(EditorJsBlocksExtensions.ParagraphDocument("Draft: Gala")));
        Assert.IsFalse(EditorJsBlocksExtensions.IsEmptyTemplate(EditorJsBlocksExtensions.TextDocument("Gala", TextWrapType.Title, is_authored_content: true)));
        Assert.IsFalse(EditorJsBlocksExtensions.IsEmptyTemplate(EditorJsBlocksExtensions.ParagraphDocument("Gala", is_authored_content: true)));
    }

    [TestMethod]
    [DataRow(null, true, DisplayName = "null")]
    [DataRow("  ", true, DisplayName = "whitespace")]
    [DataRow("""{"time":1,"blocks":[],"version":"2.31.5"}""", true, DisplayName = "an editor someone cleared")]
    [DataRow("""{"time":1,"version":"2.31.5"}""", true, DisplayName = "no blocks property")]
    [DataRow("""{"time":1,"blocks":null,"version":"2.31.5"}""", true, DisplayName = "null blocks")]
    [DataRow("""{"time":1,"blocks":[{"id":"a","type":"paragraph","data":{"text":"x"}}],"version":"2.31.5"}""", false, DisplayName = "a block")]
    [DataRow("Plain text", false, DisplayName = "not JSON")]
    [DataRow("""["not","an","object"]""", false, DisplayName = "an array")]
    public void A_value_has_no_blocks_when_nothing_in_it_would_render(string? value, bool expected)
    {
        Assert.AreEqual(expected, EditorJsBlocksExtensions.HasNoBlocks(value));
        if (value is not null && value.TrimStart().StartsWith('{') is true)
        {
            Assert.AreEqual(expected, EditorJsBlocksExtensions.HasNoBlocks(JsonNode.Parse(value)!.AsObject()));
        }
    }

    [TestMethod]
    [DataRow("""{"time":1,"blocks":[],"version":"2.31.5"}""", DisplayName = "an editor someone cleared")]
    [DataRow("""{"time":1,"blocks":[]}""", DisplayName = "no blocks and no version")]
    [DataRow("", DisplayName = "blank")]
    public void A_value_with_nothing_in_it_is_stored_as_the_empty_document(string value)
    {
        Assert.AreEqual(EditorJsBlocksExtensions.EmptyEditorJsString, EditorJsBlocksExtensions.TemplateWhenUnauthored(value));
        Assert.IsTrue(EditorJsBlocksExtensions.IsEmptyTemplate(EditorJsBlocksExtensions.TemplateWhenUnauthored(value)));
    }

    [TestMethod]
    public void Content_without_a_version_keeps_its_blocks_and_carries_the_template_version()
    {
        const string versionless = """{"time":1,"blocks":[{"id":"a","type":"text","data":{"text":"Gala"}}]}""";

        string stored = EditorJsBlocksExtensions.TemplateWhenUnauthored(versionless);
        JsonObject stored_object = EditorJsBlocksExtensions.TemplateWhenUnauthored(JsonNode.Parse(versionless)!.AsObject());

        Assert.IsTrue(EditorJsBlocksExtensions.IsEmptyTemplate(stored));
        Assert.Contains("Gala", stored);
        Assert.AreEqual(EditorJsBlocks.EmptyVersion, stored_object["version"]!.GetValue<string>());
        Assert.HasCount(1, stored_object["blocks"]!.AsArray());
    }

    [TestMethod]
    public void Authored_content_a_placeholder_and_a_non_document_are_stored_as_they_are()
    {
        string authored = EditorJsBlocksExtensions.TextDocument("Gala", TextWrapType.Title, is_authored_content: true);
        string placeholder = EditorJsBlocksExtensions.TextDocument("Draft: Gala", TextWrapType.Title);
        JsonObject saved = JsonNode.Parse("""{"time":1,"blocks":[{"id":"a","type":"text","data":{"text":"Gala"}}],"version":"2.31.5"}""")!.AsObject();

        Assert.AreEqual(authored, EditorJsBlocksExtensions.TemplateWhenUnauthored(authored));
        Assert.AreEqual(placeholder, EditorJsBlocksExtensions.TemplateWhenUnauthored(placeholder));
        Assert.AreEqual("Plain text", EditorJsBlocksExtensions.TemplateWhenUnauthored("Plain text"));
        Assert.AreSame(saved, EditorJsBlocksExtensions.TemplateWhenUnauthored(saved));
    }

    [TestMethod]
    public void A_document_reads_as_its_blocks_text_in_order()
    {
        const string document = """{"time":1,"blocks":[{"id":"a","type":"header","data":{"text":"Doors","level":2}},{"id":"b","type":"delimiter","data":{}},{"id":"c","type":"paragraph","data":{"text":"open at <b>7</b>"}}],"version":"2.31.5"}""";

        Assert.AreEqual("Doors open at <b>7</b>", EditorJsBlocksExtensions.PlainText(document));
        Assert.AreEqual(string.Empty, EditorJsBlocksExtensions.PlainText(EditorJsBlocksExtensions.EmptyEditorJsString));
        Assert.AreEqual(string.Empty, EditorJsBlocksExtensions.PlainText("  "));
        Assert.AreEqual("Plain text", EditorJsBlocksExtensions.PlainText("Plain text"));
    }
}
