namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

/// <summary>
/// A named word inside an element of more than one word — the <c>ACC</c> in <c>T4:0.ACC</c>. A closed set of
/// values, one static member each.
/// </summary>
/// <param name="Mnemonic">The name RSLogix 500 writes after the dot.</param>
public readonly record struct SubElement(string Mnemonic)
{
    /// <summary><c>ACC</c> — the accumulated value of a timer.</summary>
    public static readonly SubElement Accumulated = new("ACC");
}
