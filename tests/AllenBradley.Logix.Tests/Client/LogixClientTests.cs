using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using ViciOne.Suite.DataPort.Extensions.Testing.Logging;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixClientTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TagDefinitionTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client;

// Every act captures the `using var client` and is invoked before the test method returns.
// ReSharper disable AccessToDisposedClosure
public sealed class LogixClientTests
{
    private const string TagIsReadOnly = "tag is read-only";
    private const string TagIsWriteOnly = "tag is write-only";
    private const string TagNotFound = "tag not found";
    private const int StringStructureSize = 88;

    private static readonly DIntDataPoint Speed = new(new TagName("Motor.Speed"), DefaultPollFrequency, NoChannels);
    private static readonly DIntDataPoint Level = new(new TagName("Tank.Level"), DefaultPollFrequency, NoChannels);

    private static readonly StringDataPoint Label =
        new(new TagName("Line.Label"), DefaultPollFrequency, NoChannels, StringMaxLength.Standard);

    // Not BitConverter: that would re-derive them through the assumption the converter itself makes.
    private static readonly byte[] FortyTwoAsDint = [42, 0, 0, 0];
    private static readonly byte[] SevenAsDint = [7, 0, 0, 0];

    [Fact]
    public async Task AGroupWithAFailingTagPublishesTheValuesItsSiblingsProduced()
    {
        // Arrange
        var tagManager = TagManagerFor(
            FakeTag.Reading(Speed, AtomicMetadataFor(Speed), LogixTagReadResult.Ok(FortyTwoAsDint)),
            FakeTag.Reading(Level, AtomicMetadataFor(Level), LogixTagReadResult.Failed(TagNotFound)));
        using var client = CreateClient(tagManager);

        // Act
        var values = await client.ReadAsync(GroupOf(Speed, Level), CancellationToken.None);

        // Assert
        ILogixDataPointValue[] expected = [Speed.CreateLogixValue(42)];
        values.Should().Equal(expected);
    }

    [Fact]
    public async Task AFailingTagDoesNotStopItsSiblingsFromBeingRead()
    {
        // Arrange
        var speedTag = FakeTag.Reading(Speed, AtomicMetadataFor(Speed), LogixTagReadResult.Ok(FortyTwoAsDint));
        var tagManager = TagManagerFor(
            speedTag,
            FakeTag.Reading(Level, AtomicMetadataFor(Level), LogixTagReadResult.Failed(TagNotFound)));
        using var client = CreateClient(tagManager);

        // Act
        await client.ReadAsync(GroupOf(Speed, Level), CancellationToken.None);

        // Assert
        speedTag.WasRead.Should().BeTrue();
    }

    [Fact]
    public async Task ACancelledReadThrowsTheCancellationAndTouchesNoTag()
    {
        // Arrange
        var speedTag = FakeTag.Reading(Speed, AtomicMetadataFor(Speed), LogixTagReadResult.Ok(FortyTwoAsDint));
        using var client = CreateClient(TagManagerFor(speedTag));
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var read = await Record.ExceptionAsync(() => client.ReadAsync(GroupOf(Speed), cts.Token).AsTask());

        // Assert
        read.Should().BeAssignableTo<OperationCanceledException>();
        speedTag.WasRead.Should().BeFalse();
    }

    [Fact]
    public async Task AGroupWhereNoTagAnswersThrowsNamingEveryFailedTag()
    {
        // Arrange
        var tagManager = TagManagerFor(
            FakeTag.Reading(Speed, AtomicMetadataFor(Speed), LogixTagReadResult.Failed(TagIsWriteOnly)),
            FakeTag.Reading(Level, AtomicMetadataFor(Level), LogixTagReadResult.Failed(TagNotFound)));
        using var client = CreateClient(tagManager);

        // Act
        var reading = client.Awaiting(c => c.ReadAsync(GroupOf(Speed, Level), CancellationToken.None));

        // Assert
        var message = (await reading.Should().ThrowAsync<LogixTagException>()).Which.Message;
        message.Should().Contain(Speed.TagName.Value).And.Contain(TagIsWriteOnly);
        message.Should().Contain(Level.TagName.Value).And.Contain(TagNotFound);
    }

