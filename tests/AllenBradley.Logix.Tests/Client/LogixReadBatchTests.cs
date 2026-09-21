using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
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
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client;

public sealed class LogixReadBatchTests
{
    private const string TagNotFound = "tag not found";

    private static readonly DIntDataPoint Speed = new(TagPath.Parse("Motor.Speed"), DefaultPollFrequency, NoChannels);
    private static readonly DIntDataPoint Level = new(TagPath.Parse("Tank.Level"), DefaultPollFrequency, NoChannels);
    private static readonly DIntDataPoint Torque = new(TagPath.Parse("Motor.Torque"), DefaultPollFrequency, NoChannels);

    private static readonly IntArrayDataPoint Readings = new(
        TagPath.Parse("Tank.Readings"), DefaultPollFrequency, NoChannels, new ElementCount(10));

    // Not BitConverter: that would re-derive them through the assumption the converter itself makes.
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
        var expected = LogixReadBatch.ReadOutcome.Failed(Level.TagAddress, TagNotFound);
        batchRead.Failures.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public async Task AnArrayReplyTooShortForItsDeclaredCountCostsOnlyItsOwnValue()
    {
        // Arrange
        var tooShortForTenInts = new byte[12];
        var tagManager = TagManagerFor(
            (Speed, TagReading(LogixTagReadResult.Ok(FortyTwoAsDint))),
            (Readings, TagReading(LogixTagReadResult.Ok(tooShortForTenInts))));
        var batch = new LogixReadBatch([Speed, Readings], tagManager);

        // Act
        var batchRead = await batch.ReadAsync(CancellationToken.None);

        // Assert
        batchRead.Values.Should().Equal(Speed.CreateLogixValue(42));
        batchRead.Failures.Should().ContainSingle().Which.TagAddress.Should().Be(Readings.TagAddress);
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
        message.Should().Contain(Speed.TagAddress.Value).And.Contain("tag is write-only");
        message.Should().Contain(Level.TagAddress.Value).And.Contain(TagNotFound);
    }

    [Fact]
    public async Task ATagThatWillNotReadAndOneThatWillNotDecodeAreBothNamedInTheThrow()
    {
        // Arrange
        var tooShortForADint = new byte[2];
        var tagManager = TagManagerFor(
            (Speed, TagReading(LogixTagReadResult.Ok(tooShortForADint))),
            (Level, TagReading(LogixTagReadResult.Failed(TagNotFound))));
        var batch = new LogixReadBatch([Speed, Level], tagManager);

        // Act
        var reading = batch.Awaiting(readBatch => readBatch.ReadAsync(CancellationToken.None));

        // Assert
        var message = (await reading.Should().ThrowAsync<LogixTagException>()).Which.Message;
        message.Should().Contain(Speed.TagAddress.Value);
        message.Should().Contain(Level.TagAddress.Value).And.Contain(TagNotFound);
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
        var expected = LogixReadBatch.ReadOutcome.Failed(Speed.TagAddress, TagNotFound);
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
        var tooShortForADint = new byte[2];
        var entry = EntryFor(Speed, LogixTagReadResult.Ok(tooShortForADint));

        // Act
        var outcome = await LogixReadBatch.ReadEntryAsync(entry, CancellationToken.None);

        // Assert
        outcome.TagAddress.Should().Be(Speed.TagAddress);
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

    // The real access honours cancellation by throwing rather than returning a failed result.
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

    // A data point shape deliberately absent from DataPointConverterRegistry.
    private sealed record UnregisteredDataPoint()
        : LogixDataPoint<int>(TagPath.Parse("Mystery.RootTagName"), DefaultPollFrequency, NoChannels)
    {
        public override AllenBradleyDataType DataType => new AllenBradleyDataType(new DataTypeName("MYSTERY"), LogixGeneration.Logix5X70);

        internal override ILogixDataPointValue<int> CreateLogixValue(int value) => throw new NotSupportedException();
    }

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

    // Hand-rolled because Decode takes a ReadOnlySpan<byte>, and a ref struct cannot travel through the
    // object array a substitute routes its arguments in.
    private sealed class SpyConverter : IDataPointConverter
    {
        public int Decodes { get; private set; }

        public Func<ILogixDataPointValue>? OnDecode { get; init; }

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
