namespace EditorJsonToHtmlConverter.Models;

public sealed class EditorJsBlock
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("data")]
    public required EditorJsBlockData Data { get; set; }
}
