using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.SymbolTypes;

/// <summary>
/// The 16-bit Logix symbol-type value carried by every entry in an <c>@tags</c> listing (the CIP
/// Symbol object, class <c>0x6B</c>) and by every member descriptor of a template: whether it is a
/// structure, its array rank, and either its elementary data type or the template it follows. Laid
/// out as the one word it is on the wire, so a header holds it in place.
/// </summary>
/// <param name="Value">The word as the controller sends it.</param>
internal readonly record struct SymbolType(ushort Value)
{
    private const ushort StructMask = 0x8000;
    private const ushort DimensionMask = 0x6000;
    private const ushort SystemMask = 0x1000;
    private const ushort TemplateIdMask = 0x0FFF;
    private const ushort AtomicCodeMask = 0x00FF;

    /// <summary>Whether the tag is a structure (Logix <c>STRING</c>, a <c>TIMER</c>, any UDT).</summary>
    public bool IsStruct => (Value & StructMask) != 0;

    /// <summary>
    /// Whether the tag is one the controller keeps for itself — a program entry, a module connection —
    /// rather than one a project declares. A system structure's low bits are not a template the
    /// controller serves.
    /// </summary>
    public bool IsSystem => (Value & SystemMask) != 0;

    /// <summary>The array rank: scalar, or up to <c>3</c>.</summary>
    public DimensionCount DimensionCount => new((Value & DimensionMask) >> 13);

    /// <summary>
    /// The data type: the elementary type the low byte names, or
    /// <see cref="AllenBradleyDataType.Structure"/> for any structure, whose template says the rest.
    /// </summary>
    public AllenBradleyDataType DataType =>
        IsStruct ? AllenBradleyDataType.Structure : ((CipTypeCode)(byte)(Value & AtomicCodeMask)).ToDataType();

    /// <summary>
    /// The template a structure's low twelve bits name, or <c>null</c> for an atomic type and for a
    /// system structure, whose template read the controller refuses.
    /// </summary>
    public TemplateId? TemplateId =>
        IsStruct && !IsSystem ? new TemplateId((ushort)(Value & TemplateIdMask)) : null;

    /// <summary>
    /// Whether the tag is a <c>BOOL</c> array, which the controller declares as an array of the
    /// <c>DWORD</c>s it packs the bits into. It is the one atomic type whose dimensions do not count
    /// what <see cref="DataType"/> reports, so it is the one the element count must be derived for.
    /// </summary>
    public bool IsPackedBoolArray =>
        !IsStruct && !DimensionCount.IsScalar && (Value & AtomicCodeMask) == (ushort)CipTypeCode.Dword;

    /// <summary>
    /// The elements a declaration of <paramref name="declaredCount"/> holds: the bits of a packed
    /// <c>BOOL</c> array, whose declaration counts words, and the declared count for every other type.
    /// </summary>
    public ElementCount ToElementCount(uint declaredCount) =>
        IsPackedBoolArray
            ? BoolArrayDataPoint.ElementCountOfPackedWords(declaredCount)
            : new ElementCount(declaredCount);
}
