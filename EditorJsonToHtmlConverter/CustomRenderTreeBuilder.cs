namespace EditorJsonToHtmlConverter;

public class CustomRenderTreeBuilder
{
    public required IReadOnlyList<EditorJsStylingMap> StylingMap { get; init; }
    public required RenderTreeBuilder Builder { get; init; }

    /// <summary>
    /// By accessing the <see cref="SequenceCounter"/> it will automatically increment the value by one.
    /// </summary>
    public int SequenceCounter => _sequence_count++;
    private int _sequence_count = 0;

    /// <summary>
    /// Use this to get the current <see cref="SequenceCount"/> without changing it's value
    /// </summary>
    public int GetSequenceCount => _sequence_count;
}
