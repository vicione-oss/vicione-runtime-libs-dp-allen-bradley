using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Strings;

/// <summary>
/// The fixed part of a Logix string, laid out exactly as the controller stores it: <c>.LEN : DINT</c>,
/// the count of characters currently held. The <c>.DATA : SINT[n]</c> that follows is variable — the
/// <c>n</c> is 82 for the built-in <c>STRING</c> and something else for a <c>STRING_20</c> — so it stays
/// outside, the same split <see cref="TagsEntryHeader"/> makes for a tag's variable-length name.
/// Offsets start at <c>.LEN</c>, not at the <c>A0 02 HH HH</c> abbreviated-structure marker: libplctag strips
/// that off a read and re-attaches it on write, so the caller sees only the member bytes. Reinterpreting
/// assumes little-endian, which CIP and every .NET platform are.
/// </summary>
/// <param name="length">The count of characters held in <c>.DATA</c>.</param>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct LogixStringHeader(int length)
{
    /// <summary>
    /// <c>.LEN</c> — a DINT, 32 bits, not the 16-bit count of the elementary CIP <c>STRING</c> (0xD0).
    /// On a read it is the controller's claim about its own <c>.DATA</c>, and nothing here checks it.
    /// </summary>
    public readonly int Length = length;

    /// <summary>The header's on-the-wire size, and so the offset of <c>.DATA</c> within a string.</summary>
    public static int Size => Unsafe.SizeOf<LogixStringHeader>();

    /// <summary>Reinterprets the first <see cref="Size"/> bytes of <paramref name="buffer"/> as a header.</summary>
    public static LogixStringHeader ReadFrom(ReadOnlySpan<byte> buffer) =>
        MemoryMarshal.Read<LogixStringHeader>(buffer);

    /// <summary>Lays this header into the first <see cref="Size"/> bytes of <paramref name="buffer"/>.</summary>
    public void WriteTo(Span<byte> buffer) => MemoryMarshal.Write(buffer, in this);
}
