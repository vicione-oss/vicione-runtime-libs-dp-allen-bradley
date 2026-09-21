namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.Templates;

/// <summary>
/// The 16-bit handle a read reply of a structured tag carries behind the abbreviated-structure marker: a
/// CRC of the template's type encoding, which is how a reply says which template its bytes follow
/// (CONTEXT.md, "Structure handle").
/// </summary>
/// <param name="Value">The handle as the controller reports it.</param>
internal readonly record struct StructureHandle(ushort Value);
