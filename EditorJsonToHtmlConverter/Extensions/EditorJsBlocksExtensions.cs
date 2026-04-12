namespace EditorJsonToHtmlConverter.Extensions;

public static class EditorJsBlocksExtensions
{
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
