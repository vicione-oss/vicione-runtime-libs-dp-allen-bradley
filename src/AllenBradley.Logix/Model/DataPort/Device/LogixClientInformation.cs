using ViciOne.Suite.DataPort.Extensions.Client;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

/// <summary>
/// Identifies one controller and says how long an operation against it may take — the key
/// <c>LogixClientPool</c> holds a connection under. The controller family is deliberately absent, so that
/// two ports on one controller share a connection
/// (<c>ADR/2026-07-16-maximizing-throughput-with-one-shared-connection.md</c>).
/// </summary>
/// <param name="ConnectionEndpoint">Controller IP address or host name.</param>
/// <param name="TcpPort">The TCP port that endpoint listens on, 44818 unless something moved it.</param>
/// <param name="CipRoutePath">CIP route path from that endpoint to the CPU, e.g. <c>"1,0"</c>.</param>
/// <param name="OperationTimeout">How long one tag read or write against this controller may take.</param>
public record LogixClientInformation(
    ConnectionEndpoint ConnectionEndpoint,
    TcpPort TcpPort,
    CipRoutePath CipRoutePath,
    OperationTimeout OperationTimeout) : IClientInformation;

/// <summary>
/// Where an EtherNet/IP session is opened: the IP address or host name of the controller, or of the
/// bridge in front of it. The port it answers on is <see cref="Device.TcpPort"/>, kept apart from it.
/// </summary>
public readonly record struct ConnectionEndpoint(string Value);

/// <summary>
/// The TCP port the EtherNet/IP session is opened on. Nothing to do with a CIP port, which is a hop in
/// a route path.
/// </summary>
public readonly record struct TcpPort(ushort Value)
{
    /// <summary>
    /// The port ODVA registered for EtherNet/IP, unless a NAT rule or a tunnel moved it.
    /// </summary>
    public static TcpPort EtherNetIp => new(44818);
}

/// <summary>
/// How a request travels from the connection endpoint to the controller, as a sequence of hops.
/// </summary>
public readonly record struct CipRoutePath(string Value)
{
    /// <summary>
    /// The route path to a controller on a virtual backplane: hop 1 is the backplane, and slot 0 is where
    /// a DIN-rail controller always sits.
    /// </summary>
    public static CipRoutePath VirtualBackplane => new("1,0");
}

/// <summary>
/// How long one tag read or write may take before libplctag aborts it. Per-device rather than a
/// constant, because it is sized for draining <em>this</em> controller's session
/// (<c>ADR/2026-07-16-maximizing-throughput-with-one-shared-connection.md</c>).
/// </summary>
public readonly record struct OperationTimeout(TimeSpan Value);
