using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags;

/// <summary>
/// That a shared access runs one operation at a time. A cached access is reachable from the read and the
/// write path at once, and libplctag's single per-handle buffer cannot serve both.
/// </summary>
public class SynchronizedLogixTagAccessTests
{
    [Fact]
    public async Task WriteAsync_WhileAReadIsInFlight_WaitsForIt()
    {
        // Arrange
        var inner = new BlockingTagAccess();
        using var access = new SynchronizedLogixTagAccess(inner);
        var read = access.ReadAsync(CancellationToken.None);
        await inner.Entered;

        // Act
        var write = access.WriteAsync([1, 2, 3, 4], CancellationToken.None);

        // Assert
        // Checked before releasing, and so decided rather than raced: the inner access runs synchronously
        // up to its own block, so an ungated write is already inside by now and the mark reads 2. Without
        // the gate that write would be calling SetBuffer on the handle this read is still using.
        inner.MaxConcurrent.Should().Be(1);

        inner.Release();
        await Task.WhenAll(read, write);
    }

    [Fact]
    public async Task ReadAsync_WhenManyCallersShareTheAccess_NeverOverlap()
    {
        // Arrange
        var inner = new BlockingTagAccess();
        using var access = new SynchronizedLogixTagAccess(inner);

        // Act
        // A group naming one tag several times draws the same access for each entry, then reads them all
        // concurrently. Materialised, so all eight are started before anything is asserted.
        var reads = Enumerable.Range(0, 8)
            .Select(_ => access.ReadAsync(CancellationToken.None))
            .ToArray();
        await inner.Entered;

        // Assert
        inner.MaxConcurrent.Should().Be(1);

        inner.Release();
        await Task.WhenAll(reads);
    }

    [Fact]
    public async Task ReadAsync_ReleasesTheGate_SoLaterOperationsProceed()
    {
        // Arrange
        var inner = new BlockingTagAccess();
        using var access = new SynchronizedLogixTagAccess(inner);
        inner.Release();

        // Act
        // A gate that is taken and never released would deadlock the second read, not fail it.
        await access.ReadAsync(CancellationToken.None);
        var result = await access.ReadAsync(CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Dispose_DisposesTheInnerAccess()
    {
        // Arrange
        var inner = new BlockingTagAccess();
        var access = new SynchronizedLogixTagAccess(inner);

        // Act
        access.Dispose();

        // Assert
        inner.IsDisposed.Should().BeTrue();
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
