using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.FloatingPoints.LRealArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.FloatingPoints.LRealArray.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.FloatingPoints.RealArray;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Arrays.FloatingPoints.LRealArray.Mapping;

public sealed class LRealArrayNodeMapperTests
{
    private const int DefaultPollFrequency = 100;

    private const int DeclaredElementCount = 10;

    private const string TagName = "MyLRealArrayTag";

    private readonly IDataPointNodeMapper<LRealArrayNode> _mapper = new LRealArrayNodeMapper();

    [Fact]
    public void TheTagNameElementCountAndPollFrequencyAreReadOffTheConfiguredNode()
    {
        // Arrange
        var node = LRealArrayNodeWith(
            CreateTagName(TagName),
            CreatePollFrequency(DefaultPollFrequency),
            CreateElementCount(DeclaredElementCount));

        // Act
        var lRealArrayNode = _mapper.Map(node);

        // Assert
        lRealArrayNode.TagName.Value.Should().Be(TagName);
        lRealArrayNode.PollFrequency.Value.Should().Be(TimeSpan.FromMilliseconds(DefaultPollFrequency));
        lRealArrayNode.ElementCount.Value.Should().Be(DeclaredElementCount);
    }

    [Fact]
    public void AnLRealArrayTagIsOfferedOnA5X80Only()
    {
        // Arrange
        var node = LRealArrayNodeWith(
            CreateTagName(TagName),
            CreatePollFrequency(DefaultPollFrequency),
            CreateElementCount(DeclaredElementCount));

        // Act
        ILogixTagNode lRealArrayNode = _mapper.Map(node);

        // Assert
        lRealArrayNode.MinimumGeneration.Should().Be(LogixGeneration.Logix5X80);
    }

    [Theory]
    [InlineData(LRealArrayNode.LinkedNodeTypeId, true)]
    [InlineData(RealArrayNode.LinkedNodeTypeId, false)]
    public void ItClaimsAnLRealArrayNodeAndNoOther(string linkedNodeTypeId, bool expectedIsTargetMapper)
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
    public void AnLRealArrayNodeCarryingAllThreePropertiesIsValid()
    {
        // Arrange
        var node = LRealArrayNodeWith(
            CreateTagName(TagName),
            CreatePollFrequency(DefaultPollFrequency),
            CreateElementCount(DeclaredElementCount));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AnLRealArrayNodeWithoutAnElementCountIsRejected()
    {
        // Arrange
        var node = LRealArrayNodeWith(CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(LogixArrayNode.ElementCountPropertyName);
    }

    private static LinkedNode LRealArrayNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(LRealArrayNode.LinkedNodeTypeId, TagName, properties);
}
