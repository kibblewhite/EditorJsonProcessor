namespace EditorJsonToHtmlConverter.Renderers;

/// <summary>
/// Renders a paragraph of body copy.
/// </summary>
/// <remarks>
/// <para>
/// The default block for prose. Choose it for ordinary sentences; use <c>header</c> to title a section and
/// <c>quote</c> to attribute a passage to a speaker or source.
/// </para>
/// <para>
/// <c>text</c> (optional) — the paragraph's content. Inline HTML is preserved, so <c>&lt;b&gt;</c>,
/// <c>&lt;i&gt;</c>, <c>&lt;a&gt;</c> and <c>&lt;mark&gt;</c> may be used for emphasis and links. When the
/// field is absent or empty an empty paragraph is still rendered.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// { "id": "b1c2d3e4f5", "type": "paragraph", "data": { "text": "Doors open at &lt;b&gt;7pm&lt;/b&gt;." } }
/// </code>
/// </example>
public sealed class RenderParagraph : IBlockRenderer
{
    /// <inheritdoc />
    public static SupportedRenderers BlockType => SupportedRenderers.Paragraph;

    public static void Render(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {
        string id = block.Id;
        string? text = block.Data.Text;

        render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "p");
        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "id", id);

        EditorJsStylingMap? css = render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.Paragraph && item.Id == id);
        css ??= render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.Paragraph && item.Id == null);

        if (css is not null)
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "class", css.Style);
        }

        render_tree_builder.Builder.AddMarkupContent(render_tree_builder.SequenceCounter, text ?? string.Empty);
        render_tree_builder.Builder.CloseElement();
    }
}
