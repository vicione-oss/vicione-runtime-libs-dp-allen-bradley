namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

/// <summary>
/// The zero-based position of one element in a data file — the <c>12</c> in <c>N7:12</c>. Rockwell's word
/// for an entry of a data file, the way <c>Element</c> is an entry of a Logix array.
/// </summary>
/// <param name="Value">The element number; <c>0</c> for the first element.</param>
public readonly record struct ElementNumber(ushort Value);
