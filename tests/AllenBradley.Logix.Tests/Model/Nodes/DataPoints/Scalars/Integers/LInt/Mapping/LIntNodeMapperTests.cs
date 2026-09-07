using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.Int;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.LInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.LInt.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Scalars.Integers.LInt.Mapping;

/// <summary>The mapper on its own: a configured <c>LInt</c> node in, an <see cref="LIntNode"/> out.</summary>
public sealed class LIntNodeMapperTests
{
    private const int DefaultPollFrequency = 100;
    private const string TagName = "MyLIntTag";

    private readonly IDataPointNodeMapper<LIntNode> _mapper = new LIntNodeMapper();

    [Fact]
    public void TheTagNameAndPollFrequencyAreReadOffTheConfiguredNode()
    {
        // Arrange
        var node = LIntNodeWith(
            NodePropertyFactory.CreateTagName(TagName), NodePropertyFactory.CreatePollFrequency(DefaultPollFrequency));

        // Act
        var lIntNode = _mapper.Map(node);

        // Assert
        lIntNode.TagName.Value.Should().Be(TagName);
        lIntNode.PollFrequency.Value.Should().Be(TimeSpan.FromMilliseconds(DefaultPollFrequency));
    }

    [Fact]
    public void ALIntTagIsOfferedOnEveryGeneration()
    {
        // Arrange
        var node = LIntNodeWith(
            NodePropertyFactory.CreateTagName(TagName), NodePropertyFactory.CreatePollFrequency(DefaultPollFrequency));

        // Act
        ILogixScalarNode lIntNode = _mapper.Map(node);

        // Assert
        // LINT is a classic atomic: no MinimumGeneration of its own, so it inherits the oldest the
        // addon addresses and a container of any generation admits it.
        lIntNode.MinimumGeneration.Should().Be(LogixGeneration.Logix5X70);
    }

    [Theory]
    [InlineData(LIntNode.LinkedNodeTypeId, true)]
    [InlineData(IntNode.LinkedNodeTypeId, false)]
    public void ItClaimsALIntNodeAndNoOther(string linkedNodeTypeId, bool expected)
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
    public void ALIntNodeCarryingBothPropertiesIsValid()
    {
        // Arrange
        var node = LIntNodeWith(
            NodePropertyFactory.CreateTagName(TagName), NodePropertyFactory.CreatePollFrequency(DefaultPollFrequency));

        // Act
        var result = _mapper.Validate(node);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ALIntNodeWithoutATagNameIsRejected()
    {
        // Arrange
        // The scalar validator is what says so; this pins that the mapper hands its node to it.
        var node = LIntNodeWith(NodePropertyFactory.CreatePollFrequency(DefaultPollFrequency));

        // Act
        var result = _mapper.Validate(node);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixScalarNode.TagNamePropertyName);
    }

    private static LinkedNode LIntNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(LIntNode.LinkedNodeTypeId, TagName, properties);
}
