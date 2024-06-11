namespace EditorJsonToHtmlConverter.Models;

public sealed class EditorJsBlocks
{
    [JsonPropertyName("time")]
    public required long Time { get; set; }

    [JsonPropertyName("blocks")]
    public required List<EditorJsBlock> Blocks { get; set; }

    [JsonPropertyName("version")]
    public required string Version { get; set; }
}
