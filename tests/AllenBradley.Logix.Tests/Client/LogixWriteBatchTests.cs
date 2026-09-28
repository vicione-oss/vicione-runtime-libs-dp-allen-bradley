using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Timers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client;

public sealed class LogixWriteBatchTests
{
    private const string TagIsReadOnly = "tag is read-only";
    private const string TagNotFound = "tag not found";

    private static readonly DIntDataPoint Speed = new(TagPath.Parse("Motor.Speed"), DefaultPollFrequency, NoChannels);
    private static readonly DIntDataPoint Level = new(TagPath.Parse("Tank.Level"), DefaultPollFrequency, NoChannels);
    private static readonly DIntDataPoint Torque = new(TagPath.Parse("Motor.Torque"), DefaultPollFrequency, NoChannels);

    private static readonly StringDataPoint Label =
        new(TagPath.Parse("Line.Label"), DefaultPollFrequency, NoChannels, StringMaxLength.Standard);

    private static readonly StringDataPoint Recipe =
        new(TagPath.Parse("Line.Recipe"), DefaultPollFrequency, NoChannels, StringMaxLength.Standard);

    // Not BitConverter: that would re-derive them through the assumption the converter itself makes.
    private static readonly byte[] FortyTwoAsDint = [42, 0, 0, 0];
    private static readonly byte[] SevenAsDint = [7, 0, 0, 0];

    private static readonly string TooLongForAString = new('X', StringMaxLength.Standard.Value + 1);

