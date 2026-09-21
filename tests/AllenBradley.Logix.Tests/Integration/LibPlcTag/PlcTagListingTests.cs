namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.LibPlcTag;

/// <summary>
/// Prints the controller's tag namespace as libplctag reports it, for reading rather than for
/// asserting: what it pins is only that the controller answered the browse at all.
/// </summary>
public sealed class PlcTagListingTests(ITestOutputHelper output) : LibPlcTagIntegrationTestBase
{
    [Fact]
    public void TheControllerAnswersTheBrowseWithItsTagsProgramsAndUdts()
    {
        // Arrange

        // Act
        var listing = Lister.List();

        // Assert
        output.WriteLine("Controller Tags");
        output.WriteLine("===============");
        foreach (var tag in listing.ControllerTags)
        {
            output.WriteLine($"Id={tag.Id}  Name={tag.Name}  Type=0x{tag.Type:X4}  Length={tag.Length}");
        }

        output.WriteLine(string.Empty);
        output.WriteLine("Programs");
        output.WriteLine("========");
        foreach (var (programName, programTags) in listing.ProgramTags)
        {
            output.WriteLine(programName);
            foreach (var tag in programTags)
            {
                output.WriteLine($"    {tag.Name}");
            }
        }

        output.WriteLine(string.Empty);
        output.WriteLine("UDTs");
        output.WriteLine("====");
        foreach (var udt in listing.Udts)
        {
            output.WriteLine($"Id={udt.Id}  Name={udt.Name}  NumFields={udt.NumFields}  Size={udt.Size}");
            foreach (var field in udt.Fields)
            {
                output.WriteLine(
                    $"    Name={field.Name}  Offset={field.Offset}  DeclaredType={field.Metadata}  " +
                    $"Type=0x{field.Type:X4}");
            }
        }

        listing.ControllerTags.Should().NotBeEmpty();
    }
}
