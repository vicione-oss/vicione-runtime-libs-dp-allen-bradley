using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Strings;

/// <summary>
/// The fixed part of a Logix string: the member <c>.LEN : DINT</c>, in the layout that the controller uses.
/// The variable member <c>.DATA : SINT[n]</c> comes after it and is not part of this type.
/// The bytes are copied as they are, so this type needs the little-endian byte order that CIP and .NET use.
/// </summary>
/// <param name="length">The number of characters in <c>.DATA</c>.</param>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct LogixStringHeader(int length)
{
    /// <summary>
    /// <c>.LEN</c>. It is a DINT (32 bits). The elementary CIP <c>STRING</c> (0xD0) has a 16-bit count instead.
    /// </summary>
    public readonly int Length = length;

    /// <summary>The size of the header in bytes. It is also the offset of <c>.DATA</c> in a string.</summary>
    public static int Size => Unsafe.SizeOf<LogixStringHeader>();

    /// <summary>
    /// Reads a header from the first <see cref="Size"/> bytes of <paramref name="buffer"/>.
    /// The buffer starts at <c>.LEN</c>. libplctag removes the marker <c>A0 02 HH HH</c> from a read
    /// and adds it again on a write, so the caller sees only the bytes of the members.
    /// </summary>
    public static LogixStringHeader ReadFrom(ReadOnlySpan<byte> buffer) =>
        MemoryMarshal.Read<LogixStringHeader>(buffer);

    /// <summary>Writes this header to the first <see cref="Size"/> bytes of <paramref name="buffer"/>.</summary>
    public void WriteTo(Span<byte> buffer) => MemoryMarshal.Write(buffer, in this);
}
