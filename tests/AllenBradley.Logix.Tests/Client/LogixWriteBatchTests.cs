using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client;

/// <summary>
/// The write batch on its own, without a client around it: what constructing one does and what writing it
/// does. The contract under test is the one the read batch's mirrors — every failed tag is named in one
/// exception (ADR/2026-07-16-reading-and-writing-a-group-of-tags.md) — plus the one only this direction
/// has: an encode happens before any tag is touched, so a value that will not encode costs no partial
/// write.
/// </summary>
// The tags and the tag manager are substituted, as in LogixReadBatchTests: every assertion about them is
// "the batch did this to its collaborator". The converters are the real ones from the registry — what a
// DINT or an over-long STRING encodes to is their contract, and the batch is only being asked when it
// calls them.
public class LogixWriteBatchTests
{
    private static readonly DIntDataPoint Speed = new(new TagName("Motor.Speed"), DefaultPollFrequency, NoChannels);
    private static readonly DIntDataPoint Level = new(new TagName("Tank.Level"), DefaultPollFrequency, NoChannels);
    private static readonly DIntDataPoint Torque = new(new TagName("Motor.Torque"), DefaultPollFrequency, NoChannels);

    private static readonly StringDataPoint Label =
        new(new TagName("Line.Label"), DefaultPollFrequency, NoChannels, StringMaxLength.Standard);

    private static readonly StringDataPoint Recipe =
        new(new TagName("Line.Recipe"), DefaultPollFrequency, NoChannels, StringMaxLength.Standard);

    // 42 and 7 as DINTs on the wire. Spelled out rather than taken from BitConverter, which would
    // re-derive them through the same host-endianness assumption the converter makes.
    private static readonly byte[] FortyTwoAsDint = [42, 0, 0, 0];
    private static readonly byte[] SevenAsDint = [7, 0, 0, 0];

    // One character more than a STRING's .DATA holds: the value the converter refuses rather than
    // truncating, and the only encode failure reachable without a hand-rolled converter.
    private static readonly string TooLongForAString = new('X', StringMaxLength.Standard.Value + 1);

    [Fact]
    public async Task WriteAsync_SendsEachValuesEncodedBytesToItsOwnTag()
    {
        // Arrange
        var speedWrites = new List<byte[]>();
        var levelWrites = new List<byte[]>();
        var tagManager = TagManagerFor(
            (Speed, TagWriting(LogixTagWriteResult.Ok(), speedWrites)),
            (Level, TagWriting(LogixTagWriteResult.Ok(), levelWrites)));
        var batch = new LogixWriteBatch([Speed.CreateLogixValue(42), Level.CreateLogixValue(7)], tagManager);

        // Act
        await batch.WriteAsync(CancellationToken.None);

        // Assert
        // The writes are fanned out concurrently, so nothing but the pairing made at construction keeps a
        // value with its own tag.
        speedWrites.Should().ContainSingle().Which.Should().Equal(FortyTwoAsDint);
        levelWrites.Should().ContainSingle().Which.Should().Equal(SevenAsDint);
    }

    [Fact]
    public async Task WriteAsync_WhenOneTagFails_ThrowsNamingItAndItsReason()
    {
        // Arrange
        var tagManager = TagManagerFor(
            (Speed, TagWriting(LogixTagWriteResult.Ok())),
            (Level, TagWriting(LogixTagWriteResult.Failed("tag is read-only"))));
        var batch = new LogixWriteBatch([Speed.CreateLogixValue(42), Level.CreateLogixValue(7)], tagManager);

        // Act
        var write = await Record.ExceptionAsync(() => batch.WriteAsync(CancellationToken.None));

        // Assert
        // ILogixWriteClient.WriteAsync hands the caller a bare ValueTask, so an exception is the only
        // thing that stops a dropped write from being silent.
        write.Should().BeOfType<LogixTagException>()
            .Which.Message.Should().Contain("Tank.Level").And.Contain("tag is read-only");
    }

