using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;

/// <summary>
/// The fixed part of an <c>@tags</c> listing entry, laid out exactly as the controller sends it, so
/// <see cref="TagsDecoder"/> reads a header in one reinterpret rather than seven offset
/// calculations. The tag's name follows the header and is variable-length, so it stays outside.
/// </summary>
/// <remarks>
/// <para>
/// A plain <c>readonly struct</c> of fields, not one of the repo's record structs: this is a wire
/// layout rather than a domain concept, and marshalling reads fields, not properties.
/// </para>
/// <para>
/// <c>Pack = 1</c> is load-bearing. The trailing <see cref="NameLength"/> would otherwise be padded out
/// to the struct's four-byte alignment, making <see cref="Size"/> 24 and every entry after the first
/// start two bytes late.
/// </para>
/// <para>
/// Reinterpreting bytes takes the host's byte order instead of stating one, which holds only because
/// CIP is little-endian and so is every platform .NET runs on. The layout is documented in
/// <c>docs/AllenBradley.Documentation/cip-protocol/symbolic-tag-data-types.md</c>, section "The
/// <c>@tags</c> listing entry".
/// </para>
/// </remarks>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct TagsEntryHeader
{
    /// <summary>Symbol object instance id — unread; the name is what everything here keys on.</summary>
    public readonly uint InstanceId;

    /// <summary>
    /// The structure flag, the array rank, and either an atomic CIP code or a template id, packed into
    /// one word that <see cref="SymbolType"/> decodes.
    /// </summary>
    public readonly ushort SymbolType;

    /// <summary>The bytes one element occupies.</summary>
    public readonly ushort ElementLength;

    /// <summary>An array dimension; only the first <c>rank</c> of the three are meaningful.</summary>
    public readonly uint FirstDimension;

    /// <inheritdoc cref="FirstDimension"/>
    public readonly uint SecondDimension;

    /// <inheritdoc cref="FirstDimension"/>
    public readonly uint ThirdDimension;

    /// <summary>The length of the name that follows the header, ASCII and unterminated.</summary>
    public readonly ushort NameLength;

    /// <summary>The header's on-the-wire size, and so the offset of the name within an entry.</summary>
    public static int Size => Unsafe.SizeOf<TagsEntryHeader>();

    /// <summary>
    /// Reinterprets the first <see cref="Size"/> bytes of <paramref name="entry"/> as a header. Entries
    /// start wherever the previous name ended, so the read is deliberately alignment-agnostic.
    /// </summary>
    public static TagsEntryHeader ReadFrom(ReadOnlySpan<byte> entry) =>
        MemoryMarshal.Read<TagsEntryHeader>(entry);
}
