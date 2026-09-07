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

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client;

/// <summary>
/// How the client handles a controller that answers badly, driven through a substituted
/// <see cref="ILogixTagManager"/> that hands back one <see cref="ILogixTag"/> per
/// data point. Both directions treat their batch as one unit: a tag that will not read or write fails the
/// whole batch with an exception naming every such tag, so a group never comes home with holes in it.
/// What a tag holds is not re-checked here — <c>LogixConfigurationVerifier</c> settled that at
/// connect — so the metadata on these fakes only feeds <c>ResolveDataPoints</c>.
/// </summary>
// Every act here captures the `using var client` — as a Record delegate invoked on the spot, or as an
// Awaiting one the assertion invokes a line later. Neither outlives the test method.
// ReSharper disable AccessToDisposedClosure
public class LogixClientTests
{
    private static readonly DIntDataPoint Speed = new(new TagName("Motor.Speed"), DefaultPollFrequency, NoChannels);
    private static readonly DIntDataPoint Level = new(new TagName("Tank.Level"), DefaultPollFrequency, NoChannels);
    private static readonly StringDataPoint Label = new(new TagName("Line.Label"), DefaultPollFrequency, NoChannels, new StringMaxLength(StringMaxLength.Standard.Value));

    // Metadata the controller would report for a DINT tag. It is what ResolveDataPoints hands to
    // verification; the read and write paths never look at it, so only the device outcome decides those.
    private static TagDefinition DintMetadata(string tagName) =>
        new(new TagName(tagName), LogixTypeKind.Atomic, AllenBradleyDataType.Dint, MaxLength: null, new DimensionCount(0), new ElementCount(1));

    // What the controller reports for a built-in STRING tag: a scalar structure holding 82 characters.
    private static TagDefinition StringMetadata(string tagName) =>
        new(new TagName(tagName), LogixTypeKind.Structure, AllenBradleyDataType.String, StringMaxLength.Standard, new DimensionCount(0), new ElementCount(1));

    // 42 as a DINT on the wire. Spelled out rather than taken from BitConverter, which would re-derive
    // it through the same host-endianness assumption the converter makes and so agree by construction.
    private static readonly byte[] FortyTwoAsDint = [42, 0, 0, 0];

    [Fact]
    public async Task ReadAsync_WhenOneTagFails_ThrowsCarryingTheTagAndTheReason()
    {
        // Arrange
        var speedTag = FakeTag.Reading(Speed, DintMetadata("Motor.Speed"), LogixTagReadResult.Ok(FortyTwoAsDint));
        var tagManager = TagManagerFor(
            speedTag,
            FakeTag.Reading(Level, DintMetadata("Tank.Level"), LogixTagReadResult.Failed("tag not found")));
        using var client = CreateClient(tagManager);
        ILogixDataPoint[] dataPoints = [Speed, Level];
        var group = new LogixDataPointGroup(DefaultPollFrequency, dataPoints);

        // Act
        var read = client.Awaiting(c => c.ReadAsync(group, CancellationToken.None));

        // Assert
        // The group is the unit of delivery: one tag short makes the whole poll unusable, so it fails
        // rather than publishing a group with a hole in it
        // (ADR/2026-07-16-reading-and-writing-a-group-of-tags.md).
        (await read.Should().ThrowAsync<LogixTagException>())
            .Which.Message.Should().Contain("Tank.Level").And.Contain("tag not found");

        // Reported per batch, but detected per tag: the failing tag did not stop its sibling from being
        // read. Short-circuiting would leave the batch half-issued against the controller and cost the
        // packing the concurrent fan-out exists for.
        speedTag.WasRead.Should().BeTrue();
    }

    [Fact]
    public async Task ReadAsync_WhenCancelled_PropagatesTheCancellationAndReadsNothing()
    {
        // Arrange
        var speedTag = FakeTag.Reading(Speed, DintMetadata("Motor.Speed"), LogixTagReadResult.Ok(FortyTwoAsDint));
        var tagManager = TagManagerFor(speedTag);
        using var client = CreateClient(tagManager);
        var group = new LogixDataPointGroup(DefaultPollFrequency, [Speed]);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var read = await Record.ExceptionAsync(() => client.ReadAsync(group, cts.Token).AsTask());

        // Assert
        // Cancelling is the caller's decision, not the device's answer, so it travels as itself rather
        // than as the LogixTagException a controller that would not answer produces.
        read.Should().BeAssignableTo<OperationCanceledException>();
        speedTag.WasRead.Should().BeFalse();
    }

