using ViciOne.ManagedEngine.Communication;
using ViciOne.Suite.DataPort.Extensions.Outgoing.QueueProcessing;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;

/// <summary>
/// The device node's configuration as the engine delivers it: flat and primitive-typed, one property
/// per <c>Connection</c> declaration in <c>allen-bradley-logix.yaml</c>.
/// <see cref="Mapping.DeviceNodeMapper"/> turns it into the
/// <see cref="DataPort.Device.LogixClientInformation"/> the client stack is built from.
/// </summary>
/// <remarks>
/// Primitives rather than the model's value types, because this is what deserialization hands over.
/// The property names are a contract with the manifest — the YAML consistency test holds the two sides
/// to each other.
/// </remarks>
[Communication(LogixDataPort.AddonId)]
public sealed record LogixCommunication : DataPortCommunication
{
    /// <summary>IP address or host name of the controller's EtherNet/IP interface.</summary>
    public required string Gateway { get; init; }
    /// <summary>CIP routing path from that interface to the CPU, e.g. <c>"1,0"</c>.</summary>
    public required string Path { get; init; }
    /// <summary>
    /// The controller family, as the ordinal of a <see cref="DataPort.Device.LogixControllerType"/>.
    /// </summary>
    public required byte ControllerType { get; init; }
    /// <summary>How long one tag read or write against this controller may take, in milliseconds.</summary>
    public int OperationTimeout { get; init; } = 5_000;
    /// <summary>
    /// How many write batches the outgoing port queues before the overflow <see cref="Strategy"/>
    /// applies. Read by the outgoing port alone; an incoming-only device carries the default.
    /// </summary>
    public int MaxPendingMessages { get; init; } = 100_000;
    /// <summary>
    /// What a full write queue drops, as the ordinal of a <see cref="QueueStrategy"/>. The default
    /// drops the oldest batch, which keeps the freshest setpoint moving when a controller stalls.
    /// </summary>
    public byte Strategy { get; init; } = (byte)QueueStrategy.DropOldest;
}
