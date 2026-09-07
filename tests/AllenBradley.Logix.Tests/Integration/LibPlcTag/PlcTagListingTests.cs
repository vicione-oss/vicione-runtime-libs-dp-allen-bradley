using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.LibPlcTag.TagListing;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.LibPlcTag;

[Trait("Category", "Integration")]
public class PlcTagListingTests(ITestOutputHelper output)
{
    // ── Connection configuration ──────────────────────────────────────────────
    private static readonly string ConnectionEndpoint = Environment.GetEnvironmentVariable("CIP_GATEWAY") ?? "192.168.0.100";
    private static readonly string CipRoutePath = Environment.GetEnvironmentVariable("CIP_PATH") ?? "1,0";

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ListAllTags_OutputsControllerTagsProgramTagsAndUdts()
    {
        // Arrange
        var lister = new PlcTagLister(ConnectionEndpoint, CipRoutePath, Timeout);

        // Act
        var listing = lister.List();

        // Assert
        output.WriteLine("Controller Tags");
        output.WriteLine("===============");
        foreach (var tag in listing.ControllerTags)
            output.WriteLine($"Id={tag.Id}  Name={tag.Name}  Type=0x{tag.Type:X4}  Length={tag.Length}");

        output.WriteLine(string.Empty);
        output.WriteLine("Programs");
        output.WriteLine("========");
        foreach (var (programName, programTags) in listing.ProgramTags)
        {
            output.WriteLine(programName);
            foreach (var tag in programTags)
                output.WriteLine($"    {tag.Name}");
        }

        output.WriteLine(string.Empty);
        output.WriteLine("UDTs");
        output.WriteLine("====");
        foreach (var udt in listing.Udts)
        {
            output.WriteLine($"Id={udt.Id}  Name={udt.Name}  NumFields={udt.NumFields}  Size={udt.Size}");
            foreach (var field in udt.Fields)
                output.WriteLine($"    Name={field.Name}  Offset={field.Offset}  Metadata={field.Metadata}  Type=0x{field.Type:X4}");
        }

        Assert.NotEmpty(listing.ControllerTags);
    }
}
