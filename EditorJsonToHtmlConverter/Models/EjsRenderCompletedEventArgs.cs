namespace EditorJsonToHtmlConverter.Models;

/// <summary>
/// Event payload supplied to the callback registered on <see cref="EjsHtmlRenderer"/>
/// once a parse/render call has finished.
/// </summary>
public sealed class EjsRenderCompletedEventArgs
{
    /// <summary>
    /// Caller-supplied identifier for correlating this render with an outer
    /// request or element. <see cref="Guid.Empty"/> when the caller did not supply one.
    /// </summary>
    public required Guid CorrelationIdentifier { get; init; }

    /// <summary>
    /// Wall-clock time taken to produce the rendered output.
    /// </summary>
    public required TimeSpan Elapsed { get; init; }
}
