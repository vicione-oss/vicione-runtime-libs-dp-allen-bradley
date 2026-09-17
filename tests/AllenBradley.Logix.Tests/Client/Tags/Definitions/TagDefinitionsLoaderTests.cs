using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration.Templates;
using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TagsListingTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TemplateTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Definitions;

public sealed class TagDefinitionsLoaderTests
{
    // The symbol type a program container carries; the loader recurses into its own @tags listing.
    private const ushort ProgramSymbolType = 0x1000;

    // The template StructureSymbolType names, and the schema tag that reads it.
    private static readonly TemplateId LineTemplateId = new(0x123);
    private const string LineTemplateTag = "@udt/291";

    // The template StructureMemberType names, and the schema tag that reads it.
    private static readonly TemplateId RampTemplateId = new(0x456);
    private const string RampTemplateTag = "@udt/1110";

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
    public async Task TheTemplateAStructureTagNamesIsFoundUnderItsId()
    {
        // Arrange
        var loader = new TagDefinitionsLoader(ControllerAndProgramListings());

        // Act
        var definitions = await loader.LoadAsync(TestContext.Current.CancellationToken);

        // Assert
        definitions.LookupTemplate(LineTemplateId).Should().NotBeNull();
    }

    [Fact]
    public async Task TheTemplateAProgramStructureTagNamesIsReadToo()
    {
        // Arrange
        var factory = new FakeSchemaTagFactory
        {
            ["@tags"] = Listing(new TagEntry("Program:Main", ProgramSymbolType)),
            ["Program:Main.@tags"] = Listing(new TagEntry("Label", StructureSymbolType)),
            [LineTemplateTag] = Template(new TemplateEntry(LineTemplateId.Value, "Line", [])),
        };
        var loader = new TagDefinitionsLoader(factory);

        // Act
        var definitions = await loader.LoadAsync(TestContext.Current.CancellationToken);

        // Assert
        definitions.LookupTemplate(LineTemplateId).Should().NotBeNull();
    }

    [Fact]
    public async Task ATemplateTwoTagsNameIsReadOnce()
    {
        // Arrange
        var factory = new FakeSchemaTagFactory
        {
            ["@tags"] = Listing(
                new TagEntry("Line.Label", StructureSymbolType),
                new TagEntry("Line.Code", StructureSymbolType)),
            [LineTemplateTag] = Template(new TemplateEntry(LineTemplateId.Value, "Line", [])),
        };
        var loader = new TagDefinitionsLoader(factory);

        // Act
        await loader.LoadAsync(TestContext.Current.CancellationToken);

        // Assert
        factory.Requested.Should().ContainSingle(address => address.Value == LineTemplateTag);
    }

    [Fact]
    public async Task ATemplateNamedOnlyByAMemberOfAnotherTemplateIsReadToo()
    {
        // Arrange
        var factory = new FakeSchemaTagFactory
        {
            ["@tags"] = Listing(new TagEntry("Line.Label", StructureSymbolType)),
            [LineTemplateTag] = Template(new TemplateEntry(LineTemplateId.Value, "Line",
                [new MemberEntry("Ramp", StructureMemberType)])),
            [RampTemplateTag] = Template(new TemplateEntry(RampTemplateId.Value, "Ramp", [])),
        };
        var loader = new TagDefinitionsLoader(factory);

        // Act
        var definitions = await loader.LoadAsync(TestContext.Current.CancellationToken);

        // Assert
        definitions.LookupTemplate(RampTemplateId).Should().NotBeNull();
    }

    [Fact]
    public async Task ATemplateWhoseMemberNamesItselfIsReadOnce()
    {
        // Arrange
        var factory = new FakeSchemaTagFactory
        {
            ["@tags"] = Listing(new TagEntry("Line.Label", StructureSymbolType)),
            [LineTemplateTag] = Template(new TemplateEntry(LineTemplateId.Value, "Line",
                [new MemberEntry("Next", StructureSymbolType)])),
        };
        var loader = new TagDefinitionsLoader(factory);

        // Act
        await loader.LoadAsync(TestContext.Current.CancellationToken);

        // Assert
        factory.Requested.Should().ContainSingle(address => address.Value == LineTemplateTag);
    }

    [Fact]
    public async Task ASystemStructureTagHasNoTemplateRead()
    {
        // Arrange
        var factory = new FakeSchemaTagFactory
        {
            ["@tags"] = Listing(new TagEntry("Map:Local", SystemStructureSymbolType)),
        };
        var loader = new TagDefinitionsLoader(factory);

        // Act
        await loader.LoadAsync(TestContext.Current.CancellationToken);

        // Assert
        factory.Requested.Should().NotContain(address => address.Value.StartsWith("@udt/", StringComparison.Ordinal));
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

    [Fact]
    public async Task ABrowseWhoseTemplateReadFailsReportsARetrievalFailure()
    {
        // Arrange
        // Nothing canned for the template the structure tag names, so that read is what fails.
        var factory = new FakeSchemaTagFactory
        {
            ["@tags"] = Listing(new TagEntry("Line.Label", StructureSymbolType)),
        };
        var loader = new TagDefinitionsLoader(factory);

        // Act
        var loading = loader.Awaiting(l => l.LoadAsync(TestContext.Current.CancellationToken));

        // Assert
        await loading.Should().ThrowAsync<DataRetrievalException>();
    }

    [Fact]
    public async Task ABrowseWhoseTemplateCannotBeDecodedReportsARetrievalFailure()
    {
        // Arrange
        var factory = new FakeSchemaTagFactory
        {
            ["@tags"] = Listing(new TagEntry("Line.Label", StructureSymbolType)),
            [LineTemplateTag] = Template(StringTemplate())[..10],
        };
        var loader = new TagDefinitionsLoader(factory);

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
                new TagEntry("Line.Label", StructureSymbolType),
                new TagEntry("Program:Main", ProgramSymbolType)),
            ["Program:Main.@tags"] = Listing(new TagEntry("Count", DintSymbolType)),
            [LineTemplateTag] = Template(new TemplateEntry(LineTemplateId.Value, "Line", [])),
        };

    private sealed class FakeSchemaTagFactory : ILogixTagAccessFactory
    {
        private readonly Dictionary<string, byte[]> _listingsByTagName = [];
        private readonly List<ILogixTagAccess> _created = [];
        private readonly List<TagAddress> _requested = [];

        public IReadOnlyList<ILogixTagAccess> Created => _created;

        /// <summary>Every schema tag the browse asked for, in the order it asked.</summary>
        public IReadOnlyList<TagAddress> Requested => _requested;

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
            _requested.Add(tagAddress);
            return access;
        }
    }
}
