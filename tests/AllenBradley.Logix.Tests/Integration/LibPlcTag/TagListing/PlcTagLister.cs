using libplctag;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.LibPlcTag.TagListing;

public class PlcTagLister(string connectionEndpoint, string cipRoutePath, TimeSpan timeout)
{
    private const ushort TypeIsStruct = 0x8000;
    private const ushort TypeIsSystem = 0x1000;
    private const ushort TypeUdtIdMask = 0x0FFF;

    public PlcTagListing List()
    {
        var controllerTags = ReadControllerTags();
        var programTags = ReadProgramTags(controllerTags);
        var udts = ReadUdts(controllerTags);

        return new PlcTagListing
        {
            ControllerTags = controllerTags,
            ProgramTags = programTags,
            Udts = udts
        };
    }

    // Every Tag must be disposed. It owns a handle in libplctag's native layer, and letting the
    // finalizer release it means native code runs after the CLR has torn down, which fail-fasts
    // the process on exit (exit code 0xC0000602) even when every test has passed.
    private TagInfo[] ReadControllerTags()
    {
        using var tag = new Tag<TagInfoPlcMapper, TagInfo[]>();
        tag.Gateway = connectionEndpoint;
        tag.Path = cipRoutePath;
        tag.PlcType = PlcType.ControlLogix;
        tag.Protocol = Protocol.ab_eip;
        tag.Name = "@tags";
        tag.Timeout = timeout;

        tag.Read();
        return tag.Value;
    }

    private IReadOnlyDictionary<string, TagInfo[]> ReadProgramTags(TagInfo[] controllerTags)
    {
        var tagsByProgram = new Dictionary<string, TagInfo[]>();

        foreach (var tag in controllerTags.Where(t => t.Name.StartsWith("Program:")))
        {
            using var programTag = new Tag<TagInfoPlcMapper, TagInfo[]>();
            programTag.Gateway = connectionEndpoint;
            programTag.Path = cipRoutePath;
            programTag.PlcType = PlcType.ControlLogix;
            programTag.Protocol = Protocol.ab_eip;
            programTag.Name = $"{tag.Name}.@tags";
            programTag.Timeout = timeout;

            programTag.Read();
            tagsByProgram[tag.Name] = programTag.Value;
        }

        return tagsByProgram;
    }

    private UdtInfo[] ReadUdts(TagInfo[] controllerTags)
    {
        var udtIds = controllerTags
            .Where(t => (t.Type & TypeIsStruct) != 0 && (t.Type & TypeIsSystem) == 0)
            .Select(t => t.Type & TypeUdtIdMask)
            .Distinct();

        var udts = new List<UdtInfo>();

        foreach (var udtId in udtIds)
        {
            using var udtTag = new Tag<UdtInfoPlcMapper, UdtInfo>();
            udtTag.Gateway = connectionEndpoint;
            udtTag.Path = cipRoutePath;
            udtTag.PlcType = PlcType.ControlLogix;
            udtTag.Protocol = Protocol.ab_eip;
            udtTag.Name = $"@udt/{udtId}";
            udtTag.Timeout = timeout;

            udtTag.Read();
            udts.Add(udtTag.Value);
        }

        return udts.ToArray();
    }
}
