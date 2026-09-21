using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.SymbolTypes;
using System.Text;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.TagsListing;

/// <summary>
/// Decodes the raw bytes of an <c>@tags</c> (or <c>Program:&lt;name&gt;.@tags</c>) read into one
/// <see cref="ListedTag"/> per tag. The listing names a structure only by its template id, so it
/// cannot tell a <c>STRING</c> from a <c>TIMER</c>: every structure leaves here as a
/// <see cref="AllenBradleyDataType.Structure"/> naming the template its decoded layout is filed
/// under, and the lookup says which of them are strings once the templates have been read.
/// </summary>
internal static class TagsDecoder
{
    public static IReadOnlyList<ListedTag> Decode(ReadOnlySpan<byte> tags)
    {
        var listedTags = new List<ListedTag>();
        var offset = 0;

        while (offset + TagsEntryHeader.Size <= tags.Length)
        {
            var entry = tags[offset..];
            var header = TagsEntryHeader.ReadFrom(entry);
            var tagName = GetTagName(header, entry);

            listedTags.Add(ToListedTag(header, tagName));
            offset += TagsEntryHeader.Size + tagName.Length;
        }

        return listedTags;
    }

    // A truncated final entry is clamped to what is left rather than allowed to overrun; a genuine
    // listing never trips this.
    private static ReadOnlySpan<byte> GetTagName(in TagsEntryHeader header, ReadOnlySpan<byte> entry)
    {
        var behindHeader = entry[TagsEntryHeader.Size..];

        return behindHeader[..Math.Min(header.NameLength, behindHeader.Length)];
    }

    private static ListedTag ToListedTag(in TagsEntryHeader header, ReadOnlySpan<byte> tagAddress) =>
        new(
            new TagAddress(Encoding.ASCII.GetString(tagAddress)),
            TagDefinition.Of(header.SymbolType, GetDeclaredCount(header)));

    // Dimensions past the declared rank hold whatever the controller left there, so only the ones the
    // rank owns are multiplied — leaving a scalar at the empty product of one.
    private static uint GetDeclaredCount(in TagsEntryHeader header)
    {
        ReadOnlySpan<uint> dimensions = [header.FirstDimension, header.SecondDimension, header.ThirdDimension];

        var declaredCount = 1u;
        foreach (var dimension in dimensions[..header.SymbolType.DimensionCount.Value])
        {
            declaredCount *= dimension;
        }

        return declaredCount;
    }
}
