using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.FloatingPoints.LReal;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.FloatingPoints.Real;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.FloatingPoints.Real.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Scalars.FloatingPoints.Real.Mapping;

/// <summary>The mapper on its own: a configured <c>Real</c> node in, a <see cref="RealNode"/> out.</summary>
public sealed class RealNodeMapperTests
{
    private const int DefaultPollFrequency = 100;
    private const string TagName = "MyRealTag";

    private readonly IDataPointNodeMapper<RealNode> _mapper = new RealNodeMapper();

    [Fact]
    public void TheTagNameAndPollFrequencyAreReadOffTheConfiguredNode()
    {
        // Arrange
        var node = RealNodeWith(
            NodePropertyFactory.CreateTagName(TagName), NodePropertyFactory.CreatePollFrequency(DefaultPollFrequency));

        // Act
        var realNode = _mapper.Map(node);

        // Assert
        realNode.TagName.Value.Should().Be(TagName);
        realNode.PollFrequency.Value.Should().Be(TimeSpan.FromMilliseconds(DefaultPollFrequency));
    }

    [Fact]
    public void ARealTagIsOfferedOnEveryGeneration()
    {
        // Arrange
        var node = RealNodeWith(
            NodePropertyFactory.CreateTagName(TagName), NodePropertyFactory.CreatePollFrequency(DefaultPollFrequency));

        // Act
        ILogixScalarNode realNode = _mapper.Map(node);

        // Assert
        // REAL is a classic atomic, unlike its 64-bit sibling: no MinimumGeneration of its own, so it
        // inherits the oldest the addon addresses and a container of any generation admits it.
        realNode.MinimumGeneration.Should().Be(LogixGeneration.Logix5X70);
    }

    [Theory]
    [InlineData(RealNode.LinkedNodeTypeId, true)]
    [InlineData(LRealNode.LinkedNodeTypeId, false)]
    public void ItClaimsARealNodeAndNoOther(string linkedNodeTypeId, bool expected)
    {
        // Arrange
        var node = CreateLinkedNode(
            linkedNodeTypeId,
            TagName,
            NodePropertyFactory.CreateTagName(TagName),
            NodePropertyFactory.CreatePollFrequency(DefaultPollFrequency));

        // Act
        var isTargetMapper = _mapper.IsTargetMapperFor(node);

        // Assert
        isTargetMapper.Should().Be(expected);
    }

    [Fact]
    public void ARealNodeCarryingBothPropertiesIsValid()
    {
        // Arrange
        var node = RealNodeWith(
            NodePropertyFactory.CreateTagName(TagName), NodePropertyFactory.CreatePollFrequency(DefaultPollFrequency));

        // Act
        var result = _mapper.Validate(node);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ARealNodeWithoutATagNameIsRejected()
    {
        // Arrange
        // The scalar validator is what says so; this pins that the mapper hands its node to it.
        var node = RealNodeWith(NodePropertyFactory.CreatePollFrequency(DefaultPollFrequency));

        // Act
        var result = _mapper.Validate(node);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixScalarNode.TagNamePropertyName);
    }

    private static LinkedNode RealNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(RealNode.LinkedNodeTypeId, TagName, properties);
}
