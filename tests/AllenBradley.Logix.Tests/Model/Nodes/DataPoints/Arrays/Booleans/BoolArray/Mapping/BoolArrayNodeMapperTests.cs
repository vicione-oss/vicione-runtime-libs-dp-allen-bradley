using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Booleans.BoolArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Booleans.BoolArray.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Booleans.Bool;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Arrays.Booleans.BoolArray.
    Mapping;

public sealed class BoolArrayNodeMapperTests
{
    private const int DefaultPollFrequency = 100;

    private const uint DeclaredBitCount = 32;

    private const string TagName = "MyBoolArrayTag";

    private readonly IDataPointNodeMapper<BoolArrayNode> _mapper = new BoolArrayNodeMapper();

    [Fact]
    public void TheTagNameElementCountAndPollFrequencyAreReadOffTheConfiguredNode()
    {
        // Arrange
        var node = BoolArrayNodeWith(
            CreateTagName(TagName),
            CreatePollFrequency(DefaultPollFrequency),
            CreateElementCount(DeclaredBitCount));

        // Act
        var boolArrayNode = _mapper.Map(node);

        // Assert
        boolArrayNode.TagName.Value.Should().Be(TagName);
        boolArrayNode.PollFrequency.Value.Should().Be(TimeSpan.FromMilliseconds(DefaultPollFrequency));
        boolArrayNode.ElementCount.Value.Should().Be(DeclaredBitCount);
    }

    [Fact]
    public void ABoolArrayTagIsOfferedOnEveryGeneration()
    {
        // Arrange
        var node = BoolArrayNodeWith(
            CreateTagName(TagName),
            CreatePollFrequency(DefaultPollFrequency),
            CreateElementCount(DeclaredBitCount));

        // Act
        ILogixTagNode boolArrayNode = _mapper.Map(node);

        // Assert
        boolArrayNode.MinimumGeneration.Should().Be(LogixGeneration.Logix5X70);
    }

    [Theory]
    [InlineData(BoolArrayNode.LinkedNodeTypeId, true)]
    [InlineData(BoolNode.LinkedNodeTypeId, false)]
    public void ItClaimsABoolArrayNodeAndNoOther(string linkedNodeTypeId, bool expectedIsTargetMapper)
    {
        // Arrange
        var node = CreateLinkedNode(
            linkedNodeTypeId,
            TagName,
            CreateTagName(TagName),
            CreatePollFrequency(DefaultPollFrequency),
            CreateElementCount(DeclaredBitCount));

        // Act
        var isTargetMapper = _mapper.IsTargetMapperFor(node);

        // Assert
        isTargetMapper.Should().Be(expectedIsTargetMapper);
    }

    [Fact]
    public void ABoolArrayNodeCarryingAllThreePropertiesIsValid()
    {
        // Arrange
        var node = BoolArrayNodeWith(
            CreateTagName(TagName),
            CreatePollFrequency(DefaultPollFrequency),
            CreateElementCount(DeclaredBitCount));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ABoolArrayNodeWhoseCountDoesNotFillWholeWordsIsRejected()
    {
        // Arrange
        var node = BoolArrayNodeWith(
            CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency), CreateElementCount(10u));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(LogixArrayNode.ElementCountPropertyName);
    }

    private static LinkedNode BoolArrayNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(BoolArrayNode.LinkedNodeTypeId, TagName, properties);
}
