namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

/// <summary>
/// How far along the Logix line a controller is, which is what decides the atomic types it has
/// (<c>reference/datatype-support.md</c>). Members are compared, so they are ordered oldest first and
/// numbered after their names — a later generation slots in at its own number.
/// </summary>
public enum LogixGeneration
{
    /// <summary>ControlLogix 5550/5555/5560/5570 and CompactLogix 1769/5370.</summary>
    Logix5X70 = 70,

    /// <summary>ControlLogix 5580 and CompactLogix 5380/5480.</summary>
    Logix5X80 = 80,
}