    [Fact]
    public async Task WriteAsync_WhenOneTagFails_StillWritesEverySibling()
    {
        // Arrange
        var speedTag = TagWriting(LogixTagWriteResult.Ok());
        var torqueTag = TagWriting(LogixTagWriteResult.Ok());
        var tagManager = TagManagerFor(
            (Speed, speedTag),
            (Level, TagWriting(LogixTagWriteResult.Failed("tag not found"))),
            (Torque, torqueTag));
        var batch = new LogixWriteBatch(
            [Speed.CreateLogixValue(42), Level.CreateLogixValue(7), Torque.CreateLogixValue(7)], tagManager);

        // Act
        await Record.ExceptionAsync(() => batch.WriteAsync(CancellationToken.None));

        // Assert
        // Reported per batch, but detected per tag. A failed device write is not a reason to withhold the
        // writes the caller asked for on the tags that would have taken them.
        await speedTag.Received(1).WriteAsync(Arg.Any<byte[]>(), Arg.Any<CancellationToken>());
        await torqueTag.Received(1).WriteAsync(Arg.Any<byte[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WriteAsync_WhenSeveralTagsFail_ThrowsNamingEveryOneAndItsReason()
    {
        // Arrange
        var tagManager = TagManagerFor(
            (Speed, TagWriting(LogixTagWriteResult.Failed("tag is read-only"))),
            (Level, TagWriting(LogixTagWriteResult.Failed("tag not found"))));
        var batch = new LogixWriteBatch([Speed.CreateLogixValue(42), Level.CreateLogixValue(7)], tagManager);

        // Act
        var write = await Record.ExceptionAsync(() => batch.WriteAsync(CancellationToken.None));

        // Assert
        // Awaiting Task.WhenAll would surface only the first failure of the set, and a caller re-driving
        // the writes it was told about would leave the rest dropped and unmentioned.
        var message = write.Should().BeOfType<LogixTagException>().Which.Message;
        message.Should().Contain("Motor.Speed").And.Contain("tag is read-only");
        message.Should().Contain("Tank.Level").And.Contain("tag not found");
    }

    [Fact]
    public async Task WriteAsync_WhenCancelled_PropagatesTheCancellationAndWritesNothing()
    {
        // Arrange
        var speedWrites = new List<byte[]>();
        var speedTag = TagWriting(LogixTagWriteResult.Ok(), speedWrites);
        var batch = new LogixWriteBatch([Speed.CreateLogixValue(42)], TagManagerFor((Speed, speedTag)));
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var write = await Record.ExceptionAsync(() => batch.WriteAsync(cts.Token));

        // Assert
        // Cancelling is the caller's decision, not the device's refusal, so it travels as itself rather
        // than as the LogixTagException a controller that would not take the write produces.
        write.Should().BeAssignableTo<OperationCanceledException>();

        // Asserted on the bytes the tag was handed rather than on DidNotReceive().WriteAsync(...): a
        // Received check replays the call through the substitute's return handler, which for this tag is
        // the one that honours the token — so the assertion would throw the cancellation it is asking
        // about. The handler throws before recording, so an empty list is a cancelled write that put
        // nothing on the wire.
        speedWrites.Should().BeEmpty();
    }

    [Fact]
    public async Task WriteAsync_WithNoValues_WritesNothingAndTouchesNoTag()
    {
        // Arrange
        var tagManager = TagManagerFor();
        var batch = new LogixWriteBatch([], tagManager);

        // Act
        var write = await Record.ExceptionAsync(() => batch.WriteAsync(CancellationToken.None));

        // Assert
        // A group can be emptied by configuration; an empty fan-out is not a failure, and asking the tag
        // manager for nothing costs no round trip.
        write.Should().BeNull();
        tagManager.DidNotReceiveWithAnyArgs().TagFor(default!);
    }

    [Fact]
    public void Constructor_EncodesEveryValueAndResolvesItsTagWithoutWritingAnything()
    {
        // Arrange
        var speedTag = TagWriting(LogixTagWriteResult.Ok());
        var levelTag = TagWriting(LogixTagWriteResult.Ok());
        var tagManager = TagManagerFor((Speed, speedTag), (Level, levelTag));

        // Act
        _ = new LogixWriteBatch([Speed.CreateLogixValue(42), Level.CreateLogixValue(7)], tagManager);

        // Assert
        // Holding a batch means holding a fully encoded one — the values are bytes and the tags are
        // resolved up front — but encoding is not writing, and no I/O happens until WriteAsync.
        tagManager.Received(1).TagFor(Speed);
        tagManager.Received(1).TagFor(Level);
        speedTag.ReceivedCalls().Should().BeEmpty();
        levelTag.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WhenAValueWillNotEncode_ThrowsWithoutWritingAnySibling()
    {
        // Arrange
        // One good value and one the converter refuses. The good one is exactly what a partial write
        // would consist of, so this is the case that says whether one is possible.
        var speedTag = TagWriting(LogixTagWriteResult.Ok());
        var tagManager = TagManagerFor(
            (Speed, speedTag),
            (Label, TagWriting(LogixTagWriteResult.Ok())));

        // Act
        var construct = Record.Exception(() => new LogixWriteBatch(
            [Speed.CreateLogixValue(42), Label.CreateLogixValue(TooLongForAString)], tagManager));

        // Assert
        // The encode needs nothing from the controller, so it happens before any write is issued: a value
        // that will not encode fails the whole batch instead of leaving half of it written.
        construct.Should().BeOfType<LogixTagException>()
            .Which.Message.Should().Contain("Line.Label").And.Contain("Nothing was sent");
        speedTag.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WhenSeveralValuesWillNotEncode_ThrowsNamingEveryOne()
    {
        // Arrange
        var tagManager = TagManagerFor(
            (Label, TagWriting(LogixTagWriteResult.Ok())),
            (Recipe, TagWriting(LogixTagWriteResult.Ok())));

        // Act
        var construct = Record.Exception(() => new LogixWriteBatch(
            [Label.CreateLogixValue(TooLongForAString), Recipe.CreateLogixValue(TooLongForAString)], tagManager));

        // Assert
        // Collected rather than thrown on the spot, for the same reason a failed device write is: a caller
        // told about the first bad value out of two would fix one and be back.
        var message = construct.Should().BeOfType<LogixTagException>().Which.Message;
        message.Should().Contain("Line.Label");
        message.Should().Contain("Line.Recipe");
    }

    [Fact]
    public void Constructor_WhenADataPointHasNoConverter_ThrowsBeforeAnyWriteIsPossible()
    {
        // Arrange
        // Both points have a tag, so the only thing left that can throw is the missing converter.
        var speedTag = TagWriting(LogixTagWriteResult.Ok());
        var unconvertible = new UnregisteredDataPoint();
        var tagManager = TagManagerFor(
            (Speed, speedTag),
            (unconvertible, TagWriting(LogixTagWriteResult.Ok())));

        // Act
        var construct = Record.Exception(() => new LogixWriteBatch(
            [Speed.CreateLogixValue(42), unconvertible.CreateLogixValue(1)], tagManager));

        // Assert
        // A data point wired up without a converter is a configuration error rather than one tag's bad
        // value, so it travels as itself instead of joining the encode failures.
        construct.Should().BeOfType<InvalidOperationException>();
        speedTag.ReceivedCalls().Should().BeEmpty();
    }

    // A tag that answers with one prepared result and, when asked, records the bytes it was handed.
    // Cancellation is honoured the way the real access honours it — by throwing rather than by coming home
    // as a failed result, because a cancelled operation is not a device answer.
    private static ILogixTag TagWriting(LogixTagWriteResult result, List<byte[]>? writtenBuffers = null)
    {
        var tag = Substitute.For<ILogixTag>();
        tag.WriteAsync(Arg.Any<byte[]>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            call.Arg<CancellationToken>().ThrowIfCancellationRequested();
            writtenBuffers?.Add(call.Arg<byte[]>());
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

        internal override ILogixDataPointValue<int> CreateLogixValue(int value) => new Value(this, value);

        private sealed record Value(ILogixDataPoint DataPoint, int TypedValue) : ILogixDataPointValue<int>
        {
            public bool IsInValueRange() => true;
        }
    }
}
