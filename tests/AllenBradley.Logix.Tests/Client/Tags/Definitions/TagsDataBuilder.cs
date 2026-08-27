using System.Buffers.Binary;
using System.Text;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Definitions;

/// <summary>
/// Builds synthetic <c>@tags</c> listing bytes the way a controller lays them out, so the decoder can
/// be exercised without a device. One <see cref="TagEntry"/> per tag; <see cref="Build"/> concatenates
/// them into the 22-byte-header-plus-ASCII-name format <c>LogixTagListingDecoder</c> reads.
/// </summary>
internal static class TagsDataBuilder
{
    public static byte[] Build(params TagEntry[] tags)
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

    internal sealed record TagEntry(string Name, ushort SymbolType)
    {
        public uint InstanceId { get; init; } = 1;
        public ushort ElementLength { get; init; } = 4;
        public uint Dimension0 { get; init; }
        public uint Dimension1 { get; init; }
        public uint Dimension2 { get; init; }
    }
}
