namespace EditorJsonToHtmlConverter.Renderers;

/// <summary>
/// Renders a list of items each shown with a read-only tick box.
/// </summary>
/// <remarks>
/// <para>
/// Use a checklist where each entry has a completed state worth showing — a preparation list, a set of
/// prerequisites. Where the entries are simply parallel points, a plain <c>list</c> reads better. The boxes
/// are always rendered disabled: this is a record of state, not a control the reader can operate.
/// </para>
/// <para>
/// <c>items</c> (required) — the entries; the block renders nothing when the field is missing. Each entry
/// supplies its text in <c>text</c> — not <c>content</c>, which is the <c>list</c> block's field and is
/// ignored here — and its state in <c>checked</c>, which defaults to false when absent. An entry without
/// <c>text</c> renders as a bare box with no label, and the Editor.js checklist tool discards it on its next
/// save. Keep the list flat: this renderer draws nested <c>items</c>, but the checklist tool does not support
/// nesting and drops them on its next save.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// {
///   "id": "c1a2b3c4d5",
///   "type": "checklist",
///   "data": {
///     "items": [
///       { "text": "Photo identification", "checked": true },
///       { "text": "Printed ticket", "checked": false }
///     ]
///   }
/// }
/// </code>
/// </example>
public sealed class RenderChecklist : IBlockRenderer
{
    /// <inheritdoc />
    public static SupportedRenderers BlockType => SupportedRenderers.Checklist;

    public static void Render(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {
        string id = block.Id;
        List<EditorJsBlockContent>? items = block.Data.Items;

        if (items == null) { return; }

        render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "ul");
        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "id", id);
        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "role", "group");
        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "style", "list-style-type: none;");

        EditorJsStylingMap? css = render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.Checklist && item.Id == id);
        css ??= render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.Checklist && item.Id == null);

        if (css is not null)
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "class", css.Style);
        }

        foreach (EditorJsBlockContent item in items)
        {
            render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "li");
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "aria-hidden", "true");

            if (css is not null && css.ItemStyle is not null)
            {
                render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "class", css.ItemStyle);
            }

            // Render the checkbox
            render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "input");
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "type", "checkbox");
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "disabled", "true");

            if (item.Checked ?? false)
            {
                render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "checked", "checked");
            }

            render_tree_builder.Builder.CloseElement(); // Close the input

            // Render checklist item text
            render_tree_builder.Builder.AddMarkupContent(render_tree_builder.SequenceCounter, item.Text ?? string.Empty);

            // Check and render nested checklists
            if (item.Items != null && item.Items.Count > 0)
            {
                RenderNestedCheckList(render_tree_builder, item.Items);
            }

            render_tree_builder.Builder.CloseElement(); // Close the li
        }

        render_tree_builder.Builder.CloseElement(); // Close the ul
    }

    private static void RenderNestedCheckList(CustomRenderTreeBuilder render_tree_builder, List<EditorJsBlockContent>? items)
    {
        if (items == null) return;

        render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "ul");

        foreach (EditorJsBlockContent item in items)
        {
            render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "li");

            // Render the checkbox
            render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "input");
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "type", "checkbox");
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "disabled", "true");
            if (item.Checked ?? false)
            {
                render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "checked", "checked");
            }

            render_tree_builder.Builder.CloseElement(); // Close the input

            // Render checklist item text
            render_tree_builder.Builder.AddMarkupContent(render_tree_builder.SequenceCounter, item.Text ?? string.Empty);

            // Check and render nested checklists
            if (item.Items != null && item.Items.Count > 0)
            {
                RenderNestedCheckList(render_tree_builder, item.Items);
            }

            render_tree_builder.Builder.CloseElement(); // Close the li
        }

        render_tree_builder.Builder.CloseElement(); // Close the ul
    }
}
