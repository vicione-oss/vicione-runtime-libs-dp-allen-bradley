using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.SymbolTypes;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.TagsListing;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TagsListingTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Symbols.TagsListing;

/// <summary>
/// The struct reinterprets the controller's bytes, so nothing checks its layout at compile time and a
/// reordered or repacked field would fail silently.
/// </summary>
public sealed class TagsEntryHeaderTests
{
    private const string EntryName = "Tank.Level";
    private const uint EntryInstanceId = 0x0A0B0C0D;
    private const ushort EntrySymbolType = 0x8123;
    private const ushort EntryElementLength = 68;

    // Distinct values, so a swapped pair of fields shows up rather than passing by coincidence.
    private static readonly TagEntry Distinct = new(EntryName, EntrySymbolType)
    {
        InstanceId = EntryInstanceId,
        ElementLength = EntryElementLength,
        Dimension0 = 7,
        Dimension1 = 11,
        Dimension2 = 13,
    };

    [Fact]
    public void TheHeaderIsTheSizeThePackedLayoutGivesIt()
    {
        // Arrange

        // Act
        var size = TagsEntryHeader.Size;

        // Assert
        // Without Pack = 1 the trailing ushort is padded to four-byte alignment and this reports 24.
        size.Should().Be(22);
    }

    [Fact]
    public void EveryFieldOfAnEntryIsReadFromItsOwnOffset()
    {
        // Arrange
        var listing = Listing(Distinct);

        // Act
        var header = TagsEntryHeader.ReadFrom(listing);

        // Assert
        // Field by field because the struct offers no constructor to build a whole expectation from.
        header.InstanceId.Should().Be(EntryInstanceId);
        header.SymbolType.Should().Be(new SymbolType(EntrySymbolType));
        header.ElementLength.Should().Be(EntryElementLength);
        header.FirstDimension.Should().Be(Distinct.Dimension0);
        header.SecondDimension.Should().Be(Distinct.Dimension1);
        header.ThirdDimension.Should().Be(Distinct.Dimension2);
        header.NameLength.Should().Be((ushort)EntryName.Length);
    }

    [Fact]
    public void AnEntryAtAnUnalignedOffsetIsReadTheSameWay()
    {
        // Arrange
        // Entries start where the previous name ended, so a one-character name gives an odd address.
        var listing = Listing(new TagEntry("A", DintSymbolType), Distinct);
        var secondEntry = listing.AsSpan(TagsEntryHeader.Size + 1);

        // Act
        var header = TagsEntryHeader.ReadFrom(secondEntry);

        // Assert
        header.Should().BeEquivalentTo(TagsEntryHeader.ReadFrom(Listing(Distinct)));
    }

    [Fact]
    public void TheNameFollowingAnEntryNeverBleedsIntoItsHeader()
    {
        // Arrange
        var headerOnly = Listing(Distinct).AsSpan(0, TagsEntryHeader.Size);

        // Act
        var truncated = TagsEntryHeader.ReadFrom(headerOnly);

        // Assert
        // The read is bounded by the struct, not by the span it was handed.
        truncated.Should().BeEquivalentTo(TagsEntryHeader.ReadFrom(Listing(Distinct)));
    }
}
