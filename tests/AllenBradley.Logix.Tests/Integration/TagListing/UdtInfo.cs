namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.TagListing;

public class UdtInfo
{
    public uint Size { get; set; }
    public required string Name { get; set; }
    public ushort Id { get; set; }
    public ushort NumFields { get; set; }
    public ushort Handle { get; set; }
    public required UdtFieldInfo[] Fields { get; set; }
}
