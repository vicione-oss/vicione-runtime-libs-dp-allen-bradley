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
        // 0x00C4 = DINT, no struct bit, no dimensions.
        var listing = TagsDataBuilder.Build(new TagsDataBuilder.Entry("Motor.Speed", 0x00C4));

        var decoded = TagsDecoder.Decode(listing);

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
        var listing = TagsDataBuilder.Build(
            new TagsDataBuilder.Entry("A", 0x00C4),
            new TagsDataBuilder.Entry("Tank.Level", 0x00CA),
            new TagsDataBuilder.Entry("Program:Main", 0x1000));

        var decoded = TagsDecoder.Decode(listing);

        decoded.Select(t => t.TagName.Value).Should().Equal("A", "Tank.Level", "Program:Main");
        decoded[1].DataType.Should().Be(AllenBradleyDataType.Real);
    }

    [Fact]
    public void Decode_AStructureTag_HasNoAtomicType()
    {
        // Struct bit set (0x8000) with a UDT id in the low bits.
        var listing = TagsDataBuilder.Build(new TagsDataBuilder.Entry("Motor", 0x8123));

        var tag = TagsDecoder.Decode(listing).Single();

        tag.Kind.Should().Be(LogixTypeKind.Structure);
        tag.DataType.Should().BeNull();
    }

    [Fact]
    public void Decode_AOneDimensionalArray_ReportsRankAndElementCount()
    {
        // Dimension count 1 lives in bits 14-13: 1 << 13 = 0x2000, atomic DINT.
        var listing = TagsDataBuilder.Build(
            new TagsDataBuilder.Entry("Counts", 0x2000 | 0x00C4) { Dimension0 = 10 });

        var tag = TagsDecoder.Decode(listing).Single();

        tag.DimensionCount.Should().Be(new DimensionCount(1));
        tag.ElementCount.Should().Be(new ElementCount(10));
        tag.DataType.Should().Be(AllenBradleyDataType.Dint);
    }

    [Fact]
    public void Decode_AnEmptyListing_ReturnsNothing()
    {
        TagsDecoder.Decode([]).Should().BeEmpty();
    }
}
