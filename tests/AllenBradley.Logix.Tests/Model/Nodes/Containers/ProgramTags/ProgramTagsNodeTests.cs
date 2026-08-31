using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.FloatingPoints.LReal;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Mapper;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixCommunicationTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Containers.ProgramTags;

/// <summary>
/// A program as the configuration tree holds it: a container hanging off the device beside controller
/// scope, carrying the name its tags will address through.
/// </summary>
/// <remarks>
/// Driven through the whole <see cref="TypedLogixNodeMapper"/> rather than by constructing the node,
/// because what is under test is the engine accepting the container where the manifest offers it — the
/// mapper claiming the node type, and the device admitting what it maps to.
/// </remarks>
public sealed class ProgramTagsNodeTests
{
    private static readonly Guid s_channel = Guid.NewGuid();

    [Fact]
    public void MapToTypedNodes_WithAProgramContainerUnderAController_HangsItOffTheDevice()
    {
        // Arrange
        var communication = CreateCommunicationOf(
            [CreateProgramTagsNode("MainProgram")],
            deviceDesignId: DeviceNode.ControlLogix5x70DesignId);

        // Act
        var deviceNode = TypedLogixNodeMapper.Instance().MapToTypedNodes(communication);

        // Assert
        deviceNode.ConfigurationNodes.Should().ContainSingle()
            .Which.Should().BeOfType<ProgramTagsNode>()
            .Which.ProgramName.Should().Be(new ProgramName("MainProgram"));
    }

    /// <remarks>
    /// Every device node type offers a program container. Which one follows the generation, the way
    /// controller scope's does, because the types a program may hold are the controller's types.
    /// </remarks>
    [Theory]
    [InlineData(DeviceNode.ControlLogix5x70DesignId)]
    [InlineData(DeviceNode.ControlLogix5x80DesignId)]
    [InlineData(DeviceNode.CompactLogix5x70DesignId)]
    [InlineData(DeviceNode.CompactLogix5x80DesignId)]
    public void MapToTypedNodes_WithAProgramContainerUnderAnyDevice_HangsItOffTheDevice(string deviceDesignId)
    {
        // Arrange
        var communication = CreateCommunicationOf(
            [CreateProgramTagsNode(
                "MainProgram", containerDesignId: ProgramTagsDesignIdFor(deviceDesignId))],
            deviceDesignId);

        // Act
        var deviceNode = TypedLogixNodeMapper.Instance().MapToTypedNodes(communication);

        // Assert
        deviceNode.ConfigurationNodes.Should().ContainSingle().Which.Should().BeOfType<ProgramTagsNode>();
    }

    /// <remarks>
    /// A controller has as many programs as it has, and two containers naming the same one are harmless:
    /// they compose the same prefix and address the same tags.
    /// </remarks>
    [Fact]
    public void MapToTypedNodes_WithSeveralProgramContainers_HangsEachOffTheDevice()
    {
        // Arrange
        var communication = CreateCommunicationOf(
        [
            CreateProgramTagsNode("MainProgram"),
            CreateProgramTagsNode("Conveyor"),
        ]);

        // Act
        var deviceNode = TypedLogixNodeMapper.Instance().MapToTypedNodes(communication);

        // Assert
        deviceNode.ConfigurationNodes.OfType<ProgramTagsNode>()
            .Select(static program => program.ProgramName.Value).Should()
            .BeEquivalentTo("MainProgram", "Conveyor");
    }

    /// <remarks>
    /// The type only one generation has is what earns program scope two node types. Everything else about
    /// the two is identical, so this and its 5x70 counterpart are the whole of the difference.
    /// </remarks>
    [Fact]
    public void MapToTypedNodes_WithAnLRealUnderA5x80Program_HangsItOffTheProgram()
    {
        // Arrange
        var communication = CreateCommunicationOf(
            [
                CreateProgramTagsNode(
                    "MainProgram", ProgramTagsId, ProgramTagsNode.Logix5x80LinkedNodeTypeId),
                CreateLRealNode(s_channel.ToString(), "Position", parentId: ProgramTagsId),
            ],
            DeviceNode.ControlLogix5x80DesignId);

        // Act
        var deviceNode = TypedLogixNodeMapper.Instance().MapToTypedNodes(communication);

        // Assert
        deviceNode.ConfigurationNodes.Should().ContainSingle()
            .Which.Should().BeOfType<ProgramTagsNode>()
            .Which.DataPointNodes.Should().ContainSingle()
            .Which.Should().BeOfType<LRealNode>()
            .Which.TagName.Value.Should().Be("Position");
    }

    /// <remarks>
    /// The manifest keeps this off a 5x70's tree, so a configuration holding it did not come from the
    /// editor. It is guarded anyway, because the alternative is a tag address the controller cannot
    /// resolve reaching the poll.
    /// </remarks>
    [Fact]
    public void MapToTypedNodes_WithAnLRealUnderA5x70Program_SaysTheControllerHasNoSuchType()
    {
        // Arrange
        var communication = CreateCommunicationOf(
            [
                CreateProgramTagsNode("MainProgram", ProgramTagsId),
                CreateLRealNode(s_channel.ToString(), "Position", parentId: ProgramTagsId),
            ],
            DeviceNode.ControlLogix5x70DesignId);

        // Act
        var mapping = () => TypedLogixNodeMapper.Instance().MapToTypedNodes(communication);

        // Assert
        mapping.Should().Throw<InvalidConfigurationException>()
            .WithMessage("LREAL is not a data type of a Logix 5x70 controller.");
    }
}
