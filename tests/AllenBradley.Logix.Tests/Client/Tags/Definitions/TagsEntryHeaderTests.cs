using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Definitions;

/// <summary>
/// The marshalling contract of the <c>@tags</c> entry header on its own: the struct reinterprets the
/// controller's bytes, so nothing checks its layout at compile time and a reordered or repacked field
/// would fail silently. <see cref="TagsDataBuilder"/> writes the same layout the long way round — explicit
/// little-endian primitives at explicit offsets — which makes it an independent statement of what the
/// struct is supposed to be.
/// </summary>
public class TagsEntryHeaderTests
{
    private static readonly TagsDataBuilder.TagEntry Distinct = new("Tank.Level", SymbolType: 0x8123)
    {
        InstanceId = 0x0A0B0C0D,
        ElementLength = 68,
        Dimension0 = 7,
        Dimension1 = 11,
        Dimension2 = 13,
    };

    [Fact]
    public void Size_IsThePackedHeader_NotThePaddedOne()
    {
        // Arrange

        // Act
        var size = TagsEntryHeader.Size;

        // Assert
        // Without Pack = 1 the trailing ushort is padded to the struct's four-byte alignment and this
        // reports 24, which walks every entry after the first two bytes off the end of its name.
        size.Should().Be(22);
    }

    [Fact]
    public void ReadFrom_AnEntry_MapsEveryFieldToItsOwnOffset()
    {
        // Arrange
        // Every field holds a different value, so a swapped pair shows up as a wrong value rather than
        // passing by coincidence.
        var listing = TagsDataBuilder.Build(Distinct);

        // Act
        var header = TagsEntryHeader.ReadFrom(listing);

        // Assert
        header.InstanceId.Should().Be(0x0A0B0C0D);
        header.SymbolType.Should().Be(0x8123);
        header.ElementLength.Should().Be(68);
        header.FirstDimension.Should().Be(7);
        header.SecondDimension.Should().Be(11);
        header.ThirdDimension.Should().Be(13);
        header.NameLength.Should().Be((ushort)"Tank.Level".Length);
    }

    [Fact]
    public void ReadFrom_AnEntryAtAnUnalignedOffset_MapsEveryFieldTheSameWay()
    {
        // Arrange
        // Entries start wherever the previous name ended, so a header's four-byte fields land on
        // arbitrary addresses. A one-character name puts this second header on an odd offset.
        var listing = TagsDataBuilder.Build(new TagsDataBuilder.TagEntry("A", 0x00C4), Distinct);
        var secondEntry = listing.AsSpan(TagsEntryHeader.Size + 1);

        // Act
        var header = TagsEntryHeader.ReadFrom(secondEntry);

        // Assert
        header.InstanceId.Should().Be(0x0A0B0C0D);
        header.SymbolType.Should().Be(0x8123);
        header.ElementLength.Should().Be(68);
        header.FirstDimension.Should().Be(7);
        header.SecondDimension.Should().Be(11);
        header.ThirdDimension.Should().Be(13);
        header.NameLength.Should().Be((ushort)"Tank.Level".Length);
    }

    [Fact]
    public void ReadFrom_AnEntryLongerThanItsHeader_IgnoresTheNameThatFollows()
    {
        // Arrange
        var headerOnly = TagsDataBuilder.Build(Distinct).AsSpan(0, TagsEntryHeader.Size);
        var withName = TagsDataBuilder.Build(Distinct);

        // Act
        var truncated = TagsEntryHeader.ReadFrom(headerOnly);
        var full = TagsEntryHeader.ReadFrom(withName);

        // Assert
        // The read is bounded by the struct, not by the span, so the name never bleeds into a field.
        truncated.Should().BeEquivalentTo(full);
    }
}
