namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;

/// <summary>
/// Raised when a reply does not fit the shape the data point was configured as — the controller has
/// changed since the configuration was checked: a resized array, a retyped tag. The read batch turns it
/// into one tag's named failure while its siblings come home.
/// </summary>
internal sealed class DecodeException(string message) : Exception(message);
