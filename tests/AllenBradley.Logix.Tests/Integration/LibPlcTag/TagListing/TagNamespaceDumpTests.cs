using System.Text;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.LibPlcTag.TagListing;

/// <summary>
/// Dumps the controller's tag namespace with symbol types decoded and writes it to the file named by
/// <c>CIP_DUMP_PATH</c>. It is ground truth for the node-tree and addressing design rather than a test:
/// what it pins is only that the controller answered the browse.
/// </summary>
public sealed class TagNamespaceDumpTests(ITestOutputHelper output) : LibPlcTagIntegrationTestBase
{
    private static readonly string DumpPath =
        Environment.GetEnvironmentVariable("CIP_DUMP_PATH") ?? "tag-namespace-dump.txt";

    [Fact]
    public void TheDecodedTagNamespaceIsWrittenToTheDumpFile()
    {
        // Arrange

        // Act
        var listing = Lister.List();

        // Assert
        var report = Render(listing);
        File.WriteAllText(DumpPath, report);
        output.WriteLine(report);
        output.WriteLine($"Dump written to {Path.GetFullPath(DumpPath)}");

        listing.ControllerTags.Should().NotBeEmpty();
    }

    /// <summary>Every atomic scalar tag in the listing, with its fully qualified name.</summary>
    internal static IEnumerable<(string FullName, ushort Type)> AtomicScalars(PlcTagListing listing)
    {
        foreach (var tag in listing.ControllerTags.Where(t => SymbolType.IsAtomicScalar(t.Type)))
        {
            yield return (tag.Name, tag.Type);
        }

        foreach (var (programName, programTags) in listing.ProgramTags)
        {
            foreach (var tag in programTags.Where(t => SymbolType.IsAtomicScalar(t.Type)))
            {
                yield return ($"{programName}.{tag.Name}", tag.Type);
            }
        }
    }

    private static string Render(PlcTagListing listing)
    {
        var report = new StringBuilder();
        var clientInformation = BenchController.ClientInformation;

        report.AppendLine(
            $"ConnectionEndpoint {clientInformation.ConnectionEndpoint.Value}  " +
            $"CIP route path {clientInformation.CipRoutePath.Value}");
        report.AppendLine();

        report.AppendLine("Controller tags");
        report.AppendLine("===============");
        foreach (var tag in listing.ControllerTags)
        {
            report.AppendLine(Format(tag.Name, tag));
        }

        report.AppendLine();
        report.AppendLine("Program tags");
        report.AppendLine("============");
        foreach (var (programName, programTags) in listing.ProgramTags)
        {
            report.AppendLine(programName);
            foreach (var tag in programTags)
            {
                report.AppendLine("    " + Format(tag.Name, tag));
            }
        }

        report.AppendLine();
        report.AppendLine("UDTs");
        report.AppendLine("====");
        foreach (var udt in listing.Udts)
        {
            report.AppendLine($"Id={udt.Id}  Name={udt.Name}  Fields={udt.NumFields}  Size={udt.Size}");
            foreach (var field in udt.Fields)
            {
                report.AppendLine(
                    $"    {field.Name,-24} offset={field.Offset,-5} type=0x{field.Type:X4}  " +
                    $"{SymbolType.Describe(field.Type)}");
            }
        }

        report.AppendLine();
        report.AppendLine("Atomic scalars, by type");
        report.AppendLine("=======================");
        report.AppendLine("(These are the tags a scalar read/write slice can target directly:");
        report.AppendLine(" no structure, no array, no system flag.)");
        report.AppendLine();
        AppendAtomicScalars(report, listing);

        return report.ToString();
    }

    private static void AppendAtomicScalars(StringBuilder report, PlcTagListing listing)
    {
        var scalarsByType = AtomicScalars(listing)
            .GroupBy(tag => SymbolType.AtomicName(SymbolType.AtomicCode(tag.Type)))
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .ToList();

        if (scalarsByType.Count == 0)
        {
            report.AppendLine("(none — every tag on this controller is a structure, an array, or a system tag)");
            return;
        }

        foreach (var group in scalarsByType)
        {
            report.AppendLine($"{group.Key}:");
            foreach (var fullName in group.Select(tag => tag.FullName).OrderBy(name => name, StringComparer.Ordinal))
            {
                report.AppendLine($"    {fullName}");
            }
        }
    }

    private static string Format(string name, TagInfo tag)
    {
        var dimensions = tag.Dimensions is { Length: > 0 }
            ? $"  dims=[{string.Join(",", tag.Dimensions)}]"
            : string.Empty;

        return $"{name,-40} type=0x{tag.Type:X4}  {SymbolType.Describe(tag.Type),-20} len={tag.Length}{dimensions}";
    }
}
