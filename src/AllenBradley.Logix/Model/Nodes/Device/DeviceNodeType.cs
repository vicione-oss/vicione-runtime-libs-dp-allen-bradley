using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;

/// <summary>
/// What one device node id stands for: the family, which decides how a request reaches the CPU, and the
/// generation, which decides what the controller has to address. The manifest declares one node per
/// pair, so an integrator answers both by picking a node rather than by filling in two properties.
/// </summary>
/// <param name="Family">The line the controller belongs to.</param>
/// <param name="Generation">How far along that line it is.</param>
public readonly record struct DeviceNodeType(
    LogixControllerFamily Family,
    LogixGeneration Generation);
