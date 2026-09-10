namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

/// <summary>
/// The array rank of a Logix tag: <c>0</c> for a scalar, <c>1</c>–<c>3</c> for an array.
/// </summary>
/// <param name="Value">The rank; <c>0</c> for a scalar.</param>
public readonly record struct DimensionCount(int Value)
{
    public static DimensionCount Scalar => new(0);

    /// <summary>Rank 1. Rank 2 and 3 exist on Logix; this port does not read them.</summary>
    public static DimensionCount OneDimensional => new(1);

    public bool IsScalar => Value == 0;
}
