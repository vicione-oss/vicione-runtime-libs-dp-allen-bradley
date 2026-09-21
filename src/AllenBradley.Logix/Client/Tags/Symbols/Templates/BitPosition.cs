namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.Templates;

/// <summary>
/// Which bit of its host byte a <c>BOOL</c> member of a structure is: the controller packs up to eight
/// of them into one hidden <c>SINT</c>, and the member's offset names that byte.
/// </summary>
/// <param name="Value">The bit, <c>0</c> being the least significant.</param>
internal readonly record struct BitPosition(int Value);
