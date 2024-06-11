namespace EditorJsonToHtmlConverter.Models;

public sealed class EditorJsStylingMap
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(SupportedRenderersConverter))]
    public required SupportedRenderers Type { get; init; }

    [JsonPropertyName("style")]
    public required string Style { get; init; }

    [JsonPropertyName("item-style")]
    public string? ItemStyle { get; init; }

    [JsonPropertyName("footer-style")]
    public string? FooterStyle { get; init; }

    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("level")]
    public int? Level { get; init; }
}
