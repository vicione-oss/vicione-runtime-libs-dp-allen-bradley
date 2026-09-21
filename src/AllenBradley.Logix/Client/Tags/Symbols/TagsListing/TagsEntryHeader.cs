using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.SymbolTypes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.TagsListing;

/// <summary>
/// The fixed part of an <c>@tags</c> listing entry, laid out exactly as the controller sends it so
/// <see cref="TagsDecoder"/> can reinterpret it in one step; the variable-length name follows outside.
/// <c>Pack = 1</c> is load-bearing: without it <see cref="NameLength"/> pads the struct to 24 bytes and
/// every entry after the first starts two bytes late. Reinterpreting assumes little-endian.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct TagsEntryHeader
{
    /// <summary>Symbol object instance id — unread; the name is what everything here keys on.</summary>
    public readonly uint InstanceId;

    /// <summary>
    /// The structure flag, the array rank, and either an atomic CIP code or a template id, packed into
    /// one word.
    /// </summary>
    public readonly SymbolType SymbolType;

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
    /// start wherever the previous name ended, so the read must stay alignment-agnostic.
    /// </summary>
    public static TagsEntryHeader ReadFrom(ReadOnlySpan<byte> entry) =>
        MemoryMarshal.Read<TagsEntryHeader>(entry);
}
