using System.Collections.Frozen;

namespace EditorJsonToHtmlConverter;

/// <summary>
/// Resolves a block's <c>type</c> to the <see cref="SupportedRenderers"/> block that draws it, by name alone.
/// </summary>
/// <remarks>
/// <see cref="Enum.TryParse{TEnum}(string?, bool, out TEnum)"/> is not used, because it accepts more than names: a
/// number (<c>"1"</c> is <see cref="SupportedRenderers.Paragraph"/>), padding (<c>" paragraph "</c>) and a comma list,
/// whose values it ORs together (<c>"paragraph, header"</c> is <see cref="SupportedRenderers.List"/>). None of those is a
/// block type Editor.js writes.
/// </remarks>
internal static class SupportedRenderersLookup
{
    // Empty is the absence of a block type, not a block, so it is left out and never resolves.
    private static readonly FrozenDictionary<string, SupportedRenderers> _block_types = Enum.GetValues<SupportedRenderers>()
        .Where(renderer => renderer != SupportedRenderers.Empty)
        .ToFrozenDictionary(renderer => renderer.ToString(), StringComparer.OrdinalIgnoreCase);

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
}
