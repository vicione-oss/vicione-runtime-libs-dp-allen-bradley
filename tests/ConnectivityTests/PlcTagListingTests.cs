using ConnectivityTests.TagListing;
using Xunit.Abstractions;

namespace ConnectivityTests;

[Trait("Category", "E2E")]
public class PlcTagListingTests
{
    // ── Connection configuration ──────────────────────────────────────────────
    private static readonly string Gateway = Environment.GetEnvironmentVariable("CIP_GATEWAY") ?? "192.168.0.100";
    private static readonly string Path    = Environment.GetEnvironmentVariable("CIP_PATH")    ?? "1,0";

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);
    // ─────────────────────────────────────────────────────────────────────────

    private readonly ITestOutputHelper _output;

    public PlcTagListingTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ListAllTags_OutputsControllerTagsProgramTagsAndUdts()
    {
        // Arrange
        var lister = new PlcTagLister(Gateway, Path, Timeout);

        // Act
        var listing = lister.List();

        // Assert
        _output.WriteLine("Controller Tags");
        _output.WriteLine("===============");
        foreach (var tag in listing.ControllerTags)
            _output.WriteLine($"Id={tag.Id}  Name={tag.Name}  Type=0x{tag.Type:X4}  Length={tag.Length}");

        _output.WriteLine(string.Empty);
        _output.WriteLine("Programs");
        _output.WriteLine("========");
        foreach (var (programName, programTags) in listing.ProgramTags)
        {
            _output.WriteLine(programName);
            foreach (var tag in programTags)
                _output.WriteLine($"    {tag.Name}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("UDTs");
        _output.WriteLine("====");
        foreach (var udt in listing.Udts)
        {
            _output.WriteLine($"Id={udt.Id}  Name={udt.Name}  NumFields={udt.NumFields}  Size={udt.Size}");
            foreach (var field in udt.Fields)
                _output.WriteLine($"    Name={field.Name}  Offset={field.Offset}  Metadata={field.Metadata}  Type=0x{field.Type:X4}");
        }

        Assert.NotEmpty(listing.ControllerTags);
    }
}
