namespace EditorJsonToHtmlConverter.Models;

public sealed class EditorJsBlock : IEditorJsEntity<EditorJsBlock>
{
    /// <summary>
    /// Fixed pattern placed in the high 88 bits (bits 40-127) when include_high_bits is true.
    /// Its low 40 bits must be zero so it can never overwrite the bits taken from short_id.
    /// </summary>
    private static readonly UInt128 high_bits = new(0x0123_4567_89AB_CDEF, 0xFEDC_BA00_0000_0000);

    [JsonIgnore]
    public static EditorJsBlock Empty => new()
    {
        Id = string.Empty,
        Type = string.Empty,
        Data = EditorJsBlockData.Empty
    };

    /// <summary>
    /// Mints the short identifier Editor.js gives each block: ten hex characters from the END of a v7 GUID, which is its
    /// random part. The first twelve hex characters of a v7 are a millisecond timestamp, so a ten-character prefix would
    /// repeat for every block created within the same quarter-second.
    /// </summary>
    /// <returns>A new ten-character block identifier.</returns>
    public static string NewId()
        => Guid.CreateVersion7().ToString("N")[^10..];

    /// <summary>
    /// Converts a short id (1-10 hex characters) back into a 128-bit value.
    /// Only the low 40 bits are from short_id; the high 88 bits are either zero
    /// or the fixed pattern in high_bits.
    /// </summary>
    /// <remarks>
    /// A short id only carries 40 bits, so the 88 bits that were dropped when it was created
    /// cannot be recovered. This does not return the original 128-bit value; it returns a
    /// deterministic 128-bit value that has the same low 40 bits, so the same short_id always
    /// maps to the same result.
    /// </remarks>
    /// <param name="short_id">
    /// 1 to 10 hex characters (upper or lower case), with no "0x" prefix, sign or whitespace.
    /// Shorter input is treated as if it were left-padded with zeros.
    /// </param>
    /// <param name="include_high_bits">
    /// false: the high 88 bits are zero, so the result is numerically equal to short_id.
    /// true: the high 88 bits are filled with the fixed pattern in high_bits.
    /// </param>
    /// <returns>A 128-bit value whose low 40 bits are the value of short_id.</returns>
    /// <exception cref="FormatException">
    /// short_id is empty, longer than 10 characters, or contains a non-hex character.
    /// </exception>
    public static UInt128 Revert(ReadOnlySpan<char> short_id, bool include_high_bits = false)
        // Length check: 10 hex characters = 40 bits. Rejecting anything longer guarantees the
        // parsed value fits in the low 40 bits, so no masking is needed afterwards. Empty input
        // (including a null string, which converts to an empty span) is rejected here too.
        => short_id.Length is > 0 and <= 10
           // AllowHexSpecifier accepts hex digits only (no whitespace, sign or prefix), and
           // InvariantCulture keeps parsing identical regardless of the machine's locale.
           && UInt128.TryParse(short_id, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out UInt128 low)
            // OR is safe because high_bits has zeros in its low 40 bits and low has zeros
            // in its high 88 bits, so the two never overlap.
            ? (include_high_bits ? high_bits | low : low)
            : throw new FormatException($"'{short_id}' is not a valid short id (expected 1-10 hex characters).");

    /// <summary>
    /// Rebuilds a partial GUID from a short id: the last ten hex characters are short_id and
    /// every character that cannot be recovered is zero (00000000-0000-0000-0000-00xxxxxxxxxx).
    /// </summary>
    /// <remarks>
    /// This mirrors NewId(), which keeps the last ten hex characters of a v7 GUID, so
    /// RevertToGuid(id).ToString("N")[^10..] gives back the same id (in lower case).
    /// The result is not the original GUID and is not a valid v7 GUID: the timestamp,
    /// version and variant bits were discarded by NewId() and are zero here.
    /// </remarks>
    /// <param name="short_id">1 to 10 hex characters, as accepted by Revert.</param>
    /// <param name="include_high_bits">Passed through to Revert; false leaves the unknown bits zero.</param>
    /// <returns>A GUID whose final ten hex characters are short_id.</returns>
    /// <exception cref="FormatException">short_id is not 1-10 hex characters.</exception>
    public static Guid RevertToGuid(ReadOnlySpan<char> short_id, bool include_high_bits = false)
    {
        // Big-endian puts the most significant byte first, so the low 40 bits land in the
        // last five bytes, which are the last ten hex characters of the GUID's string form.
        Span<byte> bytes = stackalloc byte[16];
        BinaryPrimitives.WriteUInt128BigEndian(bytes, Revert(short_id, include_high_bits));

        // bigEndian: true makes the GUID's string form follow the byte order exactly. The
        // default constructor would byte-swap the first three groups.
        return new Guid(bytes, true);
    }

    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("data")]
    public required EditorJsBlockData Data { get; set; }
}