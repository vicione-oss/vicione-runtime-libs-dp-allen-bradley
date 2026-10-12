using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Tags;
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

    private static readonly StringDataPoint Label =
        new(TagPath.Parse("Line.Label"), DefaultPollFrequency, NoChannels, StringMaxLength.Standard);

    private static readonly StringDataPoint Recipe =
        new(TagPath.Parse("Line.Recipe"), DefaultPollFrequency, NoChannels, StringMaxLength.Standard);

    private static readonly string TooLongForAString = new('X', StringMaxLength.Standard.Value + 1);

    [Fact]
    public async Task ATagThatWillNotTakeItsValueIsNamedInTheThrowWithItsReason()
    {
        // Arrange
        var tagManager = TagManagerFor(
            (Speed, TagWriting(TagWriteResult.Ok())),
            (Level, TagWriting(TagWriteResult.Failed(TagIsReadOnly))));
        var batch = new LogixWriteBatch([Speed.CreateLogixValue(42), Level.CreateLogixValue(7)], tagManager);

        // Act
        var writing = batch.Awaiting(writeBatch => writeBatch.WriteAsync(CancellationToken.None));

        // Assert
        (await writing.Should().ThrowAsync<LogixTagException>())
            .Which.Message.Should().Contain(Level.TagAddress.Value).And.Contain(TagIsReadOnly);
    }

    [Fact]
    public async Task EveryTagThatWillNotTakeItsValueIsNamedInOneThrow()
    {
        // Arrange
        var tagManager = TagManagerFor(
            (Speed, TagWriting(TagWriteResult.Failed(TagIsReadOnly))),
            (Level, TagWriting(TagWriteResult.Failed(TagNotFound))));
        var batch = new LogixWriteBatch([Speed.CreateLogixValue(42), Level.CreateLogixValue(7)], tagManager);

        // Act
        var writing = batch.Awaiting(writeBatch => writeBatch.WriteAsync(CancellationToken.None));

        // Assert
        var message = (await writing.Should().ThrowAsync<LogixTagException>()).Which.Message;
        message.Should().Contain(Speed.TagAddress.Value).And.Contain(TagIsReadOnly);
        message.Should().Contain(Level.TagAddress.Value).And.Contain(TagNotFound);
    }

    [Fact]
    public void AValueThatWillNotEncodeIsRefusedWithoutASiblingReachingItsTag()
    {
        // Arrange
        var speedTag = TagWriting(TagWriteResult.Ok());
        var tagManager = TagManagerFor(
            (Speed, speedTag),
            (Label, TagWriting(TagWriteResult.Ok())));

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
        var speedTag = TagWriting(TagWriteResult.Ok());
        var delay = new TimerDataPoint(TagPath.Parse("Line.Delay"), DefaultPollFrequency, NoChannels);
        var tagManager = TagManagerFor(
            (Speed, speedTag),
            (delay, TagWriting(TagWriteResult.Ok())));

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
            (Label, TagWriting(TagWriteResult.Ok())),
            (Recipe, TagWriting(TagWriteResult.Ok())));

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
        var speedTag = TagWriting(TagWriteResult.Ok());
        var unconvertible = new UnregisteredDataPoint();
        var tagManager = TagManagerFor(
            (Speed, speedTag),
            (unconvertible, TagWriting(TagWriteResult.Ok())));

        // Act
        var construct = Record.Exception(() => new LogixWriteBatch(
            [Speed.CreateLogixValue(42), unconvertible.CreateLogixValue(1)], tagManager));

        // Assert
        construct.Should().BeOfType<InvalidOperationException>();
        speedTag.ReceivedCalls().Should().BeEmpty();
    }

    // The real access honours cancellation by throwing rather than returning a failed result.
    private static ILogixTag TagWriting(TagWriteResult result)
    {
        var tag = Substitute.For<ILogixTag>();
        tag.WriteAsync(Arg.Any<byte[]>(), Arg.Any<CancellationToken>()).Returns(call =>
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

        internal override ILogixDataPointValue<int> CreateLogixValue(int value) => new Value(this, value);

        private sealed record Value(ILogixDataPoint DataPoint, int TypedValue) : ILogixDataPointValue<int>
        {
            public bool IsInValueRange() => true;
        }
    }
}
