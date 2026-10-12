using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tags;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tests.Tags;

// Every act captures the cache the test disposes and is invoked before the test method returns.
// ReSharper disable AccessToDisposedClosure
public sealed class TagCacheTests
{
    private static readonly FakeDataPoint Speed = new("Speed");
    private static readonly FakeDataPoint Level = new("Level");

    private readonly List<ITagAccess> _created = [];

    [Fact]
    public void TwoEqualDataPointsShareOneTag()
    {
        // Arrange
        // Distinct instances, equal by record value
        // (ADR/2026-07-16-reusing-and-releasing-tag-handles.md).
        using var cache = CreateCache();
        var first = cache.GetOrAdd(new FakeDataPoint("Speed"), CreateTag);

        // Act
        var second = cache.GetOrAdd(new FakeDataPoint("Speed"), CreateTag);

        // Assert
        second.Should().BeSameAs(first);
        _created.Should().ContainSingle();
    }

    [Fact]
    public void DifferentDataPointsGetATagEach()
    {
        // Arrange
        using var cache = CreateCache();
        var speed = cache.GetOrAdd(Speed, CreateTag);

        // Act
        var level = cache.GetOrAdd(Level, CreateTag);

        // Assert
        level.Should().NotBeSameAs(speed);
        _created.Should().HaveCount(2);
    }

    [Fact]
    public void DisposingTheCacheFreesEveryTagItKeeps()
    {
        // Arrange
        var cache = CreateCache();
        cache.GetOrAdd(Speed, CreateTag);
        cache.GetOrAdd(Level, CreateTag);

        // Act
        cache.Dispose();

        // Assert
        // A libplctag handle left to its finalizer fail-fasts the process (0xC0000602).
        _created.Should().AllSatisfy(tag => tag.Received(1).Dispose());
    }

    [Fact]
    public void ACacheDisposedTwiceFreesEachTagOnce()
    {
        // Arrange
        var cache = CreateCache();
        cache.GetOrAdd(Speed, CreateTag);
        cache.Dispose();

        // Act
        cache.Dispose();

        // Assert
        _created.Should().AllSatisfy(tag => tag.Received(1).Dispose());
    }

    [Fact]
    public void ATagThatWillNotFreeDoesNotStopTheOthersFromBeingFreed()
    {
        // Arrange
        var cache = CreateCache();
        cache.GetOrAdd(Speed, CreateTag).When(tag => tag.Dispose()).Do(_ => throw new InvalidOperationException());
        cache.GetOrAdd(Level, CreateTag);

        // Act
        var dispose = Record.Exception(cache.Dispose);

        // Assert
        dispose.Should().BeNull();
        _created.Should().AllSatisfy(tag => tag.Received(1).Dispose());
    }

    [Fact]
    public void DrainingFreesEveryTagTheCacheKeeps()
    {
        // Arrange
        using var cache = CreateCache();
        cache.GetOrAdd(Speed, CreateTag);

        // Act
        cache.Drain();

        // Assert
        _created.Should().AllSatisfy(tag => tag.Received(1).Dispose());
    }

    [Fact]
    public void ADataPointAskedForAfterADrainGetsANewTag()
    {
        // Arrange
        using var cache = CreateCache();
        var beforeTheDrain = cache.GetOrAdd(Speed, CreateTag);
        cache.Drain();

        // Act
        var afterTheDrain = cache.GetOrAdd(Speed, CreateTag);

        // Assert
        afterTheDrain.Should().NotBeSameAs(beforeTheDrain);
    }

    [Fact]
    public void DrainingADisposedCacheIsRefused()
    {
        // Arrange
        var cache = CreateCache();
        cache.Dispose();

        // Act
        var draining = cache.Invoking(c => c.Drain());

        // Assert
        draining.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void AskingADisposedCacheForATagIsRefused()
    {
        // Arrange
        var cache = CreateCache();
        cache.Dispose();

        // Act
        var getOrAdd = Record.Exception(() => cache.GetOrAdd(Speed, CreateTag));

        // Assert
        getOrAdd.Should().BeOfType<ObjectDisposedException>();
        _created.Should().BeEmpty();
    }

    private static TagCache<FakeDataPoint, ITagAccess> CreateCache() => new(NullLogger.Instance);

    private ITagAccess CreateTag(FakeDataPoint dataPoint)
    {
        var tag = Substitute.For<ITagAccess>();
        _created.Add(tag);
        return tag;
    }

    private sealed record FakeDataPoint(string Name);
}
