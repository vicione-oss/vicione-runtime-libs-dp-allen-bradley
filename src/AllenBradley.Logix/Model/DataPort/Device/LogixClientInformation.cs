using ViciOne.Suite.DataPort.Extensions.Client;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

/// <summary>
/// Identifies a single controller: the pieces of the libplctag attribute string that pick out one
/// device. Intended as the key under which a handle cache / lifecycle manager pools connections.
/// </summary>
/// <param name="Gateway">Controller IP address or host name.</param>
/// <param name="Path">CIP routing path to the CPU, e.g. <c>"1,0"</c>.</param>
/// <param name="ControllerType">The Logix controller family (see <see cref="LogixControllerType"/>).</param>
public sealed record LogixClientInformation(
    Gateway Gateway, Path Path, LogixControllerType ControllerType) : IClientInformation;

public readonly record struct Gateway(string Value);

public readonly record struct Path(string Value);
