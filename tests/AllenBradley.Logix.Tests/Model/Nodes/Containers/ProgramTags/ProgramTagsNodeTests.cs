using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Mapper;
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
    /// Every device node type offers the same program container: a program is a program whatever the
    /// controller is, and the generation split that controller scope carries has nothing yet to gate.
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
            [CreateProgramTagsNode("MainProgram")], deviceDesignId);

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
}
