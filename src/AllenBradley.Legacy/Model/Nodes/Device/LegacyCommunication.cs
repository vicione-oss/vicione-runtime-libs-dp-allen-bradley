using ViciOne.ManagedEngine.Communication;
using ViciOne.Suite.DataPort.Extensions.Outgoing.QueueProcessing;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.Device;

/// <summary>
/// The device node's configuration as the engine delivers it: flat and primitive-typed, one property
/// per <c>Connection</c> and <c>QueueConfiguration</c> declaration in <c>allen-bradley-legacy.yaml</c>.
/// The property names are a contract with the manifest, held by the YAML consistency test.
/// </summary>
[Communication(LegacyDataPort.DataPortId)]
public sealed record LegacyCommunication : DataPortCommunication
{
    /// <summary>IP address or host name of the controller, or of the bridge in front of it.</summary>
    public required string ConnectionEndpoint { get; init; }
    /// <summary>TCP port that endpoint listens on.</summary>
    public ushort TcpPort { get; init; } = 44818;
    /// <summary>
    /// The hops from a bridge to the controller behind it. Empty for a controller with Ethernet of its
    /// own, which is the endpoint itself.
    /// </summary>
    public string? CipRoutePath { get; init; }
    /// <summary>How long one read or write against this controller may take, in milliseconds.</summary>
    public int OperationTimeout { get; init; } = 5_000;
    /// <summary>
    /// How many write batches the outgoing port queues before the overflow <see cref="Strategy"/>
    /// applies; an incoming-only device carries the default.
    /// </summary>
    public int MaxPendingMessages { get; init; } = 100_000;
    /// <summary>
    /// What a full write queue drops, as the ordinal of a <see cref="QueueStrategy"/>. The default
    /// drops the oldest batch, so the freshest setpoint keeps moving when a controller stalls.
    /// </summary>
    public byte Strategy { get; init; } = (byte)QueueStrategy.DropOldest;
}
