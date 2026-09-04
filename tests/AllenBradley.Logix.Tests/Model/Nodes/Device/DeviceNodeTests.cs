using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device.LogixControllerKind;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TypedNodeTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Device;

public sealed class DeviceNodeTests
{
    /// <summary>Each device node type with the controller container the editor offers under it.</summary>
    public static TheoryData<DeviceNodeWithContainerTestCase> SameGeneration =>
    [
        new DeviceNodeWithContainerTestCase(ControlLogix5X70, LogixGeneration.Logix5X70),
        new DeviceNodeWithContainerTestCase(ControlLogix5X80, LogixGeneration.Logix5X80),
        new DeviceNodeWithContainerTestCase(CompactLogix5X70, LogixGeneration.Logix5X70),
        new DeviceNodeWithContainerTestCase(CompactLogix5X80, LogixGeneration.Logix5X80),
    ];

    [Theory]
    [MemberData(nameof(SameGeneration))]
    public void AControllerContainerOfTheDevicesOwnGenerationCanBeAdded(DeviceNodeWithContainerTestCase testCase)
    {
        // Arrange
        var deviceNode = DefaultDeviceNode with { ControllerKind = testCase.DeviceNodeKind };
        var container = DefaultControllerTagsNode with { Generation = testCase.ContainerGenerationToAdd };

        // Act
        var canBeAdded = deviceNode.CanBeAdded(container);

        // Assert
        canBeAdded.Should().BeTrue();
    }

    /// <summary>Each device node type with a controller container of a different generation.</summary>
    public static TheoryData<DeviceNodeWithContainerTestCase> DifferentGenerations =>
    [
        new DeviceNodeWithContainerTestCase(ControlLogix5X70, LogixGeneration.Logix5X80),
        new DeviceNodeWithContainerTestCase(ControlLogix5X80, LogixGeneration.Logix5X70),
        new DeviceNodeWithContainerTestCase(CompactLogix5X70, LogixGeneration.Logix5X80),
        new DeviceNodeWithContainerTestCase(CompactLogix5X80, LogixGeneration.Logix5X70),
    ];

    [Theory]
    [MemberData(nameof(DifferentGenerations))]
    public void AControllerContainerOfDifferentGenerationCannotBeAdded(DeviceNodeWithContainerTestCase testCase)
    {
        // Arrange
        var deviceNode = DefaultDeviceNode with { ControllerKind = testCase.DeviceNodeKind };
        var container = DefaultControllerTagsNode with { Generation = testCase.ContainerGenerationToAdd };

        // Act
        var adding = deviceNode.Invoking(node => node.CanBeAdded(container));

        // Assert
        adding.Should().Throw<InvalidConfigurationException>();
    }

    [Theory]
    [MemberData(nameof(SameGeneration))]
    public void AProgramContainerOfTheDevicesOwnGenerationCanBeAdded(DeviceNodeWithContainerTestCase testCase)
    {
        // Arrange
        var deviceNode = DefaultDeviceNode with { ControllerKind = testCase.DeviceNodeKind };
        var container = DefaultProgramTagsNode with { Generation = testCase.ContainerGenerationToAdd };

        // Act
        var canBeAdded = deviceNode.CanBeAdded(container);

        // Assert
        canBeAdded.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(DifferentGenerations))]
    public void AProgramContainerOfAnotherGenerationCannotBeAdded(DeviceNodeWithContainerTestCase testCase)
    {
        // Arrange
        var deviceNode = DefaultDeviceNode with { ControllerKind = testCase.DeviceNodeKind };
        var container = DefaultProgramTagsNode with { Generation = testCase.ContainerGenerationToAdd };

        // Act
        var adding = deviceNode.Invoking(node => node.CanBeAdded(container));

        // Assert
        adding.Should().Throw<InvalidConfigurationException>();
    }

    [Fact]
    public void ANodeThatIsNoTagScopeContainerCannotBeAdded()
    {
        // Arrange
        var deviceNode = DefaultDeviceNode;
        var anotherDeviceNode = DefaultDeviceNode;

        // Act
        var canBeAdded = deviceNode.CanBeAdded(anotherDeviceNode);

        // Assert
        canBeAdded.Should().BeFalse();
    }
}
