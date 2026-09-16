using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ArrayContainer.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Containers.ArrayContainer.Mapping;

/// <summary>
/// The gate an array container passes: the tag-name rule alone. No poll frequency, because a container
/// is not polled itself, and no element count, because the controller declares that.
/// </summary>
public sealed class ArrayContainerNodePropertyValidatorTests
{
    private readonly ArrayContainerNodePropertyValidator _validator = new();

    [Fact]
    public void AContainerWithATagNameIsAccepted()
    {
        // Arrange
        var node = ArrayContainerNodeWith(CreateTagName("Readings"));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AContainerWithoutAPollFrequencyIsStillAccepted()
    {
        // Arrange
        var node = ArrayContainerNodeWith(CreateTagName("Readings"));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Select(static error => error.PropertyName)
            .Should().NotContain(ILogixDataPointNode.PollFrequencyPropertyName);
    }

    [Fact]
    public void ATagNameStudio5000WouldNotDeclareIsRefusedOnAContainerToo()
    {
        // Arrange
        var node = ArrayContainerNodeWith(CreateTagName("1Readings"));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixDataPointNode.TagNamePropertyName);
    }

    [Fact]
    public void AContainerWithNothingConfiguredIsRefusedOnTheTagNameProperty()
    {
        // Arrange
        var node = ArrayContainerNodeWith();

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixDataPointNode.TagNamePropertyName);
    }

    private static LinkedNode ArrayContainerNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode("IntArrayContainer", "Readings", properties);
}
