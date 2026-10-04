namespace EditorJsonToHtmlConverter.Extensions;

public static class EditorJsBlocksExtensions
{
    // Unset block fields are left out, so a built block carries only the fields its tool writes, as the editor writes it.
    private static readonly JsonSerializerOptions _single_block_document_serializer_options = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    /// <summary>
    /// Gets an empty <see cref="JsonObject"/> instance representing an Editor.js object.
    /// </summary>
    public static JsonObject EmptyEditorJsObject => JsonNode.Parse(EmptyEditorJsString)?.AsObject() ?? [];

    /// <summary>
    /// Gets an empty JSON string representation for an editor, including a timestamp, empty blocks, and a default
    /// version.
    /// </summary>
    /// <remarks>This property is useful for initialising or resetting editor content to a default empty
    /// state. The timestamp is generated based on the current UTC time.</remarks>
    public static string EmptyEditorJsString => JsonSerializer.Serialize(EditorJsBlocks.Empty);

    /// <summary>
    /// Whether a value is an Editor.js document, checked as far as <paramref name="check"/> asks. By default only the
    /// envelope is checked — JSON that parses to an object carrying a <c>blocks</c> array — so a document that passes may
    /// still hold blocks that do not render; ask for <see cref="EditorJsDocumentCheck.Renderable"/> to know that it will.
    /// </summary>
    /// <param name="value">The candidate document. A blank value is not a document.</param>
    /// <param name="check">How far to check it; each level includes the ones before it and costs more.</param>
    /// <returns><see langword="true"/> when the value passes every check of the level asked for.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="check"/> is not an <see cref="EditorJsDocumentCheck"/> level.</exception>
    public static bool IsEditorJsDocument(string value, EditorJsDocumentCheck check = EditorJsDocumentCheck.Envelope)
    {
        if (Enum.IsDefined(check) is false)
        {
            throw new ArgumentOutOfRangeException(nameof(check), check, $"Not an {nameof(EditorJsDocumentCheck)} level.");
        }

        if (string.IsNullOrWhiteSpace(value) is true)
        {
            return false;
        }

        try
        {
            using JsonDocument json_document = JsonDocument.Parse(value);
            JsonElement root_element = json_document.RootElement;
            return root_element.ValueKind == JsonValueKind.Object
                && root_element.TryGetProperty("blocks", out JsonElement blocks_element) is true
                && blocks_element.ValueKind == JsonValueKind.Array && check switch
                {
                    EditorJsDocumentCheck.Envelope => true,
                    EditorJsDocumentCheck.Structure => HasEditorJsStructure(root_element, blocks_element),
                    _ => HasEditorJsStructure(root_element, blocks_element) && IsRenderable(json_document, root_element, blocks_element)
                };
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Whether a content field's value is a template: an Editor.js document whose <c>version</c> is
    /// <see cref="EditorJsBlocks.EmptyVersion"/>, whether it holds no blocks or only placeholder content no one has
    /// authored. Such a field counts as not authored. A blank value counts as a template; a value that is not a JSON
    /// object, or has no string <c>version</c>, does not, as <see cref="IsEditorJsDocument"/> refuses it.
    /// </summary>
    /// <param name="value">A content field's stored value.</param>
    /// <returns><see langword="true"/> when the value is blank or carries <see cref="EditorJsBlocks.EmptyVersion"/>.</returns>
    public static bool IsEmptyTemplate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) is true)
        {
            return true;
        }

