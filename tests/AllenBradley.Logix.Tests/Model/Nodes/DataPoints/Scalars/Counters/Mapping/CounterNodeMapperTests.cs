using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Counters;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Counters.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.DInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Timers;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Scalars.Counters.Mapping;

public sealed class CounterNodeMapperTests
{
    private const int DefaultPollFrequency = 100;
    private const string TagName = "PartCount";

    private readonly IDataPointNodeMapper<CounterNode> _mapper = new CounterNodeMapper();

    [Fact]
    public void TheTagNameAndPollFrequencyAreReadOffTheConfiguredNode()
    {
        // Arrange
        var node = CounterNodeWith(CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var counterNode = _mapper.Map(node);

        // Assert
        counterNode.TagName.Value.Should().Be(TagName);
        counterNode.PollFrequency.Value.Should().Be(TimeSpan.FromMilliseconds(DefaultPollFrequency));
    }

    [Fact]
    public void ACounterIsOfferedOnEveryGeneration()
    {
        // Arrange
        var node = CounterNodeWith(CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        ILogixDataPointNode counterNode = _mapper.Map(node);

        // Assert
        counterNode.MinimumGeneration.Should().Be(LogixGeneration.Logix5X70);
    }

    [Theory]
    [InlineData(CounterNode.LinkedNodeTypeId, true)]
    [InlineData(DIntNode.LinkedNodeTypeId, false)]
    [InlineData(TimerNode.LinkedNodeTypeId, false)]
    public void ItClaimsACounterNodeAndNoOther(string linkedNodeTypeId, bool expectedIsTargetMapper)
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
    public void ACounterNodeCarryingBothPropertiesIsValid()
    {
        // Arrange
        var node = CounterNodeWith(CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ACounterNodeNamingAMemberIsRejected()
    {
        // Arrange
        var node = CounterNodeWith(CreateTagName($"{TagName}.ACC"), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixDataPointNode.TagNamePropertyName);
    }

    [Fact]
    public void ACounterNodeWithoutATagNameIsRejected()
    {
        // Arrange
        var node = CounterNodeWith(CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixDataPointNode.TagNamePropertyName);
    }

    private static LinkedNode CounterNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(CounterNode.LinkedNodeTypeId, TagName, properties);
}
