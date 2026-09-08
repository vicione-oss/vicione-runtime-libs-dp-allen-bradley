using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client;

/// <summary>
/// The read batch on its own, without a client around it: what one entry's read and decode turns into,
/// and what a batch does with a set of those. The contract under test is that the batch is a throughput
/// device and not a unit of meaning — a tag that will not read or will not decode costs its own value and
/// is named as a failure, while its siblings come home. Only a batch that read nothing at all throws
/// (ADR/2026-07-16-reading-and-writing-a-group-of-tags.md).
/// </summary>
// The tags and the tag manager are substituted: every assertion about them is "the batch did this to its
// collaborator", which is what NSubstitute is for here. The converter is not — Decode takes a
// ReadOnlySpan&lt;byte&gt;, and a ref struct cannot travel through a substitute's argument array.
public class LogixReadBatchTests
{
    private static readonly DIntDataPoint Speed = new(new TagName("Motor.Speed"), DefaultPollFrequency, NoChannels);
    private static readonly DIntDataPoint Level = new(new TagName("Tank.Level"), DefaultPollFrequency, NoChannels);
    private static readonly DIntDataPoint Torque = new(new TagName("Motor.Torque"), DefaultPollFrequency, NoChannels);

    // 42 and 7 as DINTs on the wire. Spelled out rather than taken from BitConverter, which would
    // re-derive them through the same host-endianness assumption the converter makes.
    private static readonly byte[] FortyTwoAsDint = [42, 0, 0, 0];
    private static readonly byte[] SevenAsDint = [7, 0, 0, 0];

    [Fact]
    public async Task ReadAsync_WhenEveryTagAnswers_ReturnsOneValuePerPointInGroupOrder()
    {
        // Arrange
        var tagManager = TagManagerFor(
            (Speed, TagReading(LogixTagReadResult.Ok(FortyTwoAsDint))),
            (Level, TagReading(LogixTagReadResult.Ok(SevenAsDint))));
        var batch = new LogixReadBatch([Speed, Level], tagManager);

        // Act
        var result = await batch.ReadAsync(CancellationToken.None);

        // Assert
        // The reads are fanned out concurrently, so the order the batch was constructed with is the only
        // thing that can put the values back in the caller's order.
        result.Failures.Should().BeEmpty();
        result.Values.Should().HaveCount(2);
        result.Values[0].DataPoint.Should().Be(Speed);
        result.Values[0].Value.Should().Be(42);
        result.Values[1].DataPoint.Should().Be(Level);
        result.Values[1].Value.Should().Be(7);
    }

    [Fact]
    public async Task ReadAsync_WhenOneTagFails_ReturnsTheValuesItsSiblingsProduced()
    {
        // Arrange
        // Two tags out of three answer perfectly well.
        var tagManager = TagManagerFor(
            (Speed, TagReading(LogixTagReadResult.Ok(FortyTwoAsDint))),
            (Level, TagReading(LogixTagReadResult.Failed("tag not found"))),
            (Torque, TagReading(LogixTagReadResult.Ok(SevenAsDint))));
        var batch = new LogixReadBatch([Speed, Level, Torque], tagManager);

        // Act
        var result = await batch.ReadAsync(CancellationToken.None);

        // Assert
        // The batch exists to get these tags onto the wire together, not because they mean anything as a
        // set, so one tag that would not read costs its own value and nothing else.
        result.Values.Should().HaveCount(2);
        result.Values[0].DataPoint.Should().Be(Speed);
        result.Values[1].DataPoint.Should().Be(Torque);
    }

    [Fact]
    public async Task ReadAsync_WhenOneTagFails_NamesItAndItsReasonAmongTheFailures()
    {
        // Arrange
        var tagManager = TagManagerFor(
            (Speed, TagReading(LogixTagReadResult.Ok(FortyTwoAsDint))),
            (Level, TagReading(LogixTagReadResult.Failed("tag not found"))));
        var batch = new LogixReadBatch([Speed, Level], tagManager);

        // Act
        var result = await batch.ReadAsync(CancellationToken.None);

        // Assert
        // Keeping the values does not mean losing the failure: the tag that produced none is carried out
        // by name, so the client can log which point went stale and why.
        result.Failures.Should().ContainSingle()
            .Which.TagName.Should().Be(Level.TagName);
        result.DescribeFailures().Should().Contain("Tank.Level").And.Contain("tag not found");
    }

