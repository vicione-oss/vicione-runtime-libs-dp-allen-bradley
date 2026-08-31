namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

/// <summary>
/// How far along the Logix line a controller is, which is what decides the atomic types it has. The
/// 5x70 controllers and everything before them have <c>BOOL</c>, <c>SINT</c>, <c>INT</c>, <c>DINT</c>,
/// <c>LINT</c> and <c>REAL</c>; the 5x80 controllers add the unsigned integers and <c>LREAL</c>.
/// </summary>
/// <remarks>
/// Resolved from the device node's design id rather than configured, the same way
/// <see cref="LogixControllerFamily"/> is. See
/// <c>docs/AllenBradley.Documentation/cip-protocol/symbolic-tag-data-types.md</c>.
/// </remarks>
public enum LogixGeneration
{
    /// <summary>ControlLogix 5550/5555/5560/5570 and CompactLogix 1769/5370.</summary>
    Logix5x70,

    /// <summary>ControlLogix 5580 and CompactLogix 5380/5480.</summary>
    Logix5x80,
}
