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

    private const uint DeclaredElementCount = 10;

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

    /// <summary>Values the manifest's UInt32 property cannot carry: text, and a signed integer.</summary>
    public static TheoryData<object> ValuesThatAreNotAnUnsignedInteger => ["10", -1];

    [Theory]
    [MemberData(nameof(ValuesThatAreNotAnUnsignedInteger))]
    public void AnElementCountThatIsNotAnUnsignedIntegerIsRefusedInsteadOfThrowing(object elementCount)
    {
        // Arrange
        var node = ArrayNodeWith(
            CreateTagName("Readings"), CreatePollFrequency(DefaultPollFrequency), CreateElementCount(elementCount));

        // Act
        var validating = _validator.Invoking(validator => validator.Validate(node));

        // Assert
        validating.Should().NotThrow().Which.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(LogixArrayNode.ElementCountPropertyName);
    }

    [Fact]
    public void AnElementCountOfZeroIsRefused()
    {
        // Arrange
        var node = ArrayNodeWith(
            CreateTagName("TestTag"), CreatePollFrequency(DefaultPollFrequency), CreateElementCount(0u));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse("a tag that holds nothing is not an array tag");
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
