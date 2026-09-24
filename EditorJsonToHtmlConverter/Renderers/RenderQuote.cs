namespace EditorJsonToHtmlConverter.Renderers;

/// <summary>
/// Renders a quotation with an optional attribution line.
/// </summary>
/// <remarks>
/// <para>
/// Use a quote for words attributable to a person or source — a testimonial, a policy extract. A passage
/// that simply needs emphasis is a paragraph, not a quote.
/// </para>
/// <para>
/// <c>text</c> (optional) — the quoted passage; inline HTML is preserved. <c>caption</c> (optional) — the
/// attribution, rendered as a footer beneath the passage and omitted entirely when absent.
/// <c>alignment</c> (optional) — <c>"left"</c> or <c>"center"</c>, applied as an alignment class. The
/// Editor.js quote tool offers only these two and replaces any other value, including <c>"right"</c>, with
/// <c>"left"</c> when the block is next opened.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// {
///   "id": "q1a2b3c4d5",
///   "type": "quote",
///   "data": { "text": "The best evening we have had all year.", "caption": "A. Visitor", "alignment": "left" }
/// }
/// </code>
/// </example>
public sealed class RenderQuote : IBlockRenderer
{
    /// <inheritdoc />
    public static SupportedRenderers BlockType => SupportedRenderers.Quote;

    public static void Render(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {
        string id = block.Id;
        string? text = block.Data.Text;
        string? caption = block.Data.Caption;
        string? alignment = block.Data.Alignment;

        render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "blockquote");
        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "id", id);

        EditorJsStylingMap? css = render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.Quote && item.Id == id);
        css ??= render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.Quote && item.Id == null);

        if (css is not null && !string.IsNullOrEmpty(alignment))
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "class", $"text-{alignment} {css.Style}");
        }
        else if (css is not null)
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "class", css.Style);
        }
        else if (!string.IsNullOrEmpty(alignment))
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "class", $"text-{alignment}");
        }

        render_tree_builder.Builder.AddMarkupContent(render_tree_builder.SequenceCounter, text ?? string.Empty);

        if (!string.IsNullOrEmpty(caption))
        {
            render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "footer");

            if (css is not null && css.FooterStyle is not null)
            {
                render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "class", css.FooterStyle);
            }

            render_tree_builder.Builder.AddMarkupContent(render_tree_builder.SequenceCounter, caption);
            render_tree_builder.Builder.CloseElement(); // Close the footer
        }

        render_tree_builder.Builder.CloseElement(); // Close the blockquote
    }
}
