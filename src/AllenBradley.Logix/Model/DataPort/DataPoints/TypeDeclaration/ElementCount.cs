namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

/// <summary>
/// How many elements a tag holds: the product of its array dimensions, or <c>1</c> for a scalar.
/// </summary>
/// <param name="Value">The element count; <c>1</c> for a scalar.</param>
public readonly record struct ElementCount(uint Value)
{
    /// <summary>The one element a scalar tag holds.</summary>
    public static ElementCount Scalar => new(1);
}
