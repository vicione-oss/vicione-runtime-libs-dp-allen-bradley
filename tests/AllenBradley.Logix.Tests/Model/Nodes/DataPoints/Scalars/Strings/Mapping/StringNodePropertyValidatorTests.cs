using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Strings.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Scalars.Strings.Mapping;

public sealed class StringNodePropertyValidatorTests
{
    private const int DefaultPollFrequency = 100;

    private readonly StringNodePropertyValidator _validator = new();

    [Fact]
    public void AStringTagWithAllThreePropertiesWellFormedIsAccepted()
    {
        // Arrange
        var node = StringNodeWith(
            CreateTagName("TestTag"),
            CreatePollFrequency(DefaultPollFrequency),
            CreateMaxLength(StringMaxLength.Standard.Value));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AMissingMaxLengthIsRefusedNamingTheProperty()
    {
        // Arrange
        var node = StringNodeWith(CreateTagName("Label"), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(StringNode.MaxLengthPropertyName);
    }

    [Fact]
    public void AMaxLengthThatIsNotAnIntegerIsRefusedInsteadOfThrowing()
    {
        // Arrange
        var node = StringNodeWith(
            CreateTagName("Label"), CreatePollFrequency(DefaultPollFrequency), CreateMaxLength("82"));

        // Act
        var validating = _validator.Invoking(validator => validator.Validate(node));

        // Assert
        validating.Should().NotThrow().Which.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(StringNode.MaxLengthPropertyName);
    }

    [Theory]
    [InlineData(0, "a tag that holds nothing is not a string tag")]
    [InlineData(-1, "a capacity cannot be negative")]
    public void AMaxLengthThatIsNotPositiveIsRefused(int maxLength, string invalidBecause)
    {
        // Arrange
        var node = StringNodeWith(
            CreateTagName("TestTag"), CreatePollFrequency(DefaultPollFrequency), CreateMaxLength(maxLength));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse(invalidBecause);
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(StringNode.MaxLengthPropertyName);
    }

    [Fact]
    public void ATagNameStudio5000WouldNotDeclareIsRefusedOnAStringTagToo()
    {
        // Arrange
        var node = StringNodeWith(
            CreateTagName("1Label"),
            CreatePollFrequency(DefaultPollFrequency),
            CreateMaxLength(StringMaxLength.Standard.Value));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixDataPointNode.TagNamePropertyName);
    }

    [Fact]
    public void AStringTagWithNothingConfiguredIsRefusedOnAllThreeProperties()
    {
        // Arrange
        var node = StringNodeWith();

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Select(static error => error.PropertyName).Should().BeEquivalentTo(
            ILogixDataPointNode.TagNamePropertyName,
            ILogixDataPointNode.PollFrequencyPropertyName,
            StringNode.MaxLengthPropertyName);
    }

    private static LinkedNode StringNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(StringNode.LinkedNodeTypeId, "TestTag", properties);
}
