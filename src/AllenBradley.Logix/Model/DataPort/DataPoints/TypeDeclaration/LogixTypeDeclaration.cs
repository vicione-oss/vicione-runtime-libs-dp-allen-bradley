namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

/// <summary>
/// What the controller's symbol table reports for one tag, decoded from an <c>@tags</c> listing: the
/// device truth a configured <see cref="ILogixDataPoint"/> is verified against.
/// </summary>
/// <remarks>
/// A structure carries no elementary code, so <see cref="AtomicType"/> is <c>null</c> exactly when
/// <see cref="Kind"/> is <see cref="LogixTypeKind.Structure"/>. <see cref="DimensionCount"/> is
/// <see cref="DimensionCount.Scalar"/> for a scalar and rank <c>1</c>–<c>3</c> for an array;
/// <see cref="ElementCount"/> is the product of its dimensions, or <c>1</c> for a scalar.
/// </remarks>
/// <param name="TagName">The tag's name as the controller reports it (program-qualified for program tags).</param>
/// <param name="Kind">Whether the tag is an atomic type or a structure (the <c>0x8000</c> symbol-type marker).</param>
/// <param name="AtomicType">The elementary CIP type, or <c>null</c> for a structure.</param>
/// <param name="DimensionCount">The array rank: scalar, or up to <c>3</c>.</param>
/// <param name="ElementCount">The number of elements: the product of the dimensions, or <c>1</c> for a scalar.</param>
/// <param name="ElementLength">The size in bytes of a single element, as the listing reports it.</param>
public readonly record struct LogixTypeDeclaration(
    TagName TagName,
    LogixTypeKind Kind,
    CipType? AtomicType,
    DimensionCount DimensionCount,
    ElementCount ElementCount,
    ElementLength ElementLength);
