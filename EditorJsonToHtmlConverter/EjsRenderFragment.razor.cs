namespace EditorJsonToHtmlConverter;

/// <summary>
/// A Razor component that converts JSON output from the EditorJS block editor into a Blazor RenderFragment.
/// </summary>
public partial class EjsRenderFragment : ComponentBase
{
    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public required RenderFragment ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the JSON output from the EditorJS block editor.
    /// </summary>
    /// <remarks>This JSON string is used to generate a segment of UI content.</remarks>
    [Parameter]
    public required string Value { get; set; }

    /// <summary>
    /// Gets or sets the JSON string that defines the styling map for the EditorJS blocks.
    /// </summary>
    /// <remarks>This JSON string is used to apply styles to the UI content.</remarks>
    [Parameter]
    public required string StylingMap { get; set; } = "[]";

    /// <summary>
    /// Gets or sets the logger instance used for logging within the component.
    /// </summary>
    [Inject]
    public required ILogger<EjsRenderFragment> logger { get; init; }

    protected override void OnInitialized()
    {
        ChildContent = new(ConvertJsonToRenderFragment);
        StateHasChanged();
    }

    /// <summary>
    /// Converts the JSON input into a RenderFragment for rendering in the Blazor component.
    /// </summary>
    private RenderFragment ConvertJsonToRenderFragment => builder =>
    {

        EditorJsBlocks? blocks;
        IEnumerable<EditorJsStylingMap>? editor_js_styling_map;

        try
        {
            blocks = JsonSerializer.Deserialize<EditorJsBlocks>(Value);
            ArgumentNullException.ThrowIfNull(blocks, nameof(blocks));
        }
        catch (Exception ex)
        {
            logger.LogTrace("Deserialise EditorJsBlocks Failed: {Exception}", ex.Message);
            throw;
        }

        try
        {
            editor_js_styling_map = JsonSerializer.Deserialize<IEnumerable<EditorJsStylingMap>>(StylingMap);
            ArgumentNullException.ThrowIfNull(editor_js_styling_map, nameof(editor_js_styling_map));
        }
        catch (Exception ex)
        {
            logger.LogTrace("Deserialise EditorJsStylingMap Failed: {Exception}", ex.Message);
            throw;
        }

        CustomRenderTreeBuilder custom_render_tree_builder = new()
        {
            Builder = builder,
            StylingMap = editor_js_styling_map?.ToList().AsReadOnly() ?? Enumerable.Empty<EditorJsStylingMap>().ToList().AsReadOnly()
        };

        foreach (EditorJsBlock block in blocks.Blocks)
        {
            RenderBlock(custom_render_tree_builder, block);
        }
    };

    /// <summary>
    /// Renders a single EditorJS block using the appropriate renderer based on the block type.
    /// </summary>
    /// <param name="render_tree_builder">The custom render tree builder used for rendering.</param>
    /// <param name="block">The EditorJS block to render.</param>
    internal static void RenderBlock(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {
        if (Enum.TryParse(block.Type, true, out SupportedRenderers renderer) is false)
        {
            return;
        }

        switch (renderer)
        {
            case SupportedRenderers.Paragraph:
                RenderParagraph.Render(render_tree_builder, block);
                break;
            case SupportedRenderers.Header:
                RenderHeader.Render(render_tree_builder, block);
                break;
            case SupportedRenderers.List:
                RenderList.Render(render_tree_builder, block);
                break;
            case SupportedRenderers.Quote:
                RenderQuote.Render(render_tree_builder, block);
                break;
            case SupportedRenderers.Checklist:
                RenderChecklist.Render(render_tree_builder, block);
                break;
            case SupportedRenderers.Table:
                RenderTable.Render(render_tree_builder, block);
                break;
            case SupportedRenderers.Image:
                RenderImage.Render(render_tree_builder, block);
                break;
            case SupportedRenderers.Delimiter:
                RenderDelimiter.Render(render_tree_builder, block);
                break;
            case SupportedRenderers.Warning:
                RenderWarning.Render(render_tree_builder, block);
                break;
            case SupportedRenderers.Embed:
                RenderEmbed.Render(render_tree_builder, block);
                break;
            case SupportedRenderers.Text:
                RenderText.Render(render_tree_builder, block);
                break;
        }
    }
}