    [Fact]
    public async Task ReadAsync_WhenSeveralTagsFail_ThrowsNamingEveryOne()
    {
        // Arrange
        var tagManager = TagManagerFor(
            FakeTag.Reading(Speed, DintMetadata("Motor.Speed"), LogixTagReadResult.Failed("tag is write-only")),
            FakeTag.Reading(Level, DintMetadata("Tank.Level"), LogixTagReadResult.Failed("tag not found")));
        using var client = CreateClient(tagManager);
        ILogixDataPoint[] dataPoints = [Speed, Level];
        var group = new LogixDataPointGroup(DefaultPollFrequency, dataPoints);

        // Act
        var read = client.Awaiting(c => c.ReadAsync(group, CancellationToken.None));

        // Assert
        // A failing tag does not stop its siblings, so reporting only the first would leave the caller
        // looking at the wrong tag.
        var message = (await read.Should().ThrowAsync<LogixTagException>()).Which.Message;
        message.Should().Contain("Motor.Speed").And.Contain("tag is write-only");
        message.Should().Contain("Tank.Level").And.Contain("tag not found");
    }

    [Fact]
    public async Task ReadAsync_WhenTheReplyIsTooShortForTheType_ThrowsNamingThatTag()
    {
        // Arrange
        // Two bytes where a DINT needs four. Nothing checks that before the decode any more — the
        // controller's type was verified at connect — so the converter is what discovers it, by throwing.
        var tagManager = TagManagerFor(
            FakeTag.Reading(Speed, DintMetadata("Motor.Speed"), LogixTagReadResult.Ok(new byte[2])),
            FakeTag.Reading(Level, DintMetadata("Tank.Level"), LogixTagReadResult.Ok(FortyTwoAsDint)));
        using var client = CreateClient(tagManager);
        ILogixDataPoint[] dataPoints = [Speed, Level];
        var group = new LogixDataPointGroup(DefaultPollFrequency, dataPoints);

        // Act
        var read = client.Awaiting(c => c.ReadAsync(group, CancellationToken.None));

        // Assert
        // A decode that throws fails the group like a failed read does, and is caught narrowly on the way
        // out so the tag that could not be decoded is named rather than a bare ArgumentException
        // travelling with no address in it.
        (await read.Should().ThrowAsync<LogixTagException>())
            .Which.Message.Should().Contain("Motor.Speed");
    }

    [Fact]
    public async Task ReadAsync_WhenEveryTagAnswers_ReturnsAValuePerPointInGroupOrder()
    {
        // Arrange
        var tagManager = TagManagerFor(
            FakeTag.Reading(Speed, DintMetadata("Motor.Speed"), LogixTagReadResult.Ok(FortyTwoAsDint)),
            FakeTag.Reading(Level, DintMetadata("Tank.Level"), LogixTagReadResult.Ok(new byte[] { 7, 0, 0, 0 })));
        using var client = CreateClient(tagManager);
        ILogixDataPoint[] dataPoints = [Speed, Level];

        // Act
        var values = await client.ReadAsync(
            new LogixDataPointGroup(DefaultPollFrequency, dataPoints), CancellationToken.None);

        // Assert
        // The reads are fanned out concurrently, so the order the group listed them in is the only thing
        // that can put the values back in the caller's order.
        values.Should().HaveCount(2);
        values[0].DataPoint.Should().Be(Speed);
        values[0].Value.Should().Be(42);
        values[1].DataPoint.Should().Be(Level);
        values[1].Value.Should().Be(7);
    }

    [Fact]
    public async Task ReadAsync_WhenADataPointHasNoConverter_ThrowsBeforeReadingAnything()
    {
        // Arrange
        // Both points have a tag, so the only thing left that can throw is the missing converter.
        var speedTag = FakeTag.Reading(Speed, DintMetadata("Motor.Speed"), LogixTagReadResult.Ok(FortyTwoAsDint));
        var unconvertible = new UnregisteredDataPoint();
        var tagManager = TagManagerFor(
            speedTag,
            FakeTag.Reading(unconvertible, metadata: null, LogixTagReadResult.Ok(FortyTwoAsDint)));
        using var client = CreateClient(tagManager);
        ILogixDataPoint[] dataPoints = [Speed, unconvertible];
        var group = new LogixDataPointGroup(DefaultPollFrequency, dataPoints);

        // Act
        var read = await Record.ExceptionAsync(() => client.ReadAsync(group, CancellationToken.None).AsTask());

        // Assert
        // The group resolves whole before any I/O, so a data point wired up without a converter is a
        // configuration error that costs no round trip and leaves no half-read group behind.
        read.Should().BeOfType<InvalidOperationException>();
        speedTag.WasRead.Should().BeFalse();
    }

