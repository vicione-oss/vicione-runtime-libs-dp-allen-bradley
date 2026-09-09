using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.DInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.UDInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.UDInt.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Scalars.Integers.UDInt.Mapping;

/// <summary>The mapper on its own: a configured <c>UDInt</c> node in, a <see cref="UDIntNode"/> out.</summary>
public sealed class UDIntNodeMapperTests
{
    private const int DefaultPollFrequency = 100;
    private const string TagName = "MyUDIntTag";

    private readonly IDataPointNodeMapper<UDIntNode> _mapper = new UDIntNodeMapper();

    [Fact]
    public void TheTagNameAndPollFrequencyAreReadOffTheConfiguredNode()
    {
        // Arrange
        var node = UDIntNodeWith(
            CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var uDIntNode = _mapper.Map(node);

        // Assert
        uDIntNode.TagName.Value.Should().Be(TagName);
        uDIntNode.PollFrequency.Value.Should().Be(TimeSpan.FromMilliseconds(DefaultPollFrequency));
    }

    [Fact]
    public void AUDIntTagIsOfferedOnA5X80Only()
    {
        // Arrange
        var node = UDIntNodeWith(
            CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        ILogixScalarNode uDIntNode = _mapper.Map(node);

        // Assert
        // UDINT arrived with the 5X80 controllers, so the node states the generation and a container of
        // an older one turns it away — one line, and no edit to either tag-scope container.
        uDIntNode.MinimumGeneration.Should().Be(LogixGeneration.Logix5X80);
    }

    [Theory]
    [InlineData(UDIntNode.LinkedNodeTypeId, true)]
    [InlineData(DIntNode.LinkedNodeTypeId, false)]
    public void ItClaimsAUDIntNodeAndNoOther(string linkedNodeTypeId, bool expectedIsTargetMapper)
    {
        // Arrange
        // The signed twin is the node it must not claim: the two are four bytes each and differ only in
        // the type the controller declares, so claiming both would read every DINT as unsigned.
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
    public void AUDIntNodeCarryingBothPropertiesIsValid()
    {
        // Arrange
        var node = UDIntNodeWith(
            CreateTagName(TagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AUDIntNodeWithoutATagNameIsRejected()
    {
        // Arrange
        // The scalar validator is what says so; this pins that the mapper hands its node to it.
        var node = UDIntNodeWith(CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixScalarNode.TagNamePropertyName);
    }

    private static LinkedNode UDIntNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(UDIntNode.LinkedNodeTypeId, TagName, properties);
}
