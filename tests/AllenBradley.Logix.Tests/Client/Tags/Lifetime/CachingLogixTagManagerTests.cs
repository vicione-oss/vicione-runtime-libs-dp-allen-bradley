using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TagDefinitionTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Lifetime;

// Every act captures the manager the test disposes and is invoked before the test method returns.
// ReSharper disable AccessToDisposedClosure
public sealed class CachingLogixTagManagerTests
{
    private static readonly TagAddress SpeedTagAddress = new("Motor.Speed");
    private static readonly TagAddress LevelTagAddress = new("Tank.Level");
    private static readonly TagAddress GhostTagAddress = new("Ghost");

    private static readonly TagDefinition SpeedDefinition =
        DefaultAtomicTagDefinition() with { TagAddress = SpeedTagAddress };

    [Fact]
    public async Task ASecondLoadOfTheTagDefinitionsDoesNotBrowseAgain()
    {
        // Arrange
        var browser = new FakeTagDefinitionsLoader();
        using var manager = CreateManager(new CountingAccessFactory(), browser);
        await manager.LoadTagDefinitionsAsync(TestContext.Current.CancellationToken);

        // Act
        await manager.LoadTagDefinitionsAsync(TestContext.Current.CancellationToken);

        // Assert
        browser.BrowseCount.Should().Be(1);
    }

    [Fact]
    public async Task ConcurrentLoadsBrowseOnceAndBothWaitForThatBrowse()
    {
        // Arrange
        // The first browse blocks until the test releases it.
        var browseGate = new TaskCompletionSource();
        var browser = new FakeTagDefinitionsLoader { OnBrowse = () => browseGate.Task };
        using var manager = CreateManager(new CountingAccessFactory(), browser);

        // Act
        // A bounded wait is the only way to observe that the second load has not completed yet.
        var first = manager.LoadTagDefinitionsAsync(TestContext.Current.CancellationToken);
        await browser.BrowseStarted.Task;
        var second = manager.LoadTagDefinitionsAsync(TestContext.Current.CancellationToken);
        await Task.Delay(50, TestContext.Current.CancellationToken);
        var secondCompletedEarly = second.IsCompleted;
        browseGate.SetResult();
        await Task.WhenAll(first, second);

        // Assert
        secondCompletedEarly.Should().BeFalse();
        browser.BrowseCount.Should().Be(1);
    }

    [Fact]
    public async Task ALoadAfterAFailedBrowseGoesToTheControllerAgain()
    {
        // Arrange
        var browser = new FakeTagDefinitionsLoader
        {
            OnBrowse = () => Task.FromException(new DataRetrievalException("no route to host")),
        };
        using var manager = CreateManager(new CountingAccessFactory(), browser);
        _ = await Record.ExceptionAsync(() => manager.LoadTagDefinitionsAsync(TestContext.Current.CancellationToken));

        // Act
        var retry = await Record.ExceptionAsync(
            () => manager.LoadTagDefinitionsAsync(TestContext.Current.CancellationToken));

        // Assert
        retry.Should().BeOfType<DataRetrievalException>();
        browser.BrowseCount.Should().Be(2);
    }

    [Fact]
    public async Task ATagCarriesTheControllersDefinitionForItsName()
    {
        // Arrange
        var browser = new FakeTagDefinitionsLoader { [SpeedTagAddress] = SpeedDefinition };
        using var manager = CreateManager(new CountingAccessFactory(), browser);
        await manager.LoadTagDefinitionsAsync(TestContext.Current.CancellationToken);

        // Act
        var tag = manager.TagFor(DataPointNamed(SpeedTagAddress));

        // Assert
        tag.Metadata.Should().Be(SpeedDefinition);
    }

    [Fact]
    public async Task ATagAbsentFromTheDefinitionsIsStillResolvedAndCarriesNone()
    {
        // Arrange
        var browser = new FakeTagDefinitionsLoader { [SpeedTagAddress] = SpeedDefinition };
        using var manager = CreateManager(new CountingAccessFactory(), browser);
        await manager.LoadTagDefinitionsAsync(TestContext.Current.CancellationToken);

        // Act
        var tag = manager.TagFor(DataPointNamed(GhostTagAddress));

        // Assert
        tag.Metadata.Should().BeNull();
    }

