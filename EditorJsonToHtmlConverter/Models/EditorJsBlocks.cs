namespace EditorJsonToHtmlConverter.Models;

public sealed class EditorJsBlocks : IEditorJsEntity<EditorJsBlocks>
{
    /// <summary>
    /// The version written into a document built outside the editor. Editor.js replaces it with its own library version
    /// (e.g. <c>2.31.0</c>) when the document is next saved, so a document still carrying it has not been edited.
    /// </summary>
    public const string EmptyVersion = "0.0.0";

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
