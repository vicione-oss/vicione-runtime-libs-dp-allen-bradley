namespace ConnectivityTests.TagListing;

public class UdtInfo
{
    public uint Size { get; set; }
    public string Name { get; set; }
    public ushort Id { get; set; }
    public ushort NumFields { get; set; }
    public ushort Handle { get; set; }
    public UdtFieldInfo[] Fields { get; set; }
}
