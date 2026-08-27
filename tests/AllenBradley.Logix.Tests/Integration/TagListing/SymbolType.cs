namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.TagListing;

/// <summary>
/// Decodes the 16-bit Logix symbol-type value carried by every tag returned from the
/// Symbol object (class 0x6B). Bit layout per
/// docs/AllenBradley.Documentation/cip-protocol/cip-datatypes-reference.md,
/// section "The Logix symbol-type bitfield".
/// </summary>
public static class SymbolType
{
    private const ushort StructMask = 0x8000;
    private const ushort DimensionMask = 0x6000;
    private const ushort SystemMask = 0x1000;
    private const ushort UdtIdMask = 0x0FFF;
    private const ushort AtomicCodeMask = 0x00FF;
    private const ushort BitPositionMask = 0x0700;

    public static bool IsStruct(ushort type) => (type & StructMask) != 0;

    public static bool IsSystem(ushort type) => (type & SystemMask) != 0;

    public static int DimensionCount(ushort type) => (type & DimensionMask) >> 13;

    public static int UdtId(ushort type) => type & UdtIdMask;

    public static byte AtomicCode(ushort type) => (byte)(type & AtomicCodeMask);

    /// <summary>Bit position of a BOOL aliased onto a bit of a word; meaningless for other types.</summary>
    public static int BitPosition(ushort type) => (type & BitPositionMask) >> 8;

    /// <summary>A plain scalar of an atomic type: not a structure, not a system tag, not an array.</summary>
    public static bool IsAtomicScalar(ushort type) =>
        !IsStruct(type) && !IsSystem(type) && DimensionCount(type) == 0;

    public static string Describe(ushort type)
    {
        var suffix = DimensionCount(type) switch
        {
            0 => string.Empty,
            var d => $"[{new string(',', d - 1)}]"
        };

        if (IsStruct(type))
            return $"STRUCT(udt={UdtId(type)}){suffix}";

        var code = AtomicCode(type);
        var name = AtomicName(code);
        var bit = BitPosition(type);

        return code == 0xC1 && bit != 0
            ? $"{name}.{bit}{suffix}"
            : $"{name}{suffix}";
    }

    public static string AtomicName(byte code) => code switch
    {
        0xC1 => "BOOL",
        0xC2 => "SINT",
        0xC3 => "INT",
        0xC4 => "DINT",
        0xC5 => "LINT",
        0xC6 => "USINT",
        0xC7 => "UINT",
        0xC8 => "UDINT",
        0xC9 => "ULINT",
        0xCA => "REAL",
        0xCB => "LREAL",
        0xD1 => "BYTE",
        0xD2 => "WORD",
        0xD3 => "DWORD",
        0xD4 => "LWORD",
        0xDB => "TIME",
        0xD7 => "LTIME",
        _ => $"UNKNOWN(0x{code:X2})"
    };
}
