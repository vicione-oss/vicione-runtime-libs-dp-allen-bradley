using System.Buffers.Binary;
using System.Text;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

/// <summary>
/// Builds synthetic <c>@tags</c> listing bytes the way a controller lays them out, so the decoder can be
/// exercised without a device. One <see cref="TagEntry"/> per tag; <see cref="Listing"/> concatenates them
/// into the 22-byte-header-plus-ASCII-name format the decoder reads, written the long way round — explicit
/// little-endian primitives at explicit offsets.
/// </summary>
internal static class TagsListingTestDataFactory
{
    /// <summary>The elementary CIP code for a <c>DINT</c>.</summary>
    internal const ushort DintSymbolType = 0x00C4;

    /// <summary>The elementary CIP code for a <c>REAL</c>.</summary>
    internal const ushort RealSymbolType = 0x00CA;

    /// <summary>The structure bit set, with a template id in the low bits.</summary>
    internal const ushort StructureSymbolType = 0x8123;

    /// <summary>The bits an array of rank one sets in the symbol type.</summary>
    internal const ushort OneDimensionSymbolType = 0x2000;

    internal static byte[] Listing(params TagEntry[] tags)
    {
        var buffer = new List<byte>();
        foreach (var tag in tags)
        {
            var tagName = Encoding.ASCII.GetBytes(tag.Name);
            var header = new byte[22];
            BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0), tag.InstanceId);
            BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(4), tag.SymbolType);
            BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(6), tag.ElementLength);
            BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(8), tag.Dimension0);
            BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(12), tag.Dimension1);
            BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(16), tag.Dimension2);
            BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(20), (ushort)tagName.Length);

            buffer.AddRange(header);
            buffer.AddRange(tagName);
        }

        return [.. buffer];
    }

    /// <summary>One tag as the controller's listing describes it, before the decoder reads it.</summary>
    internal sealed record TagEntry(string Name, ushort SymbolType)
    {
        public uint InstanceId { get; init; } = 1;

        public ushort ElementLength { get; init; } = 4;

        public uint Dimension0 { get; init; }

        public uint Dimension1 { get; init; }

        public uint Dimension2 { get; init; }
    }
}