    [Fact]
    public async Task AReplyTooShortForItsTypeCostsThatTagItsValueAndNoOther()
    {
        // Arrange
        var tooShortForADint = new byte[2];
        var tagManager = TagManagerFor(
            FakeTag.Reading(Speed, AtomicMetadataFor(Speed), LogixTagReadResult.Ok(tooShortForADint)),
            FakeTag.Reading(Level, AtomicMetadataFor(Level), LogixTagReadResult.Ok(FortyTwoAsDint)));
        using var client = CreateClient(tagManager);

        // Act
        var values = await client.ReadAsync(GroupOf(Speed, Level), CancellationToken.None);

        // Assert
        ILogixDataPointValue[] expected = [Level.CreateLogixValue(42)];
        values.Should().Equal(expected);
    }

    [Fact]
    public async Task AGroupWhereEveryTagAnswersPublishesOneValuePerPointInGroupOrder()
    {
        // Arrange
        var tagManager = TagManagerFor(
            FakeTag.Reading(Speed, AtomicMetadataFor(Speed), LogixTagReadResult.Ok(FortyTwoAsDint)),
            FakeTag.Reading(Level, AtomicMetadataFor(Level), LogixTagReadResult.Ok(SevenAsDint)));
        using var client = CreateClient(tagManager);

        // Act
        var values = await client.ReadAsync(GroupOf(Speed, Level), CancellationToken.None);

        // Assert
        // The reads are fanned out concurrently, so group order is what puts the values back in order.
        ILogixDataPointValue[] expected = [Speed.CreateLogixValue(42), Level.CreateLogixValue(7)];
        values.Should().Equal(expected);
    }

    [Fact]
    public async Task ADataPointWithoutAConverterIsRefusedBeforeAnyTagIsRead()
    {
        // Arrange
        var speedTag = FakeTag.Reading(Speed, AtomicMetadataFor(Speed), LogixTagReadResult.Ok(FortyTwoAsDint));
        var unconvertible = new UnregisteredDataPoint();
        var tagManager = TagManagerFor(
            speedTag,
            FakeTag.Reading(unconvertible, metadata: null, LogixTagReadResult.Ok(FortyTwoAsDint)));
        using var client = CreateClient(tagManager);

        // Act
        var read = await Record.ExceptionAsync(
            () => client.ReadAsync(GroupOf(Speed, unconvertible), CancellationToken.None).AsTask());

        // Assert
        read.Should().BeOfType<InvalidOperationException>();
        speedTag.WasRead.Should().BeFalse();
    }

    [Fact]
    public async Task AStringStructureReadFromTheControllerComesHomeAsItsText()
    {
        // Arrange
        var structure = new byte[StringStructureSize];
        structure[0] = 2;
        structure[4] = (byte)'H';
        structure[5] = (byte)'i';
        var tagManager = TagManagerFor(
            FakeTag.Reading(Label, StringMetadataFor(Label), LogixTagReadResult.Ok(structure)));
        using var client = CreateClient(tagManager);

        // Act
        var values = await client.ReadAsync(GroupOf(Label), CancellationToken.None);

        // Assert
        ILogixDataPointValue[] expected = [Label.CreateLogixValue("Hi")];
        values.Should().Equal(expected);
    }

    [Fact]
    public async Task ATagThatWillNotTakeItsValueIsNamedInTheThrowWithItsReason()
    {
        // Arrange
        var tagManager = TagManagerFor(
            FakeTag.Writing(Speed, AtomicMetadataFor(Speed), LogixTagWriteResult.Failed(TagIsReadOnly)));
        using var client = CreateClient(tagManager);
        ILogixDataPointValue[] values = [Speed.CreateLogixValue(42)];

        // Act
        var writing = client.Awaiting(c => c.WriteAsync(values, CancellationToken.None));

        // Assert
        (await writing.Should().ThrowAsync<LogixTagException>())
            .Which.Message.Should().Contain(Speed.TagName.Value).And.Contain(TagIsReadOnly);
    }

