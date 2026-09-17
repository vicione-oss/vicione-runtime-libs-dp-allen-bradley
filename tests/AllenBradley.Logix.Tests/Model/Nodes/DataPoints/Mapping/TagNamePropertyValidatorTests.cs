using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ArrayContainer;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ControllerTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.DInt;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Mapping;

/// <summary>The tag-name gate every configured node passes, checked a case per boundary.</summary>
public sealed class TagNamePropertyValidatorTests
{
    private readonly TagNamePropertyValidator _validator = new();

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
        var node = DIntNodeWith(CreateTagName(tagName));

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
        var node = DIntNodeWith(CreateTagName(tagName));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse(invalidBecause);
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixDataPointNode.TagNamePropertyName);
    }

    [Theory]
    [InlineData("Motor.Speed")]
    [InlineData("Program:MainProgram.Count")]
    public void AnAddressBeyondAControllerScopeScalarIsRefusedOnTheTagNameProperty(string tagName)
    {
        // Arrange
        var node = DIntNodeWith(CreateTagName(tagName));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixDataPointNode.TagNamePropertyName);
    }

    [Theory]
    [InlineData("[0]", "the first element")]
    [InlineData("[5]", "a single digit")]
    [InlineData("[1234]", "several digits")]
    public void ASubscriptUnderAnArrayContainerIsAccepted(string subscript, string validBecause)
    {
        // Arrange
        var node = ArrayElementNodeWith(CreateTagName(subscript));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue(validBecause);
    }

    [Theory]
    [InlineData("Readings", "a declared tag name addresses no element")]
    [InlineData("5", "the brackets are part of the subscript")]
    [InlineData("[]", "an empty subscript selects nothing")]
    [InlineData("[-1]", "an element is never negative")]
    [InlineData("[007]", "leading zeros are refused")]
    [InlineData("[00]", "zero is written once")]
    [InlineData("[a]", "a subscript is a number, not a name")]
    [InlineData("[1,2]", "the container is one-dimensional")]
    [InlineData("Readings[5]", "the container already names the array")]
    public void ANameThatIsNotASubscriptUnderAnArrayContainerIsRefusedOnTheTagNameProperty(
        string tagName, string invalidBecause)
    {
        // Arrange
        var node = ArrayElementNodeWith(CreateTagName(tagName));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse(invalidBecause);
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixDataPointNode.TagNamePropertyName);
    }

    [Fact]
    public void ASubscriptOutsideAnArrayContainerIsRefusedOnTheTagNameProperty()
    {
        // Arrange
        var node = DIntNodeWith(CreateTagName("[5]"));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixDataPointNode.TagNamePropertyName);
    }

    [Theory]
    [InlineData("Motor", true)]
    [InlineData("[5]", false)]
    public void AChildOfAScopeContainerIsHeldToTheTagNameRule(string tagName, bool expectedIsValid)
    {
        // Arrange
        var node = CreateChildLinkedNode(
            ControllerTagsNode.Logix5X80LinkedNodeTypeId, DIntNode.LinkedNodeTypeId, "TestTag", CreateTagName(tagName));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().Be(expectedIsValid);
    }

    [Fact]
    public void AMissingTagNameUnderAnArrayContainerIsRefusedOnce()
    {
        // Arrange
        var node = ArrayElementNodeWith();

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixDataPointNode.TagNamePropertyName);
    }

    [Fact]
    public void AMissingTagNameIsRefusedNamingTheProperty()
    {
        // Arrange
        var node = DIntNodeWith();

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixDataPointNode.TagNamePropertyName);
    }

    [Fact]
    public void ATagNameThatIsNotAStringIsRefusedInsteadOfThrowing()
    {
        // Arrange
        var node = DIntNodeWith(CreateTagName(42));

        // Act
        var validating = _validator.Invoking(validator => validator.Validate(node));

        // Assert
        validating.Should().NotThrow().Which.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixDataPointNode.TagNamePropertyName);
    }

    private static LinkedNode DIntNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(DIntNode.LinkedNodeTypeId, "TestTag", properties);

    private static LinkedNode ArrayElementNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateChildLinkedNode(
            ArrayContainerNode.DIntLinkedNodeTypeId, DIntNode.LinkedNodeTypeId, "Element", properties);
}
