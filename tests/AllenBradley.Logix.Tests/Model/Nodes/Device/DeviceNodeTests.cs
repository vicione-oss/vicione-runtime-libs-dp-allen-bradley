using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Mapper;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixCommunicationTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Device;

/// <summary>
/// The gate a controller-scope container passes through on its way onto the device. A container's node
/// type is what the tags below it are held to, so a container of the wrong generation would hand them
/// the wrong answer — and this is the only place the two are visible at once.
/// </summary>
/// <remarks>
/// Driven through the whole <see cref="TypedLogixNodeMapper"/>, because the engine's dispatch is what
/// attaches a container to a device and so the only thing that calls the guard.
/// </remarks>
public sealed class DeviceNodeTests
{
    private static readonly Guid s_channel = Guid.NewGuid();

    /// <remarks>
    /// The mismatch is the way past <see cref="ControllerTagsNode.CanBeAdded(IDataPointNode)"/>: the
    /// container believes its own node type, so a 5x80 one under a 5x70 device would let an
    /// <c>LREAL</c> onto a controller that has none.
    /// </remarks>
    [Fact]
    public void MapToTypedNodes_WithA5x80ContainerUnderA5x70Device_RefusesThePairing()
    {
        // Arrange
        var communication = CreateCommunication(
            [CreateLRealNode(s_channel.ToString(), "PrecisionValue")],
            deviceDesignId: DeviceNode.CompactLogix5x70DesignId,
            containerDesignId: ControllerTagsNode.Logix5x80LinkedNodeTypeId);

        // Act
        var mapping = () => TypedLogixNodeMapper.Instance().MapToTypedNodes(communication);

        // Assert
        mapping.Should().Throw<InvalidConfigurationException>()
            .WithMessage(
                "A 'ControllerTags5x80' container cannot hang off a 'DeviceCompactLogix5x70' device.");
    }

    /// <remarks>
    /// The other way round is a configuration error too, and a quieter one: a 5x70 container under a
    /// 5x80 device offers fewer types than the controller has rather than more.
    /// </remarks>
    [Fact]
    public void MapToTypedNodes_WithA5x70ContainerUnderA5x80Device_RefusesThePairing()
    {
        // Arrange
        var communication = CreateCommunication(
            [CreateDIntNode(s_channel.ToString(), "Counter")],
            deviceDesignId: DeviceNode.CompactLogix5x80DesignId,
            containerDesignId: ControllerTagsNode.Logix5x70LinkedNodeTypeId);

        // Act
        var mapping = () => TypedLogixNodeMapper.Instance().MapToTypedNodes(communication);

        // Assert
        mapping.Should().Throw<InvalidConfigurationException>()
            .WithMessage(
                "A 'ControllerTags5x70' container cannot hang off a 'DeviceCompactLogix5x80' device.");
    }

    /// <remarks>
    /// The guard must not cost the matching pairings anything — every configuration the editor can
    /// actually produce is one of these.
    /// </remarks>
    [Theory]
    [InlineData(DeviceNode.ControlLogix5x70DesignId)]
    [InlineData(DeviceNode.ControlLogix5x80DesignId)]
    [InlineData(DeviceNode.CompactLogix5x70DesignId)]
    [InlineData(DeviceNode.CompactLogix5x80DesignId)]
    public void MapToTypedNodes_WithTheContainerOfItsOwnGeneration_AttachesIt(string deviceDesignId)
    {
        // Arrange
        var communication = CreateCommunication(
            [CreateDIntNode(s_channel.ToString(), "Counter")], deviceDesignId: deviceDesignId);

        // Act
        var deviceNode = TypedLogixNodeMapper.Instance().MapToTypedNodes(communication);

        // Assert
        deviceNode.ConfigurationNodes.Should().ContainSingle()
            .Which.Should().BeOfType<ControllerTagsNode>()
            .Which.Generation.Should().Be(deviceNode.Generation);
    }
}
