using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.DInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Scalars.Mapping;

/// <summary>
/// The gate every configured tag passes before it is mapped. Most of it is one regex, and a regex is
/// worth a case per boundary: each of the Studio 5000 rules it encodes is a single character away from
/// the opposite verdict.
/// </summary>
public sealed class ScalarNodePropertyValidatorTests
{
    private const int DefaultPollFrequency = 100;

    private readonly ScalarNodePropertyValidator _validator = new();

    [Theory]
    [InlineData("Motor", "the plain case")]
    [InlineData("MotorSpeed", "mixed case")]
    [InlineData("Tag1", "a trailing digit")]
    [InlineData("_private", "a leading underscore, which Studio 5000 allows")]
    [InlineData("A_B_C", "single underscores between characters")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "exactly the 40-character limit")]
    public void ATagNameStudio5000WouldDeclareIsAccepted(string tagName, string validBecause)
    {
        // Arrange
        var node = DIntNodeWith(CreateTagName(tagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue(validBecause);
    }

    [Theory]
    [InlineData("", "an empty name addresses nothing")]
    [InlineData("1Motor", "a tag name cannot start with a digit")]
    [InlineData("Motor__Speed", "two underscores in a row are rejected")]
    [InlineData("Motor_", "a trailing underscore is rejected")]
    [InlineData("Motor Speed", "a space is not a tag-name character")]
    [InlineData("Motor-Speed", "a hyphen is not a tag-name character")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "41 characters is one past the limit")]
    public void ATagNameStudio5000WouldNotDeclareIsRefusedOnTheTagNameProperty(string tagName, string invalidBecause)
    {
        // Arrange
        var node = DIntNodeWith(CreateTagName(tagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse(invalidBecause);
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixScalarNode.TagNamePropertyName);
    }

    [Theory]
    [InlineData("Motor.Speed")]
    [InlineData("Program:MainProgram.Count")]
    public void AnAddressBeyondAControllerScopeScalarIsRefusedOnTheTagNameProperty(string tagName)
    {
        // Arrange
        var node = DIntNodeWith(CreateTagName(tagName), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixScalarNode.TagNamePropertyName);
    }

    [Fact]
    public void AMissingTagNameIsRefusedNamingTheProperty()
    {
        // Arrange
        var node = DIntNodeWith(CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixScalarNode.TagNamePropertyName);
    }

    [Fact]
    public void ATagNameThatIsNotAStringIsRefusedInsteadOfThrowing()
    {
        // Arrange
        var node = DIntNodeWith(CreateTagName(42), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validating = () => _validator.Validate(node);

        // Assert
        validating.Should().NotThrow().Which.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixScalarNode.TagNamePropertyName);
    }

    [Fact]
    public void AMissingPollFrequencyIsRefusedNamingTheProperty()
    {
        // Arrange
        var node = DIntNodeWith(CreateTagName("Motor"));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixScalarNode.PollFrequencyPropertyName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void APollFrequencyThatIsNotPositiveIsRefused(int pollFrequency)
    {
        // Arrange
        var node = DIntNodeWith(CreateTagName("Motor"), CreatePollFrequency(pollFrequency));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixScalarNode.PollFrequencyPropertyName);
    }

    [Fact]
    public void ATagWithNothingConfiguredIsRefusedOnBothProperties()
    {
        // Arrange
        var node = DIntNodeWith();

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Select(static error => error.PropertyName).Should().BeEquivalentTo(
            ILogixScalarNode.TagNamePropertyName,
            ILogixScalarNode.PollFrequencyPropertyName);
    }

    private static LinkedNode DIntNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(DIntNode.LinkedNodeTypeId, "TestTag", properties);
}