    [Fact]
    public async Task WriteAsync_WhenTheTagFails_ThrowsCarryingTheTagAndTheReason()
    {
        // Arrange
        var tagManager = TagManagerFor(
            FakeTag.Writing(Speed, DintMetadata("Motor.Speed"), LogixTagWriteResult.Failed("tag is read-only")));
        using var client = CreateClient(tagManager);

        var value = Speed.CreateLogixValue(42);

        // Act
        var write = client.Awaiting(c => c.WriteAsync([value], CancellationToken.None));

        // Assert
        // Silence here would be a dropped write the caller cannot detect.
        (await write.Should().ThrowAsync<LogixTagException>())
            .Which.Message.Should().Contain("Motor.Speed").And.Contain("tag is read-only");
    }

    [Fact]
    public async Task WriteAsync_WhenSeveralTagsFail_ThrowsNamingEveryOne()
    {
        // Arrange
        var tagManager = TagManagerFor(
            FakeTag.Writing(Speed, DintMetadata("Motor.Speed"), LogixTagWriteResult.Failed("tag is read-only")),
            FakeTag.Writing(Level, DintMetadata("Tank.Level"), LogixTagWriteResult.Failed("tag not found")));
        using var client = CreateClient(tagManager);
        ILogixDataPointValue[] values =
        [
            Speed.CreateLogixValue(42),
            Level.CreateLogixValue(7),
        ];

        // Act
        var write = client.Awaiting(c => c.WriteAsync(values, CancellationToken.None));

        // Assert
        // A failing tag does not stop its siblings, so reporting only the first would leave the caller
        // re-driving the wrong set.
        var message = (await write.Should().ThrowAsync<LogixTagException>()).Which.Message;
        message.Should().Contain("Motor.Speed").And.Contain("tag is read-only");
        message.Should().Contain("Tank.Level").And.Contain("tag not found");
    }

    [Fact]
    public async Task WriteAsync_WhenCancelled_PropagatesTheCancellationAndWritesNothing()
    {
        // Arrange
        var tag = FakeTag.Writing(Speed, DintMetadata("Motor.Speed"), LogixTagWriteResult.Ok());
        var tagManager = TagManagerFor(tag);
        using var client = CreateClient(tagManager);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var write = await Record.ExceptionAsync(
            () => client.WriteAsync([Speed.CreateLogixValue(42)], cts.Token).AsTask());

        // Assert
        // The write mirror of the read case: a cancelled batch is a shutdown, and reporting it as a
        // failed write would send the caller looking for a controller fault that never happened.
        write.Should().BeAssignableTo<OperationCanceledException>();
        tag.Written.Should().BeNull();
    }

    [Fact]
    public async Task WriteAsync_WhenSeveralValuesWillNotEncode_ThrowsNamingEveryOneAndSendsNothing()
    {
        // Arrange
        // Two tags declared to hold four characters, handed values that do not fit. Encoding happens
        // before any tag is touched, so this is the batch's last chance to say anything at all.
        var shortLabel = new StringDataPoint(
            new TagName("Line.Short"), DefaultPollFrequency, NoChannels, new StringMaxLength(4));
        var shortCode = new StringDataPoint(
            new TagName("Line.Code"), DefaultPollFrequency, NoChannels, new StringMaxLength(4));
        var labelTag = FakeTag.Writing(shortLabel, StringMetadata("Line.Short"), LogixTagWriteResult.Ok());
        var codeTag = FakeTag.Writing(shortCode, StringMetadata("Line.Code"), LogixTagWriteResult.Ok());
        var tagManager = TagManagerFor(labelTag, codeTag);
        using var client = CreateClient(tagManager);
        ILogixDataPointValue[] values =
        [
            shortLabel.CreateLogixValue("far too long"),
            shortCode.CreateLogixValue("also too long"),
        ];

        // Act
        var write = client.Awaiting(c => c.WriteAsync(values, CancellationToken.None));

        // Assert
        // The same contract a failed device write has: every tag that could not be served is named, so a
        // caller does not fix one value and come straight back with the next.
        var message = (await write.Should().ThrowAsync<LogixTagException>()).Which.Message;
        message.Should().Contain("Line.Short").And.Contain("Line.Code");

        // And unlike a device failure, this one reached no tag at all — which is the point of encoding
        // up front rather than per write.
        labelTag.Written.Should().BeNull();
        codeTag.Written.Should().BeNull();
    }

