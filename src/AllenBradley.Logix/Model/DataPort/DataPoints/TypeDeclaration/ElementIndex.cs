namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

/// <summary>
/// The zero-based position of one element in a one-dimensional array — the <c>5</c> in <c>Arr[5]</c>.
/// </summary>
/// <param name="Value">The index; <c>0</c> for the first element.</param>
public readonly record struct ElementIndex(uint Value)
{
    /// <summary>Whether an array of <paramref name="elementCount"/> elements has this element.</summary>
    public bool IsWithin(ElementCount elementCount) => Value < elementCount.Value;
}
