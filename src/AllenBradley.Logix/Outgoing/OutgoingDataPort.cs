using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using ViciOne.ManagedEngine.ExternalCommunication;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Pool;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Mapper;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;
using ViciOne.Suite.DataPort.Extensions.Client;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Outgoing;
using ViciOne.Suite.DataPort.Extensions.Outgoing.QueueProcessing;
using ViciOne.Suite.DataPort.Extensions.Outgoing.QueueProcessing.Retry;
using ViciOne.Suite.DataPort.Extensions.Verification;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Outgoing;

/// <summary>
/// The write direction: engine values in, tag writes out. The incoming port's wiring with a queue in
/// place of a polling schedule, down to the shared client pool.
/// </summary>
public sealed class OutgoingDataPort : OutgoingDataPortBase<
    ILogixDataPoint, ILogixDataPointValue, ILogixClient, DeviceNode, LogixCommunication, LogixClientInformation>
{
    // Industrial writes are state, so a controller that is down is waited for rather than written off.
    private static readonly InfiniteRetryPolicy SRetryPolicy = new();

    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public constructor used by engine.")]
    public OutgoingDataPort(LogixCommunication communication, ILoggerFactory loggerFactory)
        : this(communication, loggerFactory, clientLifecycleManager: null, delayProvider: null)
    {
    }

    internal OutgoingDataPort(
        LogixCommunication communication,
        ILoggerFactory loggerFactory,
        IClientLifecycleManager<ILogixClient, LogixClientInformation>? clientLifecycleManager,
        IDelayProvider? delayProvider)
        : base(
            communication,
            loggerFactory,
            TypedLogixNodeMapper.Instance(),
            new LogixDataPointsGroupsMapper(),
            new QueueConfiguration(
                (QueueStrategy)communication.Strategy,
                new QueueSize(communication.MaxPendingMessages)),
            delayProvider ?? new DefaultDelayProvider())
    {
        ClientLifecycleManager = clientLifecycleManager ?? LogixClientPool.GetInstance(loggerFactory);
        ClientInformation = DeviceNode.ClientInformation;
    }

    /// <inheritdoc />
    public override LogixClientInformation ClientInformation { get; }

    /// <inheritdoc />
    protected override IClientLifecycleManager<ILogixClient, LogixClientInformation> ClientLifecycleManager { get; }

    /// <inheritdoc />
    protected override IRetryPolicy RetryPolicy => SRetryPolicy;

    /// <summary>
    /// Worth running on a write-only device too: a tag whose type contradicts the configuration is
    /// otherwise a write that retries forever.
    /// </summary>
    protected override IDataPointConfigurationVerifier<ILogixDataPoint> CreateConfigurationVerifier(
        ILogixClient client) => new LogixConfigurationVerifier(client);

    /// <inheritdoc />
    protected override DataPointValueConversion<ILogixDataPointValue> ConvertToDataPointValue(
        ExternalValue externalValue, ILogixDataPoint dataPoint) => dataPoint.ConvertValue(externalValue.Value);
}
