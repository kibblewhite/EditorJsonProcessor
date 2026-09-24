namespace EditorJsonToHtmlConverter.Renderers;

/// <summary>
/// Renders a single line of inline content with no wrapper element of its own.
/// </summary>
/// <remarks>
/// <para>
/// The block for a single-line field — a title, a synopsis, a label — where the surrounding markup already
/// provides the element. Such a field's document holds exactly one <c>text</c> block and nothing else. For
/// body copy choose <c>paragraph</c>, which renders a real paragraph element and can run to several lines.
/// </para>
/// <para>
/// <c>text</c> (required) — the line itself, as a string, emitted as-is with inline HTML preserved; nothing
/// at all is rendered when it is absent or empty. It is edited by the single-line <c>editorjs-text</c> tool,
/// which keeps it to one line of inline markup on its next save.
/// </para>
/// <para>
/// Line breaks and block elements are removed. A <c>&lt;br&gt;</c> is deleted without a space in its place,
/// so the words either side run together; separate phrases with punctuation instead.
/// </para>
/// <para>
/// Only markup the editor's inline tools produce survives. With Editor.js's built-in bold, italic and link
/// tools that is <c>&lt;b&gt;</c>, <c>&lt;i&gt;</c> and <c>&lt;a href&gt;</c>; <c>&lt;strong&gt;</c>,
/// <c>&lt;em&gt;</c>, <c>&lt;mark&gt;</c> and any other tag are reduced to their text.
/// </para>
/// <para>
/// A block whose <c>text</c> is blank, or not a string, is dropped, leaving an empty document.
/// </para>
/// <para>
/// <c>wrap</c> (optional) — a semantic tag (<c>text</c>, <c>custom</c>, <c>title</c> or <c>synopsis</c>) that
/// the editor writes from its own configuration, replacing any value supplied. This renderer does not read
/// it, so omit it.
/// </para>
/// <para>
/// A single-line field's editor is usually configured to hold one block: it keeps the first block and
/// deletes any after it, so put the whole line in one <c>text</c> block rather than splitting it. Other
/// block types are not converted there — a <c>paragraph</c> block loses its text on the next save when the
/// editor disables the paragraph tool, and a <c>header</c> cannot be edited at all.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// { "id": "x1a2b3c4d5", "type": "text", "data": { "text": "An evening of &lt;b&gt;live&lt;/b&gt; music" } }
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
