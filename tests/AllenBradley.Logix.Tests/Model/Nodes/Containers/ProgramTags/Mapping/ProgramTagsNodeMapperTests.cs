using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Containers.ProgramTags.Mapping;

public sealed class ProgramTagsNodeMapperTests
{
    private const string MainProgramName = "MainProgram";

    private static readonly LinkedNode X70ProgramNode = CreateLinkedNode(
        ProgramTagsNode.Logix5X70LinkedNodeTypeId, MainProgramName, CreateProgramName(MainProgramName));

    private static readonly LinkedNode X80ProgramNode = CreateLinkedNode(
        ProgramTagsNode.Logix5X80LinkedNodeTypeId, MainProgramName, CreateProgramName(MainProgramName));

    private readonly IBranchConfigurationNodeMapper<ProgramTagsNode> _x70NodeMapper = new ProgramTags5X70NodeMapper();
    private readonly IBranchConfigurationNodeMapper<ProgramTagsNode> _x80NodeMapper = new ProgramTags5X80NodeMapper();

    [Fact]
    public void AProgramMappedByThe5X70MapperHasGenerationLogix5X70()
    {
        // Arrange

        // Act
        var programTagsNode = _x70NodeMapper.Map(X70ProgramNode);

        // Assert
        programTagsNode.Generation.Should().Be(LogixGeneration.Logix5X70);
    }

    [Fact]
    public void AProgramMappedByThe5X80MapperHasGenerationLogix5X80()
    {
        // Arrange

        // Act
        var programTagsNode = _x80NodeMapper.Map(X80ProgramNode);

        // Assert
        programTagsNode.Generation.Should().Be(LogixGeneration.Logix5X80);
    }

    [Theory]
    [InlineData(ProgramTagsNode.Logix5X70LinkedNodeTypeId, true)]
    [InlineData(ProgramTagsNode.Logix5X80LinkedNodeTypeId, false)]
    public void The5X70MapperClaimsA5X70ProgramAndNoOther(string linkedNodeTypeId, bool expectedIsTargetMapper)
    {
        // Arrange
        var node = CreateLinkedNode(linkedNodeTypeId, MainProgramName, CreateProgramName(MainProgramName));

        // Act
        var isTargetMapper = _x70NodeMapper.IsTargetMapperFor(node);

        // Assert
        isTargetMapper.Should().Be(expectedIsTargetMapper);
    }

    [Theory]
    [InlineData(ProgramTagsNode.Logix5X80LinkedNodeTypeId, true)]
    [InlineData(ProgramTagsNode.Logix5X70LinkedNodeTypeId, false)]
    public void The5X80MapperClaimsA5X80ProgramAndNoOther(string linkedNodeTypeId, bool expectedIsTargetMapper)
    {
        // Arrange
        var node = CreateLinkedNode(linkedNodeTypeId, MainProgramName, CreateProgramName(MainProgramName));

        // Act
        var isTargetMapper = _x80NodeMapper.IsTargetMapperFor(node);

        // Assert
        isTargetMapper.Should().Be(expectedIsTargetMapper);
    }

    [Fact]
    public void AMappedProgramCarriesTheProgramNameItsNodeDeclares()
    {
        // Arrange

        // Act
        var programTagsNode = _x70NodeMapper.Map(X70ProgramNode);

        // Assert
        programTagsNode.ProgramName.Should().Be(new ProgramName(MainProgramName));
    }

    [Fact]
    public void AMappedProgramKeepsTheNodeItWasMadeFrom()
    {
        // Arrange

        // Act
        var programTagsNode = _x70NodeMapper.Map(X70ProgramNode);

        // Assert
        programTagsNode.OriginalNode.Should().BeSameAs(X70ProgramNode);
    }
}
