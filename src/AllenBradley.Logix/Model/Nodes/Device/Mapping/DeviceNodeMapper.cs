using FluentValidation.Results;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device.Mapping;

/// <summary>
/// Turns the engine's <see cref="LogixCommunication"/> into the tree's root: the
/// <see cref="LogixClientInformation"/> everything below the client seam is keyed on, and the
/// controller kind the design id stands for.
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
    /// A CompactLogix's virtual backplane fixes the path at slot 0, so its node never asks for one.
    /// </summary>
    private static CipRoutePath ToCipRoutePath(LogixCommunication communication, LogixControllerFamily family) =>
        family is LogixControllerFamily.CompactLogix
            ? CipRoutePath.VirtualBackplane
            : new CipRoutePath(communication.CipRoutePath ?? string.Empty);
}