    [Fact]
    public async Task TwoEqualDataPointsShareOneTag()
    {
        // Arrange
        // Distinct instances, equal by record value
        // (ADR/2026-07-16-reusing-and-releasing-tag-handles.md).
        var factory = new CountingAccessFactory();
        using var manager = CreateManager(factory, new FakeTagDefinitionsLoader());
        await manager.LoadTagDefinitionsAsync(TestContext.Current.CancellationToken);
        var first = manager.TagFor(DataPointNamed(SpeedTagAddress));

        // Act
        var second = manager.TagFor(DataPointNamed(SpeedTagAddress));

        // Assert
        second.Should().BeSameAs(first);
        factory.CreatedCount.Should().Be(1);
    }

    [Fact]
    public async Task DifferentDataPointsGetATagEach()
    {
        // Arrange
        var factory = new CountingAccessFactory();
        using var manager = CreateManager(factory, new FakeTagDefinitionsLoader());
        await manager.LoadTagDefinitionsAsync(TestContext.Current.CancellationToken);
        var speed = manager.TagFor(DataPointNamed(SpeedTagAddress));

        // Act
        var level = manager.TagFor(DataPointNamed(LevelTagAddress));

        // Assert
        level.Should().NotBeSameAs(speed);
        factory.CreatedCount.Should().Be(2);
    }

    [Fact]
    public void AskingForATagBeforeTheDefinitionsAreLoadedIsRefused()
    {
        // Arrange
        var factory = new CountingAccessFactory();
        using var manager = CreateManager(factory, new FakeTagDefinitionsLoader());

        // Act
        var tagFor = Record.Exception(() => manager.TagFor(DataPointNamed(SpeedTagAddress)));

        // Assert
        tagFor.Should().BeOfType<InvalidOperationException>();
        factory.CreatedCount.Should().Be(0);
    }

    [Fact]
    public async Task DisposingTheManagerFreesEveryTagItCreated()
    {
        // Arrange
        var factory = new CountingAccessFactory();
        var manager = CreateManager(factory, new FakeTagDefinitionsLoader());
        await manager.LoadTagDefinitionsAsync(TestContext.Current.CancellationToken);
        manager.TagFor(DataPointNamed(SpeedTagAddress));
        manager.TagFor(DataPointNamed(LevelTagAddress));

        // Act
        manager.Dispose();

        // Assert
        // A libplctag handle left to its finalizer fail-fasts the process (0xC0000602).
        factory.Created.Should().AllSatisfy(access => access.Received(1).Dispose());
    }

    [Fact]
    public async Task AManagerDisposedTwiceFreesEachTagOnce()
    {
        // Arrange
        var factory = new CountingAccessFactory();
        var manager = CreateManager(factory, new FakeTagDefinitionsLoader());
        await manager.LoadTagDefinitionsAsync(TestContext.Current.CancellationToken);
        manager.TagFor(DataPointNamed(SpeedTagAddress));
        manager.Dispose();

        // Act
        manager.Dispose();

        // Assert
        factory.Created.Should().AllSatisfy(access => access.Received(1).Dispose());
    }

    [Fact]
    public async Task ATagThatWillNotFreeDoesNotStopTheOthersFromBeingFreed()
    {
        // Arrange
        var factory = new CountingAccessFactory { ThrowOnDisposeFor = SpeedTagAddress };
        var manager = CreateManager(factory, new FakeTagDefinitionsLoader());
        await manager.LoadTagDefinitionsAsync(TestContext.Current.CancellationToken);
        manager.TagFor(DataPointNamed(SpeedTagAddress));
        manager.TagFor(DataPointNamed(LevelTagAddress));

        // Act
        var dispose = Record.Exception(manager.Dispose);

        // Assert
        dispose.Should().BeNull();
        factory.Created.Should().AllSatisfy(access => access.Received(1).Dispose());
    }

    [Fact]
    public async Task DrainingFreesEveryTagTheManagerCreated()
    {
        // Arrange
        var factory = new CountingAccessFactory();
        using var manager = CreateManager(factory, new FakeTagDefinitionsLoader());
        await manager.LoadTagDefinitionsAsync(TestContext.Current.CancellationToken);
        manager.TagFor(DataPointNamed(SpeedTagAddress));

        // Act
        manager.Drain();

        // Assert
        factory.Created.Should().AllSatisfy(access => access.Received(1).Dispose());
    }

