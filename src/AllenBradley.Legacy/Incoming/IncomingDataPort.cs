using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.Mapper;
using ViciOne.Suite.DataPort.Extensions.Client;
using ViciOne.Suite.DataPort.Extensions.Incoming;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Incoming;

/// <summary>
/// The read direction: polled data-file reads in, engine values out. The port does not verify its
/// configuration against the controller, because there is nothing meaningful to verify.
/// </summary>
public sealed class IncomingDataPort : IncomingDataPortBase<
    LegacyCommunication, DeviceNode,
    ILegacyDataPoint, LegacyDataPointGroup, ILegacyDataPointValue,
    ILegacyClient, LegacyClientInformation>
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public constructor used by engine.")]
    public IncomingDataPort(LegacyCommunication communication, ILoggerFactory loggerFactory)
        : this(communication, loggerFactory, TimeProvider.System, clientLifecycleManager: null)
    {
    }

    internal IncomingDataPort(
        LegacyCommunication communication,
        ILoggerFactory loggerFactory,
        TimeProvider timeProvider,
        IClientLifecycleManager<ILegacyClient, LegacyClientInformation>? clientLifecycleManager)
        : base(
            communication,
            loggerFactory,
            TypedLegacyNodeMapper.Instance(),
            new LegacyDataPointsGroupsMapper(),
            timeProvider)
    {
        ClientLifecycleManager = clientLifecycleManager ?? new UnimplementedLegacyClientLifecycleManager();
        ClientInformation = DeviceNode.ClientInformation;
    }

    public override LegacyClientInformation ClientInformation { get; }
    protected override IClientLifecycleManager<ILegacyClient, LegacyClientInformation> ClientLifecycleManager { get; }
}