        try
        {
            using JsonDocument json_document = JsonDocument.Parse(value);
            JsonElement root_element = json_document.RootElement;
            return root_element.ValueKind == JsonValueKind.Object
                && root_element.TryGetProperty("version", out JsonElement version_element) is true
                && version_element.ValueKind == JsonValueKind.String
                && version_element.ValueEquals(EditorJsBlocks.EmptyVersion) is true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    // The kinds the Editor.js output format gives each field (OutputData / OutputBlockData). Optional fields stay optional,
    // but one that is present must have its kind; null is a value, not an absence.
    private static bool HasEditorJsStructure(JsonElement root_element, JsonElement blocks_element)
    {
        bool has_envelope_fields = IsAbsentOrOfKind(root_element, "time", JsonValueKind.Number)
            && IsAbsentOrOfKind(root_element, "version", JsonValueKind.String);
        if (has_envelope_fields is false)
        {
            return false;
        }

        foreach (JsonElement block_element in blocks_element.EnumerateArray())
        {
            bool is_block = block_element.ValueKind == JsonValueKind.Object
                && block_element.TryGetProperty("type", out JsonElement type_element) is true
                && IsNonEmptyString(type_element) is true
                && block_element.TryGetProperty("data", out JsonElement data_element) is true
                && data_element.ValueKind == JsonValueKind.Object
                && (block_element.TryGetProperty("id", out JsonElement id_element) is false || IsNonEmptyString(id_element) is true)
                && IsAbsentOrOfKind(block_element, "tunes", JsonValueKind.Object);
            if (is_block is false)
            {
                return false;
            }
        }

        return true;
    }

    // What the renderer needs beyond the format: the fields its model makes required, ids that tell blocks apart, types it
    // draws, and — by building that model exactly as the renderer does — every data field typed as a renderer reads it.
    private static bool IsRenderable(JsonDocument json_document, JsonElement root_element, JsonElement blocks_element)
    {
        // The structure is already checked, so a present time is a number and a present version a string.
        bool has_envelope_fields = root_element.TryGetProperty("time", out JsonElement time_element) is true
            && time_element.TryGetInt64(out _) is true
            && root_element.TryGetProperty("version", out _) is true;
        if (has_envelope_fields is false)
        {
            return false;
        }

        HashSet<string> block_ids = new(StringComparer.Ordinal);
        foreach (JsonElement block_element in blocks_element.EnumerateArray())
        {
            bool is_renderable_block = block_element.TryGetProperty("id", out JsonElement id_element) is true
                && block_ids.Add(id_element.GetString()!) is true
                && SupportedRenderersLookup.TryGetBlockType(block_element.GetProperty("type").GetString(), out _) is true;
            if (is_renderable_block is false)
            {
                return false;
            }
        }

        // Deserialising from the parsed document does not read the text a second time.
        return JsonSerializer.Deserialize<EditorJsBlocks>(json_document) is not null;
    }

    private static bool IsAbsentOrOfKind(JsonElement element, string property_name, JsonValueKind value_kind)
        => element.TryGetProperty(property_name, out JsonElement property_element) is false || property_element.ValueKind == value_kind;

    private static bool IsNonEmptyString(JsonElement element)
        => element.ValueKind == JsonValueKind.String && element.ValueEquals(string.Empty) is false;

    /// <summary>
    /// Builds the document for a single-line field — a title, a synopsis, a label: exactly one <c>text</c> block holding
    /// the line, the shape the single-line <c>editorjs-text</c> tool edits (see the <c>text</c> renderer). The text is
    /// JSON-escaped, so quotes and control characters are safe, and inline HTML in it is kept as written.
    /// </summary>
    /// <param name="text">The line, as inline HTML.</param>
    /// <param name="wrap">The <c>wrap</c> tag the field's editor is configured with (<c>text</c>, <c>custom</c>, <c>title</c> or <c>synopsis</c>).</param>
    /// <param name="is_authored_content">
    /// <see langword="false"/> (the default) builds a template, carrying <see cref="EditorJsBlocks.EmptyVersion"/>, for
    /// placeholder text no one has authored yet; <see langword="true"/> builds authored content, carrying
    /// <see cref="EditorJsBlocks.ContentVersion"/>.
    /// </param>
    /// <returns>The serialised Editor.js document.</returns>
    public static string TextDocument(string text, string wrap, bool is_authored_content = false)
        => SingleBlockDocument(SupportedRenderers.Text.ToBlockType(), new EditorJsBlockData { Text = text, Wrap = wrap }, is_authored_content);

    /// <summary>
    /// Builds the document for a single-line field, as <see cref="TextDocument(string, string, bool)"/> does, taking the
    /// <c>wrap</c> tag as a <see cref="TextWrapType"/> so the tag is never retyped.
    /// </summary>
    /// <param name="text">The line, as inline HTML.</param>
    /// <param name="wrap">The <c>wrap</c> tag the field's editor is configured with.</param>
    /// <param name="is_authored_content">As for <see cref="TextDocument(string, string, bool)"/>: a template by default.</param>
    /// <returns>The serialised Editor.js document.</returns>
    public static string TextDocument(string text, TextWrapType wrap, bool is_authored_content = false)
        => TextDocument(text, wrap.ToWrap(), is_authored_content);

    /// <summary>
    /// Builds the document for a short body — a note, a message: exactly one <c>paragraph</c> block holding the text. The
    /// text is JSON-escaped, so quotes and control characters are safe, and inline HTML in it is kept as written; encode
    /// plain text from a user first so that a <c>&lt;</c> in it reads as a character rather than a tag.
    /// </summary>
    /// <param name="text">The paragraph, as inline HTML.</param>
    /// <param name="is_authored_content">As for <see cref="TextDocument(string, string, bool)"/>: a template by default.</param>
    /// <returns>The serialised Editor.js document.</returns>
    public static string ParagraphDocument(string text, bool is_authored_content = false)
        => SingleBlockDocument(SupportedRenderers.Paragraph.ToBlockType(), new EditorJsBlockData { Text = text }, is_authored_content);

    /// <summary>
    /// Builds and serialises a document holding exactly one block: the shared body of <see cref="TextDocument(string, string, bool)"/>
    /// and <see cref="ParagraphDocument(string, bool)"/>. The document is stamped with the current time and a fresh block
    /// identifier, as Editor.js would write it. Its version says whether it is a template: placeholder text no one has
    /// authored carries <see cref="EditorJsBlocks.EmptyVersion"/>, so a field it fills still counts as not authored until
    /// someone writes it; authored content carries <see cref="EditorJsBlocks.ContentVersion"/>.
    /// </summary>
    /// <param name="type">The block's type, as the Editor.js tool names it (e.g. <c>text</c> or <c>paragraph</c>).</param>
    /// <param name="data">The block's data, holding only the fields that tool writes; unset fields are left out.</param>
    /// <param name="is_authored_content">
    /// <see langword="true"/> when the text is authored content, stamped <see cref="EditorJsBlocks.ContentVersion"/>;
    /// <see langword="false"/> when it is a template, stamped <see cref="EditorJsBlocks.EmptyVersion"/>.
    /// </param>
    /// <returns>The serialised Editor.js document.</returns>
    private static string SingleBlockDocument(string type, EditorJsBlockData data, bool is_authored_content)
    {
        EditorJsBlocks document = new()
        {
            Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Blocks = [new EditorJsBlock { Id = EditorJsBlock.NewId(), Type = type, Data = data }],
            Version = is_authored_content is true ? EditorJsBlocks.ContentVersion : EditorJsBlocks.EmptyVersion
        };

        return JsonSerializer.Serialize(document, _single_block_document_serializer_options);
    }

    /// <summary>
    /// Adds a specified <see cref="EditorJsBlock"/> to the current <see cref="EditorJsBlocks"/> instance.
    /// </summary>
    /// <param name="editor_js_blocks">The Editor.js blocks collection to add the block to.</param>
    /// <param name="block">The block to add to the collection.</param>
    /// <returns>The updated <see cref="EditorJsBlocks"/> instance with the new block added, enabling method chaining.</returns>
    /// <remarks>This extension method provides a fluent interface for building Editor.js content programmatically.</remarks>
    public static EditorJsBlocks AddBlock(this EditorJsBlocks editor_js_blocks, EditorJsBlock block)
    {
        editor_js_blocks.Blocks.Add(block);
        return editor_js_blocks;
    }

    /// <summary>
    /// Adds an empty <see cref="EditorJsBlock"/> to the current <see cref="EditorJsBlocks"/> instance.
    /// </summary>
    /// <param name="editor_js_blocks">The Editor.js blocks collection to add the empty block to.</param>
    /// <returns>The updated <see cref="EditorJsBlocks"/> instance with the empty block added, enabling method chaining.</returns>
    /// <remarks>This method is useful for adding placeholder blocks or initialising content structure with empty elements.</remarks>
    public static EditorJsBlocks AddEmptyBlock(this EditorJsBlocks editor_js_blocks)
    {
        editor_js_blocks.Blocks.Add(EditorJsBlock.Empty);
        return editor_js_blocks;
    }
}
