using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client;

public sealed class LogixReadBatchTests
{
    private const string TagNotFound = "tag not found";

    private static readonly DIntDataPoint Speed = new(TagPath.Parse("Motor.Speed"), DefaultPollFrequency, NoChannels);
    private static readonly DIntDataPoint Level = new(TagPath.Parse("Tank.Level"), DefaultPollFrequency, NoChannels);

    private static readonly IntArrayDataPoint Readings = new(
        TagPath.Parse("Tank.Readings"), DefaultPollFrequency, NoChannels, new ElementCount(10));

    // Not BitConverter: that would re-derive them through the assumption the converter itself makes.
    private static readonly byte[] FortyTwoAsDint = [42, 0, 0, 0];

    [Fact]
    public async Task AnArrayReplyTooShortForItsDeclaredCountCostsOnlyItsOwnValue()
    {
        // Arrange
        var tooShortForTenInts = new byte[12];
        var tagManager = TagManagerFor(
            (Speed, TagReading(TagReadResult.Ok(FortyTwoAsDint))),
            (Readings, TagReading(TagReadResult.Ok(tooShortForTenInts))));
        var batch = new LogixReadBatch([Speed, Readings], tagManager);

        // Act
        var batchRead = await batch.ReadAsync(CancellationToken.None);

        // Assert
        batchRead.Values.Should().Equal(Speed.CreateLogixValue(42));
        batchRead.Failures.Should().ContainSingle().Which.DataPoint.Should().Be(Readings);
    }

    [Fact]
    public async Task AGroupWhereNoTagAnswersThrowsNamingEveryFailedTag()
    {
        // Arrange
        var tagManager = TagManagerFor(
            (Speed, TagReading(TagReadResult.Failed("tag is write-only"))),
            (Level, TagReading(TagReadResult.Failed(TagNotFound))));
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
            (Speed, TagReading(TagReadResult.Ok(tooShortForADint))),
            (Level, TagReading(TagReadResult.Failed(TagNotFound))));
        var batch = new LogixReadBatch([Speed, Level], tagManager);

        // Act
        var reading = batch.Awaiting(readBatch => readBatch.ReadAsync(CancellationToken.None));

        // Assert
        var message = (await reading.Should().ThrowAsync<LogixTagException>()).Which.Message;
        message.Should().Contain(Speed.TagAddress.Value);
        message.Should().Contain(Level.TagAddress.Value).And.Contain(TagNotFound);
    }

    [Fact]
    public void ADataPointWithoutAConverterIsRefusedBeforeAnyReadIsPossible()
    {
        // Arrange
        var speedTag = TagReading(TagReadResult.Ok(FortyTwoAsDint));
        var unconvertible = new UnregisteredDataPoint();
        var tagManager = TagManagerFor(
            (Speed, speedTag),
            (unconvertible, TagReading(TagReadResult.Ok(FortyTwoAsDint))));

        // Act
        var construct = Record.Exception(() => new LogixReadBatch([Speed, unconvertible], tagManager));

        // Assert
        construct.Should().BeOfType<InvalidOperationException>();
        speedTag.ReceivedCalls().Should().BeEmpty();
    }

    // The real access honours cancellation by throwing rather than returning a failed result.
    private static ILogixTag TagReading(TagReadResult result)
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
        public override AllenBradleyDataType DataType => new(new DataTypeName("MYSTERY"), LogixGeneration.Logix5X70);

        internal override ILogixDataPointValue<int> CreateLogixValue(int value) => throw new NotSupportedException();
    }
}
