using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;

/// <summary>
/// The root of a configured tree: one controller, and the tag containers configured against it. It
/// carries the <see cref="LogixClientInformation"/> the pool keys a connection under, so a port holds
/// its device identity in one place rather than re-deriving it from the communication record.
/// </summary>
/// <param name="OriginalCommunication">The configuration this node was mapped from.</param>
/// <param name="ClientInformation">Which controller to reach, and how long an operation against it may take.</param>
/// <param name="ControllerKind">
/// The family and generation of the controller, from the design id it was configured under.
/// </param>
public sealed record DeviceNode(
    LogixCommunication OriginalCommunication,
    LogixClientInformation ClientInformation,
    LogixControllerKind ControllerKind) : IRootConfigurationNode<LogixCommunication>
{
    /// <summary>The manifest's node id for a ControlLogix 5550/5560/5570 in a 1756 chassis.</summary>
    public const string ControlLogix5X70DesignId = "DeviceControlLogix5X70";

    /// <summary>The manifest's node id for a ControlLogix 5580 in a 1756 chassis.</summary>
    public const string ControlLogix5X80DesignId = "DeviceControlLogix5X80";

    /// <summary>The manifest's node id for a CompactLogix 1769/5370 on a DIN rail.</summary>
    public const string CompactLogix5X70DesignId = "DeviceCompactLogix5X70";

    /// <summary>The manifest's node id for a CompactLogix 5380/5480 on a DIN rail.</summary>
    public const string CompactLogix5X80DesignId = "DeviceCompactLogix5X80";

    /// <summary>
    /// What the device node type <paramref name="designId"/> names stands for, or <c>null</c> for a node
    /// type this addon does not declare.
    /// </summary>
    public static LogixControllerKind? KindOf(string designId) => designId switch
    {
        ControlLogix5X70DesignId => LogixControllerKind.ControlLogix5X70,
        ControlLogix5X80DesignId => LogixControllerKind.ControlLogix5X80,
        CompactLogix5X70DesignId => LogixControllerKind.CompactLogix5X70,
        CompactLogix5X80DesignId => LogixControllerKind.CompactLogix5X80,
        _ => null,
    };

    /// <summary>
    /// The node id a controller of <paramref name="controllerKind"/> is configured under —
    /// <see cref="KindOf"/> read the other way round.
    /// </summary>
    public static string DesignIdFor(LogixControllerKind controllerKind) => controllerKind switch
    {
        (LogixControllerFamily.ControlLogix, LogixGeneration.Logix5X70) => ControlLogix5X70DesignId,
        (LogixControllerFamily.ControlLogix, LogixGeneration.Logix5X80) => ControlLogix5X80DesignId,
        (LogixControllerFamily.CompactLogix, LogixGeneration.Logix5X70) => CompactLogix5X70DesignId,
        (LogixControllerFamily.CompactLogix, LogixGeneration.Logix5X80) => CompactLogix5X80DesignId,
        _ => throw new ArgumentOutOfRangeException(
            nameof(controllerKind),
            $"No device node for a {controllerKind.Generation} {controllerKind.Family}."),
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

    /// <summary>
    /// A scope container of this controller's own generation — controller scope, or a program. The other
    /// container of each pair stands for a type vocabulary this controller has not got, and would carry
    /// that answer down to the tags below it: a <c>ControllerTags5X80</c> under a 5X70 device is what
    /// would let an <c>LREAL</c> past <see cref="ITagScopeNode.CanBeAdded(IDataPointNode)"/>, which
    /// believes the container.
    /// </summary>
    public bool CanBeAdded(IConfigurationNode configurationNode)
    {
        if (configurationNode is not ITagScopeNode tagScopeNode)
        {
            return false;
        }

        if (tagScopeNode.Generation != ControllerKind.Generation)
        {
            throw InvalidChildNodeException(tagScopeNode);
        }

        return true;
    }

    private InvalidConfigurationException InvalidChildNodeException(ITagScopeNode tagScopeNode)
    {
        return new InvalidConfigurationException(
            $"A '{tagScopeNode.OriginalNode.DesignId}' container cannot be added to "
            + $"'{OriginalCommunication.DesignId}' device.");
    }

    /// <summary>Tags hang off a scope container, never off the device itself.</summary>
    public bool CanBeAdded(IDataPointNode dataPointNode) => false;

    /// <inheritdoc />
    public DeviceIdentifier DeviceIdentifier =>
        new($"{ClientInformation.ConnectionEndpoint.Value}:{ClientInformation.TcpPort.Value}"
            + $"/{ClientInformation.CipRoutePath.Value}");
}
