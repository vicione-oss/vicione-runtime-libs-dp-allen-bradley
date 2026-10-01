using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.SymbolTypes;

/// <summary>
/// One node of the symbol-table walk: what the controller declares a value to be, and the template to
/// walk into when the value is a structure. A listed tag and a template member both hold one, so a
/// member and a tag of the same type read alike. It carries no address, because a member has none of
/// its own; the walk puts the address on when it hands the node out as a <see cref="DeclaredType"/>.
/// </summary>
/// <param name="DataType">
/// The atomic data type, <see cref="AllenBradleyDataType.String"/> for a string structure,
/// <see cref="AllenBradleyDataType.Timer"/> for a timer, or <see cref="AllenBradleyDataType.Counter"/>
/// for a counter. Any other structure is <see cref="AllenBradleyDataType.Structure"/> here and names its
/// template beside it.
/// </param>
/// <param name="TemplateId">
/// The template that lays out a structure's members; <c>null</c> for an atomic type, and for a system
/// structure, whose template the controller does not serve.
/// </param>
/// <param name="MaxLength">The character capacity of a string structure; <c>null</c> for every other type.</param>
/// <param name="DimensionCount">The array rank: scalar, or up to <c>3</c>.</param>
/// <param name="ElementCount">The number of elements: the product of the dimensions, or <c>1</c> for a scalar.</param>
internal readonly record struct TagDefinition(
    AllenBradleyDataType DataType,
    TemplateId? TemplateId,
    StringMaxLength? MaxLength,
    DimensionCount DimensionCount,
    ElementCount ElementCount)
{
    /// <summary>
    /// The node a symbol type declares, whether it came off a listing entry or a member descriptor.
    /// <paramref name="declaredCount"/> is the count the declaration carries — the product of the
    /// dimensions, <c>1</c> for a scalar — and is words rather than elements for a packed <c>BOOL</c> array.
    /// A structure is <see cref="AllenBradleyDataType.Structure"/> here; the lookup says which are strings, timers and counters.
    /// </summary>
    internal static TagDefinition Of(SymbolType symbolType, uint declaredCount) => new(
        symbolType.DataType,
        symbolType.TemplateId,
        MaxLength: null,
        symbolType.DimensionCount,
        symbolType.ToElementCount(declaredCount));

    /// <summary>This declaration at <paramref name="tagAddress"/>, the shape the outside is handed.</summary>
    internal DeclaredType At(TagAddress tagAddress) =>
        new(tagAddress, DataType, MaxLength, DimensionCount, ElementCount);

    /// <summary>
    /// What the controller would declare one element of this array to be, if it declared elements:
    /// the same type, a scalar, one element.
    /// </summary>
    internal TagDefinition OfOneElement() =>
        this with { DimensionCount = DimensionCount.Scalar, ElementCount = ElementCount.Scalar };

    /// <summary>This structure, now known to be a string holding <paramref name="capacity"/> characters.</summary>
    internal TagDefinition AsStringOf(StringMaxLength capacity) =>
        this with { DataType = AllenBradleyDataType.String, MaxLength = capacity };

    /// <summary>This structure, now known to be a timer.</summary>
    internal TagDefinition AsTimer() => this with { DataType = AllenBradleyDataType.Timer };

    /// <summary>This structure, now known to be a counter.</summary>
    internal TagDefinition AsCounter() => this with { DataType = AllenBradleyDataType.Counter };
}
