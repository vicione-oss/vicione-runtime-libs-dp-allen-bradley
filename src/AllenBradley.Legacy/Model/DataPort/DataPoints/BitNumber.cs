namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

/// <summary>
/// The position of one bit in a 16-bit element, counted from the least significant bit — the <c>5</c> in
/// <c>B3:2/5</c>.
/// </summary>
/// <param name="Value">The bit number; <c>0</c> for the least significant bit, <c>15</c> for the most.</param>
public readonly record struct BitNumber(ushort Value);
