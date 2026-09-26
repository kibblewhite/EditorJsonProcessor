namespace EditorJsonToHtmlConverter.Extensions;

public static class EditorJsBlocksExtensions
{
    // Unset block fields are left out, so a built block carries only the fields its tool writes, as the editor writes it.
    private static readonly JsonSerializerOptions _single_block_document_serializer_options =new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

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
    /// <returns>The serialised Editor.js document.</returns>
    public static string TextDocument(string text, string wrap)
        => SingleBlockDocument("text", new EditorJsBlockData { Text = text, Wrap = wrap });

    /// <summary>
    /// Builds the document for a short body — a note, a message: exactly one <c>paragraph</c> block holding the text. The
    /// text is JSON-escaped, so quotes and control characters are safe, and inline HTML in it is kept as written; encode
    /// plain text from a user first so that a <c>&lt;</c> in it reads as a character rather than a tag.
    /// </summary>
    /// <param name="text">The paragraph, as inline HTML.</param>
    /// <returns>The serialised Editor.js document.</returns>
    public static string ParagraphDocument(string text)
        => SingleBlockDocument("paragraph", new EditorJsBlockData { Text = text });

    private static string SingleBlockDocument(string type, EditorJsBlockData data)
    {
        EditorJsBlocks document = new()
        {
            Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Blocks = [new EditorJsBlock { Id = EditorJsBlock.NewId(), Type = type, Data = data }],
            Version = EditorJsBlocks.EmptyVersion
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
