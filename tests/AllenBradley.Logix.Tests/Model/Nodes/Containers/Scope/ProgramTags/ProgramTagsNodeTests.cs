using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TypedNodeTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Containers.Scope.ProgramTags;

public sealed class ProgramTagsNodeTests
{
    [Fact]
    public void ADataPointOfTheContainersOwnGenerationCanBeAdded()
    {
        // Arrange
        var program = DefaultProgramTagsNode with { Generation = LogixGeneration.Logix5X80 };
        var lReal = DefaultLRealNode;

        // Act
        var canBeAdded = program.CanBeAdded(lReal);

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
        var program = DefaultProgramTagsNode with { Generation = generation };
        var dInt = DefaultDIntNode;

        // Act
        var canBeAdded = program.CanBeAdded(dInt);

        // Assert
        canBeAdded.Should().BeTrue();
    }

    [Fact]
    public void ADataPointRequiringALaterGenerationIsRefused()
    {
        // Arrange
        var program = DefaultProgramTagsNode with { Generation = LogixGeneration.Logix5X70 };
        var lReal = DefaultLRealNode;

        // Act
        var canBeAdded = program.CanBeAdded(lReal);

        // Assert
        canBeAdded.Should().BeFalse();
    }

    [Fact]
    public void AUSIntCanBeAddedUnderAProgramOfA5X80Controller()
    {
        // Arrange
        var program = DefaultProgramTagsNode with { Generation = LogixGeneration.Logix5X80 };
        var usInt = DefaultUSIntNode;

        // Act
        var canBeAdded = program.CanBeAdded(usInt);

        // Assert
        canBeAdded.Should().BeTrue();
    }

    [Fact]
    public void AUSIntUnderAProgramOfA5X70ControllerIsRefused()
    {
        // Arrange
        var program = DefaultProgramTagsNode with { Generation = LogixGeneration.Logix5X70 };
        var usInt = DefaultUSIntNode;

        // Act
        var canBeAdded = program.CanBeAdded(usInt);

        // Assert
        canBeAdded.Should().BeFalse();
    }




    [Fact]
    public void NoProgramNestsInsideAProgram()
    {
        // Arrange
        var program = DefaultProgramTagsNode;
        var anotherProgram = DefaultProgramTagsNode;

        // Act
        var canBeAdded = program.CanBeAdded(anotherProgram);

        // Assert
        canBeAdded.Should().BeFalse();
    }
}
