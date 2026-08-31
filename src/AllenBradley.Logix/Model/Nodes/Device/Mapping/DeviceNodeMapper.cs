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
        var family = ToControllerFamily(communication.DesignId);

        return new DeviceNode(communication, ToClientInformation(communication, family), family);
    }

    /// <inheritdoc />
    public ValidationResult Validate(LogixCommunication communication) => _validator.Validate(communication);

    /// <summary>
    /// Reads the family off the node type the device was configured under. The manifest and this switch
    /// are the same set: a design id with no arm here is a node the addon does not have.
    /// </summary>
    private static LogixControllerFamily ToControllerFamily(string designId) => designId switch
    {
        DeviceNode.ControlLogix5x70DesignId => LogixControllerFamily.ControlLogix,
        DeviceNode.CompactLogix5x70DesignId => LogixControllerFamily.CompactLogix,
        _ => throw new InvalidConfigurationException($"Unknown device design id: '{designId}'."),
    };

    private static LogixClientInformation ToClientInformation(
        LogixCommunication communication, LogixControllerFamily family) =>
        new(
            new Gateway(communication.Gateway),
            ToPath(communication, family),
            PropertyValueConverter.ToEnum<LogixControllerType>(
                communication.ControllerType, nameof(LogixControllerType)),
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
