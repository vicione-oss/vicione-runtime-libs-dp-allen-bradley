using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client;

public sealed class LogixReadBatchTests
{
    private const string TagNotFound = "tag not found";

    private static readonly DIntDataPoint Speed = new(new TagName("Motor.Speed"), DefaultPollFrequency, NoChannels);
    private static readonly DIntDataPoint Level = new(new TagName("Tank.Level"), DefaultPollFrequency, NoChannels);
    private static readonly DIntDataPoint Torque = new(new TagName("Motor.Torque"), DefaultPollFrequency, NoChannels);

    private static readonly IntArrayDataPoint Readings = new(
        new TagName("Tank.Readings"), DefaultPollFrequency, NoChannels, new ElementCount(10));

    // 42 and 7 as DINTs on the wire, spelled out rather than taken from BitConverter, which would
    // re-derive them through the same host-endianness assumption the converter makes.
    private static readonly byte[] FortyTwoAsDint = [42, 0, 0, 0];
    private static readonly byte[] SevenAsDint = [7, 0, 0, 0];

    [Fact]
    public async Task AGroupWhereEveryTagAnswersComesHomeWithOneValuePerPointInGroupOrder()
    {
        // Arrange
        var tagManager = TagManagerFor(
            (Speed, TagReading(LogixTagReadResult.Ok(FortyTwoAsDint))),
            (Level, TagReading(LogixTagReadResult.Ok(SevenAsDint))));
        var batch = new LogixReadBatch([Speed, Level], tagManager);

        // Act
        var batchRead = await batch.ReadAsync(CancellationToken.None);

        // Assert
        // The reads are fanned out concurrently, so group order is what puts the values back in order.
        ILogixDataPointValue[] expected = [Speed.CreateLogixValue(42), Level.CreateLogixValue(7)];
        batchRead.Failures.Should().BeEmpty();
        batchRead.Values.Should().Equal(expected);
    }

    [Fact]
    public async Task ABatchWithAFailingDataPointWillProduceValuesForTheSiblings()
    {
        // Arrange
        var tagManager = TagManagerFor(
            (Speed, TagReading(LogixTagReadResult.Ok(FortyTwoAsDint))),
            (Level, TagReading(LogixTagReadResult.Failed(TagNotFound))),
            (Torque, TagReading(LogixTagReadResult.Ok(SevenAsDint))));
        var batch = new LogixReadBatch([Speed, Level, Torque], tagManager);

        // Act
        var batchRead = await batch.ReadAsync(CancellationToken.None);

        // Assert
        ILogixDataPointValue[] expected = [Speed.CreateLogixValue(42), Torque.CreateLogixValue(7)];
        batchRead.Values.Should().Equal(expected);
    }

