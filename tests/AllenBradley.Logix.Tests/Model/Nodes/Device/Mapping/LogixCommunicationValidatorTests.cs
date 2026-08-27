using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device.Mapping;
using ViciOne.Suite.DataPort.Extensions.Outgoing.QueueProcessing;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixCommunicationTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Device.Mapping;

/// <summary>
/// What a device configuration has to satisfy before anything is built from it. The rules are the ones
/// decidable without a controller: whether a gateway and path are there at all, and whether the two
/// enum-backed bytes name something the model has.
/// </summary>
public sealed class LogixCommunicationValidatorTests
{
    private readonly LogixCommunicationValidator _validator = new();

    [Fact]
    public void Validate_TheFactorysConfiguration_Passes()
    {
        // Arrange

        // Act
        var result = _validator.Validate(CreateCommunication());

        // Assert
        result.IsValid.Should().BeTrue("the suites' own configuration must not be one the port refuses");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithoutAGateway_Fails(string gateway)
    {
        // Arrange

        // Act
        var result = Validate(communication => communication with { Gateway = gateway });

        // Assert
        result.Should().ContainSingle().Which.Should().Be(nameof(LogixCommunication.Gateway));
    }

    [Fact]
    public void Validate_WithoutAPath_Fails()
    {
        // Arrange

        // Act
        var result = Validate(static communication => communication with { Path = "" });

        // Assert
        result.Should().ContainSingle().Which.Should().Be(nameof(LogixCommunication.Path));
    }

    /// <remarks>
    /// The property is a byte on the wire and an enum in the model, so the only thing standing between a
    /// configuration and an undefined <see cref="LogixControllerType"/> is this rule.
    /// </remarks>
    [Fact]
    public void Validate_WithAControllerTypeTheModelDoesNotHave_Fails()
    {
        // Arrange

        // Act
        var result = Validate(static communication => communication with { ControllerType = 200 });

        // Assert
        result.Should().ContainSingle().Which.Should().Be(nameof(LogixCommunication.ControllerType));
    }

    [Theory]
    [InlineData(LogixControllerType.ControlLogix)]
    [InlineData(LogixControllerType.CompactLogix)]
    public void Validate_WithEveryControllerTypeTheModelHas_Passes(LogixControllerType controllerType)
    {
        // Arrange

        // Act
        var result = Validate(communication => communication with { ControllerType = (byte)controllerType });

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(99)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithAnOperationTimeoutBelowTheFloor_Fails(int operationTimeout)
    {
        // Arrange

        // Act
        var result = Validate(communication => communication with { OperationTimeout = operationTimeout });

        // Assert
        result.Should().ContainSingle().Which.Should().Be(nameof(LogixCommunication.OperationTimeout));
    }

    /// <remarks>
    /// A zero-capacity queue would accept no write at all, and the failure would look like a controller
    /// problem rather than a configuration one.
    /// </remarks>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithAQueueThatCanHoldNothing_Fails(int maxPendingMessages)
    {
        // Arrange

        // Act
        var result = Validate(communication => communication with { MaxPendingMessages = maxPendingMessages });

        // Assert
        result.Should().ContainSingle().Which.Should().Be(nameof(LogixCommunication.MaxPendingMessages));
    }

    [Fact]
    public void Validate_WithAQueueStrategyTheFrameworkDoesNotHave_Fails()
    {
        // Arrange

        // Act
        var result = Validate(static communication => communication with { Strategy = 200 });

        // Assert
        result.Should().ContainSingle().Which.Should().Be(nameof(LogixCommunication.Strategy));
    }

    [Theory]
    [InlineData(QueueStrategy.DropOldest)]
    [InlineData(QueueStrategy.DropNewest)]
    public void Validate_WithEveryQueueStrategyTheFrameworkHas_Passes(QueueStrategy strategy)
    {
        // Arrange

        // Act
        var result = Validate(communication => communication with { Strategy = (byte)strategy });

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>The properties the validator objected to, so a case names a property rather than a message.</summary>
    private IReadOnlyList<string> Validate(Func<LogixCommunication, LogixCommunication> change) =>
    [
        .. _validator.Validate(change(CreateCommunication()))
            .Errors.Select(static error => error.PropertyName),
    ];
}
