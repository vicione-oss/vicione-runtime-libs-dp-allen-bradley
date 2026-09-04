using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device.Mapping;
using ViciOne.Suite.DataPort.Extensions.Outgoing.QueueProcessing;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixCommunicationTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Device.Mapping;

/// <summary>What a device configuration has to satisfy before anything is built from it.</summary>
public sealed class LogixCommunicationValidatorTests
{
    private readonly LogixCommunicationValidator _validator = new();

    [Fact]
    public void TheConfigurationEverySuiteBuildsOnIsValid()
    {
        // Arrange
        var communication = DefaultTestCommunication();

        // Act
        var validation = _validator.Validate(communication);

        // Assert
        validation.IsValid.Should().BeTrue("the suites' own configuration must not be one the port refuses");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void AConfigurationThatNamesNoControllerIsRefused(string connectionEndpoint)
    {
        // Arrange
        var communication = DefaultTestCommunication() with { ConnectionEndpoint = connectionEndpoint };

        // Act
        var validation = _validator.Validate(communication);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(nameof(LogixCommunication.ConnectionEndpoint));
    }

    [Fact]
    public void ATcpPortOfZeroIsRefused()
    {
        // Arrange
        var communication = DefaultTestCommunication() with { TcpPort = 0 };

        // Act
        var validation = _validator.Validate(communication);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(nameof(LogixCommunication.TcpPort));
    }

    [Fact]
    public void AControlLogixWithoutACipRoutePathIsToldWhatACipRoutePathIs()
    {
        // Arrange
        var communication = DefaultTestCommunication() with
        {
            DesignId = DeviceNode.ControlLogix5X70DesignId,
            CipRoutePath = null,
        };

        // Act
        var validation = _validator.Validate(communication);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("CIP route path must be the sequence of hops to the CPU, e.g. \"1,0\".");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void AControlLogixWithABlankCipRoutePathIsRefused(string cipRoutePath)
    {
        // Arrange
        var communication = DefaultTestCommunication() with
        {
            DesignId = DeviceNode.ControlLogix5X70DesignId,
            CipRoutePath = cipRoutePath,
        };

        // Act
        var validation = _validator.Validate(communication);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(nameof(LogixCommunication.CipRoutePath));
    }

    [Fact]
    public void ACompactLogixNeedsNoCipRoutePath()
    {
        // Arrange
        var communication = DefaultTestCommunication() with
        {
            DesignId = DeviceNode.CompactLogix5X70DesignId,
            CipRoutePath = null,
        };

        // Act
        var validation = _validator.Validate(communication);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(99)]
    [InlineData(0)]
    [InlineData(-1)]
    public void AnOperationTimeoutBelowTheFloorIsRefused(int operationTimeout)
    {
        // Arrange
        var communication = DefaultTestCommunication() with { OperationTimeout = operationTimeout };

        // Act
        var validation = _validator.Validate(communication);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(nameof(LogixCommunication.OperationTimeout));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AQueueThatCanHoldNothingIsRefused(int maxPendingMessages)
    {
        // Arrange
        var communication = DefaultTestCommunication() with { MaxPendingMessages = maxPendingMessages };

        // Act
        var validation = _validator.Validate(communication);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(nameof(LogixCommunication.MaxPendingMessages));
    }

    [Fact]
    public void AQueueStrategyTheFrameworkDoesNotHaveIsRefused()
    {
        // Arrange
        var communication = DefaultTestCommunication() with { Strategy = 200 };

        // Act
        var validation = _validator.Validate(communication);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(nameof(LogixCommunication.Strategy));
    }

    [Theory]
    [InlineData(QueueStrategy.DropOldest)]
    [InlineData(QueueStrategy.DropNewest)]
    public void EveryQueueStrategyTheFrameworkHasIsAccepted(QueueStrategy strategy)
    {
        // Arrange
        var communication = DefaultTestCommunication() with { Strategy = (byte)strategy };

        // Act
        var validation = _validator.Validate(communication);

        // Assert
        validation.IsValid.Should().BeTrue();
    }
}
