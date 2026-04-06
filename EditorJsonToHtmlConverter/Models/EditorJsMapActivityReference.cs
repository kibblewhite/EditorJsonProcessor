namespace EditorJsonToHtmlConverter.Models;

/// <summary>
/// Represents an activity GUID reference with a space GUID snapshot, stored in map block data.
/// </summary>
public sealed class EditorJsMapActivityReference : IEditorJsEntity<EditorJsMapActivityReference>
{
    [JsonIgnore]
    public static EditorJsMapActivityReference Empty => new();

    /// <summary>
    /// Gets or sets the activity GUID.
    /// </summary>
    [JsonPropertyName("activityGuid")]
    public string? ActivityGuid { get; set; }

    /// <summary>
    /// Gets or sets the space GUID snapshot captured at save time.
    /// </summary>
    [JsonPropertyName("spaceGuid")]
    public string? SpaceGuid { get; set; }
}
