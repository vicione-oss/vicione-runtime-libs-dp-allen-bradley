using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
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
    private const string IntArrayContainerTypeId = "IntArrayContainer";

    private const string DIntArrayContainerTypeId = "DIntArrayContainer";

    private const string TagName = "Readings";

    private static readonly LinkedNode IntArrayNode = IntArrayContainerNodeWith(CreateTagName(TagName));

    private readonly IBranchConfigurationNodeMapper<ArrayContainerNode> _mapper =
        new ArrayContainerNodeMapper(IntArrayContainerTypeId, AllenBradleyDataType.Int);

    [Fact]
    public void AMappedContainerCarriesItsNodeAndWhatTheNodeAndTheMapperDeclare()
    {
        // Arrange

        // Act
        var arrayContainerNode = _mapper.Map(IntArrayNode);

        // Assert
        var expected = new ArrayContainerNode(IntArrayNode, new TagName(TagName), AllenBradleyDataType.Int);
        arrayContainerNode.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(IntArrayContainerTypeId, true)]
    [InlineData(DIntArrayContainerTypeId, false)]
    public void TheMapperClaimsTheContainerTypeItWasBuiltForAndNoOther(
        string linkedNodeTypeId, bool expectedIsTargetMapper)
    {
        // Arrange
        var node = CreateLinkedNode(linkedNodeTypeId, TagName, CreateTagName(TagName));

        // Act
        var isTargetMapper = _mapper.IsTargetMapperFor(node);

        // Assert
        isTargetMapper.Should().Be(expectedIsTargetMapper);
    }

    [Fact]
    public void AContainerNodeCarryingATagNameIsValid()
    {
        // Arrange

        // Act
        var validation = _mapper.Validate(IntArrayNode);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AContainerNodeWithoutATagNameIsRefusedOnTheTagNameProperty()
    {
        // Arrange
        var node = IntArrayContainerNodeWith();

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixDataPointNode.TagNamePropertyName);
    }

    private static LinkedNode IntArrayContainerNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(IntArrayContainerTypeId, TagName, properties);
}
