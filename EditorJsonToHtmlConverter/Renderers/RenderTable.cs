namespace EditorJsonToHtmlConverter.Renderers;

/// <summary>
/// Renders a table of rows and cells, optionally with a header row.
/// </summary>
/// <remarks>
/// <para>
/// Use a table to compare several things across the same attributes — ticket types against price and
/// inclusions, sessions against times. Prose or a list reads better for anything that is not genuinely
/// two-dimensional, and a table with a single column is a list.
/// </para>
/// <para>
/// <c>content</c> (required) — an array of rows, each an array of cell strings; every row should carry the
/// same number of cells, and the block renders nothing when the field is missing. Cells preserve inline
/// HTML. <c>withHeadings</c> (optional, default false) — when true the FIRST row of <c>content</c> is
/// lifted out and rendered as the header row rather than as data.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// {
///   "id": "t1a2b3c4d5",
///   "type": "table",
///   "data": {
///     "withHeadings": true,
///     "content": [
///       [ "Ticket", "Price", "Includes" ],
///       [ "Standard", "£25", "Entry" ],
///       [ "Premium", "£40", "Entry and welcome drink" ]
///     ]
///   }
/// }
/// </code>
/// </example>
public sealed class RenderTable : IBlockRenderer
{
    /// <inheritdoc />
    public static SupportedRenderers BlockType => SupportedRenderers.Table;

    public static void Render(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {
        string id = block.Id;
        bool withHeadings = block.Data.WithHeadings ?? false;
        List<List<string?>>? content = block.Data.Content;

        if (content == null) { return; }

        render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "table");
        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "id", id);

        EditorJsStylingMap? css = render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.Table && item.Id == id);
        css ??= render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.Table && item.Id == null);

        if (css is not null)
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "class", css.Style);
        }

        if (withHeadings)
        {
            render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "thead");
            render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "tr");

            foreach (string? cell in content.First())
            {
                render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "th");
                render_tree_builder.Builder.AddMarkupContent(render_tree_builder.SequenceCounter, cell ?? string.Empty);
                render_tree_builder.Builder.CloseElement(); // Close the th
            }

            render_tree_builder.Builder.CloseElement(); // Close the tr
            render_tree_builder.Builder.CloseElement(); // Close the thead
        }

        render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "tbody");

        foreach (List<string?> row in content.Skip(withHeadings ? 1 : 0))
        {
            render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "tr");

            foreach (string? cell in row)
            {
                render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "td");
                render_tree_builder.Builder.AddMarkupContent(render_tree_builder.SequenceCounter, cell ?? string.Empty);
                render_tree_builder.Builder.CloseElement(); // Close the td
            }

            render_tree_builder.Builder.CloseElement(); // Close the tr
        }

        render_tree_builder.Builder.CloseElement(); // Close the tbody
        render_tree_builder.Builder.CloseElement(); // Close the table
    }
}