    [Fact]
    public async Task ATagThatWillNotReadIsNamedAmongTheFailuresWithItsReason()
    {
        // Arrange
        var tagManager = TagManagerFor(
            (Speed, TagReading(LogixTagReadResult.Ok(FortyTwoAsDint))),
            (Level, TagReading(LogixTagReadResult.Failed(TagNotFound))));
        var batch = new LogixReadBatch([Speed, Level], tagManager);

        // Act
        var batchRead = await batch.ReadAsync(CancellationToken.None);

        // Assert
        var expected = LogixReadBatch.ReadOutcome.Failed(Level.TagName, TagNotFound);
        batchRead.Failures.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public async Task AnArrayReplyTooShortForItsDeclaredCountCostsOnlyItsOwnValue()
    {
        // Arrange
        // Twelve bytes where ten INTs need twenty, which nothing checks before the decode.
        var tagManager = TagManagerFor(
            (Speed, TagReading(LogixTagReadResult.Ok(FortyTwoAsDint))),
            (Readings, TagReading(LogixTagReadResult.Ok(new byte[12]))));
        var batch = new LogixReadBatch([Speed, Readings], tagManager);

        // Act
        var batchRead = await batch.ReadAsync(CancellationToken.None);

        // Assert
        batchRead.Values.Should().Equal(Speed.CreateLogixValue(42));
        batchRead.Failures.Should().ContainSingle().Which.TagName.Should().Be(Readings.TagName);
    }

    [Fact]
    public async Task AGroupWhereNoTagAnswersThrowsNamingEveryFailedTag()
    {
        // Arrange
        var tagManager = TagManagerFor(
            (Speed, TagReading(LogixTagReadResult.Failed("tag is write-only"))),
            (Level, TagReading(LogixTagReadResult.Failed(TagNotFound))));
        var batch = new LogixReadBatch([Speed, Level], tagManager);

        // Act
        var reading = batch.Awaiting(readBatch => readBatch.ReadAsync(CancellationToken.None));

        // Assert
        var message = (await reading.Should().ThrowAsync<LogixTagException>()).Which.Message;
        message.Should().Contain(Speed.TagName.Value).And.Contain("tag is write-only");
        message.Should().Contain(Level.TagName.Value).And.Contain(TagNotFound);
    }

    [Fact]
    public async Task ATagThatWillNotReadAndOneThatWillNotDecodeAreBothNamedInTheThrow()
    {
        // Arrange
        // Two bytes where a DINT needs four: a reply the device delivered and the converter refuses.
        var tagManager = TagManagerFor(
            (Speed, TagReading(LogixTagReadResult.Ok(new byte[2]))),
            (Level, TagReading(LogixTagReadResult.Failed(TagNotFound))));
        var batch = new LogixReadBatch([Speed, Level], tagManager);

        // Act
        var reading = batch.Awaiting(readBatch => readBatch.ReadAsync(CancellationToken.None));

        // Assert
        var message = (await reading.Should().ThrowAsync<LogixTagException>()).Which.Message;
        message.Should().Contain(Speed.TagName.Value);
        message.Should().Contain(Level.TagName.Value).And.Contain(TagNotFound);
    }

    [Fact]
    public async Task ACancelledBatchThrowsTheCancellationRatherThanFailingItsTags()
    {
        // Arrange
        var tagManager = TagManagerFor((Speed, TagReading(LogixTagReadResult.Ok(FortyTwoAsDint))));
        var batch = new LogixReadBatch([Speed], tagManager);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var reading = batch.Awaiting(readBatch => readBatch.ReadAsync(cts.Token));

        // Assert
        await reading.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact(Timeout = 10_000)]
    public async Task ADataPointNamedSeveralTimesInOneGroupReadsThroughItsSharedTagOneAtATime()
    {
        // Arrange
        // The real gate over a fake handle: all four entries draw the same cached tag, so the fan-out
        // starts four reads against one handle at once.
        var handle = new OverlapRecordingTagAccess();
        using var access = new SynchronizedLogixTagAccess(handle);
        var tagManager = Substitute.For<ILogixTagManager>();
        tagManager.TagFor(Speed).Returns(new LogixTag(Speed, null, access));
        var batch = new LogixReadBatch([Speed, Speed, Speed, Speed], tagManager);

        // Act
        var batchRead = await batch.ReadAsync(CancellationToken.None);

        // Assert
        // The timeout is the assertion for the other failure: a gate taken and never released hangs.
        var expected = Enumerable.Repeat<ILogixDataPointValue>(Speed.CreateLogixValue(42), 4);
        handle.MaxConcurrentReads.Should().Be(1);
        handle.Reads.Should().Be(4);
        batchRead.Failures.Should().BeEmpty();
        batchRead.Values.Should().Equal(expected);
    }

    [Fact]
    public async Task AnEmptyGroupReadsNothingAndTouchesNoTag()
    {
        // Arrange
        var tagManager = TagManagerFor();
        var batch = new LogixReadBatch([], tagManager);

        // Act
        var batchRead = await batch.ReadAsync(CancellationToken.None);

        // Assert
        batchRead.Values.Should().BeEmpty();
        batchRead.Failures.Should().BeEmpty();
        tagManager.DidNotReceiveWithAnyArgs().TagFor(default!);
    }

    [Fact]
    public void ConstructingABatchResolvesTheTagForEveryDataPointWithoutReadingAnything()
    {
        // Arrange
        var speedTag = TagReading(LogixTagReadResult.Ok(FortyTwoAsDint));
        var levelTag = TagReading(LogixTagReadResult.Ok(SevenAsDint));
        var tagManager = TagManagerFor((Speed, speedTag), (Level, levelTag));

        // Act
        _ = new LogixReadBatch([Speed, Level], tagManager);

        // Assert
        tagManager.Received(1).TagFor(Speed);
        tagManager.Received(1).TagFor(Level);
        speedTag.ReceivedCalls().Should().BeEmpty();
        levelTag.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public void ADataPointWithoutAConverterIsRefusedBeforeAnyReadIsPossible()
    {
        // Arrange
        // Both points have a tag, so the only thing left that can throw is the missing converter.
        var speedTag = TagReading(LogixTagReadResult.Ok(FortyTwoAsDint));
        var unconvertible = new UnregisteredDataPoint();
        var tagManager = TagManagerFor(
            (Speed, speedTag),
            (unconvertible, TagReading(LogixTagReadResult.Ok(FortyTwoAsDint))));

        // Act
        var construct = Record.Exception(() => new LogixReadBatch([Speed, unconvertible], tagManager));

        // Assert
        construct.Should().BeOfType<InvalidOperationException>();
        speedTag.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public async Task AnEntryWhoseTagAnswersComesHomeDecodedAndWithoutAnError()
    {
        // Arrange
        var entry = EntryFor(Speed, LogixTagReadResult.Ok(FortyTwoAsDint));

        // Act
        var outcome = await LogixReadBatch.ReadEntryAsync(entry, CancellationToken.None);

        // Assert
        var expected = LogixReadBatch.ReadOutcome.Ok(Speed.CreateLogixValue(42));
        outcome.Should().Be(expected);
    }

    [Fact]
    public async Task AnEntryWhoseTagFailsCarriesTheFailureHomeInsteadOfThrowing()
    {
        // Arrange
        var entry = EntryFor(Speed, LogixTagReadResult.Failed(TagNotFound));

        // Act
        var outcome = await LogixReadBatch.ReadEntryAsync(entry, CancellationToken.None);

        // Assert
        var expected = LogixReadBatch.ReadOutcome.Failed(Speed.TagName, TagNotFound);
        outcome.Should().Be(expected);
    }

    [Fact]
    public async Task AnEntryWhoseTagFailsIsNeverDecoded()
    {
        // Arrange
        var converter = new SpyConverter();
        var entry = EntryFor(Speed, LogixTagReadResult.Failed(TagNotFound), converter);

        // Act
        await LogixReadBatch.ReadEntryAsync(entry, CancellationToken.None);

        // Assert
        converter.Decodes.Should().Be(0);
    }

    [Fact]
    public async Task AReplyTooShortForTheTypeBecomesThatTagsFailureUnderItsName()
    {
        // Arrange
        // Two bytes where a DINT needs four, which nothing checks before the decode.
        var entry = EntryFor(Speed, LogixTagReadResult.Ok(new byte[2]));

        // Act
        var outcome = await LogixReadBatch.ReadEntryAsync(entry, CancellationToken.None);

        // Assert
        outcome.TagName.Should().Be(Speed.TagName);
        outcome.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AConverterFailingForAnyOtherReasonTravelsOutOfTheEntry()
    {
        // Arrange
        var converter = new SpyConverter { OnDecode = () => throw new NotSupportedException("converter bug") };
        var entry = EntryFor(Speed, LogixTagReadResult.Ok(FortyTwoAsDint), converter);

        // Act
        var read = await Record.ExceptionAsync(
            () => LogixReadBatch.ReadEntryAsync(entry, CancellationToken.None));

        // Assert
        read.Should().BeOfType<NotSupportedException>();
    }

    [Fact]
    public async Task ACancelledEntryThrowsRatherThanComingHomeAsAFailedOutcome()
    {
        // Arrange
        var entry = EntryFor(Speed, LogixTagReadResult.Ok(FortyTwoAsDint));
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var read = await Record.ExceptionAsync(() => LogixReadBatch.ReadEntryAsync(entry, cts.Token));

        // Assert
        read.Should().BeAssignableTo<OperationCanceledException>();
    }

    private static LogixReadBatch.ReadEntry EntryFor(
        ILogixDataPoint dataPoint, LogixTagReadResult result, IDataPointConverter? converter = null) =>
        new(dataPoint,
            converter ?? DataPointConverterRegistry.GetConverter(dataPoint),
            TagReading(result));

    // A tag that answers with one prepared result, honouring cancellation by throwing the way the real
    // access does.
    private static ILogixTag TagReading(LogixTagReadResult result)
    {
        var tag = Substitute.For<ILogixTag>();
        tag.ReadAsync(Arg.Any<CancellationToken>()).Returns(call =>
        {
            call.Arg<CancellationToken>().ThrowIfCancellationRequested();
            return Task.FromResult(result);
        });

        return tag;
    }

    private static ILogixTagManager TagManagerFor(params (ILogixDataPoint DataPoint, ILogixTag Tag)[] tags)
    {
        var tagManager = Substitute.For<ILogixTagManager>();
        foreach (var (dataPoint, tag) in tags)
        {
            tagManager.TagFor(dataPoint).Returns(tag);
        }

        return tagManager;
    }

    // A data point shape deliberately absent from DataPointConverterRegistry: the model gaining a type
    // that nobody wired a converter for.
    private sealed record UnregisteredDataPoint()
        : LogixDataPoint<int>(new TagName("Mystery.Tag"), DefaultPollFrequency, NoChannels)
    {
        protected override LogixDataTypeName TypeName => new("MYSTERY");

        internal override ILogixDataPointValue<int> CreateLogixValue(int value) => throw new NotSupportedException();
    }

    // A handle that answers 42 and records how many reads were inside it at once.
    private sealed class OverlapRecordingTagAccess : ILogixTagAccess
    {
        private int _inFlight;
        private int _reads;

        public int Reads => _reads;

        public int MaxConcurrentReads { get; private set; }

        public async Task<LogixTagReadResult> ReadAsync(CancellationToken cancellationToken)
        {
            MaxConcurrentReads = Math.Max(MaxConcurrentReads, Interlocked.Increment(ref _inFlight));
            Interlocked.Increment(ref _reads);
            await Task.Yield();
            Interlocked.Decrement(ref _inFlight);

            return LogixTagReadResult.Ok(FortyTwoAsDint);
        }

        public Task<LogixTagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public void Dispose()
        {
        }
    }

    // A converter that records whether it was asked to decode, and can be made to fail in a way the entry
    // does not catch. Hand-rolled because Decode takes a ReadOnlySpan<byte>: a substitute routes its
    // arguments through an object array, and a ref struct cannot go in one.
    private sealed class SpyConverter : IDataPointConverter
    {
        public int Decodes { get; private set; }

        public Func<ILogixDataPointValue>? OnDecode { get; init; }

        public LogixDataTypeName ExpectedTypeName => new("DINT");

        public LogixTypeKind ExpectedKind => LogixTypeKind.Atomic;

        public AllenBradleyDataType? ExpectedDataType => AllenBradleyDataType.Dint;

        public StringMaxLength? MaxLengthFor(ILogixDataPoint dataPoint) => null;

        public ILogixDataPointValue Decode(ILogixDataPoint dataPoint, ReadOnlySpan<byte> buffer)
        {
            Decodes++;
            return OnDecode is not null
                ? OnDecode()
                : DataPointConverterRegistry.GetConverter(dataPoint).Decode(dataPoint, buffer);
        }

        public byte[] Encode(ILogixDataPointValue dataPointValue) => throw new NotSupportedException();
    }
}
