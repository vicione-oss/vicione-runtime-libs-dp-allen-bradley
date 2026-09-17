namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration.Templates;

/// <summary>
/// The name of a structure data type as its template carries it — <c>STRING</c>, <c>TIMER</c>, or the
/// name a UDT was declared with in Studio 5000.
/// </summary>
/// <param name="Value">The name text.</param>
public readonly record struct TemplateName(string Value);
