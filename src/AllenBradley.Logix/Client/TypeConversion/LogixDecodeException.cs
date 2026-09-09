namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;

// Raised when a reply does not fit the shape the data point was configured as — the tag was verified at
// connect time, so this is the controller having moved since: a resized array, a retyped tag. It is the
// converter's own failure kind rather than an ArgumentException, because nothing about it is a caller's
// mistake, and LogixReadBatch turns exactly this into one tag's named failure while its siblings come home.
internal sealed class LogixDecodeException(string message) : Exception(message);
