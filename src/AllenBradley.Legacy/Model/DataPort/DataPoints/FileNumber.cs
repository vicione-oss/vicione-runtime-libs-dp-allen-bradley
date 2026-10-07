namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

/// <summary>
/// The number of a data file in the controller's data table — the <c>7</c> in <c>N7:0</c>. Which numbers
/// exist is the program's choice, not the type's: <c>N7</c> is only where RSLogix puts the first integer
/// file.
/// </summary>
/// <param name="Value">The file number.</param>
public readonly record struct FileNumber(ushort Value);
