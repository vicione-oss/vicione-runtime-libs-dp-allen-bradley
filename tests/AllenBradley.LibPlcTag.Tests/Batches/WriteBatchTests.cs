using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Tags;
using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Tests.TestData;
using FakeWriteBatch = ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Batches.WriteBatch<
    ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Tests.TestData.FakeDataPoint,
    ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Tests.TestData.FakeValue>;

namespace ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Tests.Batches;

public sealed class WriteBatchTests
{
    private const string TagIsReadOnly = "tag is read-only";
    private const string TagNotFound = "tag not found";

    private static readonly FakeDataPoint Speed = new("Motor.Speed");
    private static readonly FakeDataPoint Level = new("Tank.Level");
    private static readonly FakeDataPoint Torque = new("Motor.Torque");

    // Not BitConverter: that would re-derive them through the assumption the converter itself makes.
    private static readonly byte[] FortyTwoAsInt32 = [42, 0, 0, 0];
    private static readonly byte[] SevenAsInt32 = [7, 0, 0, 0];

    [Fact]
    public async Task EveryValueReachesItsOwnTagAsTheBytesItsConverterProduced()
    {
        // Arrange
        var speedWrites = new List<byte[]>();
        var levelWrites = new List<byte[]>();
        var tagSource = TagSourceFor(
            (Speed, TagWriting(TagWriteResult.Ok(), speedWrites)),
            (Level, TagWriting(TagWriteResult.Ok(), levelWrites)));
        var batch = CreateBatch([new(Speed, 42), new(Level, 7)], tagSource);

        // Act
        await batch.WriteAsync(CancellationToken.None);

        // Assert
        // The writes are fanned out concurrently, so the pairing made at construction is what holds.
        speedWrites.Should().ContainSingle().Which.Should().Equal(FortyTwoAsInt32);
        levelWrites.Should().ContainSingle().Which.Should().Equal(SevenAsInt32);
    }

    [Fact]
    public async Task ATagThatWillNotTakeItsValueComesHomeAsAFailureWithItsReason()
    {
        // Arrange
        var tagSource = TagSourceFor(
            (Speed, TagWriting(TagWriteResult.Ok())),
            (Level, TagWriting(TagWriteResult.Failed(TagIsReadOnly))));
        var batch = CreateBatch([new(Speed, 42), new(Level, 7)], tagSource);

        // Act
        var failures = await batch.WriteAsync(CancellationToken.None);

        // Assert
        var expected = new FakeWriteBatch.WriteFailure(Level, TagIsReadOnly);
        failures.Should().Equal(expected);
    }

    [Fact]
    public async Task ABatchWithAFailingTagStillWritesEverySibling()
    {
        // Arrange
        var speedTag = TagWriting(TagWriteResult.Ok());
        var torqueTag = TagWriting(TagWriteResult.Ok());
        var tagSource = TagSourceFor(
            (Speed, speedTag),
            (Level, TagWriting(TagWriteResult.Failed(TagNotFound))),
            (Torque, torqueTag));
        var batch = CreateBatch([new(Speed, 42), new(Level, 7), new(Torque, 7)], tagSource);

        // Act
        await batch.WriteAsync(CancellationToken.None);

        // Assert
        await speedTag.Received(1).WriteAsync(Arg.Any<byte[]>(), Arg.Any<CancellationToken>());
        await torqueTag.Received(1).WriteAsync(Arg.Any<byte[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void EveryFailedTagIsDescribedByItsIdentifierAndItsReason()
    {
        // Arrange
        FakeWriteBatch.WriteFailure[] failures =
        [
            new(Speed, TagIsReadOnly),
            new(Level, TagNotFound),
        ];

        // Act
        var description = FakeWriteBatch.Describe(failures);

        // Assert
        description.Should().Be("Motor.Speed: tag is read-only; Tank.Level: tag not found");
    }

    [Fact]
    public async Task ACancelledBatchThrowsTheCancellationAndPutsNothingOnTheWire()
    {
        // Arrange
        var speedWrites = new List<byte[]>();
        var tagSource = TagSourceFor((Speed, TagWriting(TagWriteResult.Ok(), speedWrites)));
        var batch = CreateBatch([new(Speed, 42)], tagSource);
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
        var tagSource = TagSourceFor();
        var batch = CreateBatch([], tagSource);

        // Act
        var failures = await batch.WriteAsync(CancellationToken.None);

        // Assert
        failures.Should().BeEmpty();
        tagSource.DidNotReceiveWithAnyArgs().TagFor(default!);
    }

    [Fact]
    public void ConstructingABatchEncodesEveryValueAndResolvesItsTagWithoutWritingAnything()
    {
        // Arrange
        var speedTag = TagWriting(TagWriteResult.Ok());
        var levelTag = TagWriting(TagWriteResult.Ok());
        var tagSource = TagSourceFor((Speed, speedTag), (Level, levelTag));

        // Act
        var batch = CreateBatch([new(Speed, 42), new(Level, 7)], tagSource);

        // Assert
        batch.EncodeFailures.Should().BeEmpty();
        tagSource.Received(1).TagFor(Speed);
        tagSource.Received(1).TagFor(Level);
        speedTag.ReceivedCalls().Should().BeEmpty();
        levelTag.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public void EveryValueThatWillNotEncodeIsNamedAmongTheEncodeFailuresWithItsReason()
    {
        // Arrange
        var tagSource = TagSourceFor(
            (Speed, TagWriting(TagWriteResult.Ok())),
            (Level, TagWriting(TagWriteResult.Ok())),
            (Torque, TagWriting(TagWriteResult.Ok())));

        // Act
        var batch = CreateBatch([new(Speed, -1), new(Level, 7), new(Torque, -2)], tagSource);

        // Assert
        FakeWriteBatch.WriteFailure[] expected =
        [
            new(Speed, FakeConverter.NegativeRefusal),
            new(Torque, FakeConverter.NegativeRefusal),
        ];
        batch.EncodeFailures.Should().Equal(expected);
    }

    [Fact]
    public async Task AValueThatWillNotEncodeIsLeftOutWhileItsSiblingsAreWritten()
    {
        // Arrange
        var speedTag = TagWriting(TagWriteResult.Ok());
        var levelTag = TagWriting(TagWriteResult.Ok());
        var tagSource = TagSourceFor((Speed, speedTag), (Level, levelTag));
        var batch = CreateBatch([new(Speed, -1), new(Level, 7)], tagSource);

        // Act
        await batch.WriteAsync(CancellationToken.None);

        // Assert
        speedTag.ReceivedCalls().Should().BeEmpty();
        await levelTag.Received(1).WriteAsync(Arg.Any<byte[]>(), Arg.Any<CancellationToken>());
    }

    private static FakeWriteBatch CreateBatch(
        IReadOnlyList<FakeValue> values, ITagSource<FakeDataPoint, ITagAccess> tagSource) =>
        new(values, value => value.DataPoint, tagSource, _ => new FakeConverter());

    // The real access honours cancellation by throwing rather than returning a failed result.
    private static ITagAccess TagWriting(TagWriteResult result, List<byte[]>? writtenBuffers = null)
    {
        var tag = Substitute.For<ITagAccess>();
        tag.WriteAsync(Arg.Any<byte[]>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            call.Arg<CancellationToken>().ThrowIfCancellationRequested();
            writtenBuffers?.Add(call.Arg<byte[]>());
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
}