    [Fact]
    public async Task EveryValueReachesItsOwnTagAsTheBytesItsConverterProduced()
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
        // The writes are fanned out concurrently, so the pairing made at construction is what holds.
        speedWrites.Should().ContainSingle().Which.Should().Equal(FortyTwoAsDint);
        levelWrites.Should().ContainSingle().Which.Should().Equal(SevenAsDint);
    }

    [Fact]
    public async Task ATagThatWillNotTakeItsValueIsNamedInTheThrowWithItsReason()
    {
        // Arrange
        var tagManager = TagManagerFor(
            (Speed, TagWriting(LogixTagWriteResult.Ok())),
            (Level, TagWriting(LogixTagWriteResult.Failed(TagIsReadOnly))));
        var batch = new LogixWriteBatch([Speed.CreateLogixValue(42), Level.CreateLogixValue(7)], tagManager);

        // Act
        var writing = batch.Awaiting(writeBatch => writeBatch.WriteAsync(CancellationToken.None));

        // Assert
        (await writing.Should().ThrowAsync<LogixTagException>())
            .Which.Message.Should().Contain(Level.TagAddress.Value).And.Contain(TagIsReadOnly);
    }

    [Fact]
    public async Task ABatchWithAFailingTagStillWritesEverySibling()
    {
        // Arrange
        var speedTag = TagWriting(LogixTagWriteResult.Ok());
        var torqueTag = TagWriting(LogixTagWriteResult.Ok());
        var tagManager = TagManagerFor(
            (Speed, speedTag),
            (Level, TagWriting(LogixTagWriteResult.Failed(TagNotFound))),
            (Torque, torqueTag));
        var batch = new LogixWriteBatch(
            [Speed.CreateLogixValue(42), Level.CreateLogixValue(7), Torque.CreateLogixValue(7)], tagManager);

        // Act
        _ = await Record.ExceptionAsync(() => batch.WriteAsync(CancellationToken.None));

        // Assert
        await speedTag.Received(1).WriteAsync(Arg.Any<byte[]>(), Arg.Any<CancellationToken>());
        await torqueTag.Received(1).WriteAsync(Arg.Any<byte[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EveryTagThatWillNotTakeItsValueIsNamedInOneThrow()
    {
        // Arrange
        var tagManager = TagManagerFor(
            (Speed, TagWriting(LogixTagWriteResult.Failed(TagIsReadOnly))),
            (Level, TagWriting(LogixTagWriteResult.Failed(TagNotFound))));
        var batch = new LogixWriteBatch([Speed.CreateLogixValue(42), Level.CreateLogixValue(7)], tagManager);

        // Act
        var writing = batch.Awaiting(writeBatch => writeBatch.WriteAsync(CancellationToken.None));

        // Assert
        var message = (await writing.Should().ThrowAsync<LogixTagException>()).Which.Message;
        message.Should().Contain(Speed.TagAddress.Value).And.Contain(TagIsReadOnly);
        message.Should().Contain(Level.TagAddress.Value).And.Contain(TagNotFound);
    }

    [Fact]
    public async Task ACancelledBatchThrowsTheCancellationAndPutsNothingOnTheWire()
    {
        // Arrange
        var speedWrites = new List<byte[]>();
        var tagManager = TagManagerFor((Speed, TagWriting(LogixTagWriteResult.Ok(), speedWrites)));
        var batch = new LogixWriteBatch([Speed.CreateLogixValue(42)], tagManager);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var write = await Record.ExceptionAsync(() => batch.WriteAsync(cts.Token));

        // Assert
        // DidNotReceive replays the call, which would throw the very cancellation being asserted.
        write.Should().BeAssignableTo<OperationCanceledException>();
        speedWrites.Should().BeEmpty();
    }

    [Fact]
    public async Task AnEmptyGroupWritesNothingAndTouchesNoTag()
    {
        // Arrange
        var tagManager = TagManagerFor();
        var batch = new LogixWriteBatch([], tagManager);

        // Act
        var write = await Record.ExceptionAsync(() => batch.WriteAsync(CancellationToken.None));

        // Assert
        write.Should().BeNull();
        tagManager.DidNotReceiveWithAnyArgs().TagFor(default!);
    }

    [Fact]
    public void ConstructingABatchEncodesEveryValueAndResolvesItsTagWithoutWritingAnything()
    {
        // Arrange
        var speedTag = TagWriting(LogixTagWriteResult.Ok());
        var levelTag = TagWriting(LogixTagWriteResult.Ok());
        var tagManager = TagManagerFor((Speed, speedTag), (Level, levelTag));

        // Act
        _ = new LogixWriteBatch([Speed.CreateLogixValue(42), Level.CreateLogixValue(7)], tagManager);

        // Assert
        tagManager.Received(1).TagFor(Speed);
        tagManager.Received(1).TagFor(Level);
        speedTag.ReceivedCalls().Should().BeEmpty();
        levelTag.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public void AValueThatWillNotEncodeIsRefusedWithoutASiblingReachingItsTag()
    {
        // Arrange
        var speedTag = TagWriting(LogixTagWriteResult.Ok());
        var tagManager = TagManagerFor(
            (Speed, speedTag),
            (Label, TagWriting(LogixTagWriteResult.Ok())));

        // Act
        var construct = Record.Exception(() => new LogixWriteBatch(
            [Speed.CreateLogixValue(42), Label.CreateLogixValue(TooLongForAString)], tagManager));

        // Assert
        construct.Should().BeOfType<LogixTagException>()
            .Which.Message.Should().Contain(Label.TagAddress.Value).And.Contain("Nothing was sent");
        speedTag.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public void ANegativeAccumulatedTimeRefusesTheWholeBatchNamingTheTimer()
    {
        // Arrange
        var speedTag = TagWriting(LogixTagWriteResult.Ok());
        var delay = new TimerDataPoint(TagPath.Parse("Line.Delay"), DefaultPollFrequency, NoChannels);
        var tagManager = TagManagerFor(
            (Speed, speedTag),
            (delay, TagWriting(LogixTagWriteResult.Ok())));

        // Act
        var construct = Record.Exception(() => new LogixWriteBatch(
            [Speed.CreateLogixValue(42), delay.CreateLogixValue(-1)], tagManager));

        // Assert
        construct.Should().BeOfType<LogixTagException>()
            .Which.Message.Should().Contain(delay.TagAddress.Value).And.Contain("Nothing was sent");
        speedTag.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public void EveryValueThatWillNotEncodeIsNamedInOneRefusal()
    {
        // Arrange
        var tagManager = TagManagerFor(
            (Label, TagWriting(LogixTagWriteResult.Ok())),
            (Recipe, TagWriting(LogixTagWriteResult.Ok())));

        // Act
        var construct = Record.Exception(() => new LogixWriteBatch(
            [Label.CreateLogixValue(TooLongForAString), Recipe.CreateLogixValue(TooLongForAString)], tagManager));

        // Assert
        var message = construct.Should().BeOfType<LogixTagException>().Which.Message;
        message.Should().Contain(Label.TagAddress.Value);
        message.Should().Contain(Recipe.TagAddress.Value);
    }

    [Fact]
    public void ADataPointWithoutAConverterIsRefusedBeforeAnyWriteIsPossible()
    {
        // Arrange
        var speedTag = TagWriting(LogixTagWriteResult.Ok());
        var unconvertible = new UnregisteredDataPoint();
        var tagManager = TagManagerFor(
            (Speed, speedTag),
            (unconvertible, TagWriting(LogixTagWriteResult.Ok())));

        // Act
        var construct = Record.Exception(() => new LogixWriteBatch(
            [Speed.CreateLogixValue(42), unconvertible.CreateLogixValue(1)], tagManager));

        // Assert
        construct.Should().BeOfType<InvalidOperationException>();
        speedTag.ReceivedCalls().Should().BeEmpty();
    }

    // The real access honours cancellation by throwing rather than returning a failed result.
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

    // A data point shape deliberately absent from DataPointConverterRegistry.
    private sealed record UnregisteredDataPoint()
        : LogixDataPoint<int>(TagPath.Parse("Mystery.RootTagName"), DefaultPollFrequency, NoChannels)
    {
        public override AllenBradleyDataType DataType => new AllenBradleyDataType(new DataTypeName("MYSTERY"), LogixGeneration.Logix5X70);

        internal override ILogixDataPointValue<int> CreateLogixValue(int value) => new Value(this, value);

        private sealed record Value(ILogixDataPoint DataPoint, int TypedValue) : ILogixDataPointValue<int>
        {
            public bool IsInValueRange() => true;
        }
    }
}
