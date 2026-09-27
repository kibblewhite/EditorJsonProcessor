namespace EditorJsonToHtmlConverter;

/// <summary>
/// The semantic tag a <c>text</c> block carries in its <c>data.wrap</c> field - the tag the single-line
/// <c>editorjs-text</c> tool writes from its own configuration (see the <c>text</c> renderer).
/// </summary>
/// <remarks>
/// A <c>wrap</c> value is its member's <see cref="StringValueAttribute"/> (the member's own name) in lower case - the rule
/// <see cref="SupportedRenderers"/> follows for block types - so a consumer that builds
/// or checks a <c>text</c> block takes it from <see cref="TextWrapTypeLookup.ToWrap(TextWrapType)"/> rather than retyping
/// it, and reads one with <see cref="TextWrapTypeLookup.TryGetWrap(string?, out TextWrapType)"/>. The renderer itself does
/// not read <c>wrap</c>. New members are appended, so an existing member's number never changes.
/// </remarks>
public enum TextWrapType
{
    /// <summary>No wrap tag - the absence of a value, never written.</summary>
    [StringValue(nameof(Empty))]
    Empty,

    /// <summary>Plain text.</summary>
    [StringValue(nameof(Text))]
    Text,

    /// <summary>A custom tag the field's editor is configured with.</summary>
    [StringValue(nameof(Custom))]
    Custom,

    /// <summary>A title.</summary>
    [StringValue(nameof(Title))]
    Title,

    /// <summary>A synopsis.</summary>
    [StringValue(nameof(Synopsis))]
    Synopsis
}
