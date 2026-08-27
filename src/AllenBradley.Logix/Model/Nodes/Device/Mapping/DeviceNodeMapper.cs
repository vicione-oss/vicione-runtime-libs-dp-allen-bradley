using FluentValidation.Results;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping.Properties;
using Path = ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device.Path;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device.Mapping;

/// <summary>
/// Turns the engine's <see cref="LogixCommunication"/> into the tree's root, which means turning four
/// primitives into the <see cref="LogixClientInformation"/> everything below the client seam is keyed
/// on. Validation runs first: a configuration that fails here is never mapped.
/// </summary>
internal sealed class DeviceNodeMapper : IRootConfigurationNodeMapper<DeviceNode, LogixCommunication>
{
    private readonly LogixCommunicationValidator _validator = new();

    /// <inheritdoc />
    public DeviceNode CreateRootNode(LogixCommunication communication) =>
        new(communication, ToClientInformation(communication));

    /// <inheritdoc />
    public ValidationResult Validate(LogixCommunication communication) => _validator.Validate(communication);

    private static LogixClientInformation ToClientInformation(LogixCommunication communication) =>
        new(
            new Gateway(communication.Gateway),
            new Path(communication.Path),
            PropertyValueConverter.ToEnum<LogixControllerType>(
                communication.ControllerType, nameof(LogixControllerType)),
            new OperationTimeout(TimeSpan.FromMilliseconds(communication.OperationTimeout)));
}
