namespace EditorJsonToHtmlConverter.Models;

public sealed class EditorJsBlock : IEditorJsEntity<EditorJsBlock>
{
    [JsonIgnore]
    public static EditorJsBlock Empty => new()
    {
        Id = string.Empty,
        Type = string.Empty,
        Data = EditorJsBlockData.Empty
    };

    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("data")]
    public required EditorJsBlockData Data { get; set; }
}