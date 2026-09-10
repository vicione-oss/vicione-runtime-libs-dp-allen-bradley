namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

/// <summary>
/// A controller family and generation together: the family decides how a request reaches the CPU, the
/// generation decides what the controller has to address.
/// </summary>
/// <param name="Family">The line the controller belongs to.</param>
/// <param name="Generation">How far along that line it is.</param>
public readonly record struct LogixControllerKind(
    LogixControllerFamily Family,
    LogixGeneration Generation)
{
    /// <summary>A ControlLogix 5550/5560/5570 in a 1756 chassis.</summary>
    public static LogixControllerKind ControlLogix5X70 =>
        new(LogixControllerFamily.ControlLogix, LogixGeneration.Logix5X70);

    /// <summary>A ControlLogix 5580 in a 1756 chassis.</summary>
    public static LogixControllerKind ControlLogix5X80 =>
        new(LogixControllerFamily.ControlLogix, LogixGeneration.Logix5X80);

    /// <summary>A CompactLogix 1769/5370 on a DIN rail.</summary>
    public static LogixControllerKind CompactLogix5X70 =>
        new(LogixControllerFamily.CompactLogix, LogixGeneration.Logix5X70);

    /// <summary>A CompactLogix 5380/5480 on a DIN rail.</summary>
    public static LogixControllerKind CompactLogix5X80 =>
        new(LogixControllerFamily.CompactLogix, LogixGeneration.Logix5X80);
}
