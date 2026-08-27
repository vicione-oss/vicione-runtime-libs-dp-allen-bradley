namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Schema;

/// <summary>
/// Raised when the controller's symbol table cannot be browsed — the read of <c>@tags</c> (or a
/// program listing) failed. Because verification runs at connect, an unbrowsable symbol table is a
/// connect failure: the configuration cannot be checked at all.
/// </summary>
public sealed class LogixSchemaException : Exception
{
    /// <summary>Creates a new <see cref="LogixSchemaException"/>.</summary>
    public LogixSchemaException(string message)
        : base(message)
    {
    }

    /// <summary>Creates a new <see cref="LogixSchemaException"/> with an inner cause.</summary>
    public LogixSchemaException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