    [Fact]
    public async Task WriteAsync_WhenOneValueWillNotEncode_LeavesTheTagsThatWouldHaveSucceededUntouched()
    {
        // Arrange
        // One value that encodes and one that does not. Encoding the whole batch up front exists for
        // exactly this case: the good value must not reach its tag when a sibling in the same call
        // cannot reach its own.
        var shortLabel = new StringDataPoint(
            new TagName("Line.Short"), DefaultPollFrequency, NoChannels, new StringMaxLength(4));
        var speedTag = FakeTag.Writing(Speed, DintMetadata("Motor.Speed"), LogixTagWriteResult.Ok());
        var labelTag = FakeTag.Writing(shortLabel, StringMetadata("Line.Short"), LogixTagWriteResult.Ok());
        var tagManager = TagManagerFor(speedTag, labelTag);
        using var client = CreateClient(tagManager);
        ILogixDataPointValue[] values =
        [
            Speed.CreateLogixValue(42),
            shortLabel.CreateLogixValue("far too long"),
        ];

        // Act
        var write = client.Awaiting(c => c.WriteAsync(values, CancellationToken.None));

        // Assert
        (await write.Should().ThrowAsync<LogixTagException>())
            .Which.Message.Should().Contain("Line.Short");

        // A half-written batch is the failure mode encoding up front rules out: the controller would be
        // left holding one new value and one old one, with nothing to say which.
        speedTag.Written.Should().BeNull();
        labelTag.Written.Should().BeNull();
    }

    [Fact]
    public async Task WriteAsync_WhenAValueIsNotItsDataPointsOwn_ThrowsNamingTheTag()
    {
        // Arrange
        // ILogixDataPointValue is public, so an outside implementation can be handed down. It names a
        // real point but carries no payload the converter can encode.
        var tag = FakeTag.Writing(Speed, DintMetadata("Motor.Speed"), LogixTagWriteResult.Ok());
        var tagManager = TagManagerFor(tag);
        using var client = CreateClient(tagManager);
        ILogixDataPointValue[] values = [new ForeignDataPointValue(Speed)];

        // Act
        var write = client.Awaiting(c => c.WriteAsync(values, CancellationToken.None));

        // Assert
        // The converter's own refusal, but reported the way every other write failure is — as one
        // LogixTagException carrying the tag, rather than as the raw InvalidOperationException the
        // converter raised.
        (await write.Should().ThrowAsync<LogixTagException>())
            .Which.Message.Should().Contain("Motor.Speed");
        tag.Written.Should().BeNull();
    }

    [Fact]
    public async Task WriteAsync_EncodesTheValueOntoTheTag()
    {
        // Arrange
        var tag = FakeTag.Writing(Speed, DintMetadata("Motor.Speed"), LogixTagWriteResult.Ok());
        var tagManager = TagManagerFor(tag);
        using var client = CreateClient(tagManager);
        ILogixDataPointValue value = Speed.CreateLogixValue(42);

        // Act
        await client.WriteAsync([value], CancellationToken.None);

        // Assert
        tag.Written.Should().Equal(FortyTwoAsDint);
    }

    [Fact]
    public async Task WriteAsync_AString_HandsTheTagTheValuesOwnBytes()
    {
        // Arrange
        // A STRING is the first type whose wire size is not fixed by its type. What reaches the tag is
        // .LEN and .DATA, 86 bytes for the built-in STRING, sized from the data point's configured
        // capacity. The two bytes of alignment padding that make the tag 88 on the controller are
        // libplctag's handle's, and never pass through the client.
        var tag = FakeTag.Writing(Label, StringMetadata("Line.Label"), LogixTagWriteResult.Ok());
        var tagManager = TagManagerFor(tag);
        using var client = CreateClient(tagManager);
        var value = (ILogixDataPointValue)Label.CreateLogixValue("Hi");

        // Act
        await client.WriteAsync([value], CancellationToken.None);

        // Assert
        tag.Written.Should().HaveCount(4 + StringMaxLength.Standard.Value);
        tag.Written.AsSpan(0, 4).ToArray().Should().Equal(2, 0, 0, 0);
        tag.Written.AsSpan(4, 2).ToArray().Should().Equal((byte)'H', (byte)'i');
        tag.Written.AsSpan(6).ToArray().Should().AllSatisfy(b => b.Should().Be(0));
    }

