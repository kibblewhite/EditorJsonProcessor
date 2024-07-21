namespace EditorJsonToHtmlConverter;

/// <summary>
/// Renders the EjsRenderFragment component as HTML using a provided HtmlRenderer.
/// </summary>
/// <param name="html_renderer">The HtmlRenderer instance used to render the components.</param>
public sealed partial class EjsHtmlRenderer(HtmlRenderer html_renderer)
{
    private readonly HtmlRenderer _html_renderer = html_renderer;

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
        ParameterView parameters = BuildParameters(value, styling_map);
        string fragment = await RenderComponentAsHtmlAsync<EjsRenderFragment>(parameters);

        if (strip_html is false)
        {
            return fragment;
        }

        Regex tags_expression = StripHtmlRegex();
        string striped_fragement = tags_expression.Replace(fragment, string.Empty);
        return WebUtility.HtmlDecode(striped_fragement);
    }

    /// <summary>
    /// Parses the given JSON value and optional styling map to generate the corresponding HTML root component.
    /// </summary>
    /// <param name="value">The JSON output from the EditorJS block editor.</param>
    /// <param name="styling_map">The JSON string representing the styling map. Default is an empty array.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the generated HTML root component.</returns>
    public async Task<HtmlRootComponent> ParseAsHtmlRootComponentAsync(string value, string? styling_map = "[]")
    {
        ParameterView parameters = BuildParameters(value, styling_map);
        return await RenderComponentAsHtmlRootComponentAsync<EjsRenderFragment>(parameters);
    }

    /// <summary>
    /// Builds the parameters needed for rendering the component.
    /// </summary>
    /// <param name="value">The JSON output from the EditorJS block editor.</param>
    /// <param name="styling_map">The JSON string representing the styling map. Default is an empty array.</param>
    /// <returns>A ParameterView containing the parameters for the component.</returns>
    private static ParameterView BuildParameters(string value, string? styling_map = "[]")
    {
        Dictionary<string, object?> dictionary = new()
        {
            { nameof(EjsRenderFragment.Value), value },
            { nameof(EjsRenderFragment.StylingMap), styling_map }
        };

        ParameterView parameters = ParameterView.FromDictionary(dictionary);
        return parameters;
    }

    /// <summary>
    /// Renders the component as an HTML root component asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the component to render.</typeparam>
    /// <param name="parameters">The parameters to pass to the component.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the rendered HTML root component.</returns>
    private Task<HtmlRootComponent> RenderComponentAsHtmlRootComponentAsync<T>(ParameterView parameters) where T : IComponent =>
        _html_renderer.Dispatcher.InvokeAsync(async () => await _html_renderer.RenderComponentAsync<T>(parameters));

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
