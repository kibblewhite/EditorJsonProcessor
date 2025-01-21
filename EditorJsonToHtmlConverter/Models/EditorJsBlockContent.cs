namespace EditorJsonToHtmlConverter.Models;

public sealed class EditorJsBlockContent : IEditorJsEntity<EditorJsBlockContent>
{
    [JsonIgnore]
    public static EditorJsBlockContent Empty => new();

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("items")]
    public List<EditorJsBlockContent>? Items { get; set; }

    [JsonPropertyName("checked")]
    public bool? Checked { get; set; }  // Added property for checklist
}
