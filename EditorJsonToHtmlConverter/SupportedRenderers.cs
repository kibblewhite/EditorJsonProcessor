namespace EditorJsonToHtmlConverter;

/// <summary>
/// List of supported block types from Editor JS
/// </summary>
/// <remarks>This is used by the <see cref="EjsRenderFragment.RenderBlock(EditorJsonToHtmlConverter.CustomRenderTreeBuilder, EditorJsBlock)"/> internal method.</remarks>
public enum SupportedRenderers
{
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
    Text
}
