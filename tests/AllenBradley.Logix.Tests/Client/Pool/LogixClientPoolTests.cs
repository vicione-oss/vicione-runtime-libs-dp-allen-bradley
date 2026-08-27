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

/// <summary>
/// What the pool owes its callers: one connected client per controller however many ports ask for it,
/// the last release closing it, and a failed or abandoned acquire leaving nothing behind. The client is
/// a substitute, so every test here is about the bookkeeping — no controller, no libplctag.
/// </summary>
/// <remarks>
/// The racing cases are the reason the pool is not a plain dictionary. A second caller arriving while
/// the first is still browsing must wait for that browse rather than start its own, and whatever happens
/// to it next — the connect fails, or its own token is cancelled — must not leave a reference behind on
/// an entry that outlives it. Ported from <c>S7ClientPoolTests</c>, which covers the same contract.
/// </remarks>
public sealed class LogixClientPoolTests : IAsyncDisposable
{
    private readonly ILoggerFactory _loggerFactory = TestLogging.CreateLoggerFactory();
    private readonly LogixClientInformation _clientInformation = CreateClientInformation();
    private readonly ILogixClient _client = Substitute.For<ILogixClient>();
    private readonly LogixClientPool _pool;

    public LogixClientPoolTests()
    {
        var factory = Substitute.For<ILogixClientFactory>();
        factory.Create(Arg.Any<LogixClientInformation>()).Returns(_client);
        _pool = LogixClientPool.CreateTestInstance(_loggerFactory, factory);
    }

