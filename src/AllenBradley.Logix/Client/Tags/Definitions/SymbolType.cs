using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;

/// <summary>
/// Decodes the 16-bit Logix symbol-type value carried by every entry in an <c>@tags</c> listing (the
/// CIP Symbol object, class <c>0x6B</c>): whether it is a structure, its array rank, and — for an
/// atomic type — its elementary data type.
/// </summary>
internal static class SymbolType
{
    private const ushort StructMask = 0x8000;
    private const ushort DimensionMask = 0x6000;
    private const ushort AtomicCodeMask = 0x00FF;

    /// <summary>Whether the tag is a structure (Logix <c>STRING</c>, a <c>TIMER</c>, any UDT).</summary>
    public static bool IsStruct(ushort type) => (type & StructMask) != 0;

    /// <summary>The array rank: <c>0</c> for a scalar, up to <c>3</c>.</summary>
    public static int DimensionCount(ushort type) => (type & DimensionMask) >> 13;

    /// <summary>The elementary data type the low byte names; meaningful only for a non-structure.</summary>
    public static AllenBradleyDataType AtomicType(ushort type) =>
        ((CipTypeCode)(byte)(type & AtomicCodeMask)).ToDataType();

    /// <summary>
    /// Whether the tag is a <c>BOOL</c> array, which the controller declares as an array of the
    /// <c>DWORD</c>s it packs the bits into. It is the one atomic type whose dimensions do not count
    /// what <see cref="AtomicType"/> reports, so it is the one the element count must be derived for.
    /// </summary>
    public static bool IsPackedBoolArray(ushort type) =>
        !IsStruct(type) && DimensionCount(type) > 0 && (type & AtomicCodeMask) == (ushort)CipTypeCode.Dword;
}
