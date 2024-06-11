namespace EditorJsonToHtmlConverter;

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
