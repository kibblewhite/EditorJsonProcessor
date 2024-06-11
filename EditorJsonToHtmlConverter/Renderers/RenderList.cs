namespace EditorJsonToHtmlConverter.Renderers;

public sealed class RenderList : IBlockRenderer
{
    public static void Render(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {

        string? id = block.Id;
        string? style = block?.Data?.Style;
        List<EditorJsBlockContent>? items = block?.Data?.Items;

        if (items == null) { return; }

        render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, style == "ordered" ? "ol" : "ul");
        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "id", id);

        EditorJsStylingMap? css = render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.List && item.Id == id);
        css ??= render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.List && item.Id == null);

        if (css is not null)
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "class", css.Style);
        }

        foreach (EditorJsBlockContent item in items)
        {
            render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "li"); // Added "li" element name

            if (css is not null && css.ItemStyle is not null)
            {
                render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "class", css.ItemStyle);
            }

            // Render item content
            RenderListItemContent(render_tree_builder, item);

            // Check and render nested lists
            if (item.Items != null && item.Items.Count > 0)
            {
                RenderNestedList(render_tree_builder, item.Items);
            }

            render_tree_builder.Builder.CloseElement(); // Closes "li" element
        }

        render_tree_builder.Builder.CloseElement(); // Closes "ol" or "ul" element
    }

    private static void RenderListItemContent(CustomRenderTreeBuilder render_tree_builder, EditorJsBlockContent item)
    {
        if (item.Content != null)
        {
            render_tree_builder.Builder.AddMarkupContent(render_tree_builder.SequenceCounter, item.Content);
        }
    }

    private static void RenderNestedList(CustomRenderTreeBuilder render_tree_builder, List<EditorJsBlockContent> items)
    {
        render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "ul");

        foreach (EditorJsBlockContent subItem in items)
        {
            render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "li"); // Added "li" element name
            RenderListItemContent(render_tree_builder, subItem);
            if (subItem.Items is not null)
            {
                RenderNestedList(render_tree_builder, subItem.Items);
            }

            render_tree_builder.Builder.CloseElement(); // Closes "li" element
        }

        render_tree_builder.Builder.CloseElement(); // Closes "ul" element
    }
}
