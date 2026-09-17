using System.Text;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;

/// <summary>
/// Decodes the raw bytes of an <c>@tags</c> (or <c>Program:&lt;name&gt;.@tags</c>) read into one
/// <see cref="TagDefinition"/> per tag. The listing names a structure only by its template id, so it
/// cannot tell a <c>STRING</c> from a <c>TIMER</c>: every structure's element length leaves here as a
/// <see cref="StringMaxLength"/>, and a 12-byte <c>TIMER</c> decodes as a string of capacity 8. The
/// template id it does carry is the key the <see cref="TemplateDecoder"/>'s output is filed under.
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

    // A truncated final entry is clamped to what is left rather than allowed to overrun; a genuine
    // listing never trips this.
    private static ReadOnlySpan<byte> GetTagName(in TagsEntryHeader header, ReadOnlySpan<byte> entry)
    {
        var behindHeader = entry[TagsEntryHeader.Size..];

        return behindHeader[..Math.Min(header.NameLength, behindHeader.Length)];
    }

    private static TagDefinition ToTagDefinition(in TagsEntryHeader header, ReadOnlySpan<byte> tagAddress)
    {
        var isStruct = SymbolType.IsStruct(header.SymbolType);
        var dimensionCount = SymbolType.DimensionCount(header.SymbolType);

        return new TagDefinition(
            TagAddress: new TagAddress(Encoding.ASCII.GetString(tagAddress)),
            DataType: isStruct ? AllenBradleyDataType.String : SymbolType.AtomicType(header.SymbolType),
            TemplateId: SymbolType.TemplateId(header.SymbolType),
            MaxLength: isStruct ? StringMaxLength.OfStructure(header.ElementLength) : null,
            DimensionCount: new DimensionCount(dimensionCount),
            ElementCount: SymbolType.ToElementCount(header.SymbolType, GetDeclaredCount(header, dimensionCount)));
    }

    // Dimensions past the declared rank hold whatever the controller left there, so only the ones the
    // rank owns are multiplied — leaving a scalar at the empty product of one.
    private static uint GetDeclaredCount(in TagsEntryHeader header, int dimensionCount)
    {
        ReadOnlySpan<uint> dimensions = [header.FirstDimension, header.SecondDimension, header.ThirdDimension];

        var declaredCount = 1u;
        foreach (var dimension in dimensions[..dimensionCount])
        {
            declaredCount *= dimension;
        }

        return declaredCount;
    }
}