    [Fact]
    public async Task EveryTagThatWillNotTakeItsValueIsNamedInOneThrow()
    {
        // Arrange
        var tagManager = TagManagerFor(
            FakeTag.Writing(Speed, AtomicMetadataFor(Speed), LogixTagWriteResult.Failed(TagIsReadOnly)),
            FakeTag.Writing(Level, AtomicMetadataFor(Level), LogixTagWriteResult.Failed(TagNotFound)));
        using var client = CreateClient(tagManager);
        ILogixDataPointValue[] values = [Speed.CreateLogixValue(42), Level.CreateLogixValue(7)];

        // Act
        var writing = client.Awaiting(c => c.WriteAsync(values, CancellationToken.None));

        // Assert
        var message = (await writing.Should().ThrowAsync<LogixTagException>()).Which.Message;
        message.Should().Contain(Speed.TagName.Value).And.Contain(TagIsReadOnly);
        message.Should().Contain(Level.TagName.Value).And.Contain(TagNotFound);
    }

    [Fact]
    public async Task ACancelledWriteThrowsTheCancellationAndPutsNothingOnTheWire()
    {
        // Arrange
        var speedTag = FakeTag.Writing(Speed, AtomicMetadataFor(Speed), LogixTagWriteResult.Ok());
        using var client = CreateClient(TagManagerFor(speedTag));
        ILogixDataPointValue[] values = [Speed.CreateLogixValue(42)];
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var write = await Record.ExceptionAsync(() => client.WriteAsync(values, cts.Token).AsTask());

        // Assert
        write.Should().BeAssignableTo<OperationCanceledException>();
        speedTag.Written.Should().BeNull();
    }

    [Fact]
    public async Task EveryValueThatWillNotEncodeIsNamedAndNoTagIsTouched()
    {
        // Arrange
        var shortLabel = ShortStringDataPoint("Line.Short");
        var shortCode = ShortStringDataPoint("Line.Code");
        var labelTag = FakeTag.Writing(shortLabel, StringMetadataFor(shortLabel), LogixTagWriteResult.Ok());
        var codeTag = FakeTag.Writing(shortCode, StringMetadataFor(shortCode), LogixTagWriteResult.Ok());
        using var client = CreateClient(TagManagerFor(labelTag, codeTag));
        ILogixDataPointValue[] values =
        [
            shortLabel.CreateLogixValue("far too long"),
            shortCode.CreateLogixValue("also too long"),
        ];

        // Act
        var write = await Record.ExceptionAsync(() => client.WriteAsync(values, CancellationToken.None).AsTask());

        // Assert
        write.Should().BeOfType<LogixTagException>()
            .Which.Message.Should().Contain(shortLabel.TagName.Value).And.Contain(shortCode.TagName.Value);
        labelTag.Written.Should().BeNull();
        codeTag.Written.Should().BeNull();
    }

    [Fact]
    public async Task AValueThatWillNotEncodeLeavesTheTagsThatWouldHaveTakenTheirsUntouched()
    {
        // Arrange
        var shortLabel = ShortStringDataPoint("Line.Short");
        var speedTag = FakeTag.Writing(Speed, AtomicMetadataFor(Speed), LogixTagWriteResult.Ok());
        var labelTag = FakeTag.Writing(shortLabel, StringMetadataFor(shortLabel), LogixTagWriteResult.Ok());
        using var client = CreateClient(TagManagerFor(speedTag, labelTag));
        ILogixDataPointValue[] values = [Speed.CreateLogixValue(42), shortLabel.CreateLogixValue("far too long")];

        // Act
        var write = await Record.ExceptionAsync(() => client.WriteAsync(values, CancellationToken.None).AsTask());

        // Assert
        write.Should().BeOfType<LogixTagException>().Which.Message.Should().Contain(shortLabel.TagName.Value);
        speedTag.Written.Should().BeNull();
        labelTag.Written.Should().BeNull();
    }

