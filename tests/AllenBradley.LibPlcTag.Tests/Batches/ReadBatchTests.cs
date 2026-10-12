using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Tags;
using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Tests.TestData;
using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion;
using FakeReadBatch = ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Batches.ReadBatch<
    ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Tests.TestData.FakeDataPoint,
    ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Tests.TestData.FakeValue>;

namespace ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Tests.Batches;

public sealed class ReadBatchTests
{
    private const string TagNotFound = "tag not found";

    private static readonly FakeDataPoint Speed = new("Motor.Speed");
    private static readonly FakeDataPoint Level = new("Tank.Level");
    private static readonly FakeDataPoint Torque = new("Motor.Torque");

    // Not BitConverter: that would re-derive them through the assumption the converter itself makes.
    private static readonly byte[] FortyTwoAsInt32 = [42, 0, 0, 0];
    private static readonly byte[] SevenAsInt32 = [7, 0, 0, 0];

    private readonly FakeConverter _converter = new();

    [Fact]
    public async Task AGroupWhereEveryTagAnswersComesHomeWithOneValuePerPointInGroupOrder()
    {
        // Arrange
        var tagSource = TagSourceFor(
            (Speed, TagReading(TagReadResult.Ok(FortyTwoAsInt32))),
            (Level, TagReading(TagReadResult.Ok(SevenAsInt32))));
        var batch = CreateBatch([Speed, Level], tagSource);

        // Act
        var batchRead = await batch.ReadAsync(CancellationToken.None);

        // Assert
        // The reads are fanned out concurrently, so group order is what puts the values back in order.
        FakeValue[] expected = [new(Speed, 42), new(Level, 7)];
        batchRead.Failures.Should().BeEmpty();
        batchRead.Values.Should().Equal(expected);
    }

    [Fact]
    public async Task ABatchWithAFailingDataPointWillProduceValuesForTheSiblings()
    {
        // Arrange
        var tagSource = TagSourceFor(
            (Speed, TagReading(TagReadResult.Ok(FortyTwoAsInt32))),
            (Level, TagReading(TagReadResult.Failed(TagNotFound))),
            (Torque, TagReading(TagReadResult.Ok(SevenAsInt32))));
        var batch = CreateBatch([Speed, Level, Torque], tagSource);

        // Act
        var batchRead = await batch.ReadAsync(CancellationToken.None);

        // Assert
        FakeValue[] expected = [new(Speed, 42), new(Torque, 7)];
        batchRead.Values.Should().Equal(expected);
    }

