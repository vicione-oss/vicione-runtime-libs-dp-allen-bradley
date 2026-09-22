namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

/// <summary>
/// What the controller declares the value at one address to be: the device truth a configured
/// <see cref="ILogixDataPoint"/> is verified against. The symbol-table lookup hands one back for
/// whatever a <see cref="TagPath"/> reaches — a tag, a member inside it, or one element of an array —
/// at the address that path renders to.
/// </summary>
/// <param name="TagAddress">
/// The address of the value, as libplctag is handed it: a tag's listed name, program-qualified for a
/// program tag, with the members and the element behind it when the path reaches inside.
/// </param>
/// <param name="DataType">
/// The atomic data type, <see cref="AllenBradleyDataType.String"/> for a string structure, and
/// <see cref="AllenBradleyDataType.Structure"/> for any other structure — a UDT, a <c>TIMER</c>, an
/// Add-On Instruction instance — which this dataport reads as members rather than as one value.
/// </param>
/// <param name="MaxLength">The character capacity of a string structure; <c>null</c> for every other type.</param>
/// <param name="DimensionCount">The array rank: scalar, or up to <c>3</c>.</param>
/// <param name="ElementCount">The number of elements: the product of the dimensions, or <c>1</c> for a scalar.</param>
public readonly record struct DeclaredType(
    TagAddress TagAddress,
    AllenBradleyDataType DataType,
    StringMaxLength? MaxLength,
    DimensionCount DimensionCount,
    ElementCount ElementCount);
