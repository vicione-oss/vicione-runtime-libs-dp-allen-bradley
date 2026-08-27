using System.Buffers.Binary;
using System.Text;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Schema;

/// <summary>
/// Decodes the raw bytes of an <c>@tags</c> (or <c>Program:&lt;name&gt;.@tags</c>) read into one
/// <see cref="LogixTypeDeclaration"/> per tag. This is the <c>src</c> port of the spike's
/// <c>TagInfoPlcMapper</c>, working off a <see cref="ReadOnlySpan{T}"/> with
/// <see cref="BinaryPrimitives"/> rather than the sealed <c>Tag</c>'s getters, so it is testable
/// against captured buffers and independent of the removed typed-mapper API.
/// </summary>
/// <remarks>
/// Each entry is a fixed 22-byte header — instance id (u32), symbol type (u16), element length (u16),
/// three array dimensions (u32 each), name length (u16) — followed by the ASCII name. CIP is
/// little-endian, matching .NET, so every field is a direct read.
/// </remarks>
internal static class LogixSymbolListingDecoder
{
    private const int HeaderSize = 22;

    public static IReadOnlyList<LogixTypeDeclaration> Decode(ReadOnlySpan<byte> listing)
    {
        var declarations = new List<LogixTypeDeclaration>();
        var offset = 0;

        while (offset + HeaderSize <= listing.Length)
        {
            var symbolType = BinaryPrimitives.ReadUInt16LittleEndian(listing[(offset + 4)..]);
            var elementLength = BinaryPrimitives.ReadUInt16LittleEndian(listing[(offset + 6)..]);
            var dimension0 = BinaryPrimitives.ReadUInt32LittleEndian(listing[(offset + 8)..]);
            var dimension1 = BinaryPrimitives.ReadUInt32LittleEndian(listing[(offset + 12)..]);
            var dimension2 = BinaryPrimitives.ReadUInt32LittleEndian(listing[(offset + 16)..]);
            var nameLength = BinaryPrimitives.ReadUInt16LittleEndian(listing[(offset + 20)..]);

            var nameStart = offset + HeaderSize;
            // A truncated final entry (a name that runs off the end of the buffer) is clamped rather
            // than allowed to overrun; a genuine listing never trips this.
            var actualNameLength = Math.Min(nameLength, listing.Length - nameStart);
            var name = Encoding.ASCII.GetString(listing.Slice(nameStart, actualNameLength));

            declarations.Add(ToDeclaration(name, symbolType, dimension0, dimension1, dimension2, elementLength));

            offset = nameStart + actualNameLength;

            // Guard against a zero-length name with a non-zero declared length, which would otherwise
            // spin on the same offset forever.
            if (actualNameLength == 0 && nameLength != 0)
            {
                break;
            }
        }

        return declarations;
    }

    private static LogixTypeDeclaration ToDeclaration(
        string name, ushort symbolType, uint dimension0, uint dimension1, uint dimension2, ushort elementLength)
    {
        var isStruct = SymbolType.IsStruct(symbolType);
        var dimensionCount = SymbolType.DimensionCount(symbolType);

        return new LogixTypeDeclaration(
            TagName: new TagName(name),
            Kind: isStruct ? LogixTypeKind.Structure : LogixTypeKind.Atomic,
            AtomicType: isStruct ? null : SymbolType.AtomicType(symbolType),
            DimensionCount: new DimensionCount(dimensionCount),
            ElementCount: ElementCountOf(dimensionCount, dimension0, dimension1, dimension2),
            ElementLength: new ElementLength(elementLength));
    }

    private static ElementCount ElementCountOf(int dimensionCount, uint dimension0, uint dimension1, uint dimension2) =>
        new(dimensionCount switch
        {
            0 => 1,
            1 => (int)dimension0,
            2 => (int)(dimension0 * dimension1),
            _ => (int)(dimension0 * dimension1 * dimension2),
        });
}
