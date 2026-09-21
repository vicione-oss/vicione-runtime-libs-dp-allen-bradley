namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.SymbolTypes;

/// <summary>
/// The id of a template — the instance of the Template object (class <c>0x6C</c>) that lays out one
/// structure data type. A structure's symbol type names it in its low twelve bits, so the range is
/// <c>0</c> to <c>4095</c> (CONTEXT.md, "Template id").
/// </summary>
/// <param name="Value">The template instance id.</param>
internal readonly record struct TemplateId(ushort Value);
