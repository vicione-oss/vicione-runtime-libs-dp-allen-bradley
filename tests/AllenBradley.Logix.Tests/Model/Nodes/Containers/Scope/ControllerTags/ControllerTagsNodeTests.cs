using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TypedNodeTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Containers.Scope.ControllerTags;

public sealed class ControllerTagsNodeTests
{
    [Fact]
    public void ADataPointOfTheContainersOwnGenerationCanBeAdded()
    {
        // Arrange
        var controllerTags = DefaultControllerTagsNode with { Generation = LogixGeneration.Logix5X80 };
        var lReal = DefaultLRealNode;

        // Act
        var canBeAdded = controllerTags.CanBeAdded(lReal);

        // Assert
        canBeAdded.Should().BeTrue();
    }

    [Theory]
    [InlineData(LogixGeneration.Logix5X70)]
    [InlineData(LogixGeneration.Logix5X80)]
    public void ADataPointOfTheOldestGenerationCanBeAddedWhateverTheControllersGeneration(
        LogixGeneration generation)
    {
        // Arrange
        var controllerTags = DefaultControllerTagsNode with { Generation = generation };
        var dInt = DefaultDIntNode;

        // Act
        var canBeAdded = controllerTags.CanBeAdded(dInt);

        // Assert
        canBeAdded.Should().BeTrue();
    }

    [Fact]
    public void ADataPointRequiringALaterGenerationIsRefused()
    {
        // Arrange
        var controllerTags = DefaultControllerTagsNode with { Generation = LogixGeneration.Logix5X70 };
        var lReal = DefaultLRealNode;

        // Act
        var canBeAdded = controllerTags.CanBeAdded(lReal);

        // Assert
        canBeAdded.Should().BeFalse();
    }

    [Fact]
    public void AUSIntCanBeAddedUnderA5X80Controller()
    {
        // Arrange
        var controllerTags = DefaultControllerTagsNode with { Generation = LogixGeneration.Logix5X80 };
        var usInt = DefaultUSIntNode;

        // Act
        var canBeAdded = controllerTags.CanBeAdded(usInt);

        // Assert
        canBeAdded.Should().BeTrue();
    }

    [Fact]
    public void AUSIntUnderA5X70ControllerIsRefused()
    {
        // Arrange
        var controllerTags = DefaultControllerTagsNode with { Generation = LogixGeneration.Logix5X70 };
        var usInt = DefaultUSIntNode;

        // Act
        var canBeAdded = controllerTags.CanBeAdded(usInt);

        // Assert
        canBeAdded.Should().BeFalse();
    }

    [Theory]
    [InlineData(LogixGeneration.Logix5X70)]
    [InlineData(LogixGeneration.Logix5X80)]
    public void AnArrayContainerOfTheOldestGenerationCanBeAddedWhateverTheControllersGeneration(
        LogixGeneration generation)
    {
        // Arrange
        var controllerTags = DefaultControllerTagsNode with { Generation = generation };
        var dIntArray = DefaultDIntArrayContainerNode;

        // Act
        var canBeAdded = controllerTags.CanBeAdded(dIntArray);

        // Assert
        canBeAdded.Should().BeTrue();
    }

    [Fact]
    public void AnArrayContainerOfTheContainersOwnGenerationCanBeAdded()
    {
        // Arrange
        var controllerTags = DefaultControllerTagsNode with { Generation = LogixGeneration.Logix5X80 };
        var lRealArray = DefaultLRealArrayContainerNode;

        // Act
        var canBeAdded = controllerTags.CanBeAdded(lRealArray);

        // Assert
        canBeAdded.Should().BeTrue();
    }

    [Fact]
    public void AnArrayContainerRequiringALaterGenerationIsRefused()
    {
        // Arrange
        var controllerTags = DefaultControllerTagsNode with { Generation = LogixGeneration.Logix5X70 };
        var lRealArray = DefaultLRealArrayContainerNode;

        // Act
        var canBeAdded = controllerTags.CanBeAdded(lRealArray);

        // Assert
        canBeAdded.Should().BeFalse();
    }

    [Fact]
    public void NothingButAnArrayContainerNestsInsideControllerScope()
    {
        // Arrange
        var controllerTags = DefaultControllerTagsNode;
        var anotherContainer = DefaultControllerTagsNode;

        // Act
        var canBeAdded = controllerTags.CanBeAdded(anotherContainer);

        // Assert
        canBeAdded.Should().BeFalse();
    }
}
