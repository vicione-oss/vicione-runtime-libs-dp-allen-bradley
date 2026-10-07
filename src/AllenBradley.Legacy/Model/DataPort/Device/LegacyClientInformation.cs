using ViciOne.Suite.DataPort.Extensions.Client;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.Device;

/// <summary>
/// Identifies one controller and says how long an operation against it may take. The family is part of
/// it, because it decides how libplctag talks to the controller.
/// </summary>
/// <param name="Family">Which line the controller belongs to.</param>
/// <param name="ConnectionEndpoint">IP address or host name of the controller, or of the bridge in front of it.</param>
/// <param name="TcpPort">The TCP port that endpoint listens on, 44818 unless something moved it.</param>
/// <param name="CipRoutePath">
/// The hops from a bridge to the controller, or <c>null</c> for a controller with Ethernet of its own.
/// </param>
/// <param name="OperationTimeout">How long one read or write against this controller may take.</param>
public record LegacyClientInformation(
    LegacyControllerFamily Family,
    ConnectionEndpoint ConnectionEndpoint,
    TcpPort TcpPort,
    CipRoutePath? CipRoutePath,
    OperationTimeout OperationTimeout) : IClientInformation;

/// <summary>
/// Where an EtherNet/IP session is opened: the IP address or host name of the controller, or of the
/// bridge in front of it.
/// </summary>
public readonly record struct ConnectionEndpoint(string Value);

/// <summary>
/// The TCP port the EtherNet/IP session is opened on. Nothing to do with a CIP port, which is a hop in
/// a route path.
/// </summary>
public readonly record struct TcpPort(ushort Value)
{
    /// <summary>The port ODVA registered for EtherNet/IP, unless a NAT rule or a tunnel moved it.</summary>
    public static TcpPort EtherNetIp => new(44818);
}

/// <summary>
/// How a request travels from a bridge to the controller behind it, as a sequence of hops — through a
/// ControlLogix gateway onto DH+, for example.
/// </summary>
public readonly record struct CipRoutePath(string Value);

/// <summary>How long one read or write may take before libplctag aborts it.</summary>
public readonly record struct OperationTimeout(TimeSpan Value);
