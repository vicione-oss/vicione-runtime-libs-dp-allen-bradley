using FluentValidation.Results;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

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
        var controllerKind = ToControllerKind(communication.DesignId);

        return new DeviceNode(
            communication,
            ToClientInformation(communication, controllerKind.Family),
            controllerKind);
    }

    /// <inheritdoc />
    public ValidationResult Validate(LogixCommunication communication) => _validator.Validate(communication);

    /// <summary>
    /// Reads the family and generation off the node the device was configured under. A design id the
    /// addon does not declare gets no further than here.
    /// </summary>
    private static LogixControllerKind ToControllerKind(string designId) =>
        DeviceNode.KindOf(designId)
        ?? throw new InvalidConfigurationException($"Unknown device design id: '{designId}'.");

    private static LogixClientInformation ToClientInformation(
        LogixCommunication communication, LogixControllerFamily family) =>
        new(
            new ConnectionEndpoint(communication.ConnectionEndpoint),
            new TcpPort(communication.TcpPort),
            ToCipRoutePath(communication, family),
            new OperationTimeout(TimeSpan.FromMilliseconds(communication.OperationTimeout)));

    /// <summary>
    /// A CompactLogix is reached at slot 0 of its virtual backplane and its node never asks for a route
    /// path. A ControlLogix declares one, because its CPU sits wherever the chassis was built to put it.
    /// </summary>
    private static CipRoutePath ToCipRoutePath(LogixCommunication communication, LogixControllerFamily family) =>
        family is LogixControllerFamily.CompactLogix
            ? CipRoutePath.VirtualBackplane
            : new CipRoutePath(communication.CipRoutePath ?? string.Empty);
}
