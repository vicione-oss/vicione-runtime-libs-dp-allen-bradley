using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.IntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.SIntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.SIntArray.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Arrays.Integers.SIntArray.Mapping;

public sealed class SIntArrayNodeMapperTests
{
    private const int DefaultPollFrequency = 100;

    private const int DeclaredElementCount = 10;

    private const string TagName = "MySIntArrayTag";

    private readonly IDataPointNodeMapper<SIntArrayNode> _mapper = new SIntArrayNodeMapper();

    [Fact]
    public void TheTagNameElementCountAndPollFrequencyAreReadOffTheConfiguredNode()
    {
        // Arrange
        var node = SIntArrayNodeWith(
            CreateTagName(TagName),
            CreatePollFrequency(DefaultPollFrequency),
            CreateElementCount(DeclaredElementCount));

        // Act
        var sIntArrayNode = _mapper.Map(node);

        // Assert
        sIntArrayNode.TagName.Value.Should().Be(TagName);
        sIntArrayNode.PollFrequency.Value.Should().Be(TimeSpan.FromMilliseconds(DefaultPollFrequency));
        sIntArrayNode.ElementCount.Value.Should().Be(DeclaredElementCount);
    }

    [Fact]
    public void AnSIntArrayTagIsOfferedOnEveryGeneration()
    {
        // Arrange
        var node = SIntArrayNodeWith(
            CreateTagName(TagName),
            CreatePollFrequency(DefaultPollFrequency),
            CreateElementCount(DeclaredElementCount));

        // Act
        ILogixTagNode sIntArrayNode = _mapper.Map(node);

        // Assert
        sIntArrayNode.MinimumGeneration.Should().Be(LogixGeneration.Logix5X70);
    }

    [Theory]
    [InlineData(SIntArrayNode.LinkedNodeTypeId, true)]
    [InlineData(IntArrayNode.LinkedNodeTypeId, false)]
    public void ItClaimsAnSIntArrayNodeAndNoOther(string linkedNodeTypeId, bool expectedIsTargetMapper)
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
    public void AnSIntArrayNodeCarryingAllThreePropertiesIsValid()
    {
        // Arrange
        var node = SIntArrayNodeWith(
            CreateTagName(TagName),
            CreatePollFrequency(DefaultPollFrequency),
            CreateElementCount(DeclaredElementCount));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AnSIntArrayNodeWithoutAnElementCountIsRejected()
    {
        // Arrange
        var node = SIntArrayNodeWith(CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(LogixArrayNode.ElementCountPropertyName);
    }

    private static LinkedNode SIntArrayNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(SIntArrayNode.LinkedNodeTypeId, TagName, properties);
}
