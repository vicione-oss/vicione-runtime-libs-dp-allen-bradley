namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

/// <summary>
/// Which line a controller belongs to: how its hardware is shaped, and with it how a request reaches
/// the CPU. ControlLogix is the 1756 chassis line, where the CPU sits in whichever slot the chassis was
/// built with; CompactLogix clips onto a DIN rail and places itself at slot 0 of a virtual backplane.
/// </summary>
/// <remarks>
/// Resolved from the device node's design id rather than configured: an integrator picks the family by
/// picking a node type. See
/// <c>docs/AllenBradley.Documentation/cip-protocol/controller-families-and-routing.md</c>.
/// </remarks>
public enum LogixControllerFamily
{
    /// <summary>ControlLogix (1756) chassis-based controller.</summary>
    ControlLogix,

    /// <summary>CompactLogix (1769 / 5069) DIN-rail controller.</summary>
    CompactLogix,
}
