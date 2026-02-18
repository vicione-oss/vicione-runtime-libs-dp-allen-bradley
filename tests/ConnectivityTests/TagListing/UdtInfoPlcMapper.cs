using libplctag;
using libplctag.DataTypes;

namespace ConnectivityTests.TagListing;

public class UdtInfoPlcMapper : IPlcMapper<UdtInfo>
{
    public PlcType PlcType { get; set; }
    public int? ElementSize => null;
    public int[] ArrayDimensions { get => null; set => throw new NotImplementedException("This plcMapper can only be used to read"); }

    public UdtInfo Decode(Tag tag)
    {
        var templateId = tag.GetUInt16(0);
        var memberDescSize = tag.GetUInt32(2);
        var udtInstanceSize = tag.GetUInt32(6);
        var numMembers = tag.GetUInt16(10);
        var structHandle = tag.GetUInt16(12);

        var udtInfo = new UdtInfo()
        {
            Fields = new UdtFieldInfo[numMembers],
            NumFields = numMembers,
            Handle = structHandle,
            Id = templateId,
            Size = udtInstanceSize
        };

        var offset = 14;

        for (int fieldIndex = 0; fieldIndex < numMembers; fieldIndex++)
        {
            var fieldMetadata = tag.GetUInt16(offset);
            offset += 2;

            var fieldElementType = tag.GetUInt16(offset);
            offset += 2;

            var fieldOffset = tag.GetUInt16(offset);
            offset += 4;

            udtInfo.Fields[fieldIndex] = new UdtFieldInfo()
            {
                Offset = fieldOffset,
                Metadata = fieldMetadata,
                Type = fieldElementType,
            };
        }

        var nameStr = tag.GetString(offset).Split(';')[0];
        udtInfo.Name = nameStr;

        offset += tag.GetStringTotalLength(offset);

        for (int fieldIndex = 0; fieldIndex < numMembers; fieldIndex++)
        {
            udtInfo.Fields[fieldIndex].Name = tag.GetString(offset);
            offset += tag.GetStringTotalLength(offset);
        }

        return udtInfo;
    }

    public void Encode(Tag tag, UdtInfo value)
    {
        throw new NotImplementedException("This plcMapper can only be used to read");
    }

    public int? GetElementCount() => null;
    
}
