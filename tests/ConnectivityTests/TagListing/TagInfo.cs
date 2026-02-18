namespace ConnectivityTests.TagListing;

public class TagInfo
{
    public uint Id { get; set; }
    public ushort Type { get; set; }
    public string Name { get; set; }
    public ushort Length { get; set; }
    public uint[] Dimensions { get; set; }
}
