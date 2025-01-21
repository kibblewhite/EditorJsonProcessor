namespace EditorJsonToHtmlConverter.Models;

public interface IEditorJsEntity<T>
{
    [JsonIgnore]
    static abstract T Empty { get; }
}
