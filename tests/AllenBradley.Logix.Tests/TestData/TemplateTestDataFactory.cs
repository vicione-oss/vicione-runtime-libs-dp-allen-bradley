using System.Buffers.Binary;
using System.Text;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

/// <summary>
/// Builds synthetic <c>@udt/&lt;id&gt;</c> bytes the way libplctag hands them back, so the decoder can be
/// exercised without a device: the 14-byte header the library synthesises, then the raw template payload
/// — one 8-byte descriptor per member, the template's name, and the member names, each zero-terminated.
/// </summary>
internal static class TemplateTestDataFactory
{
    internal const ushort DintMemberType = 0x00C4;

    internal const ushort SintMemberType = 0x00C2;

    internal const ushort BoolMemberType = 0x00C1;

    /// <summary>The code a <c>BOOL</c> array member carries: the words its bits are packed into.</summary>
    internal const ushort DwordMemberType = 0x00D3;

    /// <summary>The bit an array member of rank one sets in its symbol type.</summary>
    internal const ushort ArrayMemberType = 0x2000;

    /// <summary>The structure bit set, with template id <c>0x456</c> in the low bits.</summary>
    internal const ushort StructureMemberType = 0x8456;

    /// <summary>The template id the built-in <c>STRING</c> carries on a Logix controller.</summary>
    internal const ushort StringTemplateId = 0x0FCE;

    /// <summary>
    /// The built-in <c>STRING</c>: <c>.LEN : DINT</c> at offset 0 and <c>.DATA : SINT[82]</c> at offset 4,
    /// 88 bytes once padded.
    /// </summary>
    internal static TemplateEntry StringTemplate() =>
        new(StringTemplateId, "STRING",
        [
            new MemberEntry("LEN", DintMemberType),
            new MemberEntry("DATA", ArrayMemberType | SintMemberType) { Info = 82, Offset = 4 },
        ])
        {
            StructureSize = 88,
        };

    internal static byte[] Template(TemplateEntry template)
    {
        var buffer = new List<byte>();
        var header = new byte[14];
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(0), template.Id);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(2), template.MemberDescriptionSize);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(6), template.StructureSize);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(10), (ushort)template.Members.Count);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(12), template.Handle);
        buffer.AddRange(header);

        foreach (var member in template.Members)
        {
            var descriptor = new byte[8];
            BinaryPrimitives.WriteUInt16LittleEndian(descriptor.AsSpan(0), member.Info);
            BinaryPrimitives.WriteUInt16LittleEndian(descriptor.AsSpan(2), member.SymbolType);
            BinaryPrimitives.WriteUInt32LittleEndian(descriptor.AsSpan(4), member.Offset);
            buffer.AddRange(descriptor);
        }

        buffer.AddRange(ZeroTerminated(template.Name));
        foreach (var member in template.Members)
        {
            buffer.AddRange(ZeroTerminated(member.Name));
        }

        return [.. buffer];
    }

    private static byte[] ZeroTerminated(string name) => [.. Encoding.ASCII.GetBytes(name), 0];

    /// <summary>One template as libplctag reports it, before the decoder reads it.</summary>
    internal sealed record TemplateEntry(ushort Id, string Name, IReadOnlyList<MemberEntry> Members)
    {
        /// <summary>The descriptors' size in 32-bit words: two per member.</summary>
        public uint MemberDescriptionSize { get; init; } = (uint)Members.Count * 2;

        public uint StructureSize { get; init; } = 4;

        public ushort Handle { get; init; } = 0xABCD;
    }

    /// <summary>One member as its descriptor and name describe it.</summary>
    internal sealed record MemberEntry(string Name, ushort SymbolType)
    {
        public ushort Info { get; init; }

        public uint Offset { get; init; }
    }
}
