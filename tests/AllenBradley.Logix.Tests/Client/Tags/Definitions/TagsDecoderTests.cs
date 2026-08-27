using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Definitions;

/// <summary>
/// The <c>@tags</c> byte layout, decoded off synthetic buffers — no device, no native library. The
/// buffers stand in for the real controller listing a device-tier test confirms.
/// </summary>
public class TagsDecoderTests
{
    [Fact]
    public void Decode_AScalarAtomicTag_ReadsNameTypeAndScalarShape()
    {
        // Arrange
        // 0x00C4 = DINT, no struct bit, no dimensions.
        var listing = TagsDataBuilder.Build(new TagsDataBuilder.TagEntry("Motor.Speed", 0x00C4));

        // Act
        var decoded = TagsDecoder.Decode(listing);

        // Assert
        decoded.Should().ContainSingle();
        var tag = decoded[0];
        tag.TagName.Should().Be(new TagName("Motor.Speed"));
        tag.Kind.Should().Be(LogixTypeKind.Atomic);
        tag.DataType.Should().Be(AllenBradleyDataType.Dint);
        tag.DimensionCount.Should().Be(new DimensionCount(0));
        tag.ElementCount.Should().Be(new ElementCount(1));
    }

    [Fact]
    public void Decode_MultipleVariableLengthEntries_WalksEveryEntry()
    {
        // Arrange
        var listing = TagsDataBuilder.Build(
            new TagsDataBuilder.TagEntry("A", 0x00C4),
            new TagsDataBuilder.TagEntry("Tank.Level", 0x00CA),
            new TagsDataBuilder.TagEntry("Program:Main", 0x1000));

        // Act
        var decoded = TagsDecoder.Decode(listing);

        // Assert
        decoded.Select(t => t.TagName.Value).Should().Equal("A", "Tank.Level", "Program:Main");
        decoded[1].DataType.Should().Be(AllenBradleyDataType.Real);
    }

    [Fact]
    public void Decode_AStringTag_ReadsItsCapacityFromTheElementLength()
    {
        // Arrange
        // Struct bit set (0x8000) with a template id in the low bits, and the 86 member bytes the
        // listing reports for a built-in STRING: .LEN (4) + .DATA[82].
        var listing = TagsDataBuilder.Build(
            new TagsDataBuilder.TagEntry("Line.Label", 0x8123) { ElementLength = 86 });

        // Act
        var tag = TagsDecoder.Decode(listing).Single();

        // Assert
        // The element length is spent here and leaves as a capacity: nothing above the decoder is told
        // a byte count.
        tag.Kind.Should().Be(LogixTypeKind.Structure);
        tag.DataType.Should().Be(AllenBradleyDataType.String);
        tag.MaxLength.Should().Be(StringMaxLength.Standard);
    }

    [Fact]
    public void Decode_ACustomStringType_ReportsItsOwnCapacity()
    {
        // Arrange
        // A STRING_20 is the same .LEN + .DATA[n] shape with a different n, and the listing describes
        // it the same way — which is what makes the capacity worth verifying at all.
        var listing = TagsDataBuilder.Build(
            new TagsDataBuilder.TagEntry("Line.Code", 0x8124) { ElementLength = 24 });

        // Act
        var tag = TagsDecoder.Decode(listing).Single();

        // Assert
        tag.DataType.Should().Be(AllenBradleyDataType.String);
        tag.MaxLength.Should().Be(new StringMaxLength(20));
    }

    [Fact]
    public void Decode_AnAtomicTag_HasNoCapacity()
    {
        // Arrange
        // Capacity belongs to the one type whose size the type does not fix; a DINT has none to report.
        var listing = TagsDataBuilder.Build(new TagsDataBuilder.TagEntry("Motor.Speed", 0x00C4));

        // Act
        var tag = TagsDecoder.Decode(listing).Single();

        // Assert
        tag.MaxLength.Should().BeNull();
    }

    [Fact]
    public void Decode_AOneDimensionalArray_ReportsRankAndElementCount()
    {
        // Arrange
        // Dimension count 1 lives in bits 14-13: 1 << 13 = 0x2000, atomic DINT.
        var listing = TagsDataBuilder.Build(
            new TagsDataBuilder.TagEntry("Counts", 0x2000 | 0x00C4) { Dimension0 = 10 });

        // Act
        var tag = TagsDecoder.Decode(listing).Single();

        // Assert
        tag.DimensionCount.Should().Be(new DimensionCount(1));
        tag.ElementCount.Should().Be(new ElementCount(10));
        tag.DataType.Should().Be(AllenBradleyDataType.Dint);
    }

    [Fact]
    public void Decode_AnUnmodelledTypeCode_ReportsUnknownRatherThanTheRawCode()
    {
        // Arrange
        // 0x00DE is outside the elementary range (0xC1-0xCB) the addon knows how to decode.
        var listing = TagsDataBuilder.Build(new TagsDataBuilder.TagEntry("Exotic", 0x00DE));

        // Act
        var tag = TagsDecoder.Decode(listing).Single();

        // Assert
        // Still atomic — the structure bit is clear — but nothing above the decoder can act on it, and
        // the wire code stops here rather than travelling into the model as a number.
        tag.Kind.Should().Be(LogixTypeKind.Atomic);
        tag.DataType.Should().Be(AllenBradleyDataType.Unknown);
    }

    [Fact]
    public void Decode_AnEmptyListing_ReturnsNothing()
    {
        // Arrange

        // Act
        var decoded = TagsDecoder.Decode([]);

        // Assert
        decoded.Should().BeEmpty();
    }
}
