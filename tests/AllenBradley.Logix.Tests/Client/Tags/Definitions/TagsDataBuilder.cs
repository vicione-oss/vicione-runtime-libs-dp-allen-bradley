using System.Buffers.Binary;
using System.Text;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Definitions;

/// <summary>
/// Builds synthetic <c>@tags</c> listing bytes the way a controller lays them out, so the decoder can
/// be exercised without a device. One <see cref="Entry"/> per tag; <see cref="Build"/> concatenates
/// them into the 22-byte-header-plus-ASCII-name format <c>TagsDecoder</c> reads.
/// </summary>
internal static class TagsDataBuilder
{
    public static byte[] Build(params Entry[] entries)
    {
        var buffer = new List<byte>();
        foreach (var entry in entries)
        {
            var name = Encoding.ASCII.GetBytes(entry.Name);

            var header = new byte[22];
            BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0), entry.InstanceId);
            BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(4), entry.SymbolType);
            BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(6), entry.ElementLength);
            BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(8), entry.Dimension0);
            BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(12), entry.Dimension1);
            BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(16), entry.Dimension2);
            BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(20), (ushort)name.Length);

            buffer.AddRange(header);
            buffer.AddRange(name);
        }

        return [.. buffer];
    }

    internal sealed record Entry(string Name, ushort SymbolType)
    {
        public uint InstanceId { get; init; } = 1;
        public ushort ElementLength { get; init; } = 4;
        public uint Dimension0 { get; init; }
        public uint Dimension1 { get; init; }
        public uint Dimension2 { get; init; }
    }
}
