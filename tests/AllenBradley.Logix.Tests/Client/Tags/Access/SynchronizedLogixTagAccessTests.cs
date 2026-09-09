using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Access;

public sealed class SynchronizedLogixTagAccessTests : IDisposable
{
    private readonly BlockingTagAccess _handle = new();
    private readonly SynchronizedLogixTagAccess _access;

    public SynchronizedLogixTagAccessTests() => _access = new SynchronizedLogixTagAccess(_handle);

    [Fact]
    public async Task AWriteStaysOutOfTheHandleWhileAReadIsInsideIt()
    {
        // Arrange
        var read = _access.ReadAsync(CancellationToken.None);
        await _handle.Entered;

        // Act
        // The mark is taken before the gate is released, so the answer is decided rather than raced: the
        // handle runs synchronously up to its own block, and an ungated write would already be inside.
        var write = _access.WriteAsync([1, 2, 3, 4], CancellationToken.None);
        var callersInsideTheHandle = _handle.MaxConcurrent;
        _handle.Release();
        await Task.WhenAll(read, write);

        // Assert
        callersInsideTheHandle.Should().Be(1);
    }

    [Fact]
    public async Task ManyCallersSharingOneAccessEnterTheHandleOneAtATime()
    {
        // Arrange
        // A group naming one tag several times draws the same access for each entry.
        var reads = Enumerable.Range(0, 8)
            .Select(_ => _access.ReadAsync(CancellationToken.None))
            .ToArray();
        await _handle.Entered;

        // Act
        var callersInsideTheHandle = _handle.MaxConcurrent;
        _handle.Release();
        await Task.WhenAll(reads);

        // Assert
        callersInsideTheHandle.Should().Be(1);
    }

    [Fact]
    public async Task AGateTakenByOneReadIsReleasedForTheNextOne()
    {
        // Arrange
        // A gate taken and never released would deadlock the second read rather than fail it.
        _handle.Release();
        await _access.ReadAsync(CancellationToken.None);

        // Act
        var secondRead = await _access.ReadAsync(CancellationToken.None);

        // Assert
        secondRead.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void DisposingTheGateDisposesTheHandleItWraps()
    {
        // Arrange

        // Act
        _access.Dispose();

        // Assert
        _handle.IsDisposed.Should().BeTrue();
    }

    public void Dispose()
    {
        _handle.Release();
        _access.Dispose();
    }

    // Blocks inside every operation until Release, so overlap is observable rather than a matter of
    // timing: it records the high-water mark of concurrent callers.
    private sealed class BlockingTagAccess : ILogixTagAccess
    {
        private readonly TaskCompletionSource _released = new();
        private readonly TaskCompletionSource _entered = new();
        private int _inFlight;

        public Task Entered => _entered.Task;

        public int MaxConcurrent { get; private set; }

        public bool IsDisposed { get; private set; }

        public void Release() => _released.TrySetResult();

        public async Task<LogixTagReadResult> ReadAsync(CancellationToken cancellationToken)
        {
            await RunAsync().ConfigureAwait(false);
            return LogixTagReadResult.Ok(ReadOnlyMemory<byte>.Empty);
        }

        public async Task<LogixTagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            await RunAsync().ConfigureAwait(false);
            return LogixTagWriteResult.Ok();
        }

        public void Dispose() => IsDisposed = true;

        private async Task RunAsync()
        {
            MaxConcurrent = Math.Max(MaxConcurrent, Interlocked.Increment(ref _inFlight));
            _entered.TrySetResult();
            await _released.Task.ConfigureAwait(false);
            Interlocked.Decrement(ref _inFlight);
        }
    }
}
