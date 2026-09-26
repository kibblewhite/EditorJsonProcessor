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

    /// <summary>
    /// Mints the short identifier Editor.js gives each block: ten hex characters from the END of a v7 GUID, which is its
    /// random part. The first twelve hex characters of a v7 are a millisecond timestamp, so a ten-character prefix would
    /// repeat for every block created within the same quarter-second.
    /// </summary>
    /// <returns>A new ten-character block identifier.</returns>
    public static string NewId()
        => Guid.CreateVersion7().ToString("N")[^10..];

    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("data")]
    public required EditorJsBlockData Data { get; set; }
}