    [Fact]
    public async Task ATagThatWillNotReadIsNamedAmongTheFailuresWithItsReason()
    {
        // Arrange
        var tagSource = TagSourceFor(
            (Speed, TagReading(TagReadResult.Ok(FortyTwoAsInt32))),
            (Level, TagReading(TagReadResult.Failed(TagNotFound))));
        var batch = CreateBatch([Speed, Level], tagSource);

        // Act
        var batchRead = await batch.ReadAsync(CancellationToken.None);

        // Assert
        var expected = FakeReadBatch.ReadOutcome.Failed(Level, TagNotFound);
        batchRead.Failures.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public async Task AReplyTooShortForItsTypeCostsOnlyItsOwnValue()
    {
        // Arrange
        var tooShortForAnInt32 = new byte[2];
        var tagSource = TagSourceFor(
            (Speed, TagReading(TagReadResult.Ok(FortyTwoAsInt32))),
            (Level, TagReading(TagReadResult.Ok(tooShortForAnInt32))));
        var batch = CreateBatch([Speed, Level], tagSource);

        // Act
        var batchRead = await batch.ReadAsync(CancellationToken.None);

        // Assert
        batchRead.Values.Should().Equal(new FakeValue(Speed, 42));
        batchRead.Failures.Should().ContainSingle().Which.DataPoint.Should().Be(Level);
    }

    [Fact]
    public async Task AGroupWhereNoTagAnswersSaysThatNothingWasRead()
    {
        // Arrange
        var tagSource = TagSourceFor(
            (Speed, TagReading(TagReadResult.Failed("tag is write-only"))),
            (Level, TagReading(TagReadResult.Failed(TagNotFound))));
        var batch = CreateBatch([Speed, Level], tagSource);

        // Act
        var batchRead = await batch.ReadAsync(CancellationToken.None);

        // Assert
        batchRead.NothingWasRead.Should().BeTrue();
    }

    [Fact]
    public async Task AGroupWhereOneTagAnswersDoesNotSayThatNothingWasRead()
    {
        // Arrange
        var tagSource = TagSourceFor(
            (Speed, TagReading(TagReadResult.Ok(FortyTwoAsInt32))),
            (Level, TagReading(TagReadResult.Failed(TagNotFound))));
        var batch = CreateBatch([Speed, Level], tagSource);

        // Act
        var batchRead = await batch.ReadAsync(CancellationToken.None);

        // Assert
        batchRead.NothingWasRead.Should().BeFalse();
    }

    [Fact]
    public async Task EveryFailedTagIsDescribedByItsIdentifierAndItsReason()
    {
        // Arrange
        var tagSource = TagSourceFor(
            (Speed, TagReading(TagReadResult.Failed("tag is write-only"))),
            (Level, TagReading(TagReadResult.Failed(TagNotFound))));
        var batch = CreateBatch([Speed, Level], tagSource);
        var batchRead = await batch.ReadAsync(CancellationToken.None);

        // Act
        var description = batchRead.DescribeFailures();

        // Assert
        description.Should().Be("Motor.Speed: tag is write-only; Tank.Level: tag not found");
    }

    [Fact]
    public async Task ACancelledBatchThrowsTheCancellationRatherThanFailingItsTags()
    {
        // Arrange
        var tagSource = TagSourceFor((Speed, TagReading(TagReadResult.Ok(FortyTwoAsInt32))));
        var batch = CreateBatch([Speed], tagSource);
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
        using var access = new SynchronizedTagAccess(handle);
        var tagSource = TagSourceFor((Speed, access));
        var batch = CreateBatch([Speed, Speed, Speed, Speed], tagSource);

        // Act
        var batchRead = await batch.ReadAsync(CancellationToken.None);

        // Assert
        // The timeout is the assertion for the other failure: a gate taken and never released hangs.
        var expected = Enumerable.Repeat(new FakeValue(Speed, 42), 4);
        handle.MaxConcurrentReads.Should().Be(1);
        handle.Reads.Should().Be(4);
        batchRead.Failures.Should().BeEmpty();
        batchRead.Values.Should().Equal(expected);
    }

    [Fact]
    public async Task AnEmptyGroupReadsNothingAndTouchesNoTag()
    {
        // Arrange
        var tagSource = TagSourceFor();
        var batch = CreateBatch([], tagSource);

        // Act
        var batchRead = await batch.ReadAsync(CancellationToken.None);

        // Assert
        batchRead.Values.Should().BeEmpty();
        batchRead.Failures.Should().BeEmpty();
        batchRead.NothingWasRead.Should().BeFalse();
        tagSource.DidNotReceiveWithAnyArgs().TagFor(default!);
    }

    [Fact]
    public void ConstructingABatchResolvesTheTagForEveryDataPointWithoutReadingAnything()
    {
        // Arrange
        var speedTag = TagReading(TagReadResult.Ok(FortyTwoAsInt32));
        var levelTag = TagReading(TagReadResult.Ok(SevenAsInt32));
        var tagSource = TagSourceFor((Speed, speedTag), (Level, levelTag));

        // Act
        _ = CreateBatch([Speed, Level], tagSource);

        // Assert
        tagSource.Received(1).TagFor(Speed);
        tagSource.Received(1).TagFor(Level);
        speedTag.ReceivedCalls().Should().BeEmpty();
        levelTag.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public async Task AnEntryWhoseTagAnswersComesHomeDecodedAndWithoutAnError()
    {
        // Arrange
        var entry = EntryFor(Speed, TagReadResult.Ok(FortyTwoAsInt32));

        // Act
        var outcome = await FakeReadBatch.ReadEntryAsync(entry, CancellationToken.None);

        // Assert
        var expected = FakeReadBatch.ReadOutcome.Ok(Speed, new FakeValue(Speed, 42));
        outcome.Should().Be(expected);
    }

    [Fact]
    public async Task AnEntryWhoseTagFailsCarriesTheFailureHomeInsteadOfThrowing()
    {
        // Arrange
        var entry = EntryFor(Speed, TagReadResult.Failed(TagNotFound));

        // Act
        var outcome = await FakeReadBatch.ReadEntryAsync(entry, CancellationToken.None);

        // Assert
        var expected = FakeReadBatch.ReadOutcome.Failed(Speed, TagNotFound);
        outcome.Should().Be(expected);
    }

    [Fact]
    public async Task AnEntryWhoseTagFailsIsNeverDecoded()
    {
        // Arrange
        var entry = EntryFor(Speed, TagReadResult.Failed(TagNotFound));

        // Act
        await FakeReadBatch.ReadEntryAsync(entry, CancellationToken.None);

        // Assert
        _converter.Decodes.Should().Be(0);
    }

    [Fact]
    public async Task AReplyTooShortForTheTypeBecomesThatTagsFailureUnderItsName()
    {
        // Arrange
        var tooShortForAnInt32 = new byte[2];
        var entry = EntryFor(Speed, TagReadResult.Ok(tooShortForAnInt32));

        // Act
        var outcome = await FakeReadBatch.ReadEntryAsync(entry, CancellationToken.None);

        // Assert
        outcome.DataPoint.Should().Be(Speed);
        outcome.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AReplyTheConverterRefusesAsTheWrongShapeBecomesThatTagsFailureWithTheReason()
    {
        // Arrange
        var converter = new FakeConverter { OnDecode = () => throw new DecodeException("array was resized") };
        var entry = EntryFor(Speed, TagReadResult.Ok(FortyTwoAsInt32), converter);

        // Act
        var outcome = await FakeReadBatch.ReadEntryAsync(entry, CancellationToken.None);

        // Assert
        var expected = FakeReadBatch.ReadOutcome.Failed(Speed, "array was resized");
        outcome.Should().Be(expected);
    }

    [Fact]
    public async Task AConverterFailingForAnyOtherReasonTravelsOutOfTheEntry()
    {
        // Arrange
        var converter = new FakeConverter { OnDecode = () => throw new NotSupportedException("converter bug") };
        var entry = EntryFor(Speed, TagReadResult.Ok(FortyTwoAsInt32), converter);

        // Act
        var read = await Record.ExceptionAsync(
            () => FakeReadBatch.ReadEntryAsync(entry, CancellationToken.None));

        // Assert
        read.Should().BeOfType<NotSupportedException>();
    }

    [Fact]
    public async Task ACancelledEntryThrowsRatherThanComingHomeAsAFailedOutcome()
    {
        // Arrange
        var entry = EntryFor(Speed, TagReadResult.Ok(FortyTwoAsInt32));
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var read = await Record.ExceptionAsync(() => FakeReadBatch.ReadEntryAsync(entry, cts.Token));

        // Assert
        read.Should().BeAssignableTo<OperationCanceledException>();
    }

    private FakeReadBatch CreateBatch(
        IReadOnlyList<FakeDataPoint> dataPoints, ITagSource<FakeDataPoint, ITagAccess> tagSource) =>
        new(dataPoints, tagSource, _ => _converter);

    private FakeReadBatch.ReadEntry EntryFor(
        FakeDataPoint dataPoint, TagReadResult result, FakeConverter? converter = null) =>
        new(dataPoint, converter ?? _converter, TagReading(result));

    // The real access honours cancellation by throwing rather than returning a failed result.
    private static ITagAccess TagReading(TagReadResult result)
    {
        var tag = Substitute.For<ITagAccess>();
        tag.ReadAsync(Arg.Any<CancellationToken>()).Returns(call =>
        {
            call.Arg<CancellationToken>().ThrowIfCancellationRequested();
            return Task.FromResult(result);
        });

        return tag;
    }

    private static ITagSource<FakeDataPoint, ITagAccess> TagSourceFor(
        params (FakeDataPoint DataPoint, ITagAccess Tag)[] tags)
    {
        var tagSource = Substitute.For<ITagSource<FakeDataPoint, ITagAccess>>();
        foreach (var (dataPoint, tag) in tags)
        {
            tagSource.TagFor(dataPoint).Returns(tag);
        }

        return tagSource;
    }

    private sealed class OverlapRecordingTagAccess : ITagAccess
    {
        private int _inFlight;
        private int _reads;

        public int Reads => _reads;

        public int MaxConcurrentReads { get; private set; }

        public async Task<TagReadResult> ReadAsync(CancellationToken cancellationToken)
        {
            MaxConcurrentReads = Math.Max(MaxConcurrentReads, Interlocked.Increment(ref _inFlight));
            Interlocked.Increment(ref _reads);
            await Task.Yield();
            Interlocked.Decrement(ref _inFlight);

            return TagReadResult.Ok(FortyTwoAsInt32);
        }

        public Task<TagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public void Dispose()
        {
        }
    }
}
