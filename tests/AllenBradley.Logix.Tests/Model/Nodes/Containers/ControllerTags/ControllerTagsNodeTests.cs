using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.FloatingPoints.LReal;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Mapper;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixCommunicationTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Containers.ControllerTags;

/// <summary>
/// The gate a configured tag passes through on its way into controller scope. The manifest keeps the
/// editor from offering a type the controller has not got; this is the guard behind it, for a
/// configuration that did not come from the editor.
/// </summary>
/// <remarks>
/// Driven through the whole <see cref="TypedLogixNodeMapper"/> rather than by calling <c>CanBeAdded</c>
/// directly, because what is under test includes the container knowing its own generation — which it
/// takes from the node type it was mapped from, and only the engine's dispatch puts it there.
/// </remarks>
public sealed class ControllerTagsNodeTests
{
    private static readonly Guid s_channel = Guid.NewGuid();

    [Fact]
    public void MapToTypedNodes_WithAnLRealUnderA5x70Controller_SaysTheControllerHasNoSuchType()
    {
        // Arrange
        var communication = CreateCommunication(
            [CreateLRealNode(s_channel.ToString(), "PrecisionValue")],
            deviceDesignId: DeviceNode.CompactLogix5x70DesignId);

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
        var communication = CreateCommunication(
            [CreateLRealNode(s_channel.ToString(), "PrecisionValue")],
            deviceDesignId: DeviceNode.CompactLogix5x80DesignId);

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
        var communication = CreateCommunication(
            [CreateDIntNode(s_channel.ToString(), "Counter")],
            deviceDesignId: DeviceNode.CompactLogix5x70DesignId);

        // Act
        var deviceNode = TypedLogixNodeMapper.Instance().MapToTypedNodes(communication);

        // Assert
        deviceNode.ConfigurationNodes.Should().ContainSingle()
            .Which.Should().BeOfType<ControllerTagsNode>()
            .Which.DataPointNodes.Should().ContainSingle();
    }

    /// <remarks>
    /// Both container node types are mapped, and each stamps its own generation on the node it makes.
    /// A device pointing at the wrong one would gate the wrong set of types.
    /// </remarks>
    [Theory]
    [InlineData(DeviceNode.CompactLogix5x70DesignId, ControllerTagsNode.Logix5x70LinkedNodeTypeId)]
    [InlineData(DeviceNode.CompactLogix5x80DesignId, ControllerTagsNode.Logix5x80LinkedNodeTypeId)]
    public void MapToTypedNodes_MapsBothContainerNodeTypes(string deviceDesignId, string containerDesignId)
    {
        // Arrange
        var communication = CreateCommunication(
            [CreateDIntNode(s_channel.ToString(), "Counter")], deviceDesignId: deviceDesignId);

        // Act
        var deviceNode = TypedLogixNodeMapper.Instance().MapToTypedNodes(communication);

        // Assert
        deviceNode.ConfigurationNodes.Should().ContainSingle()
            .Which.Should().BeOfType<ControllerTagsNode>()
            .Which.OriginalNode.DesignId.Should().Be(containerDesignId);
    }
}
