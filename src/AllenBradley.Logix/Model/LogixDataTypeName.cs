namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model;

/// <summary>
/// The Logix spelling of a data type — <c>DINT</c>, <c>REAL</c>, <c>STRING</c> — as Studio 5000 names it.
/// A display name for messages, never identity.
/// </summary>
/// <param name="Value">The type name.</param>
public readonly record struct LogixDataTypeName(string Value)
{
    /// <summary>A single-byte boolean; nonzero is true.</summary>
    public static LogixDataTypeName Bool => new("BOOL");

    /// <summary>An 8-bit signed integer.</summary>
    public static LogixDataTypeName SInt => new("SINT");

    /// <summary>A 16-bit signed integer.</summary>
    public static LogixDataTypeName Int => new("INT");

    /// <summary>A 32-bit signed integer.</summary>
    public static LogixDataTypeName DInt => new("DINT");

    /// <summary>A 64-bit signed integer.</summary>
    public static LogixDataTypeName LInt => new("LINT");

    /// <summary>An 8-bit unsigned integer. A 5X80 controller's type; a 5X70 has no such thing.</summary>
    public static LogixDataTypeName USInt => new("USINT");

    /// <summary>An IEEE-754 single.</summary>
    public static LogixDataTypeName Real => new("REAL");

    /// <summary>An IEEE-754 double. A 5X80 controller's type; a 5X70 has no such thing.</summary>
    public static LogixDataTypeName LReal => new("LREAL");

    /// <summary>The predefined string structure — <c>.LEN</c> + <c>.DATA</c>.</summary>
    public static LogixDataTypeName String => new("STRING");

    /// <summary>The type name, so it interpolates and logs as itself.</summary>
    public override string ToString() => Value;
}
