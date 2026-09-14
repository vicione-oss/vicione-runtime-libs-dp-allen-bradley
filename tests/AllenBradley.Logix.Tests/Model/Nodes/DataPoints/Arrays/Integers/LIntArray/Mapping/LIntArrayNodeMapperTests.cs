using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.IntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.LIntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.LIntArray.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Arrays.Integers.LIntArray.Mapping;

public sealed class LIntArrayNodeMapperTests
{
    private const int DefaultPollFrequency = 100;

    private const uint DeclaredElementCount = 10;

    private const string TagName = "MyLIntArrayTag";

    private readonly IDataPointNodeMapper<LIntArrayNode> _mapper = new LIntArrayNodeMapper();

    [Fact]
    public void TheTagNameElementCountAndPollFrequencyAreReadOffTheConfiguredNode()
    {
        // Arrange
        var node = LIntArrayNodeWith(
            CreateTagName(TagName),
            CreatePollFrequency(DefaultPollFrequency),
            CreateElementCount(DeclaredElementCount));

        // Act
        var lIntArrayNode = _mapper.Map(node);

        // Assert
        lIntArrayNode.TagName.Value.Should().Be(TagName);
        lIntArrayNode.PollFrequency.Value.Should().Be(TimeSpan.FromMilliseconds(DefaultPollFrequency));
        lIntArrayNode.ElementCount.Value.Should().Be(DeclaredElementCount);
    }

    [Fact]
    public void AnLIntArrayTagIsOfferedOnEveryGeneration()
    {
        // Arrange
        var node = LIntArrayNodeWith(
            CreateTagName(TagName),
            CreatePollFrequency(DefaultPollFrequency),
            CreateElementCount(DeclaredElementCount));

        // Act
        ILogixTagNode lIntArrayNode = _mapper.Map(node);

        // Assert
        lIntArrayNode.MinimumGeneration.Should().Be(LogixGeneration.Logix5X70);
    }

    [Theory]
    [InlineData(LIntArrayNode.LinkedNodeTypeId, true)]
    [InlineData(IntArrayNode.LinkedNodeTypeId, false)]
    public void ItClaimsAnLIntArrayNodeAndNoOther(string linkedNodeTypeId, bool expectedIsTargetMapper)
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
    public void AnLIntArrayNodeCarryingAllThreePropertiesIsValid()
    {
        // Arrange
        var node = LIntArrayNodeWith(
            CreateTagName(TagName),
            CreatePollFrequency(DefaultPollFrequency),
            CreateElementCount(DeclaredElementCount));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AnLIntArrayNodeWithoutAnElementCountIsRejected()
    {
        // Arrange
        var node = LIntArrayNodeWith(CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(LogixArrayNode.ElementCountPropertyName);
    }

    private static LinkedNode LIntArrayNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(LIntArrayNode.LinkedNodeTypeId, TagName, properties);
}
