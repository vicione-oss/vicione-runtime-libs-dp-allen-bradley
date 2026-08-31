using FluentValidation.Results;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping.Properties;
using Path = ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device.Path;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device.Mapping;

/// <summary>
/// Turns the engine's <see cref="LogixCommunication"/> into the tree's root, which means turning four
/// primitives into the <see cref="LogixClientInformation"/> everything below the client seam is keyed
/// on, and the design id into the controller family that node type stands for. Validation runs first: a
/// configuration that fails here is never mapped.
/// </summary>
internal sealed class DeviceNodeMapper : IRootConfigurationNodeMapper<DeviceNode, LogixCommunication>
{
    private readonly LogixCommunicationValidator _validator = new();

    /// <inheritdoc />
    public DeviceNode CreateRootNode(LogixCommunication communication)
    {
        var nodeType = ToNodeType(communication.DesignId);

        return new DeviceNode(
            communication,
            ToClientInformation(communication, nodeType.Family),
            nodeType.Family,
            nodeType.Generation);
    }

    /// <inheritdoc />
    public ValidationResult Validate(LogixCommunication communication) => _validator.Validate(communication);

    /// <summary>
    /// Reads the family and generation off the node type the device was configured under. A design id
    /// the addon does not declare gets no further than here.
    /// </summary>
    private static DeviceNodeType ToNodeType(string designId) =>
        DeviceNode.TypeOf(designId)
        ?? throw new InvalidConfigurationException($"Unknown device design id: '{designId}'.");

    private static LogixClientInformation ToClientInformation(
        LogixCommunication communication, LogixControllerFamily family) =>
        new(
            new Gateway(communication.Gateway),
            ToPath(communication, family),
            new OperationTimeout(TimeSpan.FromMilliseconds(communication.OperationTimeout)));

    /// <summary>
    /// A CompactLogix is reached at slot 0 of its virtual backplane and its node never asks for a path.
    /// A ControlLogix declares one, because its CPU sits wherever the chassis was built to put it.
    /// </summary>
    private static Path ToPath(LogixCommunication communication, LogixControllerFamily family) =>
        family is LogixControllerFamily.CompactLogix
            ? Path.VirtualBackplane
            : new Path(communication.Path ?? string.Empty);
}
