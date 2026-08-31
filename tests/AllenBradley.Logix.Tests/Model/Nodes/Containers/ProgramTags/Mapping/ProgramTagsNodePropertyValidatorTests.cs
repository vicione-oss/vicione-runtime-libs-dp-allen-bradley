using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixCommunicationTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Containers.ProgramTags.Mapping;

/// <summary>
/// The gate a program container passes before it is mapped. The rule is the tag-name rule, and it is
/// checked here as well because the two reach the controller through different doors: a tag name is
/// looked up, a program name is interpolated into the <c>Program:&lt;name&gt;.@tags</c> the browse asks for.
/// </summary>
public sealed class ProgramTagsNodePropertyValidatorTests
{
    private readonly ProgramTagsNodePropertyValidator _validator = new();

    [Theory]
    [InlineData("MainProgram", "the plain case")]
    [InlineData("_Staging", "a leading underscore, which Studio 5000 allows")]
    [InlineData("Line_2_Fill", "single underscores between characters")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "exactly the 40-character limit")]
    public void Validate_AcceptsAProgramNameStudio5000WouldDeclare(string programName, string because)
    {
        // Arrange
        var node = ProgramTagsLinkedNode("Main", (ProgramTagsNode.ProgramNamePropertyName, programName));

        // Act
        var result = _validator.Validate(node);

        // Assert
        result.IsValid.Should().BeTrue(because);
    }

    [Theory]
    [InlineData("1Main")]
    [InlineData("Main Prog")]
    [InlineData("Main__1")]
    public void Validate_RejectsAProgramNameStudio5000CouldNotDeclare(string programName)
    {
        // Arrange
        var node = ProgramTagsLinkedNode("Main", (ProgramTagsNode.ProgramNamePropertyName, programName));

        // Act
        var result = _validator.Validate(node);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.ErrorMessage.Should().Be(
            $"The program name '{programName}' in node 'Main' "
            + $"({ProgramTagsNode.Logix5x70LinkedNodeTypeId}) is not a valid Logix program name.");
    }

    /// <remarks>
    /// The qualified address is what an integrator reaches for when they know the tag's full name. It is
    /// the container's job to add <c>Program:</c>, so typing it here would compose
    /// <c>Program:Program:MainProgram.Count</c> and find nothing.
    /// </remarks>
    [Theory]
    [InlineData("", "an empty name prefixes nothing")]
    [InlineData("Main_", "a trailing underscore is rejected")]
    [InlineData("Program:MainProgram", "the qualified form belongs to the address, not to this field")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "41 characters is one past the limit")]
    public void Validate_RejectsWhatIsNotAProgramName(string programName, string because)
    {
        // Arrange
        var node = ProgramTagsLinkedNode("Main", (ProgramTagsNode.ProgramNamePropertyName, programName));

        // Act
        var result = _validator.Validate(node);

        // Assert
        result.IsValid.Should().BeFalse(because);
        result.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ProgramTagsNode.ProgramNamePropertyName);
    }

    /// <remarks>
    /// The name rule is skipped when the property is absent, so it has nothing to say about a container
    /// carrying none. Without that <c>When</c> guard it would throw out of <c>GetRequiredPropertyValue</c>
    /// while the configuration was being validated, rather than being reported as the missing property it
    /// is.
    /// </remarks>
    [Fact]
    public void Validate_WhenTheProgramNameIsMissing_DoesNotThrow()
    {
        // Arrange
        var node = ProgramTagsLinkedNode("Main");

        // Act
        var validate = () => _validator.Validate(node);

        // Assert
        validate.Should().NotThrow();
    }
}
