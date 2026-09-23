namespace EditorJsonToHtmlConverter.Renderers;

/// <summary>
/// Renders inline content with no wrapper element of its own.
/// </summary>
/// <remarks>
/// <para>
/// A bare text block, used where surrounding markup already provides the container — most commonly a
/// single-block body holding a short title or synopsis. For ordinary body copy choose <c>paragraph</c>,
/// which renders a real paragraph element and can be styled as one.
/// </para>
/// <para>
/// <c>text</c> (optional) — the content, emitted as-is with inline HTML preserved; nothing at all is
/// rendered when it is absent or empty. Other fields sometimes present on a text block, such as
/// <c>wrap</c>, are carried by the editor and are not read by this renderer.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// { "id": "x1a2b3c4d5", "type": "text", "data": { "text": "An evening of live music" } }
/// </code>
/// </example>
public sealed class RenderText : IBlockRenderer
{
    /// <inheritdoc />
    public static SupportedRenderers BlockType => SupportedRenderers.Text;

    public static void Render(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {
        string? text = block.Data.Text;
        if (!string.IsNullOrEmpty(text))
        {
            render_tree_builder.Builder.AddMarkupContent(render_tree_builder.SequenceCounter, text);
        }
    }
}
