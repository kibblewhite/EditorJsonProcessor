namespace EditorJsonToHtmlConverter.Renderers;

public sealed class RenderWarning : IBlockRenderer
{
    public static void Render(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {
        string? id = block.Id;
        string? title = block.Data?.Title;
        string? message = block.Data?.Message;

        // Render warning block
        render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "div");
        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "id", id);

        EditorJsStylingMap? css = render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.Warning && item.Id == id);
        css ??= render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.Warning && item.Id == null);

        if (css is not null)
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "class", css.Style);
        }

        // Render title
        if (!string.IsNullOrEmpty(title))
        {
            render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "strong");
            render_tree_builder.Builder.AddMarkupContent(render_tree_builder.SequenceCounter, title);
            render_tree_builder.Builder.CloseElement(); // Close the strong element
        }

        // Render message
        if (!string.IsNullOrEmpty(message))
        {
            render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "p");
            render_tree_builder.Builder.AddMarkupContent(render_tree_builder.SequenceCounter, message);
            render_tree_builder.Builder.CloseElement(); // Close the p element
        }

        render_tree_builder.Builder.CloseElement(); // Close the div element
    }
}
