using libplctag;
using libplctag.DataTypes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.TagListing;

public class UdtInfoPlcMapper : IPlcMapper<UdtInfo>
{
    public PlcType PlcType { get; set; }
    public int? ElementSize => null;

    // libplctag's interface is nullable-oblivious and treats null as "not an array".
    public int[] ArrayDimensions { get => null!; set => throw new NotImplementedException("This plcMapper can only be used to read"); }

    public UdtInfo Decode(Tag tag)
    {
        var templateId = tag.GetUInt16(0);
        var memberDescSize = tag.GetUInt32(2);
        var udtInstanceSize = tag.GetUInt32(6);
        var numMembers = tag.GetUInt16(10);
        var structHandle = tag.GetUInt16(12);

        var offset = 14;

        // The UDT lays out every field's descriptor first, then the UDT name, then the field
        // names — so the descriptors have to be parked until the names have been walked.
        var descriptors = new (ushort Metadata, ushort Type, uint Offset)[numMembers];

        for (int fieldIndex = 0; fieldIndex < numMembers; fieldIndex++)
        {
            var fieldMetadata = tag.GetUInt16(offset);
            offset += 2;

            var fieldElementType = tag.GetUInt16(offset);
            offset += 2;

            var fieldOffset = tag.GetUInt16(offset);
            offset += 4;

            descriptors[fieldIndex] = (fieldMetadata, fieldElementType, fieldOffset);
        }

        var udtName = tag.GetString(offset).Split(';')[0];
        offset += tag.GetStringTotalLength(offset);

        var fields = new UdtFieldInfo[numMembers];

        for (int fieldIndex = 0; fieldIndex < numMembers; fieldIndex++)
        {
            fields[fieldIndex] = new UdtFieldInfo
            {
                Name = tag.GetString(offset),
                Offset = descriptors[fieldIndex].Offset,
                Metadata = descriptors[fieldIndex].Metadata,
                Type = descriptors[fieldIndex].Type,
            };

            offset += tag.GetStringTotalLength(offset);
        }

        return new UdtInfo
        {
            Id = templateId,
            Size = udtInstanceSize,
            NumFields = numMembers,
            Handle = structHandle,
            Name = udtName,
            Fields = fields,
        };
    }

    public void Encode(Tag tag, UdtInfo value)
    {
        throw new NotImplementedException("This plcMapper can only be used to read");
    }

    public int? GetElementCount() => null;
}
