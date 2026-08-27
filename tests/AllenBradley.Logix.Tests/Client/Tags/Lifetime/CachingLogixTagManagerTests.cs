using Microsoft.Extensions.Logging.Abstractions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Exceptions;

using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Lifetime;

/// <summary>
/// The reuse rules of <see cref="CachingLogixTagManager"/> and the schema it now owns, exercised
/// against a fake factory and a fake browser. These run with no controller and no native library — which
/// is why access creation lives behind <see cref="ILogixTagAccessFactory"/> and the browse behind
/// <see cref="ITagDefinitionsLoader"/> rather than inside the manager.
/// </summary>
public class CachingLogixTagManagerTests
{
    private static TagDefinition Dint(string tagName) =>
        new(new TagName(tagName), LogixTypeKind.Atomic, AllenBradleyDataType.Dint, MaxLength: null, new DimensionCount(0), new ElementCount(1));

    private static CachingLogixTagManager NewManager(
        CountingAccessFactory factory, FakeSchemaBrowser browser) =>
        new(factory, browser, NullLogger<CachingLogixTagManager>.Instance);

    private static Task LoadAsync(CachingLogixTagManager manager) =>
        manager.LoadTagDefinitionsAsync(TestContext.Current.CancellationToken);

    [Fact]
    public async Task LoadSchemaAsync_CalledTwice_BrowsesOnce()
    {
        // Arrange
        var browser = new FakeSchemaBrowser();
        using var manager = NewManager(new CountingAccessFactory(), browser);

        // Act
        await LoadAsync(manager);
        await LoadAsync(manager);

        // Assert
        // The browse is N device reads (controller + one per program); connect must not pay it twice.
        browser.BrowseCount.Should().Be(1);
    }

    [Fact]
    public async Task LoadSchemaAsync_CalledConcurrently_BrowsesOnceAndMakesBothCallersWaitForIt()
    {
        // Arrange — the first browse blocks until the test releases it
        var browseGate = new TaskCompletionSource();
        var browser = new FakeSchemaBrowser { OnBrowse = () => browseGate.Task };
        using var manager = NewManager(new CountingAccessFactory(), browser);

        // Act
        var first = LoadAsync(manager);
        await browser.BrowseStarted.Task;
        var second = LoadAsync(manager);
        await Task.Delay(50, TestContext.Current.CancellationToken);
        var secondCompletedEarly = second.IsCompleted;

        browseGate.SetResult();
        await Task.WhenAll(first, second);

        // Assert
        // Checking the field and then browsing is a check-then-act, so without a gate both callers reach
        // the controller and one round trip is thrown away. The second must also not return before the
        // schema exists, or the TagFor behind it would find none.
        secondCompletedEarly.Should().BeFalse();
        browser.BrowseCount.Should().Be(1);
    }

    [Fact]
    public async Task LoadSchemaAsync_AfterABrowseFailed_TriesAgain()
    {
        // Arrange
        var browser = new FakeSchemaBrowser
        {
            OnBrowse = () => Task.FromException(new DataRetrievalException("no route to host")),
        };
        using var manager = NewManager(new CountingAccessFactory(), browser);

        var failed = async () => await LoadAsync(manager);
        await failed.Should().ThrowAsync<DataRetrievalException>();

        // Act
        var retry = async () => await LoadAsync(manager);

        // Assert
        // A controller that was unreachable a moment ago is not unreachable forever. Remembering the
        // failed browse — which is what caching the task rather than gating it would do — would make the
        // first failure the answer every later connect got.
        await retry.Should().ThrowAsync<DataRetrievalException>();
        browser.BrowseCount.Should().Be(2);
    }

    [Fact]
    public async Task TagFor_StampsTheControllerMetadataOntoTheTag()
    {
        // Arrange
        var browser = new FakeSchemaBrowser { ["Motor.Speed"] = Dint("Motor.Speed") };
        using var manager = NewManager(new CountingAccessFactory(), browser);
        await LoadAsync(manager);

        // Act
        var tag = manager.TagFor(CreateDInt("Motor.Speed"));

        // Assert
        tag.Metadata.Should().Be(Dint("Motor.Speed"));
        tag.DataPoint.Should().Be(CreateDInt("Motor.Speed"));
    }

