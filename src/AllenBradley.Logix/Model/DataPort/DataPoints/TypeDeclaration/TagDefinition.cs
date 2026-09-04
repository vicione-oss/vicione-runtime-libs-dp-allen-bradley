namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

/// <summary>
/// What the controller's symbol table reports for one tag, decoded from an <c>@tags</c> listing: the
/// device truth a configured <see cref="ILogixDataPoint"/> is verified against.
/// </summary>
/// <param name="TagName">The tag's name as the controller reports it (program-qualified for program tags).</param>
/// <param name="Kind">Whether the tag is an atomic type or a structure (the <c>0x8000</c> symbol-type marker).</param>
/// <param name="DataType">The data type the controller declares, or <c>null</c> for a structure this addon does not model.</param>
/// <param name="MaxLength">The declared character capacity of a string tag; <c>null</c> for every other type.</param>
/// <param name="DimensionCount">The array rank: scalar, or up to <c>3</c>.</param>
/// <param name="ElementCount">The number of elements: the product of the dimensions, or <c>1</c> for a scalar.</param>
public readonly record struct TagDefinition(
    TagName TagName,
    LogixTypeKind Kind,
    AllenBradleyDataType? DataType,
    StringMaxLength? MaxLength,
    DimensionCount DimensionCount,
    ElementCount ElementCount);
