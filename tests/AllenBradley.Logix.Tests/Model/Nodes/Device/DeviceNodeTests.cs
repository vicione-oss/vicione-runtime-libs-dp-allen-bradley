using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.FloatingPoints.LReal;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Mapper;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixCommunicationTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Device;

/// <summary>
/// The gate a tag container passes through on its way onto the device. The manifest keeps the editor
/// from offering a type the controller has not got; this is the guard behind it, for a configuration
/// that did not come from the editor.
/// </summary>
/// <remarks>
/// Driven through the whole <see cref="TypedLogixNodeMapper"/> rather than by calling <c>CanBeAdded</c>
/// directly, because the gate depends on the order the engine assembles the tree in: a container is
/// offered to the device node only once it holds its data points, and that is the one moment the
/// generation and the tags are in the same place.
/// </remarks>
public sealed class DeviceNodeTests
{
    private static readonly Guid s_channel = Guid.NewGuid();

    [Fact]
    public void MapToTypedNodes_WithAnLRealUnderA5x70Controller_SaysTheControllerHasNoSuchType()
    {
        // Arrange
        var communication = CreateCommunication([CreateLRealNode(s_channel.ToString(), "PrecisionValue")]) with
        {
            DesignId = DeviceNode.CompactLogix5x70DesignId,
        };

        // Act
        var mapping = () => TypedLogixNodeMapper.Instance().MapToTypedNodes(communication);

        // Assert
        mapping.Should().Throw<InvalidConfigurationException>()
            .WithMessage("LREAL is not a data type of a Logix 5x70 controller.");
    }

    [Fact]
    public void MapToTypedNodes_WithAnLRealUnderA5x80Controller_HangsItOffControllerScope()
    {
        // Arrange
        var communication = CreateCommunication([CreateLRealNode(s_channel.ToString(), "PrecisionValue")]) with
        {
            DesignId = DeviceNode.CompactLogix5x80DesignId,
        };

        // Act
        var deviceNode = TypedLogixNodeMapper.Instance().MapToTypedNodes(communication);

        // Assert
        deviceNode.ConfigurationNodes.Should().ContainSingle()
            .Which.Should().BeOfType<ControllerTagsNode>()
            .Which.DataPointNodes.Should().ContainSingle()
            .Which.Should().BeOfType<LRealNode>()
            .Which.TagName.Value.Should().Be("PrecisionValue");
    }

    /// <remarks>
    /// A <c>DINT</c> is every controller's type, so the generation must not gate anything but the types
    /// a 5x70 genuinely lacks.
    /// </remarks>
    [Fact]
    public void MapToTypedNodes_WithADIntUnderA5x70Controller_HangsItOffControllerScope()
    {
        // Arrange
        var communication = CreateCommunication([CreateDIntNode(s_channel.ToString(), "Counter")]) with
        {
            DesignId = DeviceNode.CompactLogix5x70DesignId,
        };

        // Act
        var deviceNode = TypedLogixNodeMapper.Instance().MapToTypedNodes(communication);

        // Assert
        deviceNode.ConfigurationNodes.Should().ContainSingle()
            .Which.Should().BeOfType<ControllerTagsNode>()
            .Which.DataPointNodes.Should().ContainSingle();
    }
}