    [Fact]
    public async Task ReadAsync_WhenEveryTagFails_ThrowsNamingEveryOne()
    {
        // Arrange
        var tagManager = TagManagerFor(
            (Speed, TagReading(LogixTagReadResult.Failed("tag is write-only"))),
            (Level, TagReading(LogixTagReadResult.Failed("tag not found"))));
        var batch = new LogixReadBatch([Speed, Level], tagManager);

        // Act
        var read = await Record.ExceptionAsync(() => batch.ReadAsync(CancellationToken.None));

        // Assert
        // A batch that read nothing has nothing to hand up, and returning an empty list would make a dead
        // controller look like a poll that simply had nothing to fetch.
        var message = read.Should().BeOfType<LogixTagException>().Which.Message;
        message.Should().Contain("Motor.Speed").And.Contain("tag is write-only");
        message.Should().Contain("Tank.Level").And.Contain("tag not found");
    }

    [Fact]
    public async Task ReadAsync_WhenOneTagFails_StillReadsEverySibling()
    {
        // Arrange
        var speedTag = TagReading(LogixTagReadResult.Ok(FortyTwoAsDint));
        var torqueTag = TagReading(LogixTagReadResult.Ok(SevenAsDint));
        var tagManager = TagManagerFor(
            (Speed, speedTag),
            (Level, TagReading(LogixTagReadResult.Failed("tag not found"))),
            (Torque, torqueTag));
        var batch = new LogixReadBatch([Speed, Level, Torque], tagManager);

        // Act
        await batch.ReadAsync(CancellationToken.None);

        // Assert
        // Detected per tag. Short-circuiting on the first failure would leave the batch half-issued
        // against the controller and cost the packing the concurrent fan-out exists for.
        await speedTag.Received(1).ReadAsync(Arg.Any<CancellationToken>());
        await torqueTag.Received(1).ReadAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReadAsync_WhenOneTagFailsToReadAndAnotherToDecode_NamesBoth()
    {
        // Arrange
        // Two bytes where a DINT needs four: a reply the device delivered and the converter refuses.
        var tagManager = TagManagerFor(
            (Speed, TagReading(LogixTagReadResult.Ok(new byte[2]))),
            (Level, TagReading(LogixTagReadResult.Failed("tag not found"))));
        var batch = new LogixReadBatch([Speed, Level], tagManager);

        // Act
        var read = await Record.ExceptionAsync(() => batch.ReadAsync(CancellationToken.None));

        // Assert
        // The two kinds of failure are one kind by the time they reach the caller: a tag that produced no
        // value, whatever stopped it. Neither tag produced one, so the batch has nothing to hand up.
        var message = read.Should().BeOfType<LogixTagException>().Which.Message;
        message.Should().Contain("Motor.Speed");
        message.Should().Contain("Tank.Level").And.Contain("tag not found");
    }

    [Fact]
    public async Task ReadAsync_WhenCancelled_PropagatesTheCancellationRatherThanFailingTheTags()
    {
        // Arrange
        // The token is what the tag honours, the way the real access does: the batch passes it down, and
        // a cancelled read throws out of the tag rather than coming home as a failed result.
        var batch = new LogixReadBatch([Speed], TagManagerFor((Speed, TagReading(LogixTagReadResult.Ok(FortyTwoAsDint)))));
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var read = await Record.ExceptionAsync(() => batch.ReadAsync(cts.Token));

        // Assert
        // Cancelling is the caller's decision, not the device's answer, so it travels as itself rather
        // than being collected as one more failed tag in a LogixTagException.
        read.Should().BeAssignableTo<OperationCanceledException>();
    }

    [Fact]
    public async Task ReadAsync_WithNoDataPoints_ReturnsNothingAndTouchesNoTag()
    {
        // Arrange
        var tagManager = TagManagerFor();
        var batch = new LogixReadBatch([], tagManager);

        // Act
        var result = await batch.ReadAsync(CancellationToken.None);

        // Assert
        // A group can be emptied by configuration; an empty fan-out is not a failure — nothing failed, so
        // reading nothing is the answer rather than the throw an all-failed batch produces — and asking
        // the tag manager for nothing costs no round trip.
        result.Values.Should().BeEmpty();
        result.Failures.Should().BeEmpty();
        tagManager.DidNotReceiveWithAnyArgs().TagFor(default!);
    }

    [Fact]
    public void Constructor_ResolvesTheTagForEveryDataPointWithoutReadingAnything()
    {
        // Arrange
        var speedTag = TagReading(LogixTagReadResult.Ok(FortyTwoAsDint));
        var levelTag = TagReading(LogixTagReadResult.Ok(SevenAsDint));
        var tagManager = TagManagerFor((Speed, speedTag), (Level, levelTag));

        // Act
        _ = new LogixReadBatch([Speed, Level], tagManager);

        // Assert
        // Holding a batch means holding a fully resolved one — the tags and converters are paired up
        // front — but resolving is not reading, and no I/O happens until ReadAsync.
        tagManager.Received(1).TagFor(Speed);
        tagManager.Received(1).TagFor(Level);
        speedTag.ReceivedCalls().Should().BeEmpty();
        levelTag.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WhenADataPointHasNoConverter_ThrowsBeforeAnyReadIsPossible()
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
        // A data point wired up without a converter is a configuration error, and failing on construction
        // makes it cost no round trip and leave no half-read group behind.
        construct.Should().BeOfType<InvalidOperationException>();
        speedTag.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public async Task ReadEntryAsync_WhenTheTagAnswers_ReturnsTheDecodedValueAndNoError()
    {
        // Arrange
        var entry = EntryFor(Speed, LogixTagReadResult.Ok(FortyTwoAsDint));

        // Act
        var outcome = await LogixReadBatch.ReadEntryAsync(entry, CancellationToken.None);

        // Assert
        // The one place bytes become a typed value: the converter decodes them and the data point wraps
        // them, and the outcome carries the value the batch will hand out unchanged.
        outcome.Error.Should().BeNull();
        outcome.TagName.Should().Be(Speed.TagName);
        outcome.Value!.Value.Should().Be(42);
    }

    [Fact]
    public async Task ReadEntryAsync_WhenTheTagFails_ReturnsTheFailureInsteadOfThrowing()
    {
        // Arrange
        var entry = EntryFor(Speed, LogixTagReadResult.Failed("tag not found"));

        // Act
        var outcome = await LogixReadBatch.ReadEntryAsync(entry, CancellationToken.None);

        // Assert
        // A device failure rides home as an outcome rather than an exception, because awaiting the
        // fan-out's Task.WhenAll would rethrow only the first exception of the set and lose the rest.
        outcome.Value.Should().BeNull();
        outcome.TagName.Should().Be(Speed.TagName);
        outcome.Error.Should().Be("tag not found");
    }

    [Fact]
    public async Task ReadEntryAsync_WhenTheTagFails_DoesNotDecode()
    {
        // Arrange
        var converter = new SpyConverter();
        var entry = EntryFor(Speed, LogixTagReadResult.Failed("tag not found"), converter);

        // Act
        await LogixReadBatch.ReadEntryAsync(entry, CancellationToken.None);

        // Assert
        // A failed read carries no bytes, so decoding them would be decoding an empty buffer and reporting
        // the wrong reason for the tag's failure.
        converter.Decodes.Should().Be(0);
    }

    [Fact]
    public async Task ReadEntryAsync_WhenTheReplyIsTooShortForTheType_ReturnsTheTagsFailureWithItsName()
    {
        // Arrange
        // Two bytes where a DINT needs four. Nothing checks that before the decode — the controller's
        // type was verified at connect — so the converter is what discovers it, by throwing.
        var entry = EntryFor(Speed, LogixTagReadResult.Ok(new byte[2]));

        // Act
        var outcome = await LogixReadBatch.ReadEntryAsync(entry, CancellationToken.None);

        // Assert
        // Caught narrowly so it is reported as this tag's failure with its name on it, rather than as a
        // raw ArgumentException travelling with no address in its message.
        outcome.Value.Should().BeNull();
        outcome.TagName.Should().Be(Speed.TagName);
        outcome.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ReadEntryAsync_WhenTheConverterFailsForAnyOtherReason_LetsItTravel()
    {
        // Arrange
        var converter = new SpyConverter { OnDecode = () => throw new NotSupportedException("converter bug") };
        var entry = EntryFor(Speed, LogixTagReadResult.Ok(FortyTwoAsDint), converter);

        // Act
        var read = await Record.ExceptionAsync(
            () => LogixReadBatch.ReadEntryAsync(entry, CancellationToken.None));

        // Assert
        // The catch is the buffer being the wrong shape for the decode and nothing else. A converter that
        // throws anything else is a bug in the converter, and dressing it up as a device failure would
        // send the reader to the controller for it.
        read.Should().BeOfType<NotSupportedException>();
    }

    [Fact]
    public async Task ReadEntryAsync_WhenCancelled_ThrowsRatherThanReturningAFailedOutcome()
    {
        // Arrange
        var entry = EntryFor(Speed, LogixTagReadResult.Ok(FortyTwoAsDint));
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var read = await Record.ExceptionAsync(() => LogixReadBatch.ReadEntryAsync(entry, cts.Token));

        // Assert
        // A cancelled read must not join the failed tags in the batch's message: it is a shutdown, and
        // naming the tag would report a controller fault that never happened.
        read.Should().BeAssignableTo<OperationCanceledException>();
    }

    private static LogixReadBatch.ReadEntry EntryFor(
        ILogixDataPoint dataPoint, LogixTagReadResult result, IDataPointConverter? converter = null) =>
        new(dataPoint,
            converter ?? DataPointConverterRegistry.GetConverter(dataPoint),
            TagReading(result));

    // A tag that answers with one prepared result. Cancellation is honoured the way the real access
    // honours it — by throwing rather than by coming home as a failed result, because a cancelled
    // operation is not a device answer.
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
