using ViciOne.Suite.DataPort.Extensions.Client;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

/// <summary>
/// Identifies a single controller and says how long one operation against it may take: the pieces of
/// the libplctag attribute string that pick out one device, plus the timeout stamped onto every handle
/// built for it. It is the key <c>LogixClientPool</c> holds a connection under.
/// </summary>
/// <remarks>
/// It is a record, so equality is the "same connection" test the pool needs — and the timeout is part
/// of it. Every value here arrives together from one device node, so two ports on one controller agree
/// on all four or they are configured against two different devices.
/// </remarks>
/// <param name="Gateway">Controller IP address or host name.</param>
/// <param name="Path">CIP routing path to the CPU, e.g. <c>"1,0"</c>.</param>
/// <param name="ControllerType">The Logix controller family (see <see cref="LogixControllerType"/>).</param>
/// <param name="OperationTimeout">How long one tag read or write against this controller may take.</param>
public sealed record LogixClientInformation(
    Gateway Gateway,
    Path Path,
    LogixControllerType ControllerType,
    OperationTimeout OperationTimeout) : IClientInformation;

public readonly record struct Gateway(string Value);

public readonly record struct Path(string Value)
{
    /// <summary>
    /// The path to a controller that places itself on a virtual backplane: hop 1 is the backplane, and
    /// slot 0 is where a DIN-rail controller always sits. Nothing about it is configurable, which is why
    /// a CompactLogix node does not ask for a path at all.
    /// </summary>
    public static Path VirtualBackplane => new("1,0");
}

/// <summary>
/// How long one tag read or write may take before libplctag aborts it. Per-device configuration rather
/// than a constant on the factory: the shared-connection ADR sizes it for the worst case of draining
/// <em>this</em> controller's session, which depends on how much is configured against it.
/// </summary>
public readonly record struct OperationTimeout(TimeSpan Value);
