using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixCommunicationTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Scalars.Mapping;

/// <summary>
/// The gate every configured tag passes before it is mapped. Most of it is one regex, and a regex is
/// worth a case per boundary: each of the Studio 5000 rules it encodes is a single character away from
/// the opposite verdict.
/// </summary>
public sealed class ScalarNodePropertyValidatorTests
{
    private readonly ScalarNodePropertyValidator _validator = new();

    [Theory]
    [InlineData("Motor", "the plain case")]
    [InlineData("MotorSpeed", "mixed case")]
    [InlineData("Tag1", "a trailing digit")]
    [InlineData("_private", "a leading underscore, which Studio 5000 allows")]
    [InlineData("A_B_C", "single underscores between characters")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "exactly the 40-character limit")]
    public void Validate_AcceptsATagNameStudio5000WouldDeclare(string tagName, string because)
    {
        // Arrange

        // Act
        var result = _validator.Validate(ValidScalarLinkedNode(tagName));

        // Assert
        result.IsValid.Should().BeTrue(because);
    }

    [Theory]
    [InlineData("", "an empty name addresses nothing")]
    [InlineData("1Motor", "a tag name cannot start with a digit")]
    [InlineData("Motor__Speed", "two underscores in a row are rejected")]
    [InlineData("Motor_", "a trailing underscore is rejected")]
    [InlineData("Motor Speed", "a space is not a tag-name character")]
    [InlineData("Motor-Speed", "a hyphen is not a tag-name character")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "41 characters is one past the limit")]
    public void Validate_RejectsATagNameStudio5000WouldNot(string tagName, string because)
    {
        // Arrange

        // Act
        var result = _validator.Validate(ValidScalarLinkedNode(tagName));

        // Assert
        result.IsValid.Should().BeFalse(because);
        result.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixScalarNode.TagNamePropertyName);
    }

    /// <remarks>
    /// Both separators are real Logix syntax — <c>Motor.Speed</c> is a structure member and
    /// <c>Program:MainProgram.Count</c> is a program-scoped tag — and the model's own doc comments use
    /// them as examples. Neither is typed into this field. A program-scoped tag is configured as
    /// <c>Count</c> under a program container, which is what supplies the prefix, so program scope came
    /// and went without this gate opening. A structure member has no such container yet, and would be the
    /// one that opens it.
    /// </remarks>
    [Theory]
    [InlineData("Motor.Speed")]
    [InlineData("Program:MainProgram.Count")]
    public void Validate_RejectsAnAddressBeyondAControllerScopeScalar(string tagName)
    {
        // Arrange

        // Act
        var result = _validator.Validate(ValidScalarLinkedNode(tagName));

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenTheTagNameIsMissing_FailsNamingTheProperty()
    {
        // Arrange
        var node = ScalarLinkedNode((ILogixScalarNode.PollFrequencyPropertyName, 100));

        // Act
        var result = _validator.Validate(node);

        // Assert
        result.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixScalarNode.TagNamePropertyName);
    }

    /// <remarks>
    /// The regex rule is skipped when the property is not a string, so a mistyped tag name has to fail
    /// on the presence rule alone. Without that <c>When</c> guard it would throw out of
    /// <c>GetRequiredPropertyValue</c> instead of coming back as a validation failure.
    /// </remarks>
    [Fact]
    public void Validate_WhenTheTagNameIsNotAString_FailsInsteadOfThrowing()
    {
        // Arrange
        var node = ScalarLinkedNode(
            (ILogixScalarNode.TagNamePropertyName, 42),
            (ILogixScalarNode.PollFrequencyPropertyName, 100));

        // Act
        var validate = () => _validator.Validate(node);

        // Assert
        validate.Should().NotThrow().Which.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixScalarNode.TagNamePropertyName);
    }

    [Fact]
    public void Validate_WhenThePollFrequencyIsMissing_FailsNamingTheProperty()
    {
        // Arrange
        var node = ScalarLinkedNode((ILogixScalarNode.TagNamePropertyName, "Motor"));

        // Act
        var result = _validator.Validate(node);

        // Assert
        result.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixScalarNode.PollFrequencyPropertyName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenThePollFrequencyIsNotPositive_Fails(int pollFrequency)
    {
        // Arrange

        // Act
        var result = _validator.Validate(ValidScalarLinkedNode(pollFrequency: pollFrequency));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixScalarNode.PollFrequencyPropertyName);
    }

    [Fact]
    public void Validate_WhenNothingIsConfigured_ReportsBothProperties()
    {
        // Arrange

        // Act
        var result = _validator.Validate(ScalarLinkedNode());

        // Assert
        result.Errors.Select(static error => error.PropertyName).Should().BeEquivalentTo(
            ILogixScalarNode.TagNamePropertyName,
            ILogixScalarNode.PollFrequencyPropertyName);
    }
}
