namespace EditorJsonToHtmlConverter.Models;

/// <summary>
/// Represents the centre coordinates of a leaflet-map block.
/// </summary>
public sealed class EditorJsMapCenter : IEditorJsEntity<EditorJsMapCenter>
{
    [JsonIgnore]
    public static EditorJsMapCenter Empty => new();

    /// <summary>
    /// Gets or sets the latitude coordinate.
    /// </summary>
    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    /// <summary>
    /// Gets or sets the longitude coordinate.
    /// </summary>
    [JsonPropertyName("lng")]
    public double Lng { get; set; }
}
