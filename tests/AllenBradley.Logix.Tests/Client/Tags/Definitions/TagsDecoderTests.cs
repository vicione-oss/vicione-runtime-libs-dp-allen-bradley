using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TagDefinitionTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TagsListingTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Definitions;

public sealed class TagsDecoderTests
{
    // Outside the elementary range 0xC1-0xCB the addon knows how to decode.
    private const ushort UnmodelledSymbolType = 0x00DE;

    // The member bytes the listing reports for a built-in STRING: .LEN (4) + .DATA[82].
    private const ushort StringElementLength = 86;

    [Fact]
    public void AScalarAtomicEntryDecodesToItsNameTypeAndScalarShape()
    {
        // Arrange
        var listing = Listing(new TagEntry("Motor.Speed", DintSymbolType));

        // Act
        var decoded = TagsDecoder.Decode(listing);

        // Assert
        var expected = DefaultAtomicTagDefinition() with { TagName = new TagName("Motor.Speed") };
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
        TagDefinition[] expected =
        [
            DefaultAtomicTagDefinition() with { TagName = new TagName("A") },
            DefaultAtomicTagDefinition() with
            {
                TagName = new TagName("Tank.Level"), DataType = AllenBradleyDataType.Real,
            },
            DefaultAtomicTagDefinition() with
            {
                TagName = new TagName("Program:Main"), DataType = AllenBradleyDataType.Unknown,
            },
        ];
        decoded.Should().Equal(expected);
    }

    [Fact]
    public void ABuiltInStringEntryDecodesToAStructureOfTheStandardCapacity()
    {
        // Arrange
        var listing = Listing(
            new TagEntry("Line.Label", StructureSymbolType) { ElementLength = StringElementLength });

        // Act
        var decoded = TagsDecoder.Decode(listing);

        // Assert
        // The element length is spent here and leaves as a capacity: nothing above the decoder is told a
        // byte count.
        var expected = DefaultStringTagDefinition() with { TagName = new TagName("Line.Label") };
        decoded.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void ACustomStringEntryDecodesToItsOwnCapacity()
    {
        // Arrange
        // A STRING_20 is the same .LEN + .DATA[n] shape with a different n, described the same way.
        var listing = Listing(new TagEntry("Line.Code", StructureSymbolType) { ElementLength = 24 });

        // Act
        var decoded = TagsDecoder.Decode(listing);

        // Assert
        var expected = DefaultStringTagDefinition() with
        {
            TagName = new TagName("Line.Code"),
            MaxLength = new StringMaxLength(20),
        };
        decoded.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void AnAtomicEntryDecodesWithNoCapacity()
    {
        // Arrange
        // Capacity belongs to the one type whose size the type does not fix; a DINT has none to report.
        var listing = Listing(new TagEntry("Motor.Speed", DintSymbolType));

        // Act
        var decoded = TagsDecoder.Decode(listing);

        // Assert
        decoded.Should().ContainSingle().Which.MaxLength.Should().BeNull();
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
        var expected = DefaultAtomicTagDefinition() with
        {
            TagName = new TagName("Counts"),
            DimensionCount = new DimensionCount(1),
            ElementCount = new ElementCount(10),
        };
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
        // Still atomic, because the structure bit is clear, but the wire code stops at the decoder.
        var expected = DefaultAtomicTagDefinition() with
        {
            TagName = new TagName("Exotic"),
            DataType = AllenBradleyDataType.Unknown,
        };
        decoded.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void AnEmptyListingDecodesToNothing()
    {
        // Arrange

        // Act
        var decoded = TagsDecoder.Decode([]);

        // Assert
        decoded.Should().BeEmpty();
    }
}
