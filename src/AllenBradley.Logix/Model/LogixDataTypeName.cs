namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model;

/// <summary>
/// The Logix spelling of a data type — <c>DINT</c>, <c>REAL</c>, <c>STRING</c> — as Studio 5000 names it.
/// A display name for messages, never identity.
/// </summary>
/// <param name="Value">The type name.</param>
public readonly record struct LogixDataTypeName(string Value)
{
    public static LogixDataTypeName Bool => new("BOOL");

    public static LogixDataTypeName SInt => new("SINT");

    public static LogixDataTypeName Int => new("INT");

    public static LogixDataTypeName DInt => new("DINT");

    public static LogixDataTypeName LInt => new("LINT");

    public static LogixDataTypeName USInt => new("USINT");

    public static LogixDataTypeName UInt => new("UINT");

    public static LogixDataTypeName UDInt => new("UDINT");

    public static LogixDataTypeName ULInt => new("ULINT");

    public static LogixDataTypeName Real => new("REAL");

    public static LogixDataTypeName LReal => new("LREAL");

    public static LogixDataTypeName String => new("STRING");

    /// <summary>
    /// A one-dimensional <c>SINT</c> array. The length is per declaration rather than per type, so no
    /// array name carries it: Studio 5000 would say <c>SINT[10]</c>.
    /// </summary>
    public static LogixDataTypeName SIntArray => new("SINT[]");

    public static LogixDataTypeName IntArray => new("INT[]");

    public static LogixDataTypeName DIntArray => new("DINT[]");

    public static LogixDataTypeName LIntArray => new("LINT[]");

    public static LogixDataTypeName USIntArray => new("USINT[]");

    public static LogixDataTypeName UIntArray => new("UINT[]");

    public static LogixDataTypeName UDIntArray => new("UDINT[]");

    public static LogixDataTypeName ULIntArray => new("ULINT[]");

    public static LogixDataTypeName RealArray => new("REAL[]");

    public static LogixDataTypeName LRealArray => new("LREAL[]");

    public override string ToString() => Value;
}
