namespace EditorJsonToHtmlConverter.Models;

public sealed class EditorJsBlocks : IEditorJsEntity<EditorJsBlocks>
{
    /// <summary>
    /// The version written into a document built outside the editor. Editor.js replaces it with its own library version
    /// (e.g. <c>2.31.0</c>) when the document is next saved, so a document still carrying it has not been edited.
    /// </summary>
    /// <remarks>
    /// It also marks a template: a document with no content, or with placeholder content no one has authored yet (the
    /// builders write it by default). A content field still holding it counts as not authored; see
    /// <see cref="Extensions.EditorJsBlocksExtensions.IsEmptyTemplate"/>.
    /// </remarks>
    public const string EmptyVersion = "0.0.0";

    /// <summary>
    /// The version the builders write when asked to build authored content (their <c>is_authored_content</c> argument),
    /// so the document is never mistaken for a template. Any value other than <see cref="EmptyVersion"/> would do; this
    /// one follows no Editor.js release, so it never needs updating.
    /// </summary>
    public const string ContentVersion = "1.0.0";

    [JsonIgnore]
    public static EditorJsBlocks Empty => new()
    {
        Time = 0,
        Blocks = [],
        Version = EmptyVersion
    };

    [JsonPropertyName("time")]
    public required long Time { get; set; }

    [JsonPropertyName("blocks")]
    public required List<EditorJsBlock> Blocks { get; set; }

    [JsonPropertyName("version")]
    public required string Version { get; set; }
}
