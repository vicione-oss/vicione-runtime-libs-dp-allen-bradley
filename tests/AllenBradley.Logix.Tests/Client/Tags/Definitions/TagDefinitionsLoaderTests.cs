using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TagsListingTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Definitions;

public sealed class TagDefinitionsLoaderTests
{
    // The symbol type a program container carries; the loader recurses into its own @tags listing.
    private const ushort ProgramSymbolType = 0x1000;

    [Fact]
    public async Task AControllerTagIsFoundUnderItsOwnName()
    {
        // Arrange
        var loader = new TagDefinitionsLoader(ControllerAndProgramListings());

        // Act
        var definitions = await loader.LoadAsync(TestContext.Current.CancellationToken);

        // Assert
        definitions.Lookup(new TagAddress("Motor.Speed")).Should().NotBeNull();
    }

    [Fact]
    public async Task AProgramTagIsFoundUnderItsProgramQualifiedName()
    {
        // Arrange
        var loader = new TagDefinitionsLoader(ControllerAndProgramListings());

        // Act
        var definitions = await loader.LoadAsync(TestContext.Current.CancellationToken);

        // Assert
        definitions.Lookup(new TagAddress("Program:Main.Count")).Should().NotBeNull();
    }

    [Fact]
    public async Task AProgramTagIsNotFoundUnderItsBareName()
    {
        // Arrange
        var loader = new TagDefinitionsLoader(ControllerAndProgramListings());

        // Act
        var definitions = await loader.LoadAsync(TestContext.Current.CancellationToken);

        // Assert
        definitions.Lookup(new TagAddress("Count")).Should().BeNull();
    }

    [Fact]
    public async Task EveryTransientHandleTheBrowseOpenedIsFreed()
    {
        // Arrange
        var factory = ControllerAndProgramListings();
        var loader = new TagDefinitionsLoader(factory);

        // Act
        await loader.LoadAsync(TestContext.Current.CancellationToken);

        // Assert
        // A leaked libplctag handle fail-fasts the process (0xC0000602).
        factory.Created.Should().AllSatisfy(access => access.Received(1).Dispose());
    }

    [Fact]
    public async Task ABrowseWhoseDirectoryReadFailsReportsARetrievalFailure()
    {
        // Arrange
        // Nothing canned for "@tags", so the controller directory read is what fails.
        var loader = new TagDefinitionsLoader(new FakeSchemaTagFactory());

        // Act
        var loading = loader.Awaiting(l => l.LoadAsync(TestContext.Current.CancellationToken));

        // Assert
        await loading.Should().ThrowAsync<DataRetrievalException>();
    }

    private static FakeSchemaTagFactory ControllerAndProgramListings() =>
        new()
        {
            ["@tags"] = Listing(
                new TagEntry("Motor.Speed", DintSymbolType),
                new TagEntry("Program:Main", ProgramSymbolType)),
            ["Program:Main.@tags"] = Listing(new TagEntry("Count", DintSymbolType)),
        };

    private sealed class FakeSchemaTagFactory : ILogixTagAccessFactory
    {
        private readonly Dictionary<string, byte[]> _listingsByTagName = [];
        private readonly List<ILogixTagAccess> _created = [];

        public IReadOnlyList<ILogixTagAccess> Created => _created;

        public byte[] this[string tagName]
        {
            set => _listingsByTagName[tagName] = value;
        }

        public ILogixTagAccess Create(ILogixDataPoint dataPoint) =>
            throw new NotSupportedException("The loader only reads schema tags.");

        public ILogixTagAccess CreateForSchemaTag(TagAddress tagAddress)
        {
            var access = Substitute.For<ILogixTagAccess>();
            access.ReadAsync(Arg.Any<CancellationToken>()).Returns(
                _listingsByTagName.TryGetValue(tagAddress.Value, out var listing)
                    ? LogixTagReadResult.Ok(listing)
                    : LogixTagReadResult.Failed($"no such schema tag '{tagAddress.Value}'"));
            _created.Add(access);
            return access;
        }
    }
}
