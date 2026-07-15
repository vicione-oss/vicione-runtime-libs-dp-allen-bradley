using System.Text;
using libplctag;
using libplctag.DataTypes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.TagListing;

public class TagInfoPlcMapper : IPlcMapper<TagInfo[]>
{
    private const int TagStringSize = 200;

    public PlcType PlcType { get; set; }

    public int? ElementSize => null;

    // libplctag's interface is nullable-oblivious and treats null as "not an array".
    public int[] ArrayDimensions { get => null!; set => throw new NotImplementedException("This plcMapper can only be used to read Tag Information"); }

    private static TagInfo Decode(Tag tag, int offset, out int elementSize)
    {
        var tagInstanceId = tag.GetUInt32(offset);
        var tagType = tag.GetUInt16(offset + 4);
        var tagLength = tag.GetUInt16(offset + 6);
        var tagArrayDims = new uint[]
        {
            tag.GetUInt32(offset + 8),
            tag.GetUInt32(offset + 12),
            tag.GetUInt32(offset + 16)
        };

        var apparentTagNameLength = (int)tag.GetUInt16(offset + 20);
        var actualTagNameLength = Math.Min(apparentTagNameLength, TagStringSize * 2 - 1);

        var tagNameBytes = Enumerable.Range(offset + 22, actualTagNameLength)
            .Select(tag.GetUInt8)
            .Select(Convert.ToByte)
            .ToArray();

        var tagName = Encoding.ASCII.GetString(tagNameBytes);

        elementSize = 22 + actualTagNameLength;

        return new TagInfo()
        {
            Id = tagInstanceId,
            Type = tagType,
            Name = tagName,
            Length = tagLength,
            Dimensions = tagArrayDims
        };
    }

    public TagInfo[] Decode(Tag tag)
    {
        var buffer = new List<TagInfo>();

        var tagSize = tag.GetSize();

        int offset = 0;
        while (offset < tagSize)
        {
            buffer.Add(Decode(tag, offset, out int elementSize));
            offset += elementSize;
        }

        return buffer.ToArray();
    }

    public void Encode(Tag tag, TagInfo[] value)
    {
        throw new NotImplementedException("This plcMapper can only be used to read Tag Information");
    }

    public int? GetElementCount() => null;
}
