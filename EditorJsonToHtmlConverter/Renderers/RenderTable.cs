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
/// <c>content</c> (required) — an array of rows, each an array of cell strings; the block renders nothing
/// when the field is missing or holds no rows. Every row must carry the same number of cells as the
/// first: the Editor.js table tool takes its column count from the first row, so a longer row stops the
/// table opening in the editor and a shorter one is padded with empty cells. A row whose cells are all empty
/// is dropped by the editor on its next save. Cells preserve inline HTML. <c>withHeadings</c> (optional,
/// default false) — when true the FIRST row of <c>content</c> is lifted out and rendered as the header row
/// rather than as data. <c>stretched</c> is saved by the editor and not read by this renderer.
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

        // With no rows there is no header row to lift out, so an empty table renders nothing — as a missing one does.
        if (content is null || content.Count == 0) { return; }

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
