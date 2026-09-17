using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions.Templates;

/// <summary>
/// The 14-byte header libplctag puts in front of an <c>@udt/&lt;id&gt;</c> read, built from the
/// template's attributes rather than copied off the wire
/// (<c>docs/AllenBradley.Documentation/libPlcTag/reading-a-udt-definition.md</c>). Laid out exactly as
/// the library writes it so <see cref="TemplateDecoder"/> can reinterpret it in one step; <c>Pack = 1</c>
/// is load-bearing, because the 4-byte fields sit at offsets 2 and 6. Reinterpreting assumes little-endian.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct TemplateHeader
{
    /// <summary>The template's id, the one the read asked for.</summary>
    public readonly ushort TemplateId;

    /// <summary>The size of the member descriptions in 32-bit words — unread; the payload is walked by count.</summary>
    public readonly uint MemberDescriptionSize;

    /// <summary>The bytes one instance of the structure occupies.</summary>
    public readonly uint StructureSize;

    /// <summary>How many member descriptors, and then member names, follow the header.</summary>
    public readonly ushort MemberCount;

    /// <summary>The handle a read reply of a tag of this structure carries.</summary>
    public readonly ushort StructureHandle;

    /// <summary>The header's size, and so the offset of the first member descriptor.</summary>
    public static int Size => Unsafe.SizeOf<TemplateHeader>();

    /// <summary>Reinterprets the first <see cref="Size"/> bytes of <paramref name="template"/> as the header.</summary>
    public static TemplateHeader ReadFrom(ReadOnlySpan<byte> template) =>
        MemoryMarshal.Read<TemplateHeader>(template);
}
