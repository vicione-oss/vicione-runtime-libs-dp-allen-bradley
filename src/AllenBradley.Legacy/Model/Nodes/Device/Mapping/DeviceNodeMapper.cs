using FluentValidation.Results;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.Device.Mapping;

/// <summary>
/// Turns the engine's <see cref="LegacyCommunication"/> into the tree's root, and the family the design id
/// stands for into the <see cref="LegacyClientInformation"/> a connection is opened with.
/// </summary>
internal sealed class DeviceNodeMapper : IRootConfigurationNodeMapper<DeviceNode, LegacyCommunication>
{
    private readonly LegacyCommunicationValidator _validator = new();

    /// <inheritdoc />
    public DeviceNode CreateRootNode(LegacyCommunication communication) =>
        new(communication, ToClientInformation(communication));

    /// <inheritdoc />
    public ValidationResult Validate(LegacyCommunication communication) => _validator.Validate(communication);

    private static LegacyClientInformation ToClientInformation(LegacyCommunication communication) =>
        new(
            ToFamily(communication.DesignId),
            new ConnectionEndpoint(communication.ConnectionEndpoint),
            new TcpPort(communication.TcpPort),
            string.IsNullOrEmpty(communication.CipRoutePath) ? null : new CipRoutePath(communication.CipRoutePath),
            new OperationTimeout(TimeSpan.FromMilliseconds(communication.OperationTimeout)));

    private static LegacyControllerFamily ToFamily(string designId) =>
        DeviceNode.FamilyOf(designId)
        ?? throw new InvalidConfigurationException($"Unknown device design id: '{designId}'.");
}
