using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Strings;

/// <summary>
/// The fixed part of a Logix string, laid out exactly as the controller stores it: <c>.LEN : DINT</c>. The
/// variable <c>.DATA : SINT[n]</c> that follows stays outside. Reinterpreted in place, which assumes the
/// little-endian layout CIP and every .NET platform share.
/// </summary>
/// <param name="length">The count of characters held in <c>.DATA</c>.</param>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct LogixStringHeader(int length)
{
    /// <summary>
    /// <c>.LEN</c> — a DINT, 32 bits, not the 16-bit count of the elementary CIP <c>STRING</c> (0xD0).
    /// </summary>
    public readonly int Length = length;

    /// <summary>The header's on-the-wire size, and so the offset of <c>.DATA</c> within a string.</summary>
    public static int Size => Unsafe.SizeOf<LogixStringHeader>();

    /// <summary>
    /// Reinterprets the first <see cref="Size"/> bytes of <paramref name="buffer"/> as a header. The buffer
    /// opens at <c>.LEN</c>: libplctag strips the <c>A0 02 HH HH</c> abbreviated-structure marker off a read
    /// and re-attaches it on write, so the caller sees only the member bytes.
    /// </summary>
    public static LogixStringHeader ReadFrom(ReadOnlySpan<byte> buffer) =>
        MemoryMarshal.Read<LogixStringHeader>(buffer);

    /// <summary>Lays this header into the first <see cref="Size"/> bytes of <paramref name="buffer"/>.</summary>
    public void WriteTo(Span<byte> buffer) => MemoryMarshal.Write(buffer, in this);
}
