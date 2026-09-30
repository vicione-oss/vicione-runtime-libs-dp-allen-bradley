using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.DInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Timers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Timers.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Scalars.Timers.Mapping;

public sealed class TimerNodeMapperTests
{
    private const int DefaultPollFrequency = 100;
    private const string TagName = "StartDelay";

    private readonly IDataPointNodeMapper<TimerNode> _mapper = new TimerNodeMapper();

    [Fact]
    public void TheTagNameAndPollFrequencyAreReadOffTheConfiguredNode()
    {
        // Arrange
        var node = TimerNodeWith(CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var timerNode = _mapper.Map(node);

        // Assert
        timerNode.TagName.Value.Should().Be(TagName);
        timerNode.PollFrequency.Value.Should().Be(TimeSpan.FromMilliseconds(DefaultPollFrequency));
    }

    [Fact]
    public void ATimerIsOfferedOnEveryGeneration()
    {
        // Arrange
        var node = TimerNodeWith(CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        ILogixDataPointNode timerNode = _mapper.Map(node);

        // Assert
        timerNode.MinimumGeneration.Should().Be(LogixGeneration.Logix5X70);
    }

    [Theory]
    [InlineData(TimerNode.LinkedNodeTypeId, true)]
    [InlineData(DIntNode.LinkedNodeTypeId, false)]
    public void ItClaimsATimerNodeAndNoOther(string linkedNodeTypeId, bool expectedIsTargetMapper)
    {
        // Arrange
        var node = CreateLinkedNode(
            linkedNodeTypeId,
            TagName,
            CreateTagName(TagName),
            CreatePollFrequency(DefaultPollFrequency));

        // Act
        var isTargetMapper = _mapper.IsTargetMapperFor(node);

        // Assert
        isTargetMapper.Should().Be(expectedIsTargetMapper);
    }

    [Fact]
    public void ATimerNodeCarryingBothPropertiesIsValid()
    {
        // Arrange
        var node = TimerNodeWith(CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ATimerNodeNamingAMemberIsRejected()
    {
        // Arrange
        var node = TimerNodeWith(CreateTagName($"{TagName}.ACC"), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixDataPointNode.TagNamePropertyName);
    }

    [Fact]
    public void ATimerNodeWithoutATagNameIsRejected()
    {
        // Arrange
        var node = TimerNodeWith(CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixDataPointNode.TagNamePropertyName);
    }

    private static LinkedNode TimerNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(TimerNode.LinkedNodeTypeId, TagName, properties);
}