    [Fact]
    public async Task AValueNoDataPointMadeIsRefusedUnderItsTagsName()
    {
        // Arrange
        var speedTag = FakeTag.Writing(Speed, AtomicMetadataFor(Speed), LogixTagWriteResult.Ok());
        using var client = CreateClient(TagManagerFor(speedTag));
        ILogixDataPointValue[] values = [new ForeignDataPointValue(Speed)];

        // Act
        var write = await Record.ExceptionAsync(() => client.WriteAsync(values, CancellationToken.None).AsTask());

        // Assert
        write.Should().BeOfType<LogixTagException>().Which.Message.Should().Contain(Speed.TagName.Value);
        speedTag.Written.Should().BeNull();
    }

    [Fact]
    public async Task AWrittenValueReachesItsTagAsTheBytesItsConverterProduced()
    {
        // Arrange
        var speedTag = FakeTag.Writing(Speed, AtomicMetadataFor(Speed), LogixTagWriteResult.Ok());
        using var client = CreateClient(TagManagerFor(speedTag));
        ILogixDataPointValue[] values = [Speed.CreateLogixValue(42)];

        // Act
        await client.WriteAsync(values, CancellationToken.None);

        // Assert
        speedTag.Written.Should().Equal(FortyTwoAsDint);
    }

    [Fact]
    public async Task AWrittenStringReachesItsTagAsLenFollowedByItsWholeCapacityOfData()
    {
        // Arrange
        var labelTag = FakeTag.Writing(Label, StringMetadataFor(Label), LogixTagWriteResult.Ok());
        using var client = CreateClient(TagManagerFor(labelTag));
        ILogixDataPointValue[] values = [Label.CreateLogixValue("Hi")];

        // Act
        await client.WriteAsync(values, CancellationToken.None);

        // Assert
        // The two padding bytes that make the tag 88 on the controller belong to libplctag, not the client.
        var expected = new byte[4 + StringMaxLength.Standard.Value];
        expected[0] = 2;
        expected[4] = (byte)'H';
        expected[5] = (byte)'i';
        labelTag.Written.Should().Equal(expected);
    }

    [Fact]
    public async Task EachResolvedDataPointCarriesTheControllersDefinitionForItsTag()
    {
        // Arrange
        var tagManager = TagManagerFor(
            FakeTag.Reading(Speed, AtomicMetadataFor(Speed), LogixTagReadResult.Ok(FortyTwoAsDint)),
            FakeTag.Reading(Label, metadata: null, LogixTagReadResult.Ok(new byte[StringStructureSize])));
        using var client = CreateClient(tagManager);
        await client.ConnectAsync(CancellationToken.None);

        // Act
        var resolved = await client.ResolveDataPoints([Speed, Label], CancellationToken.None);

        // Assert
        ResolvedDataPoint[] expected =
        [
            new(Speed, AtomicMetadataFor(Speed)),
            new(Label, TagDefinition: null),
        ];
        resolved.Should().Equal(expected);
    }

