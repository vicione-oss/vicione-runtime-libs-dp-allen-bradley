using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using ViciOne.ManagedEngine.ExternalCommunication;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.Mapper;
using ViciOne.Suite.DataPort.Extensions.Client;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Outgoing;
using ViciOne.Suite.DataPort.Extensions.Outgoing.QueueProcessing;
using ViciOne.Suite.DataPort.Extensions.Outgoing.QueueProcessing.Retry;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Outgoing;

/// <summary>
/// The write direction: engine values in, data-file writes out. Like the incoming port, it does not
/// verify its configuration against the controller, because there is nothing meaningful to verify.
/// </summary>
public sealed class OutgoingDataPort : OutgoingDataPortBase<
    ILegacyDataPoint, ILegacyDataPointValue, ILegacyClient, DeviceNode, LegacyCommunication, LegacyClientInformation>
{
    private static readonly InfiniteRetryPolicy SRetryPolicy = new();

    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public constructor used by engine.")]
    public OutgoingDataPort(LegacyCommunication communication, ILoggerFactory loggerFactory)
        : this(communication, loggerFactory, clientLifecycleManager: null, delayProvider: null)
    {
    }

    internal OutgoingDataPort(
        LegacyCommunication communication,
        ILoggerFactory loggerFactory,
        IClientLifecycleManager<ILegacyClient, LegacyClientInformation>? clientLifecycleManager,
        IDelayProvider? delayProvider)
        : base(
            communication,
            loggerFactory,
            TypedLegacyNodeMapper.Instance(),
            new LegacyDataPointsGroupsMapper(),
            new QueueConfiguration(
                (QueueStrategy)communication.Strategy,
                new QueueSize(communication.MaxPendingMessages)),
            delayProvider ?? new DefaultDelayProvider())
    {
        ClientLifecycleManager = clientLifecycleManager ?? new UnimplementedLegacyClientLifecycleManager();
        ClientInformation = DeviceNode.ClientInformation;
    }

    /// <inheritdoc />
    public override LegacyClientInformation ClientInformation { get; }

    /// <inheritdoc />
    protected override IClientLifecycleManager<ILegacyClient, LegacyClientInformation> ClientLifecycleManager { get; }

    /// <inheritdoc />
    protected override IRetryPolicy RetryPolicy => SRetryPolicy;

    /// <inheritdoc />
    protected override DataPointValueConversion<ILegacyDataPointValue> ConvertToDataPointValue(
        ExternalValue externalValue, ILegacyDataPoint dataPoint) => dataPoint.ConvertValue(externalValue.Value);
}
