using ViciOne.ManagedEngine.Communication;
using ViciOne.Suite.DataPort.Extensions.Outgoing.QueueProcessing;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;

/// <summary>
/// The device node's configuration as the engine delivers it: flat and primitive-typed, one property
/// per <c>Connection</c> declaration in <c>allen-bradley-logix.yaml</c>. The property names are a
/// contract with the manifest, held by the YAML consistency test.
/// </summary>
[Communication(LogixDataPort.DataPortId)]
public sealed record LogixCommunication : DataPortCommunication
{
    /// <summary>IP address or host name of the controller's EtherNet/IP interface.</summary>
    public required string ConnectionEndpoint { get; init; }
    /// <summary>
    /// TCP port that interface listens on. Its own property rather than a suffix on the address, even
    /// though libplctag takes the two as one <c>host:port</c> string.
    /// </summary>
    public ushort TcpPort { get; init; } = 44818;
    /// <summary>
    /// CIP route path from that interface to the CPU, e.g. <c>"1,0"</c>. Absent on a CompactLogix,
    /// whose virtual backplane fixes it at
    /// <see cref="DataPort.Device.CipRoutePath.VirtualBackplane"/>.
    /// </summary>
    public string? CipRoutePath { get; init; }
    /// <summary>How long one tag read or write against this controller may take, in milliseconds.</summary>
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
