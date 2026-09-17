using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.UdtContainer;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.UdtContainer.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Containers.UdtContainer.Mapping;

public sealed class UdtContainerNodeMapperTests
{
    private const string TagName = "Motor";

    private readonly IBranchConfigurationNodeMapper<UdtContainerNode> _mapper = new Udt5X70NodeMapper();

    /// <summary>Each container node type with the generation its mapper stamps on the node.</summary>
    public static TheoryData<string, LogixGeneration> NodeIdsAndTheirGenerations =>
        new()
        {
            { UdtContainerNode.Logix5X70LinkedNodeTypeId, LogixGeneration.Logix5X70 },
            { UdtContainerNode.Logix5X80LinkedNodeTypeId, LogixGeneration.Logix5X80 },
        };

    [Theory]
    [MemberData(nameof(NodeIdsAndTheirGenerations))]
    public void EachContainerNodeTypeMapsToItsGeneration(string linkedNodeTypeId, LogixGeneration expectedGeneration)
    {
        // Arrange
        var linkedNode = CreateLinkedNode(linkedNodeTypeId, TagName, CreateTagName(TagName));
        IBranchConfigurationNodeMapper<UdtContainerNode>[] all =
            [new Udt5X70NodeMapper(), new Udt5X80NodeMapper()];
        var mapper = all.Single(candidate => candidate.IsTargetMapperFor(linkedNode));

        // Act
        var udt = mapper.Map(linkedNode);

        // Assert
        udt.Generation.Should().Be(expectedGeneration);
    }

    [Fact]
    public void OriginalNodeTagNameAndGenerationAreMapped()
    {
        // Arrange
        var linkedNode = CreateLinkedNode(UdtContainerNode.Logix5X70LinkedNodeTypeId, TagName, CreateTagName(TagName));

        // Act
        var udt = _mapper.Map(linkedNode);

        // Assert
        var expected = new UdtContainerNode(linkedNode, new TagName(TagName), LogixGeneration.Logix5X70);
        udt.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ALinkedNodeWithAValidTagNameIsAccepted()
    {
        // Arrange
        var linkedNode = CreateLinkedNode(UdtContainerNode.Logix5X70LinkedNodeTypeId, TagName, CreateTagName(TagName));

        // Act
        var validation = _mapper.Validate(linkedNode);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ALinkedNodeWithoutATagNameIsRefused()
    {
        // Arrange
        var linkedNode = CreateLinkedNode(UdtContainerNode.Logix5X70LinkedNodeTypeId, TagName);

        // Act
        var validation = _mapper.Validate(linkedNode);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixDataPointNode.TagNamePropertyName);
    }
}
