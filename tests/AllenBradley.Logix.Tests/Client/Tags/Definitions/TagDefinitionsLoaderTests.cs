using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Exceptions;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Definitions;

/// <summary>
/// The browse orchestration over a fake factory that returns canned listing bytes: it reads the
/// controller directory then each program's, qualifies program tags, and disposes every transient
/// handle. No device, no native library.
/// </summary>
public class TagDefinitionsLoaderTests
{
    [Fact]
    public async Task BrowseAsync_ReadsControllerAndProgramTags_QualifyingProgramNames()
    {
        // Arrange
        var factory = new FakeSchemaTagFactory
        {
            ["@tags"] = TagsDataBuilder.Build(
                new TagsDataBuilder.TagEntry("Motor.Speed", 0x00C4),
                new TagsDataBuilder.TagEntry("Program:Main", 0x1000)),
            ["Program:Main.@tags"] = TagsDataBuilder.Build(
                new TagsDataBuilder.TagEntry("Count", 0x00C4)),
        };
        var browser = new TagDefinitionsLoader(factory);

        // Act
        var schema = await browser.LoadAsync(TestContext.Current.CancellationToken);

        // Assert
        schema.Lookup(new TagName("Motor.Speed")).Should().NotBeNull();
        // The program tag is reachable only by its qualified name.
        schema.Lookup(new TagName("Program:Main.Count")).Should().NotBeNull();
        schema.Lookup(new TagName("Count")).Should().BeNull();
    }

    [Fact]
    public async Task BrowseAsync_DisposesEveryTransientAccess()
    {
        // Arrange
        var factory = new FakeSchemaTagFactory
        {
            ["@tags"] = TagsDataBuilder.Build(new TagsDataBuilder.TagEntry("Motor.Speed", 0x00C4)),
        };
        var browser = new TagDefinitionsLoader(factory);

        // Act
        await browser.LoadAsync(TestContext.Current.CancellationToken);

        // Assert
        // A leaked libplctag handle fail-fasts the process (0xC0000602); the browse owns its transients.
        factory.Created.Should().OnlyContain(access => access.IsDisposed);
    }

    [Fact]
    public async Task BrowseAsync_WhenTheDirectoryReadFails_ThrowsSchemaException()
    {
        // Arrange
        var factory = new FakeSchemaTagFactory(); // no canned "@tags" ⇒ the read fails
        var browser = new TagDefinitionsLoader(factory);

        // Act
        var browse = browser.Awaiting(b => b.LoadAsync(TestContext.Current.CancellationToken));

        // Assert
        await browse.Should().ThrowAsync<DataRetrievalException>();
    }

    private sealed class FakeSchemaTagFactory : ILogixTagAccessFactory
    {
        private readonly Dictionary<string, byte[]> _listingsByTagName = [];
        private readonly List<FakeSchemaTagAccess> _created = [];

        public IReadOnlyList<FakeSchemaTagAccess> Created => _created;

        public byte[] this[string tagName]
        {
            set => _listingsByTagName[tagName] = value;
        }

        public ILogixTagAccess Create(ILogixDataPoint dataPoint) =>
            throw new NotSupportedException("The browser only reads schema tags.");

        public ILogixTagAccess CreateForSchemaTag(TagName tagName)
        {
            var access = new FakeSchemaTagAccess(
                _listingsByTagName.TryGetValue(tagName.Value, out var bytes) ? bytes : null, tagName.Value);
            _created.Add(access);
            return access;
        }
    }

    private sealed class FakeSchemaTagAccess(byte[]? listing, string tagName) : ILogixTagAccess
    {
        public bool IsDisposed { get; private set; }

        public Task<LogixTagReadResult> ReadAsync(CancellationToken cancellationToken) =>
            Task.FromResult(listing is null
                ? LogixTagReadResult.Failed($"no such schema tag '{tagName}'")
                : LogixTagReadResult.Ok(listing));

        public Task<LogixTagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public void Dispose() => IsDisposed = true;
    }
}