    [Fact]
    public async Task ReadAsync_AString_DecodesTheStructureIntoATypedValue()
    {
        // Arrange
        var structure = new byte[88];
        structure[0] = 2;
        structure[4] = (byte)'H';
        structure[5] = (byte)'i';
        var tagManager = TagManagerFor(
            FakeTag.Reading(Label, StringMetadata("Line.Label"), LogixTagReadResult.Ok(structure)));
        using var client = CreateClient(tagManager);
        ILogixDataPoint[] dataPoints = [Label];
        var logixDataPointGroup = new LogixDataPointGroup(DefaultPollFrequency, dataPoints);

        // Act
        var values = await client.ReadAsync(logixDataPointGroup, CancellationToken.None);

        // Assert
        values.Should().ContainSingle();
        values[0].Value.Should().Be("Hi");
    }

    [Fact]
    public async Task ResolveDataPoints_PairsEachPointWithTheControllerMetadataForItsTag()
    {
        // Arrange
        // Line.Label is deliberately absent from the controller's symbol table, which its tag carries as
        // null metadata.
        var tagManager = TagManagerFor(
            FakeTag.Reading(Speed, DintMetadata("Motor.Speed"), LogixTagReadResult.Ok(FortyTwoAsDint)),
            FakeTag.Reading(Label, metadata: null, LogixTagReadResult.Ok(new byte[88])));
        using var client = CreateClient(tagManager);
        await client.ConnectAsync(CancellationToken.None);

        // Act
        var resolved = await client.ResolveDataPoints([Speed, Label], CancellationToken.None);

        // Assert
        // An absent tag is resolved, not skipped: dropping it would hide the one misconfiguration the
        // verifier most needs to report. Order is the caller's, so a result can be read positionally.
        resolved.Should().HaveCount(2);
        resolved[0].DataPoint.Should().Be(Speed);
        resolved[0].TagDefinition.Should().Be(DintMetadata("Motor.Speed"));
        resolved[1].DataPoint.Should().Be(Label);
        resolved[1].TagDefinition.Should().BeNull();
    }