    [Fact]
    public async Task TagFor_ForATagAbsentFromTheSchema_LeavesTheMetadataNull()
    {
        // Arrange
        var browser = new FakeSchemaBrowser { ["Motor.Speed"] = Dint("Motor.Speed") };
        using var manager = NewManager(new CountingAccessFactory(), browser);
        await LoadAsync(manager);

        // Act
        // A tag absent from the schema still resolves — for diagnostics and uniform handling; null metadata
        // is the "not on the controller" signal verification reports.
        var tag = manager.TagFor(CreateDInt("Ghost"));

        // Assert
        tag.Metadata.Should().BeNull();
    }

    [Fact]
    public async Task TagFor_CalledTwiceForEqualDataPoints_CreatesTheTagOnce()
    {
        // Arrange
        var factory = new CountingAccessFactory();
        using var manager = NewManager(factory, new FakeSchemaBrowser());
        await LoadAsync(manager);

        // Act
        // Distinct instances, equal by record value — the same data point, so the same tag (ADR-002).
        var first = manager.TagFor(CreateDInt("Motor.Speed"));
        var second = manager.TagFor(CreateDInt("Motor.Speed"));

        // Assert
        second.Should().BeSameAs(first);
        factory.CreatedCount.Should().Be(1);
    }

    [Fact]
    public async Task TagFor_ForDifferentTags_CreatesATagEach()
    {
        // Arrange
        var factory = new CountingAccessFactory();
        using var manager = NewManager(factory, new FakeSchemaBrowser());
        await LoadAsync(manager);

        // Act
        var speed = manager.TagFor(CreateDInt("Motor.Speed"));
        var level = manager.TagFor(CreateDInt("Tank.Level"));

        // Assert
        level.Should().NotBeSameAs(speed);
        factory.CreatedCount.Should().Be(2);
    }

    [Fact]
    public void TagFor_BeforeLoadSchema_Throws()
    {
        // Arrange
        var factory = new CountingAccessFactory();
        using var manager = NewManager(factory, new FakeSchemaBrowser());

        // Act
        // The schema is a hard connect precondition — a tag with no metadata to stamp on would be a
        // half-built object, so the ordering is enforced rather than papered over with a lazy load.
        var tagFor = () => manager.TagFor(CreateDInt("Motor.Speed"));

        // Assert
        tagFor.Should().Throw<InvalidOperationException>();
        factory.CreatedCount.Should().Be(0);
    }

    [Fact]
    public async Task Dispose_DisposesEveryTagItCreated()
    {
        // Arrange
        var factory = new CountingAccessFactory();
        var manager = NewManager(factory, new FakeSchemaBrowser());
        await LoadAsync(manager);
        manager.TagFor(CreateDInt("Motor.Speed"));
        manager.TagFor(CreateDInt("Tank.Level"));

        // Act
        manager.Dispose();

        // Assert
        // A libplctag handle left to its finalizer fail-fasts the process (0xC0000602), so "every" is the
        // whole assertion.
        factory.Created.Should().OnlyContain(access => access.IsDisposed);
    }

    [Fact]
    public async Task Dispose_CalledTwice_DisposesEachTagOnce()
    {
        // Arrange
        var factory = new CountingAccessFactory();
        var manager = NewManager(factory, new FakeSchemaBrowser());
        await LoadAsync(manager);
        manager.TagFor(CreateDInt("Motor.Speed"));

        // Act
        manager.Dispose();
        manager.Dispose();

        // Assert
        factory.Created.Should().OnlyContain(access => access.DisposeCount == 1);
    }

    [Fact]
    public async Task Dispose_WhenOneTagThrows_StillFreesTheOthers()
    {
        // Arrange
        var factory = new CountingAccessFactory { ThrowOnDisposeFor = "Motor.Speed" };
        var manager = NewManager(factory, new FakeSchemaBrowser());
        await LoadAsync(manager);
        manager.TagFor(CreateDInt("Motor.Speed"));
        manager.TagFor(CreateDInt("Tank.Level"));

        // Act
        var dispose = () => manager.Dispose();

        // Assert
        // A tag that will not free is the one case where giving up costs the most: every tag still
        // queued behind it would be left to its finalizer, and that fail-fasts the process (0xC0000602).
        dispose.Should().NotThrow();
        factory.Created.Should().OnlyContain(access => access.DisposeCount == 1);
    }

