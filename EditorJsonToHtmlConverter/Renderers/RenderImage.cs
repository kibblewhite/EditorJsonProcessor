namespace EditorJsonToHtmlConverter.Renderers;

public sealed class RenderImage : IBlockRenderer
{
    public static void Render(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {
        string? id = block.Id;
        string? url = block.Data?.Url;
        string? caption = block.Data?.Caption;
        bool withBorder = block.Data?.WithBorder ?? false;
        bool withBackground = block.Data?.WithBackground ?? false;
        bool stretched = block.Data?.Stretched ?? false;

        // Render image
        render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "img");
        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "id", id);

        EditorJsStylingMap? css = render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.Image && item.Id == id);
        css ??= render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.Image && item.Id == null);

        if (css is not null)
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "class", css.Style);
        }

        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "src", url);
        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "alt", caption);

        StringBuilder styleBuilder = new();

        if (withBorder)
        {
            styleBuilder.Append("border: 1px solid #ddd; ");
        }

        if (withBackground)
        {
            styleBuilder.Append("background-color: #f2f2f2; ");
        }

        if (stretched)
        {
            styleBuilder.Append("width: 100%; ");
        }

        string style = styleBuilder.ToString().Trim();

        if (!string.IsNullOrWhiteSpace(style))
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "style", style);
        }

        render_tree_builder.Builder.CloseElement(); // Close the img element

        // Render caption
        if (!string.IsNullOrEmpty(caption))
        {
            render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "p");
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "style", "text-align: center;");
            render_tree_builder.Builder.AddMarkupContent(render_tree_builder.SequenceCounter, caption);
            render_tree_builder.Builder.CloseElement(); // Close the p element
        }
    }
}
