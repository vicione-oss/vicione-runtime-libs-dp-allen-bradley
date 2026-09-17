using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions.Templates;

/// <summary>
/// One 8-byte member descriptor of a template, as the Read Template service lays it out; the member's
/// name comes later in the payload, in the same order. <c>Pack = 1</c> is load-bearing here too, because
/// the 4-byte offset sits at offset 4 and the run is walked by a fixed stride.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct TemplateMemberDescriptor
{
    /// <summary>The element count of an array member, or the bit position of a <c>BOOL</c> member.</summary>
    public readonly ushort Info;

    /// <summary>The same encoding as a listing entry's symbol type, decoded by <see cref="SymbolType"/>.</summary>
    public readonly ushort SymbolType;

    /// <summary>The byte offset of the member within the structure instance.</summary>
    public readonly uint Offset;

    /// <summary>The descriptor's size, and so the stride of the descriptor run.</summary>
    public static int Size => Unsafe.SizeOf<TemplateMemberDescriptor>();

    /// <summary>Reinterprets the first <see cref="Size"/> bytes of <paramref name="descriptor"/>.</summary>
    public static TemplateMemberDescriptor ReadFrom(ReadOnlySpan<byte> descriptor) =>
        MemoryMarshal.Read<TemplateMemberDescriptor>(descriptor);
}
