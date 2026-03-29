namespace EditorJsonToHtmlConverter;

/// <summary>
/// Wraps a <see cref="RenderTreeBuilder"/> with additional rendering context used by
/// block renderers: styling lookups, data retrieval mode, locale, and an auto-incrementing
/// sequence counter for Blazor render tree operations.
/// </summary>
public class CustomRenderTreeBuilder
{
    /// <summary>
    /// The styling map used to resolve CSS classes for individual blocks by type and optional block ID.
    /// </summary>
    public required IReadOnlyList<EditorJsStylingMap> StylingMap { get; init; }

    /// <summary>
    /// The underlying Blazor <see cref="RenderTreeBuilder"/> used to emit HTML elements and attributes.
    /// </summary>
    public required RenderTreeBuilder Builder { get; init; }

    /// <summary>
    /// Controls whether leaflet-map blocks render embedded resolved data or data-* attribute
    /// references for external JS viewer hydration.
    /// </summary>
    public required DataRetrievalMode DataRetrievalMode { get; init; }

    /// <summary>
    /// The locale for rendering. Available to block renderers for locale-aware output
    /// (e.g. <c>data-locale</c> attributes for client-side hydration). Null means omitted.
    /// </summary>
    public CultureInfo? Locale { get; init; }

    /// <summary>
    /// Returns the current sequence count and increments it by one. Each call to this property
    /// produces a unique sequence number for Blazor render tree operations.
    /// </summary>
    public int SequenceCounter => _sequence_count++;
    private int _sequence_count = 0;

    /// <summary>
    /// Returns the current sequence count without incrementing it.
    /// </summary>
    public int GetSequenceCount => _sequence_count;
}