    [Fact]
    public async Task ResolvingBeforeAConnectIsRefusedAndBrowsesNothing()
    {
        // Arrange
        var tagManager = TagManagerFor(
            FakeTag.Reading(Speed, AtomicMetadataFor(Speed), LogixTagReadResult.Ok(FortyTwoAsDint)));
        using var client = CreateClient(tagManager);

        // Act
        var resolve = await Record.ExceptionAsync(() => client.ResolveDataPoints([Speed], CancellationToken.None));

        // Assert
        resolve.Should().BeOfType<InvalidOperationException>();
        await tagManager.DidNotReceive().LoadTagDefinitionsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResolvingAfterADisconnectIsRefused()
    {
        // Arrange
        var tagManager = TagManagerFor(
            FakeTag.Reading(Speed, AtomicMetadataFor(Speed), LogixTagReadResult.Ok(FortyTwoAsDint)));
        using var client = CreateClient(tagManager);
        await client.ConnectAsync(CancellationToken.None);
        await client.DisconnectAsync(CancellationToken.None);

        // Act
        var resolving = client.Awaiting(c => c.ResolveDataPoints([Speed], CancellationToken.None));

        // Assert
        await resolving.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ASecondConnectDoesNotBrowseTheSymbolTableAgain()
    {
        // Arrange
        var tagManager = Substitute.For<ILogixTagManager>();
        using var client = CreateClient(tagManager);
        await client.ConnectAsync(CancellationToken.None);

        // Act
        await client.ConnectAsync(CancellationToken.None);

        // Assert
        await tagManager.Received(1).LoadTagDefinitionsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AClientWhoseBrowseFailedReportsAConnectionFailureAndStaysDisconnected()
    {
        // Arrange
        var tagManager = Substitute.For<ILogixTagManager>();
        tagManager.LoadTagDefinitionsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new DataRetrievalException("no route to host")));
        using var client = CreateClient(tagManager);

        // Act
        var connect = await Record.ExceptionAsync(() => client.ConnectAsync(CancellationToken.None));

        // Assert
        connect.Should().BeOfType<ConnectionFailureException>()
            .Which.InnerException.Should().BeOfType<DataRetrievalException>();
        client.IsConnected.Should().BeFalse();
    }

    [Fact]
    public async Task ACancelledConnectThrowsTheCancellationRatherThanAConnectionFailure()
    {
        // Arrange
        var tagManager = Substitute.For<ILogixTagManager>();
        tagManager.LoadTagDefinitionsAsync(Arg.Any<CancellationToken>())
            .Returns(load => Task.FromCanceled(load.Arg<CancellationToken>()));
        using var client = CreateClient(tagManager);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var connect = await Record.ExceptionAsync(() => client.ConnectAsync(cts.Token));

        // Assert
        connect.Should().BeAssignableTo<OperationCanceledException>();
        client.IsConnected.Should().BeFalse();
    }

    [Fact]
    public async Task ADisconnectFreesTheTagsWithoutEndingTheTagManager()
    {
        // Arrange
        var tagManager = Substitute.For<ILogixTagManager>();
        using var client = CreateClient(tagManager);
        await client.ConnectAsync(CancellationToken.None);

        // Act
        await client.DisconnectAsync(CancellationToken.None);

        // Assert
        tagManager.Received(1).Drain();
        tagManager.DidNotReceive().Dispose();
    }

    [Fact]
    public async Task ASecondDisconnectDoesNotFreeTheTagsAgain()
    {
        // Arrange
        var tagManager = Substitute.For<ILogixTagManager>();
        using var client = CreateClient(tagManager);
        await client.ConnectAsync(CancellationToken.None);
        await client.DisconnectAsync(CancellationToken.None);

        // Act
        await client.DisconnectAsync(CancellationToken.None);

        // Assert
        tagManager.Received(1).Drain();
    }

    [Fact]
    public async Task AReconnectBrowsesTheSymbolTableAfresh()
    {
        // Arrange
        var tagManager = Substitute.For<ILogixTagManager>();
        using var client = CreateClient(tagManager);
        await client.ConnectAsync(CancellationToken.None);
        await client.DisconnectAsync(CancellationToken.None);

        // Act
        await client.ConnectAsync(CancellationToken.None);

        // Assert
        await tagManager.Received(2).LoadTagDefinitionsAsync(Arg.Any<CancellationToken>());
        client.IsConnected.Should().BeTrue();
    }

    [Fact]
    public void AClientDisposedTwiceEndsItsTagManagerOnce()
    {
        // Arrange
        var tagManager = Substitute.For<ILogixTagManager>();
        var client = CreateClient(tagManager);

        // Act
        client.Dispose();
        client.Dispose();

        // Assert
        tagManager.Received(1).Dispose();
    }

    [Fact]
    public void ADisposedClientReportsItselfDisconnected()
    {
        // Arrange
        var client = CreateClient(Substitute.For<ILogixTagManager>());

        // Act
        client.Dispose();

        // Assert
        client.IsConnected.Should().BeFalse();
    }

    [Fact]
    public async Task ConnectingADisposedClientIsRefusedAndBrowsesNothing()
    {
        // Arrange
        var tagManager = Substitute.For<ILogixTagManager>();
        var client = CreateClient(tagManager);
        client.Dispose();

        // Act
        var connect = await Record.ExceptionAsync(() => client.ConnectAsync(CancellationToken.None));

        // Assert
        connect.Should().BeOfType<ObjectDisposedException>();
        await tagManager.DidNotReceive().LoadTagDefinitionsAsync(Arg.Any<CancellationToken>());
    }

    private static LogixClient CreateClient(ILogixTagManager tagManager) =>
        new(tagManager, DefaultClientInformation(), TestLogging.CreateLogger<LogixClient>());

    private static LogixDataPointGroup GroupOf(params ILogixDataPoint[] dataPoints) =>
        new(DefaultPollFrequency, dataPoints);

    private static StringDataPoint ShortStringDataPoint(string tagName) =>
        new(new TagName(tagName), DefaultPollFrequency, NoChannels, new StringMaxLength(4));

    private static TagDefinition AtomicMetadataFor(ILogixDataPoint dataPoint) =>
        DefaultAtomicTagDefinition() with { TagName = dataPoint.TagName };

    private static TagDefinition StringMetadataFor(StringDataPoint dataPoint) =>
        DefaultStringTagDefinition() with { TagName = dataPoint.TagName, MaxLength = dataPoint.MaxLength };

    private static ILogixTagManager TagManagerFor(params ILogixTag[] tags)
    {
        var tagManager = Substitute.For<ILogixTagManager>();
        foreach (var tag in tags)
        {
            tagManager.TagFor(tag.DataPoint).Returns(tag);
        }

        return tagManager;
    }

    // An ILogixDataPointValue that no data point made: the only shape the write path's guard can meet.
    private sealed record ForeignDataPointValue(ILogixDataPoint DataPoint) : ILogixDataPointValue
    {
        public object? Value => null;

        public bool IsInValueRange() => false;
    }

    // A data point shape deliberately absent from DataPointConverterRegistry.
    private sealed record UnregisteredDataPoint()
        : LogixDataPoint<int>(new TagName("Mystery.Tag"), DefaultPollFrequency, NoChannels)
    {
        public override AllenBradleyDataType DataType => new("MYSTERY", LogixGeneration.Logix5X70);

        internal override ILogixDataPointValue<int> CreateLogixValue(int value) => throw new NotSupportedException();
    }

    private sealed class FakeTag : ILogixTag
    {
        private LogixTagReadResult _readResult = LogixTagReadResult.Ok(ReadOnlyMemory<byte>.Empty);
        private LogixTagWriteResult _writeResult = LogixTagWriteResult.Ok();

        private FakeTag(ILogixDataPoint dataPoint, TagDefinition? metadata)
        {
            DataPoint = dataPoint;
            Metadata = metadata;
        }

        public ILogixDataPoint DataPoint { get; init; }

        public TagDefinition? Metadata { get; init; }

        public bool WasRead { get; private set; }

        public byte[]? Written { get; private set; }

        public static FakeTag Reading(
            ILogixDataPoint dataPoint, TagDefinition? metadata, LogixTagReadResult result) =>
            new(dataPoint, metadata) { _readResult = result };

        public static FakeTag Writing(
            ILogixDataPoint dataPoint, TagDefinition? metadata, LogixTagWriteResult result) =>
            new(dataPoint, metadata) { _writeResult = result };

        // The real access honours cancellation by throwing rather than returning a failed result.
        public Task<LogixTagReadResult> ReadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WasRead = true;
            return Task.FromResult(_readResult);
        }

        public Task<LogixTagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Written = buffer;
            return Task.FromResult(_writeResult);
        }

        public void Dispose()
        {
        }
    }
}
