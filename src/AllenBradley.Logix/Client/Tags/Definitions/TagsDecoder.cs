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
/// </summary>
/// <remarks>
/// <para>
/// An entry is a <see cref="TagsEntryHeader"/> followed by the tag's name, and entries are packed
/// back to back, so the next one starts at the header plus the name this one declared. The symbol-type
/// bitfield the header carries is documented in
/// <c>docs/AllenBradley.Documentation/cip-protocol/symbolic-tag-data-types.md</c>, section "The Logix
/// symbol-type bitfield".
/// </para>
/// <para>
/// The header's element length is a wire fact and stops here, the way the CIP type codes do: it leaves
/// the decoder as a <see cref="StringMaxLength"/> and never as a byte count. Turning it into one costs
/// the assumption that a structure <b>is</b> a string. The listing names only the id of the template
/// that defines a structure's members, so it cannot tell a <c>STRING_8</c> from a <c>TIMER</c>, and a
/// 12-byte <c>TIMER</c> decodes here as a string of capacity 8. That is the accuracy the comparison
/// against a declared size already had — telling them apart needs the template itself
/// (<c>@udt/&lt;id&gt;</c>), which arrives with structured data-point support.
/// </para>
/// </remarks>
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
