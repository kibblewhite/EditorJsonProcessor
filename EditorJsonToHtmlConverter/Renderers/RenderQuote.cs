namespace EditorJsonToHtmlConverter.Renderers;

public sealed class RenderQuote : IBlockRenderer
{
    public static void Render(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {
        string? id = block.Id;
        string? text = block?.Data?.Text;
        string? caption = block?.Data?.Caption;
        string? alignment = block?.Data?.Alignment;

        render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "blockquote");
        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "id", id);

        EditorJsStylingMap? css = render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.Quote && item.Id == id);
        css ??= render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.Quote && item.Id == null);

        if (css is not null && !string.IsNullOrEmpty(alignment))
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "class", $"text-{alignment} {css.Style}");
        }

        if (css is not null && string.IsNullOrEmpty(alignment))
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "class", css.Style);
        }

        if (css is null && !string.IsNullOrEmpty(alignment))
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "class", $"text-{alignment}");
        }

        render_tree_builder.Builder.AddMarkupContent(render_tree_builder.SequenceCounter, text);

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
