namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.Templates;

/// <summary>
/// The name of a structure data type as its template carries it — <c>STRING</c>, <c>TIMER</c>, or the
/// name a UDT was declared with in Studio 5000.
/// </summary>
/// <param name="Value">The name text.</param>
internal readonly record struct TemplateName(string Value);
