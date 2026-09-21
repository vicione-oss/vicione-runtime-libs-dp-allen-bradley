namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.Templates;

/// <summary>
/// Where a member's bytes start within its structure. Read from the template rather than summed from
/// the members before it, because the controller aligns members and pads between them.
/// </summary>
/// <param name="Value">The byte offset from the start of the structure instance.</param>
internal readonly record struct MemberOffset(uint Value);
