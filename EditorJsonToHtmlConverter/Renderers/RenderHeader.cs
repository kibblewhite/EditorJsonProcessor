namespace EditorJsonToHtmlConverter.Renderers;

/// <summary>
/// Renders a section heading.
/// </summary>
/// <remarks>
/// <para>
/// Use headings to give a long body a scannable structure. Keep one level-2 heading per major section and
/// nest deeper levels beneath it rather than skipping levels.
/// </para>
/// <para>
/// <c>level</c> (required) — the heading depth, 1 to 6, rendered as the matching HTML heading element.
/// <c>text</c> (optional) — the heading itself; inline HTML is preserved, and an absent value renders an
/// empty heading.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// { "id": "h1a2b3c4d5", "type": "header", "data": { "text": "What is included", "level": 2 } }
/// </code>
/// </example>
public sealed class RenderHeader : IBlockRenderer
{
    /// <inheritdoc />
    public static SupportedRenderers BlockType => SupportedRenderers.Header;

    public static void Render(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {
        string id = block.Id;
        int? level = block.Data.Level;
        string? text = block.Data.Text;

        render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, $"h{level}");
        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "id", id);

        EditorJsStylingMap? css = render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.Header && item.Level == level && item.Id == id);
        css ??= render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.Header && item.Level == level && item.Id == null);

        if (css is not null)
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "class", css.Style);
        }

        render_tree_builder.Builder.AddMarkupContent(render_tree_builder.SequenceCounter, text ?? string.Empty);
        render_tree_builder.Builder.CloseElement();
    }
}
