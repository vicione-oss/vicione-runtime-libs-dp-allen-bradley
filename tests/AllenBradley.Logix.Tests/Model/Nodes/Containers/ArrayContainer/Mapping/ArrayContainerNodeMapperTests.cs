using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ArrayContainer;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ArrayContainer.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Containers.ArrayContainer.Mapping;

public sealed class ArrayContainerNodeMapperTests
{
    private const string TagName = "myTag";

    private readonly IBranchConfigurationNodeMapper<ArrayContainerNode> _mapper = ArrayContainerNodeMapper.Int();

    /// <summary>Each container type id with the element type the one factory that claims it declares.</summary>
    public static TheoryData<string, AllenBradleyDataType> NodeIdsAndTheirElementTypes =>
        new()
        {
            { ArrayContainerNode.SIntLinkedNodeTypeId, AllenBradleyDataType.Sint },
            { ArrayContainerNode.IntLinkedNodeTypeId, AllenBradleyDataType.Int },
            { ArrayContainerNode.DIntLinkedNodeTypeId, AllenBradleyDataType.Dint },
            { ArrayContainerNode.LIntLinkedNodeTypeId, AllenBradleyDataType.Lint },
            { ArrayContainerNode.USIntLinkedNodeTypeId, AllenBradleyDataType.Usint },
            { ArrayContainerNode.UIntLinkedNodeTypeId, AllenBradleyDataType.Uint },
            { ArrayContainerNode.UDIntLinkedNodeTypeId, AllenBradleyDataType.Udint },
            { ArrayContainerNode.ULIntLinkedNodeTypeId, AllenBradleyDataType.Ulint },
            { ArrayContainerNode.RealLinkedNodeTypeId, AllenBradleyDataType.Real },
            { ArrayContainerNode.LRealLinkedNodeTypeId, AllenBradleyDataType.Lreal },
            { ArrayContainerNode.TimerLinkedNodeTypeId, AllenBradleyDataType.Timer },
        };

    [Theory]
    [MemberData(nameof(NodeIdsAndTheirElementTypes))]
    public void NodeIdsAndDataTypesAreConfiguredCorrectly(string linkedNodeTypeId, AllenBradleyDataType expectedElementDataType)
    {
        // Arrange
        var linkedNode = CreateLinkedNode(linkedNodeTypeId, TagName, CreateTagName(TagName));
        IEnumerable<IBranchConfigurationNodeMapper<ArrayContainerNode>> all = ArrayContainerNodeMapper.All();
        var mapper = all.Single(candidate => candidate.IsTargetMapperFor(linkedNode));

        // Act
        var arrayContainerNode = mapper.Map(linkedNode);

        // Assert
        arrayContainerNode.ArrayDataType.Should().Be(expectedElementDataType);
    }

    [Fact]
    public void OriginalNodeTagNameAndDataTypeAreMapped()
    {
        // Arrange
        var linkedNode = CreateLinkedNode(ArrayContainerNode.IntLinkedNodeTypeId, TagName, CreateTagName(TagName));

        // Act
        var arrayContainerNode = _mapper.Map(linkedNode);

        // Assert
        var expected = new ArrayContainerNode(linkedNode, new TagName(TagName), AllenBradleyDataType.Int);
        arrayContainerNode.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void LinkedNodeWithValidTagNameIsAccepted()
    {
        // Arrange
        var linkedNode = CreateLinkedNode(ArrayContainerNode.IntLinkedNodeTypeId, TagName, CreateTagName(TagName));

        // Act
        var validation = _mapper.Validate(linkedNode);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ALinkedNodeWithoutATagNameIsRefused()
    {
        // Arrange
        var node = CreateLinkedNode(ArrayContainerNode.IntLinkedNodeTypeId, TagName);

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixDataPointNode.TagNamePropertyName);
    }
}
