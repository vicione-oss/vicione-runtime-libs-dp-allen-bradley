namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.LibPlcTag.TagListing;

public class PlcTagListing
{
    public required TagInfo[] ControllerTags { get; init; }
    public required IReadOnlyDictionary<string, TagInfo[]> ProgramTags { get; init; }
    public required UdtInfo[] Udts { get; init; }
}
