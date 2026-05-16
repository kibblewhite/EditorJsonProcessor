namespace EditorJsonToHtmlConverter;

/// <summary>
/// Renderer-level configuration. Carries cross-cutting values that the
/// rendering pipeline injects into individual block renderers via
/// <see cref="CustomRenderTreeBuilder"/>.
/// <para>
/// The <c>editorjs-leaflet</c> plugin no longer persists a <c>tileUrl</c>
/// field on map block data — the rendering layer is the source of truth
/// for which tile server to use. Set <see cref="TileUrlTemplate"/> via
/// <see cref="BuilderExtensions.AddScopedEditorJsonProcessorServices(
/// Microsoft.Extensions.DependencyInjection.IServiceCollection,
/// System.Action{EditorJsonProcessorOptions}?)"/>
/// and the map renderer will write <c>data-tile-url</c> (reference mode)
/// or the corresponding JSON field (embedded mode) on every map block.
/// </para>
/// </summary>
public sealed class EditorJsonProcessorOptions
{
    /// <summary>
    /// Tile URL template applied to every map block produced by the renderer.
    /// Use the standard Leaflet placeholders: <c>{z}</c>, <c>{x}</c>, <c>{y}</c>.
    /// When <c>null</c> the renderer omits <c>data-tile-url</c> entirely —
    /// the viewer will then surface a clear missing-tile-url error.
    /// </summary>
    public string? TileUrlTemplate { get; set; }
}
