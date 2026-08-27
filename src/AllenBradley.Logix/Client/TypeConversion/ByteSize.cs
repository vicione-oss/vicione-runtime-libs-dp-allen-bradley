namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;

/// <summary>
/// The wire size, in bytes, of one value a converter reads or writes — <c>4</c> for a DINT or a REAL.
/// Sizes the write buffer and, when the controller's type is unknown, backstops the decode-length check.
/// </summary>
/// <param name="Value">The size in bytes.</param>
internal readonly record struct ByteSize(int Value);
