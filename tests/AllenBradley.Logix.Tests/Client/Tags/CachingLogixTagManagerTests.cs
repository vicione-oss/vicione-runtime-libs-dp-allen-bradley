using Microsoft.Extensions.Logging.Abstractions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Schema;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags;

/// <summary>
/// The reuse rules of <see cref="CachingLogixTagManager"/> and the schema it now owns, exercised
/// against a fake factory and a fake browser. These run with no controller and no native library — which
/// is why access creation lives behind <see cref="ILogixTagAccessFactory"/> and the browse behind
/// <see cref="ILogixSchemaBrowser"/> rather than inside the manager.
/// </summary>
public class CachingLogixTagManagerTests
{
    private static LogixTypeDeclaration Dint(string tagName) =>
        new(new TagName(tagName), LogixTypeKind.Atomic, CipType.Dint, new DimensionCount(0), new ElementCount(1), new ElementLength(4));

    private static CachingLogixTagManager NewManager(
        CountingAccessFactory factory, FakeSchemaBrowser browser) =>
        new(factory, browser, NullLogger<CachingLogixTagManager>.Instance);

    private static Task LoadAsync(CachingLogixTagManager manager) =>
        manager.LoadSchemaAsync(TestContext.Current.CancellationToken);

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
    public async Task TagFor_StampsTheControllerMetadataOntoTheTag()
    {
        // Arrange
        var browser = new FakeSchemaBrowser { ["Motor.Speed"] = Dint("Motor.Speed") };
        using var manager = NewManager(new CountingAccessFactory(), browser);
        await LoadAsync(manager);

        // Act
        var tag = manager.TagFor(new DIntDataPoint(new TagName("Motor.Speed")));

        // Assert
        tag.Metadata.Should().Be(Dint("Motor.Speed"));
        tag.DataPoint.Should().Be(new DIntDataPoint(new TagName("Motor.Speed")));
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
        var tag = manager.TagFor(new DIntDataPoint(new TagName("Ghost")));

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
        var first = manager.TagFor(new DIntDataPoint(new TagName("Motor.Speed")));
        var second = manager.TagFor(new DIntDataPoint(new TagName("Motor.Speed")));

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
        var speed = manager.TagFor(new DIntDataPoint(new TagName("Motor.Speed")));
        var level = manager.TagFor(new DIntDataPoint(new TagName("Tank.Level")));

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
        var tagFor = () => manager.TagFor(new DIntDataPoint(new TagName("Motor.Speed")));

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
        manager.TagFor(new DIntDataPoint(new TagName("Motor.Speed")));
        manager.TagFor(new DIntDataPoint(new TagName("Tank.Level")));

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
        manager.TagFor(new DIntDataPoint(new TagName("Motor.Speed")));

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
        manager.TagFor(new DIntDataPoint(new TagName("Motor.Speed")));
        manager.TagFor(new DIntDataPoint(new TagName("Tank.Level")));

        // Act
        var dispose = () => manager.Dispose();

        // Assert
        // A tag that will not free is the one case where giving up costs the most: every tag still
        // queued behind it would be left to its finalizer, and that fail-fasts the process (0xC0000602).
        dispose.Should().NotThrow();
        factory.Created.Should().OnlyContain(access => access.DisposeCount == 1);
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
        var tagFor = () => manager.TagFor(new DIntDataPoint(new TagName("Motor.Speed")));

        // Assert
        tagFor.Should().Throw<ObjectDisposedException>();
        factory.CreatedCount.Should().Be(0);
    }

    private sealed class FakeSchemaBrowser : ILogixSchemaBrowser
    {
        private readonly Dictionary<TagName, LogixTypeDeclaration> _declarations =
            new(TagName.CaseInsensitiveComparer);

        public int BrowseCount { get; private set; }

        public LogixTypeDeclaration this[string tagName]
        {
            set => _declarations[new TagName(tagName)] = value;
        }

        public Task<LogixControllerSchema> BrowseAsync(CancellationToken cancellationToken)
        {
            BrowseCount++;
            return Task.FromResult(new LogixControllerSchema(_declarations));
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

        public ILogixTagAccess CreateForSystemTag(TagName tagName) =>
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
