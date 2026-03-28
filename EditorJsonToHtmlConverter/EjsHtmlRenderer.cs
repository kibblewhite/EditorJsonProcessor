namespace EditorJsonToHtmlConverter;

/// <summary>
/// Renders the EjsRenderFragment component as HTML using a provided HtmlRenderer.
/// </summary>
/// <param name="html_renderer">The HtmlRenderer instance used to render the components.</param>
/// <param name="data_retrieval_mode">Controls whether leaflet-map blocks render embedded data or GUID references. Defaults to Embedded.</param>
public sealed partial class EjsHtmlRenderer(HtmlRenderer html_renderer, DataRetrievalMode data_retrieval_mode = DataRetrievalMode.Embedded)
{
    private readonly HtmlRenderer _html_renderer = html_renderer;
    private readonly DataRetrievalMode _data_retrieval_mode = data_retrieval_mode;

    [GeneratedRegex(@"</?.+?>")]
    private static partial Regex StripHtmlRegex();

    /// <summary>
    /// Parses the given JSON value and optional styling map to generate the corresponding HTML string.
    /// </summary>
    /// <param name="value">The JSON output from the EditorJS block editor.</param>
    /// <param name="strip_html">When true, this will perform a basic stripping of HTML based on regular expression matching on the returning string value.</param>
    /// <param name="styling_map">The JSON string representing the styling map. Default is an empty array.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the generated HTML string.</returns>
    public async Task<string> ParseAsync(string value, bool strip_html = false, string? styling_map = "[]")
    {
        ParameterView parameters = BuildParameters(value, styling_map, _data_retrieval_mode);
        string fragment = await RenderComponentAsHtmlAsync<EjsRenderFragment>(parameters);

        return strip_html
            ? WebUtility.HtmlDecode(StripHtmlRegex().Replace(fragment, string.Empty))
            : fragment;
    }

    /// <summary>
    /// Parses the given JSON value and optional styling map to generate the corresponding HTML root component.
    /// </summary>
    /// <param name="value">The JSON output from the EditorJS block editor.</param>
    /// <param name="styling_map">The JSON string representing the styling map. Default is an empty array.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the generated HTML root component.</returns>
    public async Task<HtmlRootComponent> ParseAsHtmlRootComponentAsync(string value, string? styling_map = "[]")
    {
        ParameterView parameters = BuildParameters(value, styling_map, _data_retrieval_mode);
        return await RenderComponentAsHtmlRootComponentAsync<EjsRenderFragment>(parameters);
    }

    /// <summary>
    /// Builds the parameters needed for rendering the component.
    /// </summary>
    /// <param name="value">The JSON output from the EditorJS block editor.</param>
    /// <param name="styling_map">The JSON string representing the styling map. Default is an empty array.</param>
    /// <param name="data_retrieval_mode">Controls whether leaflet-map blocks render embedded data or GUID references.</param>
    /// <returns>A ParameterView containing the parameters for the component.</returns>
    private static ParameterView BuildParameters(string value, string? styling_map = "[]", DataRetrievalMode data_retrieval_mode = DataRetrievalMode.Embedded) =>
        ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            { nameof(EjsRenderFragment.Value), value },
            { nameof(EjsRenderFragment.StylingMap), styling_map },
            { nameof(EjsRenderFragment.DataRetrievalMode), data_retrieval_mode }
        });

    /// <summary>
    /// Renders the component as an HTML root component asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the component to render.</typeparam>
    /// <param name="parameters">The parameters to pass to the component.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the rendered HTML root component.</returns>
    private Task<HtmlRootComponent> RenderComponentAsHtmlRootComponentAsync<T>(ParameterView parameters) where T : IComponent =>
        _html_renderer.Dispatcher.InvokeAsync(() => _html_renderer.RenderComponentAsync<T>(parameters));

    /// <summary>
    /// Renders the component as an HTML string asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the component to render.</typeparam>
    /// <param name="parameters">The parameters to pass to the component.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the rendered HTML string.</returns>
    private Task<string> RenderComponentAsHtmlAsync<T>(ParameterView parameters) where T : IComponent =>
        _html_renderer.Dispatcher.InvokeAsync(async () => {
            HtmlRootComponent output = await _html_renderer.RenderComponentAsync<T>(parameters);
            return output.ToHtmlString();
        });
}
