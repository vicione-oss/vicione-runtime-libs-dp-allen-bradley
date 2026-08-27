using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Schema;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Schema;

/// <summary>
/// The browse orchestration over a fake factory that returns canned listing bytes: it reads the
/// controller directory then each program's, qualifies program tags, and disposes every transient
/// handle. No device, no native library.
/// </summary>
public class LogixSchemaBrowserTests
{
    [Fact]
    public async Task BrowseAsync_ReadsControllerAndProgramTags_QualifyingProgramNames()
    {
        var factory = new FakeSystemTagFactory
        {
            ["@tags"] = SymbolListing.Build(
                new SymbolListing.Entry("Motor.Speed", 0x00C4),
                new SymbolListing.Entry("Program:Main", 0x1000)),
            ["Program:Main.@tags"] = SymbolListing.Build(
                new SymbolListing.Entry("Count", 0x00C4)),
        };
        var browser = new LogixSchemaBrowser(factory);

        var schema = await browser.BrowseAsync(TestContext.Current.CancellationToken);

        schema.Lookup(new TagName("Motor.Speed")).Should().NotBeNull();
        // The program tag is reachable only by its qualified name.
        schema.Lookup(new TagName("Program:Main.Count")).Should().NotBeNull();
        schema.Lookup(new TagName("Count")).Should().BeNull();
    }

    [Fact]
    public async Task BrowseAsync_DisposesEveryTransientAccess()
    {
        var factory = new FakeSystemTagFactory
        {
            ["@tags"] = SymbolListing.Build(new SymbolListing.Entry("Motor.Speed", 0x00C4)),
        };
        var browser = new LogixSchemaBrowser(factory);

        await browser.BrowseAsync(TestContext.Current.CancellationToken);

        // A leaked libplctag handle fail-fasts the process (0xC0000602); the browse owns its transients.
        factory.Created.Should().OnlyContain(access => access.IsDisposed);
    }

    [Fact]
    public async Task BrowseAsync_WhenTheDirectoryReadFails_ThrowsSchemaException()
    {
        var factory = new FakeSystemTagFactory(); // no canned "@tags" ⇒ the read fails
        var browser = new LogixSchemaBrowser(factory);

        var browse = async () => await browser.BrowseAsync(TestContext.Current.CancellationToken);

        await browse.Should().ThrowAsync<LogixSchemaException>();
    }

    private sealed class FakeSystemTagFactory : ILogixTagAccessFactory
    {
        private readonly Dictionary<string, byte[]> _listingsByTagName = [];
        private readonly List<FakeSystemTagAccess> _created = [];

        public IReadOnlyList<FakeSystemTagAccess> Created => _created;

        public byte[] this[string tagName]
        {
            set => _listingsByTagName[tagName] = value;
        }

        public ILogixTagAccess Create(ILogixDataPoint dataPoint) =>
            throw new NotSupportedException("The browser only reads system tags.");

        public ILogixTagAccess CreateForSystemTag(TagName tagName)
        {
            var access = new FakeSystemTagAccess(
                _listingsByTagName.TryGetValue(tagName.Value, out var bytes) ? bytes : null, tagName.Value);
            _created.Add(access);
            return access;
        }
    }

    private sealed class FakeSystemTagAccess(byte[]? listing, string tagName) : ILogixTagAccess
    {
        public bool IsDisposed { get; private set; }

        public Task<LogixTagReadResult> ReadAsync(CancellationToken cancellationToken) =>
            Task.FromResult(listing is null
                ? LogixTagReadResult.Failed($"no such system tag '{tagName}'")
                : LogixTagReadResult.Ok(listing));

        public Task<LogixTagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public void Dispose() => IsDisposed = true;
    }
}
