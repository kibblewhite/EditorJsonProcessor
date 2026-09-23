namespace EditorJsonToHtmlConverter.Renderers;

/// <summary>
/// Renders a horizontal rule separating one passage from the next.
/// </summary>
/// <remarks>
/// <para>
/// A delimiter marks a change of subject within a single body. Where the following passage has a name, a
/// <c>header</c> carries more meaning and should be preferred.
/// </para>
/// <para>
/// The block reads no <c>data</c> fields; an empty <c>data</c> object is correct.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// { "id": "d1a2b3c4d5", "type": "delimiter", "data": { } }
/// </code>
/// </example>
public sealed class RenderDelimiter : IBlockRenderer
{
    /// <inheritdoc />
    public static SupportedRenderers BlockType => SupportedRenderers.Delimiter;

    public static void Render(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {
        string id = block.Id;
        string? text = block.Data.Text;

        render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "hr");
        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "id", id);

        EditorJsStylingMap? css = render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.Delimiter && item.Id == id);
        css ??= render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.Delimiter && item.Id == null);

        if (css is not null)
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "class", css.Style);
        }

        render_tree_builder.Builder.CloseElement();
    }
}