using libplctag;
using libplctag.DataTypes;

namespace ConnectivityTests.TagListing;

public class PlcTagLister(string gateway, string path, TimeSpan timeout)
{
    private const ushort TypeIsStruct   = 0x8000;
    private const ushort TypeIsSystem   = 0x1000;
    private const ushort TypeUdtIdMask = 0x0FFF;

    public PlcTagListing List()
    {
        var controllerTags = ReadControllerTags();
        var programTags    = ReadProgramTags(controllerTags);
        var udts           = ReadUdts(controllerTags);

        return new PlcTagListing
        {
            ControllerTags = controllerTags,
            ProgramTags    = programTags,
            Udts           = udts
        };
    }

    private TagInfo[] ReadControllerTags()
    {
        var tag = new Tag<TagInfoPlcMapper, TagInfo[]>
        {
            Gateway  = gateway,
            Path     = path,
            PlcType  = PlcType.ControlLogix,
            Protocol = Protocol.ab_eip,
            Name     = "@tags",
            Timeout  = timeout
        };

        tag.Read();
        return tag.Value;
    }

    private IReadOnlyDictionary<string, TagInfo[]> ReadProgramTags(TagInfo[] controllerTags)
    {
        var result = new Dictionary<string, TagInfo[]>();

        foreach (var tag in controllerTags.Where(t => t.Name.StartsWith("Program:")))
        {
            var programTag = new Tag<TagInfoPlcMapper, TagInfo[]>
            {
                Gateway  = gateway,
                Path     = path,
                PlcType  = PlcType.ControlLogix,
                Protocol = Protocol.ab_eip,
                Name     = $"{tag.Name}.@tags",
                Timeout  = timeout
            };

            programTag.Read();
            result[tag.Name] = programTag.Value;
        }

        return result;
    }

    private UdtInfo[] ReadUdts(TagInfo[] controllerTags)
    {
        var udtIds = controllerTags
            .Where(t => (t.Type & TypeIsStruct) != 0 && (t.Type & TypeIsSystem) == 0)
            .Select(t => t.Type & TypeUdtIdMask)
            .Distinct();

        var result = new List<UdtInfo>();

        foreach (var udtId in udtIds)
        {
            var udtTag = new Tag<UdtInfoPlcMapper, UdtInfo>
            {
                Gateway  = gateway,
                Path     = path,
                PlcType  = PlcType.ControlLogix,
                Protocol = Protocol.ab_eip,
                Name     = $"@udt/{udtId}",
                Timeout  = timeout
            };

            udtTag.Read();
            result.Add(udtTag.Value);
        }

        return result.ToArray();
    }
}
