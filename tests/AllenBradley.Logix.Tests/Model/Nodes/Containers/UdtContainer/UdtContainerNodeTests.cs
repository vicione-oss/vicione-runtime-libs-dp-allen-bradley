using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TypedNodeTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Containers.UdtContainer;

public sealed class UdtContainerNodeTests
{
    [Fact]
    public void AMemberOfTheOldestGenerationCanBeAddedWhateverTheUdtsGeneration()
    {
        // Arrange
        var udt = DefaultUdtContainerNode with { Generation = LogixGeneration.Logix5X70 };
        var speed = DefaultIntNode;

        // Act
        var canBeAdded = udt.CanBeAdded(speed);

        // Assert
        canBeAdded.Should().BeTrue();
    }

    [Fact]
    public void AMemberOfTheUdtsOwnGenerationCanBeAdded()
    {
        // Arrange
        var udt = DefaultUdtContainerNode with { Generation = LogixGeneration.Logix5X80 };
        var temperature = DefaultLRealNode;

        // Act
        var canBeAdded = udt.CanBeAdded(temperature);

        // Assert
        canBeAdded.Should().BeTrue();
    }

    [Fact]
    public void AMemberRequiringALaterGenerationIsRefused()
    {
        // Arrange
        var udt = DefaultUdtContainerNode with { Generation = LogixGeneration.Logix5X70 };
        var temperature = DefaultLRealNode;

        // Act
        var canBeAdded = udt.CanBeAdded(temperature);

        // Assert
        canBeAdded.Should().BeFalse();
    }

    [Fact]
    public void AStringMemberCanBeAdded()
    {
        // Arrange
        var udt = DefaultUdtContainerNode;
        var label = DefaultStringNode;

        // Act
        var canBeAdded = udt.CanBeAdded(label);

        // Assert
        canBeAdded.Should().BeTrue();
    }

    [Fact]
    public void AWholeArrayMemberCanBeAdded()
    {
        // Arrange
        var udt = DefaultUdtContainerNode;
        var readings = DefaultIntArrayDataPointNode;

        // Act
        var canBeAdded = udt.CanBeAdded(readings);

        // Assert
        canBeAdded.Should().BeTrue();
    }

    [Fact]
    public void ANestedUdtOfTheSameGenerationCanBeAdded()
    {
        // Arrange
        var udt = DefaultUdtContainerNode;
        var ramp = DefaultUdtContainerNode with { TagName = new("Ramp") };

        // Act
        var canBeAdded = udt.CanBeAdded(ramp);

        // Assert
        canBeAdded.Should().BeTrue();
    }

    [Fact]
    public void ANestedUdtOfAnotherGenerationIsRefused()
    {
        // Arrange
        var udt = DefaultUdtContainerNode with { Generation = LogixGeneration.Logix5X70 };
        var ramp = DefaultUdtContainerNode with { Generation = LogixGeneration.Logix5X80 };

        // Act
        var canBeAdded = udt.CanBeAdded(ramp);

        // Assert
        canBeAdded.Should().BeFalse();
    }

    [Fact]
    public void AnArrayContainerIsRefused()
    {
        // Arrange
        var udt = DefaultUdtContainerNode;
        var readings = DefaultIntArrayContainerNode;

        // Act
        var canBeAdded = udt.CanBeAdded(readings);

        // Assert
        canBeAdded.Should().BeFalse();
    }

    [Fact]
    public void OtherContainerNodesAreRefused()
    {
        // Arrange
        var udt = DefaultUdtContainerNode;
        var container = Substitute.For<ILogixContainerNode>();

        // Act
        var canBeAdded = udt.CanBeAdded(container);

        // Assert
        canBeAdded.Should().BeFalse();
    }
}
