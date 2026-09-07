namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.LibPlcTag.TagListing;

public class UdtFieldInfo
{
    public required string Name { get; set; }
    public ushort Type { get; set; }
    public ushort Metadata { get; set; }
    public uint Offset { get; set; }
}
