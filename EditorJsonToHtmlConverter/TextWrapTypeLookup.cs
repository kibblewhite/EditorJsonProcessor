using System.Collections.Frozen;
using System.Reflection;

namespace EditorJsonToHtmlConverter;

/// <summary>
/// Resolves a <c>text</c> block's <c>wrap</c> value to its <see cref="TextWrapType"/>, and back - the same name-only rule
/// <see cref="SupportedRenderersLookup"/> applies to block types.
/// </summary>
/// <remarks>
/// <see cref="Enum.TryParse{TEnum}(string?, bool, out TEnum)"/> is not used, for the reason given on
/// <see cref="SupportedRenderersLookup"/>: it accepts numbers, padding and comma lists, none of which is a wrap value.
/// </remarks>
public static class TextWrapTypeLookup
{
    // Each member's wrap value: its StringValue (the member's name) in lower case, read once - the rule
    // SupportedRenderersLookup applies to block types. Empty is the absence of a wrap, so it is left out.
    private static readonly FrozenDictionary<TextWrapType, string> _wrap_by_type = Enum.GetValues<TextWrapType>()
        .Where(wrap_type => wrap_type != TextWrapType.Empty)
        .ToFrozenDictionary(wrap_type => wrap_type, wrap_type => typeof(TextWrapType)
            .GetField(wrap_type.ToString())!
            .GetCustomAttribute<StringValueAttribute>()!
            .Value
            .ToLowerInvariant());

    private static readonly FrozenDictionary<string, TextWrapType> _wrap_types = _wrap_by_type
        .ToFrozenDictionary(pair => pair.Value, pair => pair.Key, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Resolves a <c>wrap</c> value, ignoring case, to its <see cref="TextWrapType"/>.
    /// </summary>
    /// <param name="wrap">The block's <c>data.wrap</c>, as written in the document.</param>
    /// <param name="wrap_type">The wrap type; <see cref="TextWrapType.Empty"/> when there is none.</param>
    /// <returns><see langword="true"/> when the value is a known wrap tag.</returns>
    public static bool TryGetWrap(string? wrap, out TextWrapType wrap_type)
    {
        wrap_type = TextWrapType.Empty;
        return wrap is not null && _wrap_types.TryGetValue(wrap, out wrap_type) is true;
    }

    /// <summary>
    /// The exact <c>wrap</c> value written for <paramref name="wrap_type"/> - <c>"title"</c> for
    /// <see cref="TextWrapType.Title"/>; <see cref="string.Empty"/> for <see cref="TextWrapType.Empty"/>.
    /// </summary>
    /// <param name="wrap_type">The wrap type.</param>
    /// <returns>The <c>wrap</c> string.</returns>
    public static string ToWrap(this TextWrapType wrap_type)
        => _wrap_by_type.TryGetValue(wrap_type, out string? wrap) is true ? wrap : string.Empty;
}
