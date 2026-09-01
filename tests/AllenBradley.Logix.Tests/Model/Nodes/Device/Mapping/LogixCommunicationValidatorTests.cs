using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device.Mapping;
using ViciOne.Suite.DataPort.Extensions.Outgoing.QueueProcessing;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixCommunicationTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Device.Mapping;

/// <summary>
/// What a device configuration has to satisfy before anything is built from it. The rules are the ones
/// decidable without a controller: whether a connection endpoint is there at all, whether a CIP route path is where the node
/// type asks for one, and whether the numbers are in range.
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
    public void Validate_WithoutAGateway_Fails(string connectionEndpoint)
    {
        // Arrange

        // Act
        var result = Validate(communication => communication with { ConnectionEndpoint = connectionEndpoint });

        // Assert
        result.Should().ContainSingle().Which.Should().Be(nameof(LogixCommunication.ConnectionEndpoint));
    }

    /// <remarks>
    /// A <c>ushort</c> already bounds the port above, so 0 is the only value left to reject — and it is
    /// the one a configuration written without the manifest's default would carry.
    /// </remarks>
    [Fact]
    public void Validate_WithATcpPortOfZero_Fails()
    {
        // Arrange

        // Act
        var result = Validate(static communication => communication with { TcpPort = 0 });

        // Assert
        result.Should().ContainSingle().Which.Should().Be(nameof(LogixCommunication.TcpPort));
    }

    /// <remarks>
    /// A 1756 chassis puts the CPU wherever whoever built it put the CPU, so there is nothing to fall
    /// back on: the message has to say what the property wants rather than offer a guess.
    /// </remarks>
    [Fact]
    public void Validate_ForAControlLogixWithoutACipRoutePath_SaysWhatACipRoutePathIs()
    {
        // Arrange
        var communication = CreateCommunication() with
        {
            DesignId = DeviceNode.ControlLogix5x70DesignId,
            CipRoutePath = null,
        };

        // Act
        var result = _validator.Validate(communication);

        // Assert
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("CIP route path must be the sequence of hops to the CPU, e.g. \"1,0\".");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_ForAControlLogixWithABlankCipRoutePath_Fails(string cipRoutePath)
    {
        // Arrange

        // Act
        var result = Validate(communication => communication with { CipRoutePath = cipRoutePath });

        // Assert
        result.Should().ContainSingle().Which.Should().Be(nameof(LogixCommunication.CipRoutePath));
    }

    /// <remarks>
    /// The CompactLogix node declares no <c>CipRoutePath</c>, so holding its configuration to one would reject
    /// every device an integrator could actually configure under it.
    /// </remarks>
    [Fact]
    public void Validate_ForACompactLogixWithoutACipRoutePath_Passes()
    {
        // Arrange
        var communication = CreateCommunication() with
        {
            DesignId = DeviceNode.CompactLogix5x70DesignId,
            CipRoutePath = null,
        };

        // Act
        var result = _validator.Validate(communication);

        // Assert
        result.IsValid.Should().BeTrue();
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
