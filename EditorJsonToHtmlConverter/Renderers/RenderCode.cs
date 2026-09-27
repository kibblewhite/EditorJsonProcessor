namespace EditorJsonToHtmlConverter.Renderers;

/// <summary>
/// Renders a block of source code or preformatted text, exactly as typed.
/// </summary>
/// <remarks>
/// <para>
/// Use a code block for text whose characters and line breaks matter literally - a snippet, a command, a configuration
/// extract. Prose that merely needs emphasis is a paragraph, and a short fragment inside a sentence is inline code.
/// </para>
/// <para>
/// <c>code</c> (required) - the text, as the Editor.js code tool saves it from its text area: plain text, never markup.
/// It is written HTML-encoded inside <c>&lt;pre&gt;&lt;code&gt;</c>, so a <c>&lt;</c> in it is shown as a character rather
/// than read as a tag, and its line breaks and spacing are kept. A block whose <c>code</c> is absent or empty is dropped.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// {
///   "id": "c1a2b3c4d5",
///   "type": "code",
///   "data": { "code": "body {\n  font-size: 14px;\n}" }
/// }
/// </code>
/// </example>
public sealed class RenderCode : IBlockRenderer
{
    /// <inheritdoc />
    public static SupportedRenderers BlockType => SupportedRenderers.Code;

    public static void Render(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {
        string id = block.Id;
        string? code = block.Data.Code;

        if (string.IsNullOrEmpty(code))
        {
            return;
        }

        render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "pre");
        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "id", id);

        EditorJsStylingMap? css = render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.Code && item.Id == id);
        css ??= render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.Code && item.Id == null);

        if (css is not null)
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "class", css.Style);
        }

        // Plain text, so AddContent - which HTML-encodes - and never AddMarkupContent: the code tool saves exactly what was
        // typed, and markup in a snippet must be shown, not executed.
        render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "code");
        render_tree_builder.Builder.AddContent(render_tree_builder.SequenceCounter, code);
        render_tree_builder.Builder.CloseElement(); // Close the code element

        render_tree_builder.Builder.CloseElement(); // Close the pre element
    }
}
