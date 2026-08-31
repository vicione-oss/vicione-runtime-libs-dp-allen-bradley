using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;

/// <summary>
/// The root of a configured tree: one controller, and the tag containers configured against it. It
/// carries the <see cref="LogixClientInformation"/> the pool keys a connection under, so a port holds
/// its device identity in one place rather than re-deriving it from the communication record.
/// </summary>
/// <remarks>
/// One type for every device node the manifest declares. What differs between them — the controller
/// family, and later the generation — arrives as a value read off the design id, the way S7's single
/// <c>DeviceNode</c> tells its seven node ids apart.
/// </remarks>
/// <param name="OriginalCommunication">The configuration this node was mapped from.</param>
/// <param name="ClientInformation">Which controller to reach, and how long an operation against it may take.</param>
/// <param name="ControllerFamily">The line the controller belongs to, from the design id it was configured under.</param>
public sealed record DeviceNode(
    LogixCommunication OriginalCommunication,
    LogixClientInformation ClientInformation,
    LogixControllerFamily ControllerFamily) : IRootConfigurationNode<LogixCommunication>
{
    /// <summary>The manifest's node id for a ControlLogix 5550/5560/5570 in a 1756 chassis.</summary>
    public const string ControlLogix5x70DesignId = "DeviceControlLogix5x70";

    /// <summary>The manifest's node id for a CompactLogix 1769/5370 on a DIN rail.</summary>
    public const string CompactLogix5x70DesignId = "DeviceCompactLogix5x70";

    /// <summary>
    /// The family the device node type <paramref name="designId"/> stands for, or <c>null</c> for a node
    /// type this addon does not declare.
    /// </summary>
    /// <remarks>
    /// Null rather than a throw, because the two callers want different things from an id they do not
    /// know: <see cref="Mapping.DeviceNodeMapper"/> turns it into a configuration error, and
    /// <see cref="Mapping.LogixCommunicationValidator"/> has no family rule to hold it to. This switch
    /// and the manifest's device nodes are the same set.
    /// </remarks>
    public static LogixControllerFamily? ControllerFamilyOf(string designId) => designId switch
    {
        ControlLogix5x70DesignId => LogixControllerFamily.ControlLogix,
        CompactLogix5x70DesignId => LogixControllerFamily.CompactLogix,
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

    /// <inheritdoc />
    public bool CanBeAdded(IConfigurationNode configurationNode) => configurationNode is ControllerTagsNode;

    /// <summary>Tags hang off a scope container, never off the device itself.</summary>
    public bool CanBeAdded(IDataPointNode dataPointNode) => false;

    /// <inheritdoc />
    public DeviceIdentifier DeviceIdentifier =>
        new($"{ClientInformation.Gateway.Value}/{ClientInformation.Path.Value}");
}
