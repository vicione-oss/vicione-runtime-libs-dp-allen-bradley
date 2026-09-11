using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.LIntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.ULIntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.ULIntArray.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Arrays.Integers.ULIntArray.Mapping;

public sealed class ULIntArrayNodeMapperTests
{
    private const int DefaultPollFrequency = 100;

    private const int DeclaredElementCount = 10;

    private const string TagName = "MyULIntArrayTag";

    private readonly IDataPointNodeMapper<ULIntArrayNode> _mapper = new ULIntArrayNodeMapper();

    [Fact]
    public void TheTagNameElementCountAndPollFrequencyAreReadOffTheConfiguredNode()
    {
        // Arrange
        var node = ULIntArrayNodeWith(
            CreateTagName(TagName),
            CreatePollFrequency(DefaultPollFrequency),
            CreateElementCount(DeclaredElementCount));

        // Act
        var uLIntArrayNode = _mapper.Map(node);

        // Assert
        uLIntArrayNode.TagName.Value.Should().Be(TagName);
        uLIntArrayNode.PollFrequency.Value.Should().Be(TimeSpan.FromMilliseconds(DefaultPollFrequency));
        uLIntArrayNode.ElementCount.Value.Should().Be(DeclaredElementCount);
    }

    [Fact]
    public void AULIntArrayTagIsOfferedOnA5X80Only()
    {
        // Arrange
        var node = ULIntArrayNodeWith(
            CreateTagName(TagName),
            CreatePollFrequency(DefaultPollFrequency),
            CreateElementCount(DeclaredElementCount));

        // Act
        ILogixTagNode uLIntArrayNode = _mapper.Map(node);

        // Assert
        uLIntArrayNode.MinimumGeneration.Should().Be(LogixGeneration.Logix5X80);
    }

    [Theory]
    [InlineData(ULIntArrayNode.LinkedNodeTypeId, true)]
    [InlineData(LIntArrayNode.LinkedNodeTypeId, false)]
    public void ItClaimsAULIntArrayNodeAndNoOther(string linkedNodeTypeId, bool expectedIsTargetMapper)
    {
        // Arrange
        var node = CreateLinkedNode(
            linkedNodeTypeId,
            TagName,
            CreateTagName(TagName),
            CreatePollFrequency(DefaultPollFrequency),
            CreateElementCount(DeclaredElementCount));

        // Act
        var isTargetMapper = _mapper.IsTargetMapperFor(node);

        // Assert
        isTargetMapper.Should().Be(expectedIsTargetMapper);
    }

    [Fact]
    public void AULIntArrayNodeCarryingAllThreePropertiesIsValid()
    {
        // Arrange
        var node = ULIntArrayNodeWith(
            CreateTagName(TagName),
            CreatePollFrequency(DefaultPollFrequency),
            CreateElementCount(DeclaredElementCount));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AULIntArrayNodeWithoutAnElementCountIsRejected()
    {
        // Arrange
        var node = ULIntArrayNodeWith(CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(LogixArrayNode.ElementCountPropertyName);
    }

    private static LinkedNode ULIntArrayNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(ULIntArrayNode.LinkedNodeTypeId, TagName, properties);
}
