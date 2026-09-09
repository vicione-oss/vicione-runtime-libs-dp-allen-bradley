using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.IntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Arrays.Mapping;

public sealed class ArrayNodePropertyValidatorTests
{
    private const int DefaultPollFrequency = 100;

    private const int DeclaredElementCount = 10;

    private readonly ArrayNodePropertyValidator _validator = new();

    [Fact]
    public void AnArrayTagWithAllThreePropertiesWellFormedIsAccepted()
    {
        // Arrange
        var node = ArrayNodeWith(
            CreateTagName("TestTag"),
            CreatePollFrequency(DefaultPollFrequency),
            CreateElementCount(DeclaredElementCount));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AMissingElementCountIsRefusedNamingTheProperty()
    {
        // Arrange
        var node = ArrayNodeWith(CreateTagName("Readings"), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(LogixArrayNode.ElementCountPropertyName);
    }

    [Fact]
    public void AnElementCountThatIsNotAnIntegerIsRefusedInsteadOfThrowing()
    {
        // Arrange
        var node = ArrayNodeWith(
            CreateTagName("Readings"), CreatePollFrequency(DefaultPollFrequency), CreateElementCount("10"));

        // Act
        var validating = _validator.Invoking(validator => validator.Validate(node));

        // Assert
        validating.Should().NotThrow().Which.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(LogixArrayNode.ElementCountPropertyName);
    }

    [Theory]
    [InlineData(0, "a tag that holds nothing is not an array tag")]
    [InlineData(-1, "a count of elements cannot be negative")]
    public void AnElementCountThatIsNotPositiveIsRefused(int elementCount, string invalidBecause)
    {
        // Arrange
        var node = ArrayNodeWith(
            CreateTagName("TestTag"), CreatePollFrequency(DefaultPollFrequency), CreateElementCount(elementCount));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse(invalidBecause);
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(LogixArrayNode.ElementCountPropertyName);
    }

    [Fact]
    public void ATagNameStudio5000WouldNotDeclareIsRefusedOnAnArrayTagToo()
    {
        // Arrange
        var node = ArrayNodeWith(
            CreateTagName("1Readings"),
            CreatePollFrequency(DefaultPollFrequency),
            CreateElementCount(DeclaredElementCount));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixTagNode.TagNamePropertyName);
    }

    [Fact]
    public void AnArrayTagWithNothingConfiguredIsRefusedOnAllThreeProperties()
    {
        // Arrange
        var node = ArrayNodeWith();

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Select(static error => error.PropertyName).Should().BeEquivalentTo(
            ILogixTagNode.TagNamePropertyName,
            ILogixTagNode.PollFrequencyPropertyName,
            LogixArrayNode.ElementCountPropertyName);
    }

    private static LinkedNode ArrayNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(IntArrayNode.LinkedNodeTypeId, "TestTag", properties);
}
