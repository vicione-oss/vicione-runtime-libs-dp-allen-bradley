using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Strings.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixCommunicationTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Scalars.Strings.Mapping;

/// <summary>
/// The gate a configured string tag passes before it is mapped. The tag name and poll frequency belong to
/// <c>ScalarNodePropertyValidatorTests</c>; what is tested here is the capacity, and that including the
/// scalar rules really does bring them along.
/// </summary>
public sealed class StringNodePropertyValidatorTests
{
    private readonly StringNodePropertyValidator _validator = new();

    [Fact]
    public void Validate_WhenAllThreePropertiesAreWellFormed_Passes()
    {
        // Arrange
        var node = ValidStringLinkedNode();

        // Act
        var result = _validator.Validate(node);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenTheMaxLengthIsMissing_FailsNamingTheProperty()
    {
        // Arrange
        var node = StringLinkedNode(
            (ILogixScalarNode.TagNamePropertyName, "Label"),
            (ILogixScalarNode.PollFrequencyPropertyName, 100));

        // Act
        var result = _validator.Validate(node);

        // Assert
        result.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(StringNode.MaxLengthPropertyName);
    }

    /// <remarks>
    /// The positivity rule is skipped when the property is not an integer, so a mistyped capacity has to
    /// fail on the presence rule alone. Without that <c>When</c> guard it would throw out of
    /// <c>GetRequiredPropertyValue</c> instead of coming back as a validation failure.
    /// </remarks>
    [Fact]
    public void Validate_WhenTheMaxLengthIsNotAnInteger_FailsInsteadOfThrowing()
    {
        // Arrange
        var node = StringLinkedNode(
            (ILogixScalarNode.TagNamePropertyName, "Label"),
            (ILogixScalarNode.PollFrequencyPropertyName, 100),
            (StringNode.MaxLengthPropertyName, "82"));

        // Act
        var validate = () => _validator.Validate(node);

        // Assert
        validate.Should().NotThrow().Which.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(StringNode.MaxLengthPropertyName);
    }

    [Theory]
    [InlineData(0, "a tag that holds nothing is not a string tag")]
    [InlineData(-1, "a capacity cannot be negative")]
    public void Validate_WhenTheMaxLengthIsNotPositive_Fails(int maxLength, string because)
    {
        // Arrange
        var node = ValidStringLinkedNode(maxLength: maxLength);

        // Act
        var result = _validator.Validate(node);

        // Assert
        result.IsValid.Should().BeFalse(because);
        result.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(StringNode.MaxLengthPropertyName);
    }

    /// <remarks>
    /// The one case that would pass if <c>Include(new ScalarNodePropertyValidator())</c> were dropped: a
    /// string node is a scalar node, and the tag-name rule applies to it unchanged.
    /// </remarks>
    [Fact]
    public void Validate_WhenTheTagNameIsNotOneStudio5000WouldDeclare_Fails()
    {
        // Arrange
        var node = ValidStringLinkedNode("1Label");

        // Act
        var result = _validator.Validate(node);

        // Assert
        result.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixScalarNode.TagNamePropertyName);
    }

    [Fact]
    public void Validate_WhenNothingIsConfigured_ReportsAllThreeProperties()
    {
        // Arrange
        var node = StringLinkedNode();

        // Act
        var result = _validator.Validate(node);

        // Assert
        result.Errors.Select(static error => error.PropertyName).Should().BeEquivalentTo(
            ILogixScalarNode.TagNamePropertyName,
            ILogixScalarNode.PollFrequencyPropertyName,
            StringNode.MaxLengthPropertyName);
    }
}
