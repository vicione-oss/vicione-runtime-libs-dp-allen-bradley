using System.Text;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;

/// <summary>
/// Decodes the raw bytes of an <c>@tags</c> (or <c>Program:&lt;name&gt;.@tags</c>) read into one
/// <see cref="TagDefinition"/> per tag. It works off a <see cref="ReadOnlySpan{T}"/> rather than the
/// sealed <c>Tag</c>'s getters, so it is testable against captured buffers and independent of the
/// removed typed-mapper API.
/// The listing names only a structure's template id, so it cannot tell a <c>STRING</c> from a <c>TIMER</c>:
/// every structure's element length leaves here as a <see cref="StringMaxLength"/>, and a 12-byte
/// <c>TIMER</c> decodes as a string of capacity 8 until templates are read.
/// </summary>
internal static class TagsDecoder
{
    public static IReadOnlyList<TagDefinition> Decode(ReadOnlySpan<byte> tags)
    {
        var definitions = new List<TagDefinition>();
        var offset = 0;

        while (offset + TagsEntryHeader.Size <= tags.Length)
        {
            var entry = tags[offset..];
            var header = TagsEntryHeader.ReadFrom(entry);
            var tagName = GetTagName(header, entry);

            definitions.Add(ToTagDefinition(header, tagName));
            offset += TagsEntryHeader.Size + tagName.Length;
        }

        return definitions;
    }

    // The name sits directly behind the header and runs for as long as the header declares. A
    // truncated final entry is clamped to what is left rather than allowed to overrun; a genuine
    // listing never trips this.
    private static ReadOnlySpan<byte> GetTagName(in TagsEntryHeader header, ReadOnlySpan<byte> entry)
    {
        var behindHeader = entry[TagsEntryHeader.Size..];

        return behindHeader[..Math.Min(header.NameLength, behindHeader.Length)];
    }

    private static TagDefinition ToTagDefinition(in TagsEntryHeader header, ReadOnlySpan<byte> tagName)
    {
        var isStruct = SymbolType.IsStruct(header.SymbolType);
        var dimensionCount = SymbolType.DimensionCount(header.SymbolType);

        return new TagDefinition(
            TagName: new TagName(Encoding.ASCII.GetString(tagName)),
            Kind: isStruct ? LogixTypeKind.Structure : LogixTypeKind.Atomic,
            DataType: isStruct ? AllenBradleyDataType.String : SymbolType.AtomicType(header.SymbolType),
            MaxLength: isStruct ? StringMaxLength.OfStructure(header.ElementLength) : null,
            DimensionCount: new DimensionCount(dimensionCount),
            ElementCount: GetElementCount(header, dimensionCount));
    }

    // An array holds the product of its dimensions, and a scalar — rank zero — the empty product of
    // one. Dimensions past the declared rank hold whatever the controller left there, so each rank
    // multiplies only the ones it owns.
    private static ElementCount GetElementCount(in TagsEntryHeader header, int dimensionCount)
    {
        ReadOnlySpan<uint> dimensions = [header.FirstDimension, header.SecondDimension, header.ThirdDimension];

        var elementCount = 1u;
        foreach (var dimension in dimensions[..dimensionCount])
        {
            elementCount *= dimension;
        }

        return new ElementCount((int)elementCount);
    }
}
