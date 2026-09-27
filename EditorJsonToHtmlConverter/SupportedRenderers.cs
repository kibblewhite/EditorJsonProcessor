namespace EditorJsonToHtmlConverter;

/// <summary>
/// List of supported block types from Editor JS
/// </summary>
/// <remarks>
/// <para>This is used by the <see cref="EjsRenderFragment.RenderBlock(EditorJsonToHtmlConverter.CustomRenderTreeBuilder, EditorJsBlock)"/> internal method.</para>
/// <para>
/// A block's <c>type</c> - the key its Editor.js tool is registered under - is its member's
/// <see cref="StringValueAttribute"/> (the member's own name) in lower case. A consumer that writes a block, or matches one
/// where case matters (a SQL JSON path, for instance), takes the string from
/// <see cref="SupportedRenderersLookup.ToBlockType(SupportedRenderers)"/> rather than retyping it; reading a document stays
/// case-insensitive through <see cref="SupportedRenderersLookup.TryGetBlockType(string?, out SupportedRenderers)"/>. A
/// member is therefore named after its tool's key, and new members are appended, so an existing member's number never
/// changes.
/// </para>
/// </remarks>
public enum SupportedRenderers
{
    [StringValue(nameof(Empty))]
    Empty,

    [StringValue(nameof(Paragraph))]
    Paragraph,

    [StringValue(nameof(Header))]
    Header,

    [StringValue(nameof(List))]
    List,

    [StringValue(nameof(Quote))]
    Quote,

    [StringValue(nameof(Checklist))]
    Checklist,

    [StringValue(nameof(Table))]
    Table,

    [StringValue(nameof(Image))]
    Image,

    [StringValue(nameof(Delimiter))]
    Delimiter,

    [StringValue(nameof(Warning))]
    Warning,

    [StringValue(nameof(Embed))]
    Embed,

    [StringValue(nameof(Text))]
    Text,

    [StringValue(nameof(Map))]
    Map,

    [StringValue(nameof(Code))]
    Code
}
