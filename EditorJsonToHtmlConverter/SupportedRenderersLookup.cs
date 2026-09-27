using System.Collections.Frozen;
using System.Reflection;

namespace EditorJsonToHtmlConverter;

/// <summary>
/// Resolves a block's <c>type</c> to the <see cref="SupportedRenderers"/> block that draws it, by name alone — the one
/// rule the renderer, <see cref="Models.SupportedRenderersConverter"/> and
/// <see cref="Extensions.EditorJsBlocksExtensions.IsEditorJsDocument(string, EditorJsDocumentCheck)"/> use. Public so a
/// consumer that inspects a document's block types before rendering it (to refuse a type, or to treat one differently)
/// decides exactly as the renderer will.
/// </summary>
/// <remarks>
/// <see cref="Enum.TryParse{TEnum}(string?, bool, out TEnum)"/> is not used, because it accepts more than names: a
/// number (<c>"1"</c> is <see cref="SupportedRenderers.Paragraph"/>), padding (<c>" paragraph "</c>) and a comma list,
/// whose values it ORs together (<c>"paragraph, header"</c> is <see cref="SupportedRenderers.List"/>). None of those is a
/// block type Editor.js writes.
/// </remarks>
public static class SupportedRenderersLookup
{
    // Each member's Editor.js block type: its StringValue (the member's name) in lower case, read once - the same rule
    // SupportedRenderersConverter writes with. Empty is the absence of a block type, not a block, so it is left out and
    // neither resolves nor has a block type.
    private static readonly FrozenDictionary<SupportedRenderers, string> _block_type_by_renderer = Enum.GetValues<SupportedRenderers>()
        .Where(renderer => renderer != SupportedRenderers.Empty)
        .ToFrozenDictionary(renderer => renderer, renderer => typeof(SupportedRenderers)
            .GetField(renderer.ToString())!
            .GetCustomAttribute<StringValueAttribute>()!
            .Value
            .ToLowerInvariant());

    private static readonly FrozenDictionary<string, SupportedRenderers> _block_types = _block_type_by_renderer
        .ToFrozenDictionary(pair => pair.Value, pair => pair.Key, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Resolves a block type, ignoring case, to the block that renders it.
    /// </summary>
    /// <param name="block_type">The block's <c>type</c>, as written in the document.</param>
    /// <param name="renderer">The block that renders it; <see cref="SupportedRenderers.Empty"/> when there is none.</param>
    /// <returns><see langword="true"/> when a renderer draws blocks of this type.</returns>
    public static bool TryGetBlockType(string? block_type, out SupportedRenderers renderer)
    {
        renderer = SupportedRenderers.Empty;
        return block_type is not null && _block_types.TryGetValue(block_type, out renderer) is true;
    }

    /// <summary>
    /// The exact <c>type</c> Editor.js writes for <paramref name="renderer"/>'s block - <c>"map"</c> for
    /// <see cref="SupportedRenderers.Map"/>. The reverse of <see cref="TryGetBlockType(string?, out SupportedRenderers)"/>,
    /// for a consumer that writes a block or must match its type exactly; <see cref="string.Empty"/> for
    /// <see cref="SupportedRenderers.Empty"/>, which is not a block type.
    /// </summary>
    /// <param name="renderer">The block type.</param>
    /// <returns>The block's <c>type</c> string, as Editor.js writes it.</returns>
    public static string ToBlockType(this SupportedRenderers renderer)
        => _block_type_by_renderer.TryGetValue(renderer, out string? block_type) is true ? block_type : string.Empty;
}
