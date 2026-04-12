namespace EditorJsonToHtmlConverter;

/// <summary>
/// A Razor component that converts JSON output from the EditorJS block editor into a Blazor RenderFragment.
/// </summary>
public partial class EjsRenderFragment : ComponentBase
{
    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter] public required RenderFragment ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the JSON output from the EditorJS block editor.
    /// </summary>
    /// <remarks>This JSON string is used to generate a segment of UI content.</remarks>
    [Parameter] public required string Value { get; set; }

    /// <summary>
    /// Gets or sets the JSON string that defines the styling map for the EditorJS blocks.
    /// </summary>
    /// <remarks>This JSON string is used to apply styles to the UI content.</remarks>
    [Parameter] public required string StylingMap { get; set; }

    /// <summary>
    /// Gets or sets the data retrieval mode that controls whether map blocks render
    /// embedded resolved data or data-* attribute references.
    /// </summary>
    [Parameter] public required DataRetrievalMode DataRetrievalMode { get; set; }

    /// <summary>
    /// Gets or sets the locale for rendering. Available to block renderers for locale-aware
    /// output (e.g. data-locale attributes for client-side hydration). Null means omitted.
    /// </summary>
    [Parameter] public CultureInfo? Locale { get; set; }

    /// <summary>
    /// Caller-supplied identifier echoed back on <see cref="RenderCompleted"/> so external
    /// listeners can correlate a render with an outer request or element. Defaults to
    /// <see cref="Guid.Empty"/> when the caller does not supply one.
    /// </summary>
    [Parameter] public Guid CorrelationIdentifier { get; set; }

    /// <summary>
    /// Callback invoked after the component has produced its first non-empty render.
    /// Receives an <see cref="EjsRenderCompletedEventArgs"/> carrying the correlation
    /// identifier and the wall-clock time taken to build the render fragment.
    /// </summary>
    [Parameter] public EventCallback<EjsRenderCompletedEventArgs> RenderCompleted { get; set; }

    /// <summary>
    /// Gets or sets the logger instance used for logging within the component.
    /// </summary>
    [Inject] public required ILogger<EjsRenderFragment> Logger { get; init; }

    /// <summary>
    /// Indicates whether the component's child's render fragment has been built.
    /// </summary>
    /// <remarks>
    /// Once the component has had it's render fragment built, it cannot be updated or amended at this time.
    /// </remarks>
    protected bool ChildRenderFragmentBuilt;

    // note: separate internal field for the built render fragment. Previously the component
    // reassigned ChildContent directly, but ChildContent is a [Parameter] — Blazor reassigns
    // it on every parent re-render, wiping out the built content and reverting to whatever
    // the parent originally passed (typically a "Loading..." placeholder).
    private RenderFragment? _rendered_content;

    private long _render_start_timestamp;
    private bool _render_completed_fired;

    protected override Task OnInitializedAsync()
    {
        _render_start_timestamp = Stopwatch.GetTimestamp();
        return BuildChildRenderFragmentAsync();
    }

    protected override async Task OnAfterRenderAsync(bool first_render)
    {
        await BuildChildRenderFragmentAsync();

        if (_render_completed_fired || ChildRenderFragmentBuilt is false)
        {
            return;
        }

        if (RenderCompleted.HasDelegate is false)
        {
            return;
        }

        await RenderCompleted.InvokeAsync(new EjsRenderCompletedEventArgs
        {
            CorrelationIdentifier = CorrelationIdentifier,
            Elapsed = Stopwatch.GetElapsedTime(_render_start_timestamp)
        });

        _render_completed_fired = true;
    }

    private Task BuildChildRenderFragmentAsync()
    {
        if (ChildRenderFragmentBuilt || string.IsNullOrWhiteSpace(Value))
        {
            return Task.CompletedTask;
        }

        _rendered_content = ConvertJsonToRenderFragment;
        ChildRenderFragmentBuilt = true;
        return InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Converts the JSON input into a RenderFragment for rendering in the Blazor component.
    /// </summary>
    private RenderFragment ConvertJsonToRenderFragment => builder =>
    {
        EditorJsBlocks blocks;
        IEnumerable<EditorJsStylingMap> editor_js_styling_map;

        try
        {
            blocks = JsonSerializer.Deserialize<EditorJsBlocks>(Value)
                ?? throw new JsonException("Deserialised EditorJsBlocks was null.");
        }
        catch (JsonException ex)
        {
            Logger.LogError("Deserialise EditorJsBlocks Failed: {Exception}", ex.Message);
            throw;
        }

        try
        {
            editor_js_styling_map = JsonSerializer.Deserialize<IEnumerable<EditorJsStylingMap>>(StylingMap)
                ?? [];
        }
        catch (JsonException ex)
        {
            Logger.LogError("Deserialise EditorJsStylingMap Failed: {Exception}", ex.Message);
            Logger.LogError("StylingMap: {StylingMap}", StylingMap);
            editor_js_styling_map = [];
        }

        try
        {
            CustomRenderTreeBuilder custom_render_tree_builder = new()
            {
                Builder = builder,
                StylingMap = editor_js_styling_map.ToList().AsReadOnly(),
                DataRetrievalMode = DataRetrievalMode,
                Locale = Locale
            };

            foreach (EditorJsBlock block in blocks.Blocks)
            {
                RenderBlock(custom_render_tree_builder, block);
            }
        }
        catch (InvalidOperationException ex)
        {
            Logger.LogTrace("RenderBlock Failed or was null value: {Exception}", ex.Message);
            return;
        }
    };

    /// <summary>
    /// Renders a single EditorJS block using the appropriate renderer based on the block type.
    /// </summary>
    /// <param name="render_tree_builder">The custom render tree builder used for rendering.</param>
    /// <param name="block">The EditorJS block to render.</param>
    internal static void RenderBlock(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {
        if (Enum.TryParse(block.Type, true, out SupportedRenderers renderer) is false || renderer == SupportedRenderers.Empty)
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
            case SupportedRenderers.Map:
                RenderMap.Render(render_tree_builder, block);
                break;
        }
    }
}
