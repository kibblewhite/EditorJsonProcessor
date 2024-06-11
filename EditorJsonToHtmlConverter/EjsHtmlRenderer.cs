namespace EditorJsonToHtmlConverter;

public sealed class EjsHtmlRenderer(HtmlRenderer html_renderer)
{
    private readonly HtmlRenderer _html_renderer = html_renderer;

    public async Task<string> ParseAsync(string value, string? styling_map = "[]")
    {
        HtmlRootComponent html_root_component = await ParseAsHtmlRootComponentAsync(value, styling_map);
        return html_root_component.ToHtmlString();
    }

    public Task<HtmlRootComponent> ParseAsHtmlRootComponentAsync(string value, string? styling_map = "[]")
    {
        Dictionary<string, object?> dictionary = new()
        {
            { nameof(EjsRenderFragment.Value), value },
            { nameof(EjsRenderFragment.StylingMap), styling_map }
        };

        ParameterView parameters = ParameterView.FromDictionary(dictionary);
        return RenderComponentAsync<EjsRenderFragment>(parameters);
    }

    private Task<HtmlRootComponent> RenderComponentAsync<T>(ParameterView parameters) where T : IComponent =>
        // Use the default dispatcher to invoke actions in the context of the static HTML renderer and return as a string
        _html_renderer.Dispatcher.InvokeAsync(async () => await _html_renderer.RenderComponentAsync<T>(parameters));
}
