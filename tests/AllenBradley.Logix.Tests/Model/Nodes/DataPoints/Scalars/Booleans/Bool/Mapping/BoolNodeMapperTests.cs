using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Booleans.Bool;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Booleans.Bool.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.SInt;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Scalars.Booleans.Bool.Mapping;

/// <summary>The mapper on its own: a configured <c>Bool</c> node in, a <see cref="BoolNode"/> out.</summary>
public sealed class BoolNodeMapperTests
{
    private const int DefaultPollFrequency = 100;
    private const string TagName = "MyBoolTag";

    private readonly IDataPointNodeMapper<BoolNode> _mapper = new BoolNodeMapper();

    [Fact]
    public void TheTagNameAndPollFrequencyAreReadOffTheConfiguredNode()
    {
        // Arrange
        var node = BoolNodeWith(
            CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var boolNode = _mapper.Map(node);

        // Assert
        boolNode.TagName.Value.Should().Be(TagName);
        boolNode.PollFrequency.Value.Should().Be(TimeSpan.FromMilliseconds(DefaultPollFrequency));
    }

    [Fact]
    public void ABoolTagIsOfferedOnEveryGeneration()
    {
        // Arrange
        var node = BoolNodeWith(
            CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        ILogixScalarNode boolNode = _mapper.Map(node);

        // Assert
        // BOOL is a classic atomic: no MinimumGeneration of its own, so it inherits the oldest the
        // addon addresses and a container of any generation admits it.
        boolNode.MinimumGeneration.Should().Be(LogixGeneration.Logix5X70);
    }

    [Theory]
    [InlineData(BoolNode.LinkedNodeTypeId, true)]
    [InlineData(SIntNode.LinkedNodeTypeId, false)]
    public void ItClaimsABoolNodeAndNoOther(string linkedNodeTypeId, bool expectedIsTargetMapper)
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
    public void ABoolNodeCarryingBothPropertiesIsValid()
    {
        // Arrange
        var node = BoolNodeWith(
            CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ABoolNodeWithoutATagNameIsRejected()
    {
        // Arrange
        // The scalar validator is what says so; this pins that the mapper hands its node to it.
        var node = BoolNodeWith(CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixScalarNode.TagNamePropertyName);
    }

    private static LinkedNode BoolNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(BoolNode.LinkedNodeTypeId, TagName, properties);
}
