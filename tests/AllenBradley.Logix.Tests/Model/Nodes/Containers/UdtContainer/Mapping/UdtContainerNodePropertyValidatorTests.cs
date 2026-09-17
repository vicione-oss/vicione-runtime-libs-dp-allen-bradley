using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.UdtContainer;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.UdtContainer.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Containers.UdtContainer.Mapping;

/// <summary>
/// The gate a UDT container passes: the tag-name rule alone. No poll frequency, because a
/// container is not polled itself, and no type, because the controller's template declares that.
/// </summary>
public sealed class UdtContainerNodePropertyValidatorTests
{
    private readonly UdtContainerNodePropertyValidator _validator = new();

    [Fact]
    public void AUdtWithATagNameIsAccepted()
    {
        // Arrange
        var node = UdtNodeWith(CreateTagName("Motor"));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AUdtWithoutAPollFrequencyIsStillAccepted()
    {
        // Arrange
        var node = UdtNodeWith(CreateTagName("Motor"));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Select(static error => error.PropertyName)
            .Should().NotContain(ILogixDataPointNode.PollFrequencyPropertyName);
    }

    [Fact]
    public void ANestedUdtNamedAsAMemberIsAccepted()
    {
        // Arrange
        var node = CreateChildLinkedNode(
            UdtContainerNode.Logix5X70LinkedNodeTypeId, UdtContainerNode.Logix5X70LinkedNodeTypeId,
            "Ramp", CreateTagName("Ramp"));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ADottedNameIsRefusedBecauseAMemberIsANodeOfItsOwn()
    {
        // Arrange
        var node = UdtNodeWith(CreateTagName("Motor.Ramp"));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixDataPointNode.TagNamePropertyName);
    }

    [Fact]
    public void ATagNameStudio5000WouldNotDeclareIsRefusedOnAUdtToo()
    {
        // Arrange
        var node = UdtNodeWith(CreateTagName("1Motor"));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixDataPointNode.TagNamePropertyName);
    }

    [Fact]
    public void AUdtWithNothingConfiguredIsRefusedOnTheTagNameProperty()
    {
        // Arrange
        var node = UdtNodeWith();

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixDataPointNode.TagNamePropertyName);
    }

    private static LinkedNode UdtNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(UdtContainerNode.Logix5X70LinkedNodeTypeId, "Motor", properties);
}
