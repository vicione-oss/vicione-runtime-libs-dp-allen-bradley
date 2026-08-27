using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

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
    private static readonly DIntDataPoint Speed = CreateDInt("Motor.Speed");
    private static readonly DIntDataPoint Level = CreateDInt("Tank.Level");
    private static readonly StringDataPoint Label = CreateString("Line.Label");

    // Metadata the controller would report for a DINT tag — matches the DINT converter, so the type gate
    // lets the read/write through and only the device outcome decides the result.
    private static TagDefinition DintMetadata(string tagName) =>
        new(new TagName(tagName), LogixTypeKind.Atomic, AllenBradleyDataType.Dint, MaxLength: null, new DimensionCount(0), new ElementCount(1));

    // A built-in STRING as the listing reports it: a structure of .DATA[82] behind its .LEN.
    private static TagDefinition StringMetadata(string tagName) =>
        new(new TagName(tagName), LogixTypeKind.Structure, AllenBradleyDataType.String, StringMaxLength.Standard, new DimensionCount(0), new ElementCount(1));

    private static TagDefinition RealMetadata(string tagName) =>
        new(new TagName(tagName), LogixTypeKind.Atomic, AllenBradleyDataType.Real, MaxLength: null, new DimensionCount(0), new ElementCount(1));

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
    public async Task ReadAsync_WhenTheReplyIsTooShortForTheType_DegradesThatPointAndKeepsTheRest()
    {
        // Arrange
        // Two bytes where a DINT needs four. Nothing checks that before the decode any more — the
        // controller's type was verified at connect — so the converter is what discovers it, by throwing.
        var tagManager = new FakeTagManager
        {
            [Speed] = FakeTag.Reading(Speed, DintMetadata("Motor.Speed"), LogixTagReadResult.Ok(new byte[2])),
            [Level] = FakeTag.Reading(Level, DintMetadata("Tank.Level"), LogixTagReadResult.Ok(FortyTwoAsDint)),
        };
        var client = new LogixClient(tagManager);

        // Act
        var values = await client.ReadAsync([Speed, Level], CancellationToken.None);

        // Assert
        // A decode that throws is still one tag's problem: it degrades its own point and leaves the rest
        // of the group standing, exactly as a failed read does (ADR-004).
        values.Should().HaveCount(2);
        values[0].Quality.Should().Be(LogixQuality.Bad);
        values[1].Quality.Should().Be(LogixQuality.Good);
        values[1].Value.Should().Be(42);
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
        var value = CreateValue(Speed, 42);

        // Act
        var write = async () => await client.WriteAsync([value], CancellationToken.None);

        // Assert
        // Silence here would be a dropped write the caller cannot detect.
        (await write.Should().ThrowAsync<LogixTagException>())
            .Which.Message.Should().Contain("Motor.Speed").And.Contain("tag is read-only");
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
            CreateValue(Speed, 42),
            CreateValue(Level, 7),
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
        var value = CreateValue(Speed, 42);

        // Act
        await client.WriteAsync([value], CancellationToken.None);

        // Assert
        tag.Written.Should().Equal(FortyTwoAsDint);
    }

    [Fact]
    public async Task WriteAsync_AString_EncodesIntoTheBufferTheTagHandsOut()
    {
        // Arrange
        // A STRING is the first type whose wire size is not fixed by its type. The batch takes the buffer
        // from the tag rather than sizing one itself, so the 88 bytes that go out are the controller's
        // own width for Line.Label — no converter is asked how wide a STRING is.
        var tag = FakeTag.Writing(
            Label, StringMetadata("Line.Label"), LogixTagWriteResult.Ok(), tagSize: 88);
        var tagManager = new FakeTagManager { [Label] = tag };
        var client = new LogixClient(tagManager);
        var value = CreateValue(Label, "Hi");

        // Act
        await client.WriteAsync([value], CancellationToken.None);

        // Assert
        tag.Written.Should().HaveCount(88);
        tag.Written.AsSpan(0, 4).ToArray().Should().Equal(2, 0, 0, 0);
        tag.Written.AsSpan(4, 2).ToArray().Should().Equal((byte)'H', (byte)'i');
    }

    [Fact]
    public async Task ReadAsync_AString_DecodesTheStructureIntoATypedValue()
    {
        // Arrange
        var structure = new byte[88];
        structure[0] = 2;
        structure[4] = (byte)'H';
        structure[5] = (byte)'i';
        var tagManager = new FakeTagManager
        {
            [Label] = FakeTag.Reading(Label, StringMetadata("Line.Label"), LogixTagReadResult.Ok(structure)),
        };
        var client = new LogixClient(tagManager);

        // Act
        var values = await client.ReadAsync([Label], CancellationToken.None);

        // Assert
        values.Should().ContainSingle();
        values[0].Quality.Should().Be(LogixQuality.Good);
        values[0].Value.Should().Be("Hi");
    }

    // A data point shape deliberately absent from DataPointConverterRegistry: the model gaining a type
    // that nobody wired a converter for.
    private sealed record UnregisteredDataPoint()
        : LogixDataPoint<int>(new TagName("Mystery.Tag"), DefaultPollFrequency, NoChannels)
    {
        protected override LogixDataTypeName TypeName => new("MYSTERY");

        internal override ILogixDataPointValue<int> CreateLogixValue(int value) => throw new NotSupportedException();
    }

    private sealed class FakeTagManager : ILogixTagManager
    {
        private readonly Dictionary<ILogixDataPoint, FakeTag> _tagByDataPoint = [];

        public FakeTag this[ILogixDataPoint dataPoint]
        {
            set => _tagByDataPoint[dataPoint] = value;
        }

        public Task LoadTagDefinitionsAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public ILogixTag TagFor(ILogixDataPoint dataPoint) => _tagByDataPoint[dataPoint];
    }

    private sealed class FakeTag : ILogixTag
    {
        private LogixTagReadResult _readResult = LogixTagReadResult.Ok(ReadOnlyMemory<byte>.Empty);
        private LogixTagWriteResult _writeResult = LogixTagWriteResult.Ok();

        private FakeTag(ILogixDataPoint dataPoint, TagDefinition? metadata, int tagSize)
        {
            DataPoint = dataPoint;
            Metadata = metadata;
            Access = new FakeTagAccess(tagSize);
        }

        public ILogixDataPoint DataPoint { get; init; }

        public TagDefinition? Metadata { get; init; }

        /// <summary>
        /// Reached for one thing only: the write buffer, which is the tag's own width and not something
        /// the converter decides. Reads and writes are answered by this fake directly.
        /// </summary>
        public ILogixTagAccess Access { get; init; }

        public bool WasRead { get; private set; }

        public byte[]? Written { get; private set; }

        public static FakeTag Reading(
            ILogixDataPoint dataPoint, TagDefinition? metadata, LogixTagReadResult result) =>
            new(dataPoint, metadata, tagSize: 0) { _readResult = result };

        public static FakeTag Writing(
            ILogixDataPoint dataPoint, TagDefinition? metadata, LogixTagWriteResult result,
            int tagSize = sizeof(int)) =>
            new(dataPoint, metadata, tagSize) { _writeResult = result };

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

    private sealed class FakeTagAccess(int tagSize) : ILogixTagAccess
    {
        public byte[] CreateNewWriteBuffer() => new byte[tagSize];

        public Task<LogixTagReadResult> ReadAsync(CancellationToken cancellationToken) =>
            throw new NotSupportedException("FakeTag answers reads itself.");

        public Task<LogixTagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken) =>
            throw new NotSupportedException("FakeTag answers writes itself.");

        public void Dispose()
        {
        }
    }
}
