namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

/// <summary>
/// Raised when tags could not be read or written; the message names every failed tag and its reason.
/// Both directions raise it, because both treat their batch as one unit: a group that lost a tag is a
/// group the caller cannot use.
/// </summary>
public sealed class LogixTagException : Exception
{
    /// <summary>Creates a new <see cref="LogixTagException"/>.</summary>
    public LogixTagException(string message)
        : base(message)
    {
    }

    /// <summary>Creates a new <see cref="LogixTagException"/> with an inner cause.</summary>
    public LogixTagException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