    [Fact]
    public async Task ResolveDataPoints_BeforeAConnect_ThrowsAndBrowsesNothing()
    {
        // Arrange
        var tagManager = TagManagerFor(
            FakeTag.Reading(Speed, DintMetadata("Motor.Speed"), LogixTagReadResult.Ok(FortyTwoAsDint)));
        using var client = CreateClient(tagManager);

        // Act
        var resolve = await Record.ExceptionAsync(() => client.ResolveDataPoints([Speed], CancellationToken.None));

        // Assert
        // A connect is the precondition, and the dataport base always satisfies it: it builds the
        // verifier from the client it has just acquired. Browsing here instead would open handles on a
        // client that does not consider itself connected — which a later disconnect would then skip
        // freeing, and a handle left to its finalizer fail-fasts the process (0xC0000602).
        resolve.Should().BeOfType<InvalidOperationException>();
        await tagManager.DidNotReceive().LoadTagDefinitionsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResolveDataPoints_AfterADisconnect_Throws()
    {
        // Arrange
        var tagManager = TagManagerFor(
            FakeTag.Reading(Speed, DintMetadata("Motor.Speed"), LogixTagReadResult.Ok(FortyTwoAsDint)));
        using var client = CreateClient(tagManager);
        await client.ConnectAsync(CancellationToken.None);
        await client.DisconnectAsync(CancellationToken.None);

        // Act
        var resolve = client.Awaiting(c => c.ResolveDataPoints([Speed], CancellationToken.None));

        // Assert
        // The disconnect dropped the schema these would resolve against, so the precondition is about
        // the connection the client holds now, not one it held once.
        await resolve.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ConnectAsync_BrowsesTheSymbolTableOnce()
    {
        // Arrange
        var tagManager = Substitute.For<ILogixTagManager>();
        using var client = CreateClient(tagManager);

        // Act
        await client.ConnectAsync(CancellationToken.None);
        await client.ConnectAsync(CancellationToken.None);

        // Assert
        // Connecting is the browse, so a second connect must not pay for a second one — the engine calls
        // connect defensively and the browse is the expensive part of it.
        client.IsConnected.Should().BeTrue();
        await tagManager.Received(1).LoadTagDefinitionsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConnectAsync_WhenTheBrowseFails_ThrowsConnectionFailureAndStaysDisconnected()
    {
        // Arrange
        // A schema exception is what an unreachable connection endpoint or a dead route path comes back as. The
        // framework's acquire contract is a single exception type, so it must not reach the caller raw.
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
    public async Task ConnectAsync_WhenCancelled_PropagatesTheCancellation()
    {
        // Arrange
        // Cancellation is not a connection failure — wrapping it would hide a shutdown as a device fault.
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
    public async Task DisconnectAsync_DrainsTheTagManagerAndAllowsReconnecting()
    {
        // Arrange
        var tagManager = Substitute.For<ILogixTagManager>();
        using var client = CreateClient(tagManager);
        await client.ConnectAsync(CancellationToken.None);

        // Act
        await client.DisconnectAsync(CancellationToken.None);
        await client.DisconnectAsync(CancellationToken.None);
        await client.ConnectAsync(CancellationToken.None);

        // Assert
        // Disconnect frees the handles and drops the schema, exactly once for the one connection it ends;
        // it is reversible, so the reconnect browses again rather than throwing.
        tagManager.Received(1).Drain();
        tagManager.DidNotReceive().Dispose();
        await tagManager.Received(2).LoadTagDefinitionsAsync(Arg.Any<CancellationToken>());
        client.IsConnected.Should().BeTrue();
    }

    [Fact]
    public void Dispose_EndsTheTagManagerAndIsIdempotent()
    {
        // Arrange
        var tagManager = Substitute.For<ILogixTagManager>();
        var client = CreateClient(tagManager);

        // Act
        client.Dispose();
        client.Dispose();

        // Assert
        // Disposing is what frees the native handles, and a double dispose must not double-free them.
        tagManager.Received(1).Dispose();
        client.IsConnected.Should().BeFalse();
    }

    [Fact]
    public async Task ConnectAsync_AfterDispose_Throws()
    {
        // Arrange
        var tagManager = Substitute.For<ILogixTagManager>();
        var client = CreateClient(tagManager);
        client.Dispose();

        // Act
        var connect = await Record.ExceptionAsync(() => client.ConnectAsync(CancellationToken.None));

        // Assert
        // Dispose is terminal: a disposed client's tag manager is gone, so reviving it would hand out
        // tags nothing owns. The pool builds a fresh client instead.
        connect.Should().BeOfType<ObjectDisposedException>();
        await tagManager.DidNotReceive().LoadTagDefinitionsAsync(Arg.Any<CancellationToken>());
    }

    private static LogixClient CreateClient(ILogixTagManager tagManager) =>
        new(tagManager, DefaultClientInformation(), TestLogging.CreateLogger<LogixClient>());

    // A tag manager that hands back exactly these tags, each keyed on the data point it already carries.
    private static ILogixTagManager TagManagerFor(params ILogixTag[] tags)
    {
        var tagManager = Substitute.For<ILogixTagManager>();
        foreach (var tag in tags)
        {
            tagManager.TagFor(tag.DataPoint).Returns(tag);
        }

        return tagManager;
    }

    // An ILogixDataPointValue that no data point made. The interface is public, so this is the one shape
    // the write path's converter guard can ever be handed: a read returns the point's own typed value or
    // fails its batch.
    private sealed record ForeignDataPointValue(ILogixDataPoint DataPoint) : ILogixDataPointValue
    {
        public object? Value => null;

        public bool IsInValueRange() => false;
    }

    // A data point shape deliberately absent from DataPointConverterRegistry: the model gaining a type
    // that nobody wired a converter for.
    private sealed record UnregisteredDataPoint()
        : LogixDataPoint<int>(new TagName("Mystery.Tag"), DefaultPollFrequency, NoChannels)
    {
        protected override LogixDataTypeName TypeName => new("MYSTERY");

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

        // Cancellation is honoured the way the real access honours it — by throwing rather than by
        // coming home as a failed result. LogixTagAccess catches only LibPlcTagException for the same
        // reason: a cancelled operation is not a device answer.
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
