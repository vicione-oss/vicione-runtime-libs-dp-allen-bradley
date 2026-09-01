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
/// <remarks>
/// One type for every device node the manifest declares. What differs between them — the controller
/// family, and later the generation — arrives as a value read off the design id, the way S7's single
/// <c>DeviceNode</c> tells its seven node ids apart.
/// </remarks>
/// <param name="OriginalCommunication">The configuration this node was mapped from.</param>
/// <param name="ClientInformation">Which controller to reach, and how long an operation against it may take.</param>
/// <param name="ControllerFamily">The line the controller belongs to, from the design id it was configured under.</param>
/// <param name="Generation">How far along that line it is, from the same design id.</param>
public sealed record DeviceNode(
    LogixCommunication OriginalCommunication,
    LogixClientInformation ClientInformation,
    LogixControllerFamily ControllerFamily,
    LogixGeneration Generation) : IRootConfigurationNode<LogixCommunication>
{
    /// <summary>The manifest's node id for a ControlLogix 5550/5560/5570 in a 1756 chassis.</summary>
    public const string ControlLogix5x70DesignId = "DeviceControlLogix5x70";

    /// <summary>The manifest's node id for a ControlLogix 5580 in a 1756 chassis.</summary>
    public const string ControlLogix5x80DesignId = "DeviceControlLogix5x80";

    /// <summary>The manifest's node id for a CompactLogix 1769/5370 on a DIN rail.</summary>
    public const string CompactLogix5x70DesignId = "DeviceCompactLogix5x70";

    /// <summary>The manifest's node id for a CompactLogix 5380/5480 on a DIN rail.</summary>
    public const string CompactLogix5x80DesignId = "DeviceCompactLogix5x80";

    /// <summary>
    /// What the device node type <paramref name="designId"/> names stands for, or <c>null</c> for a node
    /// type this addon does not declare.
    /// </summary>
    /// <remarks>
    /// Null rather than a throw, because the two callers want different things from an id they do not
    /// know: <see cref="Mapping.DeviceNodeMapper"/> turns it into a configuration error, and
    /// <see cref="Mapping.LogixCommunicationValidator"/> has no family rule to hold it to. This switch
    /// and the manifest's device nodes are the same set.
    /// </remarks>
    public static DeviceNodeType? TypeOf(string designId) => designId switch
    {
        ControlLogix5x70DesignId =>
            new DeviceNodeType(LogixControllerFamily.ControlLogix, LogixGeneration.Logix5x70),
        ControlLogix5x80DesignId =>
            new DeviceNodeType(LogixControllerFamily.ControlLogix, LogixGeneration.Logix5x80),
        CompactLogix5x70DesignId =>
            new DeviceNodeType(LogixControllerFamily.CompactLogix, LogixGeneration.Logix5x70),
        CompactLogix5x80DesignId =>
            new DeviceNodeType(LogixControllerFamily.CompactLogix, LogixGeneration.Logix5x80),
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

    /// <summary>
    /// A scope container of this controller's own generation — controller scope, or a program. The other
    /// container of each pair stands for a type vocabulary this controller has not got, and would carry
    /// that answer down to the tags below it: a <c>ControllerTags5x80</c> under a 5x70 device is what
    /// would let an <c>LREAL</c> past <see cref="ILogixTagScopeNode.CanBeAdded(IDataPointNode)"/>, which
    /// believes the container.
    /// </summary>
    /// <remarks>
    /// A device node lists exactly one container node type per scope as its child, so the editor cannot
    /// build the mismatch — the same thing that is true of the <c>LREAL</c> itself, and the same reason to
    /// guard it anyway. This is the one place the pairing is visible: a container is mapped before it is
    /// attached, so it cannot check the device, but the device can check it.
    /// </remarks>
    public bool CanBeAdded(IConfigurationNode configurationNode)
    {
        if (configurationNode is not ILogixTagScopeNode scopeNode)
        {
            return false;
        }

        if (scopeNode.Generation != Generation)
        {
            throw new InvalidConfigurationException(
                $"A '{scopeNode.OriginalNode.DesignId}' container cannot hang off a "
                + $"'{OriginalCommunication.DesignId}' device.");
        }

        return true;
    }

    /// <summary>Tags hang off a scope container, never off the device itself.</summary>
    public bool CanBeAdded(IDataPointNode dataPointNode) => false;

    /// <inheritdoc />
    public DeviceIdentifier DeviceIdentifier =>
        new($"{ClientInformation.Gateway.Value}/{ClientInformation.CipRoutePath.Value}");
}