    [Fact]
    public async Task Drain_FreesEveryTagAndLeavesTheManagerReusable()
    {
        // Arrange
        var factory = new CountingAccessFactory();
        var browser = new FakeSchemaBrowser();
        using var manager = NewManager(factory, browser);
        await LoadAsync(manager);
        manager.TagFor(CreateDInt("Motor.Speed"));

        // Act
        manager.Drain();
        await LoadAsync(manager);
        manager.TagFor(CreateDInt("Motor.Speed"));

        // Assert
        // Drain is what a disconnect does, and a disconnect is reversible: the handles go, the schema goes
        // with them, and a reconnect browses again rather than reusing a schema the controller may have
        // changed underneath.
        browser.BrowseCount.Should().Be(2);
        factory.CreatedCount.Should().Be(2);
        factory.Created[0].IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task TagFor_AfterDrain_ThrowsUntilTheSchemaIsLoadedAgain()
    {
        // Arrange
        var factory = new CountingAccessFactory();
        using var manager = NewManager(factory, new FakeSchemaBrowser());
        await LoadAsync(manager);

        // Act
        manager.Drain();
        var tagFor = () => manager.TagFor(CreateDInt("Motor.Speed"));

        // Assert
        // Drain drops the schema, so the connect precondition is back in force — a tag built now would
        // carry metadata from a connection that has ended.
        tagFor.Should().Throw<InvalidOperationException>();
        factory.CreatedCount.Should().Be(0);
    }

    [Fact]
    public async Task Drain_AfterDispose_Throws()
    {
        // Arrange
        var manager = NewManager(new CountingAccessFactory(), new FakeSchemaBrowser());
        await LoadAsync(manager);
        manager.Dispose();

        // Act
        var drain = () => manager.Drain();

        // Assert
        // Dispose is terminal; draining a disposed manager would read as if it could be revived.
        drain.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public async Task TagFor_AfterDispose_Throws()
    {
        // Arrange
        var factory = new CountingAccessFactory();
        var manager = NewManager(factory, new FakeSchemaBrowser());
        await LoadAsync(manager);
        manager.Dispose();

        // Act
        // Handing back a tag here would create one nothing disposes — the leak Dispose just closed.
        var tagFor = () => manager.TagFor(CreateDInt("Motor.Speed"));

        // Assert
        tagFor.Should().Throw<ObjectDisposedException>();
        factory.CreatedCount.Should().Be(0);
    }

    private sealed class FakeSchemaBrowser : ITagDefinitionsLoader
    {
        private readonly Dictionary<TagName, TagDefinition> _declarations =
            new(TagName.CaseInsensitiveComparer);

        public int BrowseCount { get; private set; }

        /// <summary>Completed as the browse begins, so a test can act once one is genuinely in flight.</summary>
        public TaskCompletionSource BrowseStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>What the browse waits on, so a test can hold one open. Instant by default.</summary>
        public Func<Task> OnBrowse { get; init; } = static () => Task.CompletedTask;

        public TagDefinition this[string tagName]
        {
            set => _declarations[new TagName(tagName)] = value;
        }

        public async Task<TagDefinitions> LoadAsync(CancellationToken cancellationToken)
        {
            BrowseCount++;
            BrowseStarted.TrySetResult();
            await OnBrowse().ConfigureAwait(false);
            return new TagDefinitions(_declarations);
        }
    }

    private sealed class CountingAccessFactory : ILogixTagAccessFactory
    {
        private readonly List<FakeTagAccess> _created = [];

        public IReadOnlyList<FakeTagAccess> Created => _created;

        public int CreatedCount => _created.Count;

        /// <summary>Tag name whose access throws when freed; <c>null</c> for none.</summary>
        public string? ThrowOnDisposeFor { get; init; }

        public ILogixTagAccess Create(ILogixDataPoint dataPoint)
        {
            var access = new FakeTagAccess(dataPoint.TagName.Value == ThrowOnDisposeFor);
            _created.Add(access);
            return access;
        }

        public ILogixTagAccess CreateForSchemaTag(TagName tagName) =>
            throw new NotSupportedException("The manager browses through the injected ILogixSchemaBrowser.");
    }

    private sealed class FakeTagAccess(bool throwOnDispose = false) : ILogixTagAccess
    {
        public bool IsDisposed => DisposeCount > 0;

        public int DisposeCount { get; private set; }

        public Task<LogixTagReadResult> ReadAsync(CancellationToken cancellationToken) =>
            Task.FromResult(LogixTagReadResult.Ok(ReadOnlyMemory<byte>.Empty));

        public Task<LogixTagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken) =>
            Task.FromResult(LogixTagWriteResult.Ok());

        // The manager caches and frees accesses; nothing here writes through one.
        public byte[] CreateNewWriteBuffer() => [];

        public void Dispose()
        {
            DisposeCount++;
            if (throwOnDispose)
            {
                throw new LogixTagException("Access refused to free.");
            }
        }
    }
}
