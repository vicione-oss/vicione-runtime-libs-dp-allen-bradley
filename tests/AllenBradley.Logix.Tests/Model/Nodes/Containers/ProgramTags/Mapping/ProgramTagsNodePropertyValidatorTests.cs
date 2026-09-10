using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Containers.ProgramTags.Mapping;

/// <summary>
/// The gate a program container passes before it is mapped: the tag-name rule again, because a program
/// name is interpolated into the <c>Program:&lt;name&gt;.@tags</c> the browse asks for.
/// </summary>
public sealed class ProgramTagsNodePropertyValidatorTests
{
    private readonly ProgramTagsNodePropertyValidator _validator = new();

    [Theory]
    [InlineData("MainProgram", "the plain case")]
    [InlineData("_Staging", "a leading underscore, which Studio 5000 allows")]
    [InlineData("Line_2_Fill", "single underscores between characters")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "exactly the 40-character limit")]
    public void AProgramNameStudio5000WouldDeclareIsAccepted(string programName, string validBecause)
    {
        // Arrange
        var node = ProgramNodeNamed(programName);

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue(validBecause);
    }

    [Theory]
    [InlineData("1Main")]
    [InlineData("Main Prog")]
    [InlineData("Main__1")]
    public void AProgramNameStudio5000CouldNotDeclareIsRefusedByName(string programName)
    {
        // Arrange
        var node = ProgramNodeNamed(programName);

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.Errors.Should().ContainSingle().Which.ErrorMessage.Should().Be(
            $"The program name '{programName}' in node '{programName}' "
            + $"({ProgramTagsNode.Logix5X70LinkedNodeTypeId}) is not a valid Logix program name.");
    }

    [Theory]
    [InlineData("", "an empty name prefixes nothing")]
    [InlineData("Main_", "a trailing underscore is rejected")]
    [InlineData("Program:MainProgram", "the qualified form belongs to the address, not to this field")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "41 characters is one past the limit")]
    public void WhatIsNotAProgramNameIsRefusedOnTheProgramNameProperty(string programName, string invalidBecause)
    {
        // Arrange
        var node = ProgramNodeNamed(programName);

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse(invalidBecause);
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ProgramTagsNode.ProgramNamePropertyName);
    }

    [Fact]
    public void AMissingProgramNameIsRefusedNamingTheProperty()
    {
        // Arrange
        var node = CreateLinkedNode(ProgramTagsNode.Logix5X70LinkedNodeTypeId, "Main");

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ProgramTagsNode.ProgramNamePropertyName);
    }

    [Fact]
    public void AProgramNameThatIsNotAStringIsRefusedInsteadOfThrowing()
    {
        // Arrange
        var node = CreateLinkedNode(
            ProgramTagsNode.Logix5X70LinkedNodeTypeId, "Main", CreateProgramName(42));

        // Act
        var validating = _validator.Invoking(validator => validator.Validate(node));

        // Assert
        validating.Should().NotThrow().Which.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ProgramTagsNode.ProgramNamePropertyName);
    }

    private static LinkedNode ProgramNodeNamed(string programName) => CreateLinkedNode(
        ProgramTagsNode.Logix5X70LinkedNodeTypeId, programName, CreateProgramName(programName));
}
