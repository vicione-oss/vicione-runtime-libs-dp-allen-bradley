namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client;

/// <summary>
/// Thrown when elements could not be read or written. The message names each failed element and the
/// reason.
/// </summary>
public sealed class LegacyTagException : Exception
{
    public LegacyTagException(string message)
        : base(message)
    {
    }

    public LegacyTagException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
