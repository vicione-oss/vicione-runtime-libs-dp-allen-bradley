using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.SInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.USInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.USInt.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Scalars.Integers.USInt.Mapping;

/// <summary>The mapper on its own: a configured <c>USInt</c> node in, a <see cref="USIntNode"/> out.</summary>
public sealed class USIntNodeMapperTests
{
    private const int DefaultPollFrequency = 100;
    private const string TagName = "MyUSIntTag";

    private readonly IDataPointNodeMapper<USIntNode> _mapper = new USIntNodeMapper();

    [Fact]
    public void TheTagNameAndPollFrequencyAreReadOffTheConfiguredNode()
    {
        // Arrange
        var node = USIntNodeWith(
            CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var usIntNode = _mapper.Map(node);

        // Assert
        usIntNode.TagName.Value.Should().Be(TagName);
        usIntNode.PollFrequency.Value.Should().Be(TimeSpan.FromMilliseconds(DefaultPollFrequency));
    }

    [Fact]
    public void AUSIntTagIsOfferedOnA5X80Only()
    {
        // Arrange
        var node = USIntNodeWith(
            CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        ILogixTagNode usIntNode = _mapper.Map(node);

        // Assert
        // USINT arrived with the 5X80 controllers, so the node states the generation and a container of
        // an older one turns it away — the same one line LRealNode carries, and no edit to a container.
        usIntNode.MinimumGeneration.Should().Be(LogixGeneration.Logix5X80);
    }

    [Theory]
    [InlineData(USIntNode.LinkedNodeTypeId, true)]
    [InlineData(SIntNode.LinkedNodeTypeId, false)]
    public void ItClaimsAUSIntNodeAndNoOther(string linkedNodeTypeId, bool expectedIsTargetMapper)
    {
        // Arrange
        // The signed twin is the node it must not claim: the two are one byte each and differ only in
        // the type the controller declares, so claiming both would read every SINT as unsigned.
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
    public void AUSIntNodeCarryingBothPropertiesIsValid()
    {
        // Arrange
        var node = USIntNodeWith(
            CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AUSIntNodeWithoutATagNameIsRejected()
    {
        // Arrange
        // The scalar validator is what says so; this pins that the mapper hands its node to it.
        var node = USIntNodeWith(CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixTagNode.TagNamePropertyName);
    }

    private static LinkedNode USIntNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(USIntNode.LinkedNodeTypeId, TagName, properties);
}
