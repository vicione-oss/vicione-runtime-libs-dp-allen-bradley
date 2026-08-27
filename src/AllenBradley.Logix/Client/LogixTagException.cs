namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

/// <summary>
/// Raised by <see cref="ILogixWriteClient.WriteAsync"/> when tags could not be written; the message
/// names every failed tag and its reason. Reads never throw it — a failed read degrades its data
/// point's quality instead.
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
