using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.DInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.Int;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.Int.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Scalars.Integers.Int.Mapping;

/// <summary>The mapper on its own: a configured <c>Int</c> node in, an <see cref="IntNode"/> out.</summary>
public sealed class IntNodeMapperTests
{
    private const int DefaultPollFrequency = 100;
    private const string TagName = "MyIntTag";

    private readonly IDataPointNodeMapper<IntNode> _mapper = new IntNodeMapper();

    [Fact]
    public void TheTagNameAndPollFrequencyAreReadOffTheConfiguredNode()
    {
        // Arrange
        var node = IntNodeWith(CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var intNode = _mapper.Map(node);

        // Assert
        intNode.TagName.Value.Should().Be(TagName);
        intNode.PollFrequency.Value.Should().Be(TimeSpan.FromMilliseconds(DefaultPollFrequency));
    }

    [Fact]
    public void AnIntTagIsOfferedOnEveryGeneration()
    {
        // Arrange
        var node = IntNodeWith(CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        ILogixScalarNode intNode = _mapper.Map(node);

        // Assert
        // INT is a classic atomic: no MinimumGeneration of its own, so it inherits the oldest the
        // addon addresses and a container of any generation admits it.
        intNode.MinimumGeneration.Should().Be(LogixGeneration.Logix5X70);
    }

    [Theory]
    [InlineData(IntNode.LinkedNodeTypeId, true)]
    [InlineData(DIntNode.LinkedNodeTypeId, false)]
    public void ItClaimsAnIntNodeAndNoOther(string linkedNodeTypeId, bool expectedIsTargetMapper)
    {
        // Arrange
        var node = CreateLinkedNode(
            linkedNodeTypeId,
            TagName, CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var isTargetMapper = _mapper.IsTargetMapperFor(node);

        // Assert
        isTargetMapper.Should().Be(expectedIsTargetMapper);
    }

    [Fact]
    public void AnIntNodeCarryingBothPropertiesIsValid()
    {
        // Arrange
        var node = IntNodeWith(CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AnIntNodeWithoutATagNameIsRejected()
    {
        // Arrange
        // The scalar validator is what says so; this pins that the mapper hands its node to it.
        var node = IntNodeWith(CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixScalarNode.TagNamePropertyName);
    }

    private static LinkedNode IntNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(IntNode.LinkedNodeTypeId, TagName, properties);
}
