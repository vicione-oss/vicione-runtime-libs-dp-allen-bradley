namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration.Templates;

/// <summary>
/// The bytes one instance of a structure occupies as the controller stores it, alignment padding
/// included — <c>88</c> for the built-in <c>STRING</c>, <c>12</c> for a <c>TIMER</c>.
/// </summary>
/// <param name="Value">The instance size in bytes.</param>
public readonly record struct StructureSize(uint Value);
