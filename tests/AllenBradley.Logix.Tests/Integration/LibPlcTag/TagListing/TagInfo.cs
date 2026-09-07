namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.LibPlcTag.TagListing;

public class TagInfo
{
    public uint Id { get; set; }
    public ushort Type { get; set; }
    public required string Name { get; set; }
    public ushort Length { get; set; }
    public required uint[] Dimensions { get; set; }
}