    [Fact]
    public async Task AcquireConnectedAsync_CreatesTheClientAndConnectsIt()
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
    public async Task AcquireConnectedAsync_ForTheSameController_ReusesTheClient()
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
        _pool.PooledClients.Count.Should().Be(1);
        _pool.PooledClients[_clientInformation].RefCount.Should().Be(2);
    }

    [Fact]
    public async Task AcquireConnectedAsync_ForDifferentControllers_CreatesSeparateClients()
    {
        // Arrange
        var otherInformation = CreateClientInformation("10.0.0.2");
        var otherClient = Substitute.For<ILogixClient>();
        var factory = Substitute.For<ILogixClientFactory>();
        factory.Create(_clientInformation).Returns(_client);
        factory.Create(otherInformation).Returns(otherClient);
        await using var pool = LogixClientPool.CreateTestInstance(_loggerFactory, factory);

        // Act
        var first = await pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None);
        var second = await pool.AcquireConnectedAsync(otherInformation, CancellationToken.None);

        // Assert
        first.Should().BeSameAs(_client);
        second.Should().BeSameAs(otherClient);
        pool.PooledClients.Count.Should().Be(2);
    }

    [Fact]
    public async Task AcquireConnectedAsync_WhenTheConnectFails_RemovesTheEntryAndDisposesTheClient()
    {
        // Arrange
        _client.ConnectAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new ConnectionFailureException("no route to host"));

        // Act
        var acquire = async () => await _pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None);

        // Assert
        // A half-created entry would poison the controller for every later acquire, and an undisposed
        // client would leak its handles.
        await acquire.Should().ThrowAsync<ConnectionFailureException>();
        _client.Received(1).Dispose();
        _pool.PooledClients.Should().BeEmpty();
    }

    [Fact]
    public async Task AcquireConnectedAsync_AfterAFailedConnect_BuildsAFreshClient()
    {
        // Arrange
        var failing = Substitute.For<ILogixClient>();
        failing.ConnectAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new ConnectionFailureException("no route to host"));
        var succeeding = Substitute.For<ILogixClient>();

        var factory = Substitute.For<ILogixClientFactory>();
        factory.Create(_clientInformation).Returns(failing, succeeding);
        await using var pool = LogixClientPool.CreateTestInstance(_loggerFactory, factory);

        var firstAcquire = async () => await pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None);
        await firstAcquire.Should().ThrowAsync<ConnectionFailureException>();

        // Act
        var client = await pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None);

        // Assert
        // A controller that was unreachable a moment ago is not unreachable forever, and the disposed
        // client cannot be reconnected — so a retry has to get a new one.
        client.Should().BeSameAs(succeeding);
        failing.Received(1).Dispose();
        await succeeding.Received(1).ConnectAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AcquireConnectedAsync_WhenDisposed_Throws()
    {
        // Arrange
        await _pool.DisposeAsync();

        // Act
        var acquire = async () => await _pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None);

        // Assert
        await acquire.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task ReleaseAsync_OfTheLastReference_DisconnectsAndDisposesTheClient()
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
    public async Task ReleaseAsync_WithAReferenceRemaining_KeepsTheClientConnected()
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
    public async Task ReleaseAsync_OfAnUnknownOrAlreadyReleasedController_DoesNothing()
    {
        // Arrange — acquire once and release twice, then release a controller never acquired at all
        await _pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None);
        await _pool.ReleaseAsync(_clientInformation, CancellationToken.None);

        // Act
        var release = async () =>
        {
            await _pool.ReleaseAsync(_clientInformation, CancellationToken.None);
            await _pool.ReleaseAsync(CreateClientInformation("10.0.0.9"), CancellationToken.None);
        };

        // Assert
        // Release is teardown, and teardown that throws takes the rest of a shutdown with it.
        await release.Should().NotThrowAsync();
        _client.Received(1).Dispose();
    }

    [Fact]
    public async Task ReleaseAsync_WhenTheDisconnectFails_StillDisposesTheClient()
    {
        // Arrange
        await _pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None);
        _client.DisconnectAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("disconnect failed"));

        // Act
        var release = async () => await _pool.ReleaseAsync(_clientInformation, CancellationToken.None);

        // Assert
        // A failed disconnect is exactly when the handles most need freeing: one left to its finalizer
        // fail-fasts the process at CLR teardown.
        await release.Should().NotThrowAsync();
        _client.Received(1).Dispose();
    }

    [Fact]
    public async Task DisposeAsync_TearsDownEveryPooledClientAndIsIdempotent()
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
    public async Task ConcurrentAcquire_ForOneController_AllWaitOnTheFirstConnect()
    {
        // Arrange — the connect blocks until the test releases it
        var connectStarted = new TaskCompletionSource();
        var connectGate = new TaskCompletionSource();
        _client.ConnectAsync(Arg.Any<CancellationToken>()).Returns(async _ =>
        {
            connectStarted.SetResult();
            await connectGate.Task;
        });

        // Act
        var first = _pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None).AsTask();
        await connectStarted.Task;
        var second = _pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None).AsTask();
        await Task.Delay(50, TestContext.Current.CancellationToken);
        var secondCompletedEarly = second.IsCompleted;

        connectGate.SetResult();

        // Assert
        // Handing the second caller a client that is still connecting would let it read against a schema
        // that is not there yet — the gate is what makes "acquire" mean "connected".
        secondCompletedEarly.Should().BeFalse();
        (await first).Should().BeSameAs(_client);
        (await second).Should().BeSameAs(_client);
        await _client.Received(1).ConnectAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConcurrentAcquire_WhenTheConnectFails_FailsEveryWaiter()
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
        connectGate.SetException(new ConnectionFailureException("no route to host"));

        // Assert
        // A waiter that received a client from a connect that failed would be reading against nothing.
        await ((Func<Task>)(() => first)).Should().ThrowAsync<ConnectionFailureException>();
        await ((Func<Task>)(() => second)).Should().ThrowAsync<ConnectionFailureException>();
        _client.Received(1).Dispose();
        _pool.PooledClients.Should().BeEmpty();
    }

    [Fact]
    public async Task AcquireConnectedAsync_WhenAWaiterIsCancelled_DoesNotStrandItsReference()
    {
        // Arrange
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
        await Task.Delay(50, TestContext.Current.CancellationToken);

        // Act
        await waiterCts.CancelAsync();
        await ((Func<Task>)(() => waiter)).Should().ThrowAsync<OperationCanceledException>();
        connectGate.SetResult();
        await creator;

        await _pool.ReleaseAsync(_clientInformation, CancellationToken.None);

        // Assert
        // The reference is taken before the wait, so a waiter that walks away has to give it back —
        // otherwise the creator's release never reaches zero and the connection is held forever.
        await _client.Received(1).DisconnectAsync(Arg.Any<CancellationToken>());
        _client.Received(1).Dispose();
        _pool.PooledClients.Should().BeEmpty();
    }

    [Fact]
    public async Task DisposeAsync_WhileAConnectIsInFlight_TurnsTheAcquireAwayAndDisposesTheClientOnce()
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

        // Assert
        // The dispose took the entry while the browse was still running, so the client that connect
        // finally produced belongs to nobody. Handing it over would give the caller one already
        // disposed; releasing it a second time on the way out would free a native handle twice.
        await ((Func<Task>)(() => acquire)).Should().ThrowAsync<ObjectDisposedException>();
        _client.Received(1).Dispose();
    }

    [Fact]
    public async Task DisposeAsync_DoesNotWaitForAConnectStillInFlight()
    {
        // Arrange — this connect never completes
        var connectStarted = new TaskCompletionSource();
        _client.ConnectAsync(Arg.Any<CancellationToken>()).Returns(async _ =>
        {
            connectStarted.SetResult();
            await new TaskCompletionSource().Task;
        });

        var acquire = _pool.AcquireConnectedAsync(_clientInformation, CancellationToken.None).AsTask();
        await connectStarted.Task;

        // Act
        var dispose = async () => await _pool.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        // A browse that will not answer is exactly the case a shutdown must not hang on — the handles
        // are freed regardless, and the caller is turned away when its connect eventually settles.
        await dispose.Should().NotThrowAsync();
        _client.Received(1).Dispose();
        acquire.IsCompleted.Should().BeFalse();
    }

    public async ValueTask DisposeAsync()
    {
        await _pool.DisposeAsync();
        _loggerFactory.Dispose();
        _client.Dispose();
    }
}
