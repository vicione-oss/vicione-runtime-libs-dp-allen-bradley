using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.Int;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.SInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.SInt.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Scalars.Integers.SInt.Mapping;

/// <summary>The mapper on its own: a configured <c>SInt</c> node in, an <see cref="SIntNode"/> out.</summary>
public sealed class SIntNodeMapperTests
{
    private const int DefaultPollFrequency = 100;
    private const string TagName = "MySIntTag";

    private readonly IDataPointNodeMapper<SIntNode> _mapper = new SIntNodeMapper();

    [Fact]
    public void TheTagNameAndPollFrequencyAreReadOffTheConfiguredNode()
    {
        // Arrange
        var node = SIntNodeWith(
            CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var sIntNode = _mapper.Map(node);

        // Assert
        sIntNode.TagName.Value.Should().Be(TagName);
        sIntNode.PollFrequency.Value.Should().Be(TimeSpan.FromMilliseconds(DefaultPollFrequency));
    }

    [Fact]
    public void ASIntTagIsOfferedOnEveryGeneration()
    {
        // Arrange
        var node = SIntNodeWith(
            CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        ILogixTagNode sIntNode = _mapper.Map(node);

        // Assert
        // SINT is a classic atomic: no MinimumGeneration of its own, so it inherits the oldest the
        // addon addresses and a container of any generation admits it.
        sIntNode.MinimumGeneration.Should().Be(LogixGeneration.Logix5X70);
    }

    [Theory]
    [InlineData(SIntNode.LinkedNodeTypeId, true)]
    [InlineData(IntNode.LinkedNodeTypeId, false)]
    public void ItClaimsASIntNodeAndNoOther(string linkedNodeTypeId, bool expectedIsTargetMapper)
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
    public void ASIntNodeCarryingBothPropertiesIsValid()
    {
        // Arrange
        var node = SIntNodeWith(
            CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ASIntNodeWithoutATagNameIsRejected()
    {
        // Arrange
        // The scalar validator is what says so; this pins that the mapper hands its node to it.
        var node = SIntNodeWith(CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixTagNode.TagNamePropertyName);
    }

    private static LinkedNode SIntNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(SIntNode.LinkedNodeTypeId, TagName, properties);
}
