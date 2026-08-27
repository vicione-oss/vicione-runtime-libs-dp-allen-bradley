using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Lifetime;

/// <summary>
/// <see cref="LogixTag"/> is the join: it surfaces the data point and its metadata, and passes
/// every exchange straight through to the inner handle it wraps. These run with no controller — the inner
/// handle is a fake — because the whole point of the wrapper is that it adds only data, not exchange logic.
/// </summary>
public class LogixTagTests
{
    private static readonly DIntDataPoint Speed = new(new TagName("Motor.Speed"));

    private static readonly LogixTypeDeclaration DintMetadata =
        new(new TagName("Motor.Speed"), LogixTypeKind.Atomic, CipType.Dint, new DimensionCount(0), new ElementCount(1), new ElementLength(4));

    [Fact]
    public void DataPoint_And_Metadata_AreSurfacedAsGiven()
    {
        using var tag = new LogixTag(Speed, DintMetadata, new FakeTagAccess());

        tag.DataPoint.Should().BeSameAs(Speed);
        tag.Metadata.Should().Be(DintMetadata);
    }

    [Fact]
    public void Metadata_IsNull_WhenTheTagIsAbsentFromTheController()
    {
        using var tag = new LogixTag(Speed, metadata: null, new FakeTagAccess());

        tag.Metadata.Should().BeNull();
    }

    [Fact]
    public async Task ReadAsync_DelegatesToTheInnerAccess()
    {
        var inner = new FakeTagAccess { ReadResult = LogixTagReadResult.Ok(new byte[] { 42, 0, 0, 0 }) };
        using var tag = new LogixTag(Speed, DintMetadata, inner);

        var result = await tag.ReadAsync(TestContext.Current.CancellationToken);

        inner.ReadCount.Should().Be(1);
        result.Buffer.ToArray().Should().Equal(42, 0, 0, 0);
    }

    [Fact]
    public async Task WriteAsync_DelegatesTheBufferToTheInnerAccess()
    {
        var inner = new FakeTagAccess();
        using var tag = new LogixTag(Speed, DintMetadata, inner);

        await tag.WriteAsync([1, 2, 3, 4], TestContext.Current.CancellationToken);

        inner.Written.Should().Equal(1, 2, 3, 4);
    }

    [Fact]
    public void Dispose_DisposesTheInnerHandleOnce()
    {
        var inner = new FakeTagAccess();
        var tag = new LogixTag(Speed, DintMetadata, inner);

        tag.Dispose();

        // A libplctag handle left unfreed fail-fasts the process (0xC0000602); the wrapper must not swallow
        // the owner's dispose.
        inner.DisposeCount.Should().Be(1);
    }

    private sealed class FakeTagAccess : ILogixTagAccess
    {
        public LogixTagReadResult ReadResult { get; init; } = LogixTagReadResult.Ok(ReadOnlyMemory<byte>.Empty);

        public int ReadCount { get; private set; }

        public byte[]? Written { get; private set; }

        public int DisposeCount { get; private set; }

        public Task<LogixTagReadResult> ReadAsync(CancellationToken cancellationToken)
        {
            ReadCount++;
            return Task.FromResult(ReadResult);
        }

        public Task<LogixTagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            Written = buffer;
            return Task.FromResult(LogixTagWriteResult.Ok());
        }

        public void Dispose() => DisposeCount++;
    }
}
