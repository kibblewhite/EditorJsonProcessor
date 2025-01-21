namespace EditorJsonToHtmlConverter.Models;

public sealed class EditorJsBlocks : IEditorJsEntity<EditorJsBlocks>
{
    [JsonIgnore]
    public static EditorJsBlocks Empty => new()
    {
        Time = 0,
        Blocks = [],
        Version = string.Empty
    };

    [JsonPropertyName("time")]
    public required long Time { get; set; }

    [JsonPropertyName("blocks")]
    public required List<EditorJsBlock> Blocks { get; set; }

    [JsonPropertyName("version")]
    public required string Version { get; set; }
}
