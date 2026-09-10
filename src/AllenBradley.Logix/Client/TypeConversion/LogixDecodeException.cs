namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;

/// <summary>
/// Raised when a reply does not fit the shape the data point was configured as — the tag was verified at
/// connect, so this is the controller having moved since: a resized array, a retyped tag. LogixReadBatch
/// turns it into one tag's named failure while its siblings come home.
/// </summary>
internal sealed class LogixDecodeException(string message) : Exception(message);
