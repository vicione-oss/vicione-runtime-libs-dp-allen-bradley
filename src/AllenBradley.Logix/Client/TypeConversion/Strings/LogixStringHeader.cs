using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Strings;

/// <summary>
/// The fixed part of a Logix string, laid out exactly as the controller stores it: <c>.LEN : DINT</c>,
/// the count of characters currently held. The <c>.DATA : SINT[n]</c> that follows is variable — the
/// <c>n</c> is 82 for the built-in <c>STRING</c> and something else for a <c>STRING_20</c> — so it stays
/// outside, the same split <see cref="TagsEntryHeader"/> makes for a tag's variable-length name.
/// </summary>
/// <remarks>
/// <para>
/// A plain <c>readonly struct</c> of fields, not one of the repo's record structs: this is a wire layout
/// rather than a domain concept, and marshalling reads fields, not properties.
/// </para>
/// <para>
/// Offsets start at <c>.LEN</c>, not at the CIP abbreviated-structure marker. A structured tag's read
/// reply opens with <c>A0 02 HH HH</c> — the marker plus the template handle — but libplctag strips that
/// into its own encoded-type-info store and hands the caller only the member bytes, re-attaching it on
/// write. Its own Logix string layout says the same thing: count word 4 bytes at offset 0, capacity 82,
/// 2 pad bytes, 88 total. So the buffer this reads from starts at <c>.LEN</c>.
/// </para>
/// <para>
/// <see cref="Size"/> is therefore also the offset of <c>.DATA</c>, and the same four bytes
/// <see cref="StringMaxLength"/> subtracts to read a declared structure size back as a capacity.
/// <c>Pack = 1</c> earns nothing against a lone DINT; it is here because the type is a wire layout and
/// says so.
/// </para>
/// <para>
/// Reinterpreting bytes takes the host's byte order instead of stating one, which holds only because CIP
/// is little-endian and so is every platform .NET runs on.
/// </para>
/// </remarks>
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
