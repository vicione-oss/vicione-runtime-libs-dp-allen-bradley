using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.SymbolTypes;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.TagsListing;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TagDefinitionTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TagsListingTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Symbols.TagsListing;

public sealed class TagsDecoderTests
{
    // Outside the elementary range 0xC1-0xCB the dataport knows how to decode.
    private const ushort UnmodelledSymbolType = 0x00DE;

    [Fact]
    public void AScalarAtomicEntryDecodesToItsNameTypeAndScalarShape()
    {
        // Arrange
        var listing = Listing(new TagEntry("Motor.Speed", DintSymbolType));

        // Act
        var decoded = TagsDecoder.Decode(listing);

        // Assert
        var expected = new ListedTag(new TagAddress("Motor.Speed"), DefaultAtomicTagDefinition());
        decoded.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void ListingEntriesOfDifferentNameLengthsAreAllWalked()
    {
        // Arrange
        // Each name is a different length, so an entry walked by a fixed stride lands mid-header.
        var listing = Listing(
            new TagEntry("A", DintSymbolType),
            new TagEntry("Tank.Level", RealSymbolType),
            new TagEntry("Program:Main", UnmodelledSymbolType));

        // Act
        var decoded = TagsDecoder.Decode(listing);

        // Assert
        ListedTag[] expected =
        [
            new ListedTag(new TagAddress("A"), DefaultAtomicTagDefinition()),
            new(new TagAddress("Tank.Level"), DefaultAtomicTagDefinition() with { DataType = AllenBradleyDataType.Real }),
            new(new TagAddress("Program:Main"), DefaultAtomicTagDefinition() with { DataType = AllenBradleyDataType.Unknown }),
        ];
        decoded.Should().Equal(expected);
    }

    [Fact]
    public void AStructureEntryDecodesToAStructureNamingItsTemplate()
    {
        // Arrange
        // The listing cannot tell a STRING from a TIMER; the lookup decides that once it has the template.
        var listing = Listing(new TagEntry("Line.Label", StructureSymbolType));

        // Act
        var decoded = TagsDecoder.Decode(listing);

        // Assert
        var expected = new ListedTag(new TagAddress("Line.Label"), DefaultStructureTagDefinition());
        decoded.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void ASystemStructureEntryNamesNoTemplate()
    {
        // Arrange
        var listing = Listing(new TagEntry("Map:Local", SystemStructureSymbolType));

        // Act
        var decoded = TagsDecoder.Decode(listing);

        // Assert
        var expected = new ListedTag(
            new TagAddress("Map:Local"), DefaultStructureTagDefinition() with { TemplateId = null });
        decoded.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void AOneDimensionalArrayEntryDecodesToItsRankAndElementCount()
    {
        // Arrange
        var listing = Listing(
            new TagEntry("Counts", OneDimensionSymbolType | DintSymbolType) { Dimension0 = 10 });

        // Act
        var decoded = TagsDecoder.Decode(listing);

        // Assert
        var expected = new ListedTag(
            new TagAddress("Counts"),
            DefaultAtomicTagDefinition() with
            {
                DimensionCount = new DimensionCount(1),
                ElementCount = new ElementCount(10),
            });
        decoded.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Theory]
    [InlineData(1u, 32u)]
    [InlineData(2u, 64u)]
    public void ABoolArrayEntryDecodesItsWordDimensionIntoTheBitsItHolds(
        uint declaredWords, uint expectedBitCount)
    {
        // Arrange
        var listing = Listing(
            new TagEntry("Flags", OneDimensionSymbolType | DwordSymbolType) { Dimension0 = declaredWords });

        // Act
        var decoded = TagsDecoder.Decode(listing);

        // Assert
        var expected = new ListedTag(
            new TagAddress("Flags"),
            DefaultAtomicTagDefinition() with
            {
                DataType = AllenBradleyDataType.Bool,
                DimensionCount = new DimensionCount(1),
                ElementCount = new ElementCount(expectedBitCount),
            });
        decoded.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void AnUnmodelledTypeCodeDecodesToUnknownRatherThanTravellingAsANumber()
    {
        // Arrange
        var listing = Listing(new TagEntry("Exotic", UnmodelledSymbolType));

        // Act
        var decoded = TagsDecoder.Decode(listing);

        // Assert
        // Still atomic, because the structure bit is clear in the symbol type.
        var expected = new ListedTag(
            new TagAddress("Exotic"), DefaultAtomicTagDefinition() with { DataType = AllenBradleyDataType.Unknown });
        decoded.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void AnEmptyListingDecodesToNothing()
    {
        // Arrange
        ReadOnlySpan<byte> emptyListing = [];

        // Act
        var decoded = TagsDecoder.Decode(emptyListing);

        // Assert
        decoded.Should().BeEmpty();
    }
}
