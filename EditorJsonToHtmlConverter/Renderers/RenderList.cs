namespace EditorJsonToHtmlConverter.Renderers;

/// <summary>
/// Renders an ordered or unordered list, including nested sub-lists.
/// </summary>
/// <remarks>
/// <para>
/// Choose a list for parallel items — inclusions, steps, requirements. Use <c>checklist</c> instead when each
/// item carries a done/not-done state.
/// </para>
/// <para>
/// <c>style</c> (optional) — <c>"ordered"</c> renders a numbered list; any other value, or its absence,
/// renders a bulleted one. <c>items</c> (required) — the entries; the block renders nothing at all when the
/// field is missing. Each entry supplies its text in <c>content</c> (or <c>text</c>) and may carry its own
/// <c>items</c> array to nest a sub-list to any depth.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// {
///   "id": "l1a2b3c4d5",
///   "type": "list",
///   "data": {
///     "style": "unordered",
///     "items": [
///       { "content": "Welcome drink" },
///       { "content": "Two-course meal", "items": [ { "content": "Vegetarian option available" } ] }
///     ]
///   }
/// }
/// </code>
/// </example>
public sealed class RenderList : IBlockRenderer
{
    /// <inheritdoc />
    public static SupportedRenderers BlockType => SupportedRenderers.List;

    public static void Render(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {

        string id = block.Id;
        string? style = block.Data.Style;
        List<EditorJsBlockContent>? items = block.Data.Items;

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
