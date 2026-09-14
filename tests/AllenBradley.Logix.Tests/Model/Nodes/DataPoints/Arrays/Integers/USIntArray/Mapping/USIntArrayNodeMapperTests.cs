using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.SIntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.USIntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.USIntArray.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Arrays.Integers.USIntArray.Mapping;

public sealed class USIntArrayNodeMapperTests
{
    private const int DefaultPollFrequency = 100;

    private const uint DeclaredElementCount = 10;

    private const string TagName = "MyUSIntArrayTag";

    private readonly IDataPointNodeMapper<USIntArrayNode> _mapper = new USIntArrayNodeMapper();

    [Fact]
    public void TheTagNameElementCountAndPollFrequencyAreReadOffTheConfiguredNode()
    {
        // Arrange
        var node = USIntArrayNodeWith(
            CreateTagName(TagName),
            CreatePollFrequency(DefaultPollFrequency),
            CreateElementCount(DeclaredElementCount));

        // Act
        var usIntArrayNode = _mapper.Map(node);

        // Assert
        usIntArrayNode.TagName.Value.Should().Be(TagName);
        usIntArrayNode.PollFrequency.Value.Should().Be(TimeSpan.FromMilliseconds(DefaultPollFrequency));
        usIntArrayNode.ElementCount.Value.Should().Be(DeclaredElementCount);
    }

    [Fact]
    public void AUSIntArrayTagIsOfferedOnA5X80Only()
    {
        // Arrange
        var node = USIntArrayNodeWith(
            CreateTagName(TagName),
            CreatePollFrequency(DefaultPollFrequency),
            CreateElementCount(DeclaredElementCount));

        // Act
        ILogixTagNode usIntArrayNode = _mapper.Map(node);

        // Assert
        usIntArrayNode.MinimumGeneration.Should().Be(LogixGeneration.Logix5X80);
    }

    [Theory]
    [InlineData(USIntArrayNode.LinkedNodeTypeId, true)]
    [InlineData(SIntArrayNode.LinkedNodeTypeId, false)]
    public void ItClaimsAUSIntArrayNodeAndNoOther(string linkedNodeTypeId, bool expectedIsTargetMapper)
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
    public void AUSIntArrayNodeCarryingAllThreePropertiesIsValid()
    {
        // Arrange
        var node = USIntArrayNodeWith(
            CreateTagName(TagName),
            CreatePollFrequency(DefaultPollFrequency),
            CreateElementCount(DeclaredElementCount));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AUSIntArrayNodeWithoutAnElementCountIsRejected()
    {
        // Arrange
        var node = USIntArrayNodeWith(CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(LogixArrayNode.ElementCountPropertyName);
    }

    private static LinkedNode USIntArrayNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(USIntArrayNode.LinkedNodeTypeId, TagName, properties);
}
