using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ControllerTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ControllerTags.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Containers.Scope.ControllerTags.Mapping;

public sealed class ControllerTagsNodeMapperTests
{
    private static readonly LinkedNode X70ControllerNode =
        CreateLinkedNode(ControllerTagsNode.Logix5X70LinkedNodeTypeId, "Controller");

    private static readonly LinkedNode X80ControllerNode =
        CreateLinkedNode(ControllerTagsNode.Logix5X80LinkedNodeTypeId, "Controller");

    private readonly IBranchConfigurationNodeMapper<ControllerTagsNode> _x70NodeMapper =
        new ControllerTags5X70NodeMapper();

    private readonly IBranchConfigurationNodeMapper<ControllerTagsNode> _x80NodeMapper =
        new ControllerTags5X80NodeMapper();

    [Fact]
    public void AContainerMappedByThe5X70MapperIsStampedWithLogix5X70()
    {
        // Arrange

        // Act
        var controllerTagsNode = _x70NodeMapper.Map(X70ControllerNode);

        // Assert
        controllerTagsNode.Generation.Should().Be(LogixGeneration.Logix5X70);
    }

    [Fact]
    public void AContainerMappedByThe5X80MapperIsStampedWithLogix5X80()
    {
        // Arrange

        // Act
        var controllerTagsNode = _x80NodeMapper.Map(X80ControllerNode);

        // Assert
        controllerTagsNode.Generation.Should().Be(LogixGeneration.Logix5X80);
    }

    [Theory]
    [InlineData(ControllerTagsNode.Logix5X70LinkedNodeTypeId, true)]
    [InlineData(ControllerTagsNode.Logix5X80LinkedNodeTypeId, false)]
    public void The5X70MapperClaimsA5X70ContainerAndNoOther(string linkedNodeTypeId, bool expectedIsTargetMapper)
    {
        // Arrange
        var node = CreateLinkedNode(linkedNodeTypeId, "Controller");

        // Act
        var isTargetMapper = _x70NodeMapper.IsTargetMapperFor(node);

        // Assert
        isTargetMapper.Should().Be(expectedIsTargetMapper);
    }

    [Theory]
    [InlineData(ControllerTagsNode.Logix5X80LinkedNodeTypeId, true)]
    [InlineData(ControllerTagsNode.Logix5X70LinkedNodeTypeId, false)]
    public void The5X80MapperClaimsA5X80ContainerAndNoOther(string linkedNodeTypeId, bool expectedIsTargetMapper)
    {
        // Arrange
        var node = CreateLinkedNode(linkedNodeTypeId, "Controller");

        // Act
        var isTargetMapper = _x80NodeMapper.IsTargetMapperFor(node);

        // Assert
        isTargetMapper.Should().Be(expectedIsTargetMapper);
    }

    [Fact]
    public void AMappedContainerKeepsTheNodeItWasMadeFrom()
    {
        // Arrange

        // Act
        var controllerTagsNode = _x70NodeMapper.Map(X70ControllerNode);

        // Assert
        controllerTagsNode.OriginalNode.Should().BeSameAs(X70ControllerNode);
    }

    [Fact]
    public void AControllerTagsNodeIsValidSinceItDeclaresNoProperties()
    {
        // Arrange

        // Act
        var validation = _x70NodeMapper.Validate(X70ControllerNode);

        // Assert
        validation.IsValid.Should().BeTrue();
    }
}
