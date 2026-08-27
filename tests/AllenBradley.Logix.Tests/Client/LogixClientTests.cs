using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client;

/// <summary>
/// How the client handles a controller that answers badly, driven through a fake
/// <see cref="ILogixTagManager"/> that hands back one <see cref="ILogixTag"/> per
/// data point. Read and write diverge here by design: a failed read degrades its own data point, a failed
/// write throws, because <c>IWriteClient.WriteAsync</c> gives the caller no other way to learn a tag was
/// dropped. The tag now carries the controller's metadata, so the type-code gate is exercised too.
/// </summary>
public class LogixClientTests
{
    private static readonly DIntDataPoint Speed = new(new TagName("Motor.Speed"));
    private static readonly DIntDataPoint Level = new(new TagName("Tank.Level"));

    // Metadata the controller would report for a DINT tag — matches the DINT converter, so the type gate
    // lets the read/write through and only the device outcome decides the result.
    private static LogixTypeDeclaration DintMetadata(string tagName) =>
        new(new TagName(tagName), LogixTypeKind.Atomic, CipType.Dint, new DimensionCount(0), new ElementCount(1), new ElementLength(4));

    private static LogixTypeDeclaration RealMetadata(string tagName) =>
        new(new TagName(tagName), LogixTypeKind.Atomic, CipType.Real, new DimensionCount(0), new ElementCount(1), new ElementLength(4));

    // 42 as a DINT on the wire. Spelled out rather than taken from BitConverter, which would re-derive
    // it through the same host-endianness assumption the converter makes and so agree by construction.
    private static readonly byte[] FortyTwoAsDint = [42, 0, 0, 0];

    [Fact]
    public async Task ReadAsync_WhenOneTagFails_DegradesThatPointAndKeepsTheRest()
    {
        // Arrange
        var tagManager = new FakeTagManager
        {
            [Speed] = FakeTag.Reading(Speed, DintMetadata("Motor.Speed"), LogixTagReadResult.Ok(FortyTwoAsDint)),
            [Level] = FakeTag.Reading(Level, DintMetadata("Tank.Level"), LogixTagReadResult.Failed("tag not found")),
        };
        var client = new LogixClient(tagManager);

        // Act
        var values = await client.ReadAsync([Speed, Level], CancellationToken.None);

        // Assert
        // Per-tag partial failure: the bad tag must not sink the group (ADR-004).
        values.Should().HaveCount(2);
        values[0].Quality.Should().Be(LogixQuality.Good);
        values[0].Value.Should().Be(42);
        values[1].Quality.Should().Be(LogixQuality.Bad);
    }

    [Fact]
    public async Task ReadAsync_WhenTheControllerTypeDisagrees_DegradesThatPoint()
    {
        // Arrange
        // The read itself succeeds, but the controller reports REAL where a DINT is configured — decoding
        // those four bytes as an int would invent a plausible, wrong value. The type-code gate stops it.
        var tagManager = new FakeTagManager
        {
            [Speed] = FakeTag.Reading(Speed, RealMetadata("Motor.Speed"), LogixTagReadResult.Ok(FortyTwoAsDint)),
        };
        var client = new LogixClient(tagManager);

        // Act
        var values = await client.ReadAsync([Speed], CancellationToken.None);

        // Assert
        values.Should().ContainSingle().Which.Quality.Should().Be(LogixQuality.Bad);
    }

    [Fact]
    public async Task ReadAsync_WhenTheBufferIsShorterThanTheType_DegradesThatPoint()
    {
        // Arrange
        // A DINT needs 4 bytes; decoding 2 would read past the buffer or silently invent a value. The type
        // matches, so this is the byte-size backstop doing its job.
        var tagManager = new FakeTagManager
        {
            [Speed] = FakeTag.Reading(Speed, DintMetadata("Motor.Speed"), LogixTagReadResult.Ok(new byte[2])),
        };
        var client = new LogixClient(tagManager);

        // Act
        var values = await client.ReadAsync([Speed], CancellationToken.None);

        // Assert
        values.Should().ContainSingle().Which.Quality.Should().Be(LogixQuality.Bad);
    }

    [Fact]
    public async Task ReadAsync_WhenADataPointHasNoConverter_ThrowsBeforeReadingAnything()
    {
        // Arrange
        // Both points have a tag, so the only thing left that can throw is the missing converter.
        var speedTag = FakeTag.Reading(Speed, DintMetadata("Motor.Speed"), LogixTagReadResult.Ok(FortyTwoAsDint));
        var unconvertible = new UnregisteredDataPoint();
        var tagManager = new FakeTagManager
        {
            [Speed] = speedTag,
            [unconvertible] = FakeTag.Reading(unconvertible, metadata: null, LogixTagReadResult.Ok(FortyTwoAsDint)),
        };
        var client = new LogixClient(tagManager);

        // Act
        var read = async () => await client.ReadAsync(
            [Speed, unconvertible], CancellationToken.None);

        // Assert
        // The group resolves whole before any I/O, so a data point wired up without a converter is a
        // configuration error that costs no round trip and leaves no half-read group behind.
        await read.Should().ThrowAsync<InvalidOperationException>();
        speedTag.WasRead.Should().BeFalse();
    }

