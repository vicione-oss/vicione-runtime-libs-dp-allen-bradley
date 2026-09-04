namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

/// <summary>
/// How far along the Logix line a controller is, which is what decides the atomic types it has. The
/// 5X70 controllers and everything before them have <c>BOOL</c>, <c>SINT</c>, <c>INT</c>, <c>DINT</c>,
/// <c>LINT</c> and <c>REAL</c>; the 5X80 controllers add the unsigned integers and <c>LREAL</c>.
/// Members are ordered oldest first and compared, and their values are the numbers in the names, so a later
/// generation slots in at its own number.
/// </summary>
public enum LogixGeneration
{
    /// <summary>ControlLogix 5550/5555/5560/5570 and CompactLogix 1769/5370.</summary>
    Logix5X70 = 70,

    /// <summary>ControlLogix 5580 and CompactLogix 5380/5480.</summary>
    Logix5X80 = 80,
}
