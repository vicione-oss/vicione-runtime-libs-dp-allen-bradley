using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TypedNodeTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Containers.ArrayContainer;

public sealed class ArrayContainerNodeTests
{
    [Fact]
    public void ElementNodeWithSameDatatypeCanBeAdded()
    {
        // Arrange
        var arrayContainerNode = DefaultIntArrayContainerNode;
        var intNode = DefaultIntNode;

        // Act
        var canBeAdded = arrayContainerNode.CanBeAdded(intNode);

        // Assert
        canBeAdded.Should().BeTrue();
    }

    [Fact]
    public void ElementNodeWithDifferentDataTypeCanNotBeAdded()
    {
        // Arrange
        var arrayContainerNode = DefaultIntArrayContainerNode;
        var stringNode = DefaultStringNode;

        // Act
        var canBeAdded = arrayContainerNode.CanBeAdded(stringNode);

        // Assert
        canBeAdded.Should().BeFalse();
    }

    [Fact]
    public void ATimerNodeCanBeAnElementOfATimerArray()
    {
        // Arrange
        var delays = DefaultTimerArrayContainerNode;
        var timerNode = DefaultTimerNode;

        // Act
        var canBeAdded = delays.CanBeAdded(timerNode);

        // Assert
        canBeAdded.Should().BeTrue();
    }

    [Fact]
    public void ADIntNodeCanNotBeAnElementOfATimerArray()
    {
        // Arrange
        // The DINT a timer element carries its accumulated time in still makes no timer.
        var delays = DefaultTimerArrayContainerNode;
        var dIntNode = DefaultDIntNode;

        // Act
        var canBeAdded = delays.CanBeAdded(dIntNode);

        // Assert
        canBeAdded.Should().BeFalse();
    }

    [Fact]
    public void OtherContainerNodesCanNotBeAdded()
    {
        // Arrange
        var arrayContainerNode = DefaultIntArrayContainerNode;
        var configurationNode = Substitute.For<ILogixContainerNode>();

        // Act
        var canBeAdded = arrayContainerNode.CanBeAdded(configurationNode);

        // Assert
        canBeAdded.Should().BeFalse();
    }
}
