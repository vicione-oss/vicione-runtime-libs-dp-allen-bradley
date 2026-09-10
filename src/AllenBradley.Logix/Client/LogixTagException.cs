namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

/// <summary>
/// Raised when tags could not be read or written; the message names every failed tag and its reason.
/// </summary>
public sealed class LogixTagException : Exception
{
    public LogixTagException(string message)
        : base(message)
    {
    }

    public LogixTagException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
