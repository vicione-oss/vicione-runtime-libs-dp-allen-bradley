using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.LInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.ULInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.ULInt.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Scalars.Integers.ULInt.Mapping;

/// <summary>The mapper on its own: a configured <c>ULInt</c> node in, a <see cref="ULIntNode"/> out.</summary>
public sealed class ULIntNodeMapperTests
{
    private const int DefaultPollFrequency = 100;
    private const string TagName = "MyULIntTag";

    private readonly IDataPointNodeMapper<ULIntNode> _mapper = new ULIntNodeMapper();

    [Fact]
    public void TheTagNameAndPollFrequencyAreReadOffTheConfiguredNode()
    {
        // Arrange
        var node = ULIntNodeWith(
            NodePropertyFactory.CreateTagName(TagName), NodePropertyFactory.CreatePollFrequency(DefaultPollFrequency));

        // Act
        var uLIntNode = _mapper.Map(node);

        // Assert
        uLIntNode.TagName.Value.Should().Be(TagName);
        uLIntNode.PollFrequency.Value.Should().Be(TimeSpan.FromMilliseconds(DefaultPollFrequency));
    }

    [Fact]
    public void AULIntTagIsOfferedOnA5X80Only()
    {
        // Arrange
        var node = ULIntNodeWith(
            NodePropertyFactory.CreateTagName(TagName), NodePropertyFactory.CreatePollFrequency(DefaultPollFrequency));

        // Act
        ILogixScalarNode uLIntNode = _mapper.Map(node);

        // Assert
        // ULINT arrived with the 5X80 controllers, so the node states the generation and a container of
        // an older one turns it away — the fourth type to reach that rule through this one line, and
        // the last of them to be added without touching a container.
        uLIntNode.MinimumGeneration.Should().Be(LogixGeneration.Logix5X80);
    }

    [Theory]
    [InlineData(ULIntNode.LinkedNodeTypeId, true)]
    [InlineData(LIntNode.LinkedNodeTypeId, false)]
    public void ItClaimsAULIntNodeAndNoOther(string linkedNodeTypeId, bool expected)
    {
        // Arrange
        // The signed twin is the node it must not claim: the two are eight bytes each and differ only in
        // the type the controller declares, so claiming both would read every LINT as unsigned.
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
    public void AULIntNodeCarryingBothPropertiesIsValid()
    {
        // Arrange
        var node = ULIntNodeWith(
            NodePropertyFactory.CreateTagName(TagName), NodePropertyFactory.CreatePollFrequency(DefaultPollFrequency));

        // Act
        var result = _mapper.Validate(node);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AULIntNodeWithoutATagNameIsRejected()
    {
        // Arrange
        // The scalar validator is what says so; this pins that the mapper hands its node to it.
        var node = ULIntNodeWith(NodePropertyFactory.CreatePollFrequency(DefaultPollFrequency));

        // Act
        var result = _mapper.Validate(node);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixScalarNode.TagNamePropertyName);
    }

    private static LinkedNode ULIntNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(ULIntNode.LinkedNodeTypeId, TagName, properties);
}
