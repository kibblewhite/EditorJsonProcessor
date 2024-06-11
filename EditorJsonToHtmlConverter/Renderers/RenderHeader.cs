namespace EditorJsonToHtmlConverter.Renderers;

public sealed class RenderHeader : IBlockRenderer
{
    public static void Render(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {
        string? id = block.Id;
        int? level = block?.Data?.Level;
        string? text = block?.Data?.Text;

        render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, $"h{level}");
        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "id", id);

        EditorJsStylingMap? css = render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.Header && item.Level == level && item.Id == id);
        css ??= render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.Header && item.Level == level && item.Id == null);

        if (css is not null)
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "class", css.Style);
        }

        render_tree_builder.Builder.AddMarkupContent(render_tree_builder.SequenceCounter, text);
        render_tree_builder.Builder.CloseElement();
    }
}
