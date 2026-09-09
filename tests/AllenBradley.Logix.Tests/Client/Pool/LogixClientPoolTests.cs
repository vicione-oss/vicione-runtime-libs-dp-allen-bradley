using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Pool;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using ViciOne.Suite.DataPort.Extensions.Testing.Logging;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixClientTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Pool;

public sealed class LogixClientPoolTests : IAsyncDisposable
{
    private const string NoRouteToHost = "no route to host";

    private readonly ILoggerFactory _loggerFactory = TestLogging.CreateLoggerFactory();
    private readonly LogixClientInformation _clientInformation = DefaultClientInformation();
    private readonly ILogixClient _client = Substitute.For<ILogixClient>();
    private readonly LogixClientPool _pool;

    public LogixClientPoolTests()
    {
        var factory = Substitute.For<ILogixClientFactory>();
        factory.Create(Arg.Any<LogixClientInformation>()).Returns(_client);
        _pool = LogixClientPool.CreateTestInstance(_loggerFactory, factory);
    }

    [Fact]
    public async Task AcquiringAControllerConnectsItsClientAndTakesAReference()
    {
        // Arrange

        // Act
        var client = await _pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None);

        // Assert
        client.Should().BeSameAs(_client);
        await _client.Received(1).ConnectAsync(CancellationToken.None);
        _pool.PooledClients[_clientInformation].RefCount.Should().Be(1);
    }

    [Fact]
    public async Task TwoAcquiresOfOneControllerShareOneConnectedClient()
    {
        // Arrange
        await _pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None);

        // Act
        var second = await _pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None);

        // Assert
        // The incoming and outgoing ports for one controller are separate acquires, and sharing the one
        // session between them is the point of the pool.
        second.Should().BeSameAs(_client);
        await _client.Received(1).ConnectAsync(Arg.Any<CancellationToken>());
        _pool.PooledClients[_clientInformation].RefCount.Should().Be(2);
    }

    [Fact]
    public async Task AcquiresOfDifferentControllersGetTheirOwnClients()
    {
        // Arrange
        var otherInformation = _clientInformation with { ConnectionEndpoint = new ConnectionEndpoint("10.0.0.2") };
        var otherClient = Substitute.For<ILogixClient>();
        var factory = Substitute.For<ILogixClientFactory>();
        factory.Create(_clientInformation).Returns(_client);
        factory.Create(otherInformation).Returns(otherClient);
        await using var pool = LogixClientPool.CreateTestInstance(_loggerFactory, factory);
        await pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None);

        // Act
        var second = await pool.AcquireConnectedAsync(otherInformation, CancellationToken.None);

        // Assert
        second.Should().BeSameAs(otherClient);
        pool.PooledClients.Count.Should().Be(2);
    }

    [Fact]
    public async Task AControllerWhoseConnectFailedLeavesNoEntryAndNoUndisposedClient()
    {
        // Arrange
        _client.ConnectAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new ConnectionFailureException(NoRouteToHost));

        // Act
        var acquire = await Record.ExceptionAsync(
            () => _pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None).AsTask());

        // Assert
        // A half-created entry would poison the controller for every later acquire, and an undisposed
        // client would leak its handles.
        acquire.Should().BeOfType<ConnectionFailureException>();
        _client.Received(1).Dispose();
        _pool.PooledClients.Should().BeEmpty();
    }

    [Fact]
    public async Task AnAcquireAfterAFailedConnectBuildsAFreshClient()
    {
        // Arrange
        // The disposed client cannot be reconnected, and a controller unreachable a moment ago is not
        // unreachable forever.
        var failing = Substitute.For<ILogixClient>();
        failing.ConnectAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new ConnectionFailureException(NoRouteToHost));
        var succeeding = Substitute.For<ILogixClient>();
        var factory = Substitute.For<ILogixClientFactory>();
        factory.Create(_clientInformation).Returns(failing, succeeding);
        await using var pool = LogixClientPool.CreateTestInstance(_loggerFactory, factory);

        // ReSharper disable once AccessToDisposedClosure — Record invokes it before it returns
        _ = await Record.ExceptionAsync(
            () => pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None).AsTask());

        // Act
        var client = await pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None);

        // Assert
        client.Should().BeSameAs(succeeding);
        failing.Received(1).Dispose();
        await succeeding.Received(1).ConnectAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AcquiringFromADisposedPoolIsRefused()
    {
        // Arrange
        await _pool.DisposeAsync();

        // Act
        var acquiring = _pool.Awaiting(p => p.AcquireConnectedAsync(_clientInformation, CancellationToken.None));

        // Assert
        await acquiring.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task ReleasingTheLastReferenceDisconnectsAndDisposesTheClient()
    {
        // Arrange
        await _pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None);

        // Act
        await _pool.ReleaseAsync(_clientInformation, CancellationToken.None);

        // Assert
        await _client.Received(1).DisconnectAsync(Arg.Any<CancellationToken>());
        _client.Received(1).Dispose();
        _pool.PooledClients.Should().BeEmpty();
    }

    [Fact]
    public async Task AReleaseWithAReferenceRemainingKeepsTheClientConnected()
    {
        // Arrange
        await _pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None);
        await _pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None);

        // Act
        await _pool.ReleaseAsync(_clientInformation, CancellationToken.None);

        // Assert
        // One port shutting down must not pull the connection out from under the other.
        await _client.DidNotReceive().DisconnectAsync(Arg.Any<CancellationToken>());
        _client.DidNotReceive().Dispose();
        _pool.PooledClients[_clientInformation].RefCount.Should().Be(1);
    }

    [Fact]
    public async Task ReleasingAnAlreadyReleasedControllerDoesNothing()
    {
        // Arrange
        await _pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None);
        await _pool.ReleaseAsync(_clientInformation, CancellationToken.None);

        // Act
        var releasing = _pool.Awaiting(p => p.ReleaseAsync(_clientInformation, CancellationToken.None));

        // Assert
        // Release is teardown, and teardown that throws takes the rest of a shutdown with it.
        await releasing.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ReleasingAControllerThatWasNeverAcquiredDoesNothing()
    {
        // Arrange
        var neverAcquired = _clientInformation with { ConnectionEndpoint = new ConnectionEndpoint("10.0.0.2") };

        // Act
        var releasing = _pool.Awaiting(p => p.ReleaseAsync(neverAcquired, CancellationToken.None));

        // Assert
        await releasing.Should().NotThrowAsync();
    }

    [Fact]
    public async Task AClientWhoseDisconnectFailedIsStillDisposed()
    {
        // Arrange
        await _pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None);
        _client.DisconnectAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("disconnect failed"));

        // Act
        var release = await Record.ExceptionAsync(
            () => _pool.ReleaseAsync(_clientInformation, CancellationToken.None).AsTask());

        // Assert
        // A failed disconnect is exactly when the handles most need freeing: one left to its finalizer
        // fail-fasts the process at CLR teardown.
        release.Should().BeNull();
        _client.Received(1).Dispose();
    }

    [Fact]
    public async Task APoolDisposedTwiceTearsDownEachPooledClientOnce()
    {
        // Arrange
        await _pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None);

        // Act
        await _pool.DisposeAsync();
        await _pool.DisposeAsync();

        // Assert
        await _client.Received(1).DisconnectAsync(Arg.Any<CancellationToken>());
        _client.Received(1).Dispose();
    }

    [Fact]
    public async Task ASecondAcquireWaitsForTheConnectTheFirstOneStarted()
    {
        // Arrange
        // The connect blocks until the test releases it.
        var connectStarted = new TaskCompletionSource();
        var connectGate = new TaskCompletionSource();
        _client.ConnectAsync(Arg.Any<CancellationToken>()).Returns(async _ =>
        {
            connectStarted.SetResult();
            await connectGate.Task;
        });

        // Act
        // A bounded wait is the only way to observe that the second acquire has not completed yet.
        var first = _pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None).AsTask();
        await connectStarted.Task;
        var second = _pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None).AsTask();
        await Task.Delay(50, TestContext.Current.CancellationToken);
        var secondCompletedEarly = second.IsCompleted;
        connectGate.SetResult();

        // Assert
        // Handing the second caller a client that is still connecting would let it read against a schema
        // that is not there yet.
        secondCompletedEarly.Should().BeFalse();
        (await first).Should().BeSameAs(_client);
        (await second).Should().BeSameAs(_client);
        await _client.Received(1).ConnectAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AConnectThatFailsFailsEveryAcquireWaitingOnIt()
    {
        // Arrange
        var connectStarted = new TaskCompletionSource();
        var connectGate = new TaskCompletionSource();
        _client.ConnectAsync(Arg.Any<CancellationToken>()).Returns(async _ =>
        {
            connectStarted.SetResult();
            await connectGate.Task;
        });
        var first = _pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None).AsTask();
        await connectStarted.Task;
        var second = _pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None).AsTask();

        // Act
        connectGate.SetException(new ConnectionFailureException(NoRouteToHost));
        var firstFault = await Record.ExceptionAsync(() => first);
        var secondFault = await Record.ExceptionAsync(() => second);

        // Assert
        // A waiter that received a client from a connect that failed would be reading against nothing.
        firstFault.Should().BeOfType<ConnectionFailureException>();
        secondFault.Should().BeOfType<ConnectionFailureException>();
        _client.Received(1).Dispose();
        _pool.PooledClients.Should().BeEmpty();
    }

    [Fact]
    public async Task ACancelledWaiterGivesBackTheReferenceItTook()
    {
        // Arrange
        // The reference is taken before the wait, so the creator's release below is the only one left.
        var connectStarted = new TaskCompletionSource();
        var connectGate = new TaskCompletionSource();
        _client.ConnectAsync(Arg.Any<CancellationToken>()).Returns(async _ =>
        {
            connectStarted.SetResult();
            await connectGate.Task;
        });
        using var waiterCts = new CancellationTokenSource();
        var creator = _pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None).AsTask();
        await connectStarted.Task;
        var waiter = _pool.AcquireConnectedAsync(_clientInformation, waiterCts.Token).AsTask();

        // Act
        await waiterCts.CancelAsync();
        var waiterFault = await Record.ExceptionAsync(() => waiter);
        connectGate.SetResult();
        await creator;
        await _pool.ReleaseAsync(_clientInformation, CancellationToken.None);

        // Assert
        // A stranded reference would keep the creator's release from ever reaching zero.
        waiterFault.Should().BeAssignableTo<OperationCanceledException>();
        await _client.Received(1).DisconnectAsync(Arg.Any<CancellationToken>());
        _client.Received(1).Dispose();
        _pool.PooledClients.Should().BeEmpty();
    }

    [Fact]
    public async Task APoolDisposedDuringAConnectTurnsTheAcquireAwayAndDisposesTheClientOnce()
    {
        // Arrange
        var connectStarted = new TaskCompletionSource();
        var connectGate = new TaskCompletionSource();
        _client.ConnectAsync(Arg.Any<CancellationToken>()).Returns(async _ =>
        {
            connectStarted.SetResult();
            await connectGate.Task;
        });
        var acquire = _pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None).AsTask();
        await connectStarted.Task;

        // Act
        await _pool.DisposeAsync();
        connectGate.SetResult();
        var acquireFault = await Record.ExceptionAsync(() => acquire);

        // Assert
        // The client the connect finally produced belongs to nobody: handing it over would give the caller
        // one already disposed, and releasing it again would free a native handle twice.
        acquireFault.Should().BeOfType<ObjectDisposedException>();
        _client.Received(1).Dispose();
    }

    [Fact]
    public async Task APoolDisposedDuringAConnectDoesNotWaitForItToFinish()
    {
        // Arrange
        // This connect never completes, which is the case a shutdown must not hang on.
        var connectStarted = new TaskCompletionSource();
        _client.ConnectAsync(Arg.Any<CancellationToken>()).Returns(async _ =>
        {
            connectStarted.SetResult();
            await new TaskCompletionSource().Task;
        });
        var acquire = _pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None).AsTask();
        await connectStarted.Task;

        // Act
        var dispose = await Record.ExceptionAsync(
            () => _pool.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5)));

        // Assert
        dispose.Should().BeNull();
        _client.Received(1).Dispose();
        acquire.IsCompleted.Should().BeFalse();
    }

    public async ValueTask DisposeAsync()
    {
        await _pool.DisposeAsync();
        _client.Dispose();
        _loggerFactory.Dispose();
    }
}
