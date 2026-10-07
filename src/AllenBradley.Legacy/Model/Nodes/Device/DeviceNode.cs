using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.Device;

/// <summary>
/// The root of a configured tree: one controller, and the data files configured against it.
/// </summary>
/// <param name="OriginalCommunication">The configuration this node was mapped from.</param>
/// <param name="ClientInformation">Which controller to reach, and how long an operation against it may take.</param>
public sealed record DeviceNode(
    LegacyCommunication OriginalCommunication,
    LegacyClientInformation ClientInformation) : IRootConfigurationNode<LegacyCommunication>
{
    /// <summary>The manifest's node id for an SLC 500.</summary>
    internal const string Slc500DesignId = "DeviceSlc500";

    /// <summary>The manifest's node id for a MicroLogix.</summary>
    internal const string MicroLogixDesignId = "DeviceMicroLogix";

    /// <summary>
    /// The family the device node type <paramref name="designId"/> names, or <c>null</c> for a node type
    /// this dataport does not declare.
    /// </summary>
    internal static LegacyControllerFamily? FamilyOf(string designId) => designId switch
    {
        Slc500DesignId => LegacyControllerFamily.Slc500,
        MicroLogixDesignId => LegacyControllerFamily.MicroLogix,
        _ => null,
    };

    /// <inheritdoc />
    public IConfigurationNode? ParentConfigurationNode
    {
        get => null;
        set => throw new InvalidConfigurationException("A DeviceNode cannot have a parent.");
    }

    /// <inheritdoc />
    public List<IConfigurationNode> ConfigurationNodes { get; } = [];

    /// <inheritdoc />
    public List<IDataPointNode> DataPointNodes { get; } = [];

    /// <summary>A data file, whatever its type: every container is one, and files sit directly under the device.</summary>
    public bool CanBeAdded(IConfigurationNode configurationNode) => configurationNode is ILegacyContainerNode;

    /// <summary>Elements belong to a data file, never to the device itself.</summary>
    public bool CanBeAdded(IDataPointNode dataPointNode) => false;

    /// <inheritdoc />
    public DeviceIdentifier DeviceIdentifier =>
        new($"{ClientInformation.ConnectionEndpoint.Value}:{ClientInformation.TcpPort.Value}"
            + $"/{ClientInformation.CipRoutePath?.Value}");
}
