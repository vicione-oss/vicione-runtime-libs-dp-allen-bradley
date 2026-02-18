namespace ConnectivityTests.TagListing;

public class PlcTagListing
{
    public TagInfo[] ControllerTags { get; init; }
    public IReadOnlyDictionary<string, TagInfo[]> ProgramTags { get; init; }
    public UdtInfo[] Udts { get; init; }
}
