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
/// on all three or they are configured against two different devices.
/// <para>
/// The controller family is deliberately not here. It picks a node type and decides what that node
/// offers, but it reaches nothing on the wire: libplctag opens a ControlLogix and a CompactLogix the
/// same way. Carrying it would split one connection in two whenever two ports on one controller were
/// configured under different node types.
/// </para>
/// </remarks>
/// <param name="ConnectionEndpoint">Controller IP address or host name.</param>
/// <param name="TcpPort">The TCP port that endpoint listens on, 44818 unless something moved it.</param>
/// <param name="CipRoutePath">CIP route path from that endpoint to the CPU, e.g. <c>"1,0"</c>.</param>
/// <param name="OperationTimeout">How long one tag read or write against this controller may take.</param>
public sealed record LogixClientInformation(
    ConnectionEndpoint ConnectionEndpoint,
    TcpPort TcpPort,
    CipRoutePath CipRoutePath,
    OperationTimeout OperationTimeout) : IClientInformation
{
    /// <summary>
    /// Endpoint and port joined the way libplctag's <c>gateway</c> attribute wants them,
    /// <c>"10.0.0.1:44818"</c>. It has no port attribute of its own and splits the colon out in the
    /// native core, so this is the one place the two are a single string
    /// (<c>docs/AllenBradley.Documentation/libPlcTag/the-port-in-the-gateway-attribute.md</c>).
    /// </summary>
    public string GatewayAttribute => $"{ConnectionEndpoint.Value}:{TcpPort.Value}";
}

/// <summary>
/// Where an EtherNet/IP session is opened: the IP address or host name of the controller, or of the
/// bridge in front of it. The port it answers on is <see cref="Device.TcpPort"/>, kept apart from the
/// address here and joined only for libplctag.
/// </summary>
public readonly record struct ConnectionEndpoint(string Value);

/// <summary>
/// The TCP port the EtherNet/IP session is opened on. Nothing to do with a CIP port, which is a hop in
/// a route path.
/// </summary>
public readonly record struct TcpPort(ushort Value)
{
    /// <summary>
    /// The port ODVA registered for EtherNet/IP, and the one a controller answers on unless something
    /// between it and us — a NAT rule, a tunnel — moved it.
    /// </summary>
    public static TcpPort EtherNetIp => new(44818);
}

public readonly record struct CipRoutePath(string Value)
{
    /// <summary>
    /// The CIP route path to a controller that places itself on a virtual backplane: hop 1 is the backplane,
    /// and slot 0 is where a DIN-rail controller always sits. Nothing about it is configurable, which is
    /// why a CompactLogix node does not ask for a route path at all.
    /// </summary>
    public static CipRoutePath VirtualBackplane => new("1,0");
}

/// <summary>
/// How long one tag read or write may take before libplctag aborts it. Per-device configuration rather
/// than a constant on the factory: the shared-connection ADR sizes it for the worst case of draining
/// <em>this</em> controller's session, which depends on how much is configured against it.
/// </summary>
public readonly record struct OperationTimeout(TimeSpan Value);