    [Fact]
    public async Task WriteAsync_WhenTheTagFails_ThrowsCarryingTheTagAndTheReason()
    {
        // Arrange
        var tagManager = new FakeTagManager
        {
            [Speed] = FakeTag.Writing(Speed, DintMetadata("Motor.Speed"), LogixTagWriteResult.Failed("tag is read-only")),
        };
        var client = new LogixClient(tagManager);
        var value = new LogixDataPointValue<int>(Speed, 42, LogixQuality.Good);

        // Act
        var write = async () => await client.WriteAsync([value], CancellationToken.None);

        // Assert
        // Silence here would be a dropped write the caller cannot detect.
        (await write.Should().ThrowAsync<LogixTagException>())
            .Which.Message.Should().Contain("Motor.Speed").And.Contain("tag is read-only");
    }

    [Fact]
    public async Task WriteAsync_WhenTheControllerTypeDisagrees_ThrowsWithoutWriting()
    {
        // Arrange
        // Configured DINT, controller reports REAL: encoding the DINT bytes onto the REAL tag would corrupt
        // it, so the write must fail by name before any bytes reach the device.
        var tag = FakeTag.Writing(Speed, RealMetadata("Motor.Speed"), LogixTagWriteResult.Ok());
        var tagManager = new FakeTagManager { [Speed] = tag };
        var client = new LogixClient(tagManager);
        var value = new LogixDataPointValue<int>(Speed, 42, LogixQuality.Good);

        // Act
        var write = async () => await client.WriteAsync([value], CancellationToken.None);

        // Assert
        (await write.Should().ThrowAsync<LogixTagException>())
            .Which.Message.Should().Contain("Motor.Speed");
        tag.Written.Should().BeNull("the type gate must stop the write before it reaches the device");
    }

    [Fact]
    public async Task WriteAsync_WhenSeveralTagsFail_ThrowsNamingEveryOne()
    {
        // Arrange
        var tagManager = new FakeTagManager
        {
            [Speed] = FakeTag.Writing(Speed, DintMetadata("Motor.Speed"), LogixTagWriteResult.Failed("tag is read-only")),
            [Level] = FakeTag.Writing(Level, DintMetadata("Tank.Level"), LogixTagWriteResult.Failed("tag not found")),
        };
        var client = new LogixClient(tagManager);
        ILogixDataPointValue[] values =
        [
            new LogixDataPointValue<int>(Speed, 42, LogixQuality.Good),
            new LogixDataPointValue<int>(Level, 7, LogixQuality.Good),
        ];

        // Act
        var write = async () => await client.WriteAsync(values, CancellationToken.None);

        // Assert
        // A failing tag does not stop its siblings, so reporting only the first would leave the caller
        // re-driving the wrong set.
        var message = (await write.Should().ThrowAsync<LogixTagException>()).Which.Message;
        message.Should().Contain("Motor.Speed").And.Contain("tag is read-only");
        message.Should().Contain("Tank.Level").And.Contain("tag not found");
    }

    [Fact]
    public async Task WriteAsync_EncodesTheValueOntoTheTag()
    {
        // Arrange
        var tag = FakeTag.Writing(Speed, DintMetadata("Motor.Speed"), LogixTagWriteResult.Ok());
        var tagManager = new FakeTagManager { [Speed] = tag };
        var client = new LogixClient(tagManager);
        var value = new LogixDataPointValue<int>(Speed, 42, LogixQuality.Good);

        // Act
        await client.WriteAsync([value], CancellationToken.None);

        // Assert
        tag.Written.Should().Equal(FortyTwoAsDint);
    }

    // A data point shape deliberately absent from DataPointConverterRegistry: the model gaining a type
    // that nobody wired a converter for.
    private sealed record UnregisteredDataPoint : ILogixDataPoint
    {
        public TagName TagName => new("Mystery.Tag");
    }

    private sealed class FakeTagManager : ILogixTagManager
    {
        private readonly Dictionary<ILogixDataPoint, FakeTag> _tagByDataPoint = [];

        public FakeTag this[ILogixDataPoint dataPoint]
        {
            set => _tagByDataPoint[dataPoint] = value;
        }

        public Task LoadSchemaAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public ILogixTag TagFor(ILogixDataPoint dataPoint) => _tagByDataPoint[dataPoint];
    }

    private sealed class FakeTag : ILogixTag
    {
        private LogixTagReadResult _readResult = LogixTagReadResult.Ok(ReadOnlyMemory<byte>.Empty);
        private LogixTagWriteResult _writeResult = LogixTagWriteResult.Ok();

        private FakeTag(ILogixDataPoint dataPoint, LogixTypeDeclaration? metadata)
        {
            DataPoint = dataPoint;
            Metadata = metadata;
        }

        public ILogixDataPoint DataPoint { get; }

        public LogixTypeDeclaration? Metadata { get; }

        public bool WasRead { get; private set; }

        public byte[]? Written { get; private set; }

        public static FakeTag Reading(
            ILogixDataPoint dataPoint, LogixTypeDeclaration? metadata, LogixTagReadResult result) =>
            new(dataPoint, metadata) { _readResult = result };

        public static FakeTag Writing(
            ILogixDataPoint dataPoint, LogixTypeDeclaration? metadata, LogixTagWriteResult result) =>
            new(dataPoint, metadata) { _writeResult = result };

        public Task<LogixTagReadResult> ReadAsync(CancellationToken cancellationToken)
        {
            WasRead = true;
            return Task.FromResult(_readResult);
        }

        public Task<LogixTagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            Written = buffer;
            return Task.FromResult(_writeResult);
        }

        public void Dispose()
        {
        }
    }
}
