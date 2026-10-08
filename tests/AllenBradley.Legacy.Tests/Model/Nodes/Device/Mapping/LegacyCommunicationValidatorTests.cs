using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.Device.Mapping;
using ViciOne.Suite.DataPort.Extensions.Outgoing.QueueProcessing;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.LegacyCommunicationTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Model.Nodes.Device.Mapping;

public sealed class LegacyCommunicationValidatorTests
{
    private readonly LegacyCommunicationValidator _validator = new();

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
            .Which.PropertyName.Should().Be(nameof(LegacyCommunication.ConnectionEndpoint));
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
            .Which.PropertyName.Should().Be(nameof(LegacyCommunication.TcpPort));
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
            .Which.PropertyName.Should().Be(nameof(LegacyCommunication.OperationTimeout));
    }

    [Fact]
    public void AnOperationTimeoutAtTheFloorIsAccepted()
    {
        // Arrange
        var communication = DefaultTestCommunication() with { OperationTimeout = 100 };

        // Act
        var validation = _validator.Validate(communication);

        // Assert
        validation.IsValid.Should().BeTrue();
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
            .Which.PropertyName.Should().Be(nameof(LegacyCommunication.MaxPendingMessages));
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
            .Which.PropertyName.Should().Be(nameof(LegacyCommunication.Strategy));
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
