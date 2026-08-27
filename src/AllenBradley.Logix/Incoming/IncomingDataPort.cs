using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Pool;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Mapper;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;
using ViciOne.Suite.DataPort.Extensions.Client;
using ViciOne.Suite.DataPort.Extensions.Incoming;
using ViciOne.Suite.DataPort.Extensions.Verification;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Incoming;

public sealed class IncomingDataPort : IncomingDataPortBase<
    LogixCommunication, DeviceNode,
    ILogixDataPoint, LogixDataPointGroup, ILogixDataPointValue,
    ILogixClient, LogixClientInformation>
{
    /// <summary>
    /// Public constructor used by the vicione-engine to create an instance of the Allen-Bradley Logix
    /// data port incoming communication.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public constructor used by engine.")]
    public IncomingDataPort(LogixCommunication communication, ILoggerFactory loggerFactory)
        : this(communication, loggerFactory, TimeProvider.System, clientLifecycleManager: null)
    {
    }

    /// <summary>
    /// Internal constructor for testing, allowing injection of a custom lifecycle manager and time
    /// provider.
    /// </summary>
    internal IncomingDataPort(
        LogixCommunication communication,
        ILoggerFactory loggerFactory,
        TimeProvider timeProvider,
        IClientLifecycleManager<ILogixClient, LogixClientInformation>? clientLifecycleManager)
        : base(
            communication,
            loggerFactory,
            TypedLogixNodeMapper.Instance(),
            new LogixDataPointsGroupsMapper(),
            timeProvider)
    {
        ClientLifecycleManager = clientLifecycleManager ?? LogixClientPool.GetInstance(loggerFactory);
        ClientInformation = DeviceNode.ClientInformation;
    }

    public override LogixClientInformation ClientInformation { get; }
    protected override IClientLifecycleManager<ILogixClient, LogixClientInformation> ClientLifecycleManager { get; }

    protected override IDataPointConfigurationVerifier<ILogixDataPoint>
        CreateConfigurationVerifier(ILogixClient client) => new LogixConfigurationVerifier(client);
}
