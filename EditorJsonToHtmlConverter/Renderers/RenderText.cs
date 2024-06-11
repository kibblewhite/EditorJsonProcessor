namespace EditorJsonToHtmlConverter.Renderers;

public sealed class RenderText : IBlockRenderer
{
    public static void Render(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {
        string? text = block.Data?.Text;
        render_tree_builder.Builder.AddMarkupContent(render_tree_builder.SequenceCounter, text);
    }
}
