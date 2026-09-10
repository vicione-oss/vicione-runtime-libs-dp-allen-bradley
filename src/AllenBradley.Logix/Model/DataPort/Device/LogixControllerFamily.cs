namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

/// <summary>
/// Which line a controller belongs to: how its hardware is shaped, and with it how a request reaches
/// the CPU.
/// </summary>
public enum LogixControllerFamily
{
    /// <summary>ControlLogix (1756) chassis-based controller.</summary>
    ControlLogix,

    /// <summary>CompactLogix (1769 / 5069) DIN-rail controller.</summary>
    CompactLogix,
}
