using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Schema;

/// <summary>
/// Decodes the 16-bit Logix symbol-type value carried by every entry in an <c>@tags</c> listing (the
/// CIP Symbol object, class <c>0x6B</c>): whether it is a structure, its array rank, and — for an
/// atomic type — its elementary CIP code.
/// </summary>
/// <remarks>
/// Bit layout per the CIP data types reference, section "The Logix symbol-type bitfield": bit 15 marks
/// a structure, bits 14–13 hold the dimension count, and the low byte is the atomic CIP code.
/// </remarks>
internal static class SymbolType
{
    private const ushort StructMask = 0x8000;
    private const ushort DimensionMask = 0x6000;
    private const ushort AtomicCodeMask = 0x00FF;

    /// <summary>Whether the tag is a structure (Logix <c>STRING</c>, a <c>TIMER</c>, any UDT).</summary>
    public static bool IsStruct(ushort type) => (type & StructMask) != 0;

    /// <summary>The array rank: <c>0</c> for a scalar, up to <c>3</c>.</summary>
    public static int DimensionCount(ushort type) => (type & DimensionMask) >> 13;

    /// <summary>The elementary CIP type code; meaningful only for a non-structure.</summary>
    public static CipType AtomicType(ushort type) => (CipType)(byte)(type & AtomicCodeMask);
}