    [Fact]
    public async Task AManagerThatWasDrainedBrowsesAndBuildsItsTagsAgain()
    {
        // Arrange
        var factory = new CountingAccessFactory();
        var browser = new FakeTagDefinitionsLoader();
        using var manager = CreateManager(factory, browser);
        await manager.LoadTagDefinitionsAsync(TestContext.Current.CancellationToken);
        manager.TagFor(DataPointNamed(SpeedTagAddress));
        manager.Drain();

        // Act
        await manager.LoadTagDefinitionsAsync(TestContext.Current.CancellationToken);
        manager.TagFor(DataPointNamed(SpeedTagAddress));

        // Assert
        browser.BrowseCount.Should().Be(2);
        factory.CreatedCount.Should().Be(2);
    }

    [Fact]
    public async Task AskingForATagAfterADrainIsRefusedUntilTheDefinitionsAreLoadedAgain()
    {
        // Arrange
        var factory = new CountingAccessFactory();
        using var manager = CreateManager(factory, new FakeTagDefinitionsLoader());
        await manager.LoadTagDefinitionsAsync(TestContext.Current.CancellationToken);
        manager.Drain();

        // Act
        var tagFor = Record.Exception(() => manager.TagFor(DataPointNamed(SpeedTagAddress)));

        // Assert
        tagFor.Should().BeOfType<InvalidOperationException>();
        factory.CreatedCount.Should().Be(0);
    }

    [Fact]
    public async Task DrainingADisposedManagerIsRefused()
    {
        // Arrange
        var manager = CreateManager(new CountingAccessFactory(), new FakeTagDefinitionsLoader());
        await manager.LoadTagDefinitionsAsync(TestContext.Current.CancellationToken);
        manager.Dispose();

        // Act
        var draining = manager.Invoking(m => m.Drain());

        // Assert
        draining.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public async Task AskingADisposedManagerForATagIsRefused()
    {
        // Arrange
        var factory = new CountingAccessFactory();
        var manager = CreateManager(factory, new FakeTagDefinitionsLoader());
        await manager.LoadTagDefinitionsAsync(TestContext.Current.CancellationToken);
        manager.Dispose();

        // Act
        var tagFor = Record.Exception(() => manager.TagFor(DataPointNamed(SpeedTagAddress)));

        // Assert
        tagFor.Should().BeOfType<ObjectDisposedException>();
        factory.CreatedCount.Should().Be(0);
    }

    private static CachingLogixTagManager CreateManager(
        CountingAccessFactory factory, FakeTagDefinitionsLoader browser) =>
        new(factory, browser, NullLogger<CachingLogixTagManager>.Instance);

    private static DIntDataPoint DataPointNamed(TagAddress tagAddress) =>
        new(tagAddress, DefaultPollFrequency, NoChannels);

    private sealed class FakeTagDefinitionsLoader : ITagDefinitionsLoader
    {
        private readonly Dictionary<TagAddress, TagDefinition> _definitions = new(TagAddress.CaseInsensitiveComparer);

        public int BrowseCount { get; private set; }

        public TaskCompletionSource BrowseStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Func<Task> OnBrowse { get; init; } = static () => Task.CompletedTask;

        public TagDefinition this[TagAddress tagAddress]
        {
            set => _definitions[tagAddress] = value;
        }

        public async Task<TagDefinitions> LoadAsync(CancellationToken cancellationToken)
        {
            BrowseCount++;
            BrowseStarted.TrySetResult();
            await OnBrowse().ConfigureAwait(false);
            return new TagDefinitions(_definitions);
        }
    }

    private sealed class CountingAccessFactory : ILogixTagAccessFactory
    {
        private readonly List<ILogixTagAccess> _created = [];

        public IReadOnlyList<ILogixTagAccess> Created => _created;

        public int CreatedCount => _created.Count;

        public TagAddress? ThrowOnDisposeFor { get; init; }

        public ILogixTagAccess Create(ILogixDataPoint dataPoint)
        {
            var access = Substitute.For<ILogixTagAccess>();
            if (dataPoint.TagAddress == ThrowOnDisposeFor)
            {
                access.When(a => a.Dispose()).Do(_ => throw new LogixTagException("Access refused to free."));
            }

            _created.Add(access);
            return access;
        }

        public ILogixTagAccess CreateForSchemaTag(TagAddress tagAddress) =>
            throw new NotSupportedException("The manager browses through the injected ITagDefinitionsLoader.");
    }
}
