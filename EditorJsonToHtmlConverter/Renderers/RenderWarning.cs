namespace EditorJsonToHtmlConverter.Renderers;

/// <summary>
/// Renders a callout that draws the reader's attention to something important.
/// </summary>
/// <remarks>
/// <para>
/// Use a warning sparingly, for information a reader would be materially worse off missing — a cut-off
/// date, an access restriction, a safety note. Overuse flattens its effect.
/// </para>
/// <para>
/// <c>title</c> (optional) — the short heading, rendered in bold and omitted when absent.
/// <c>message</c> (optional) — the body, rendered as its own paragraph and likewise omitted when absent.
/// Both preserve inline HTML. A block carrying neither renders an empty container.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// {
///   "id": "w1a2b3c4d5",
///   "type": "warning",
///   "data": { "title": "Last entry", "message": "Doors close at 9pm and late arrivals cannot be admitted." }
/// }
/// </code>
/// </example>
public sealed class RenderWarning : IBlockRenderer
{
    /// <inheritdoc />
    public static SupportedRenderers BlockType => SupportedRenderers.Warning;

    public static void Render(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {
        string id = block.Id;
        string? title = block.Data.Title;
        string? message = block.Data.Message;

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
