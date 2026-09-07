using System.Text;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.LibPlcTag.TagListing;

/// <summary>
/// Dumps the controller's real tag namespace with symbol types decoded, and writes it to a file.
/// This is the ground truth for the node-tree and addressing design: it answers which tags exist,
/// what type each one actually is, and which of them are plain atomic scalars a first read can target.
///
///   CIP_GATEWAY    – IP address of the PLC/gateway  (default: 192.168.0.100)
///   CIP_PATH       – CIP route path                 (default: 1,0)
///   CIP_DUMP_PATH  – Where to write the dump        (default: tag-namespace-dump.txt)
/// </summary>
[Trait("Category", "Integration")]
[Collection(PlcCollection.Name)]
public class TagNamespaceDumpTests
{
    private static readonly string ConnectionEndpoint = Environment.GetEnvironmentVariable("CIP_GATEWAY") ?? "192.168.0.100";
    private static readonly string CipRoutePath = Environment.GetEnvironmentVariable("CIP_PATH") ?? "1,0";
    private static readonly string DumpPath = Environment.GetEnvironmentVariable("CIP_DUMP_PATH") ?? "tag-namespace-dump.txt";

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    private readonly ITestOutputHelper _output;

    public TagNamespaceDumpTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void DumpTagNamespace_WritesDecodedListingToFile()
    {
        // Arrange

        // Act
        var listing = new PlcTagLister(ConnectionEndpoint, CipRoutePath, Timeout).List();

        // Assert
        var report = Render(listing);
        File.WriteAllText(DumpPath, report);

        _output.WriteLine(report);
        _output.WriteLine($"Dump written to {Path.GetFullPath(DumpPath)}");

        Assert.NotEmpty(listing.ControllerTags);
    }

    private static string Render(PlcTagListing listing)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"ConnectionEndpoint {ConnectionEndpoint}  CIP route path {CipRoutePath}");
        sb.AppendLine();

        sb.AppendLine("Controller tags");
        sb.AppendLine("===============");
        foreach (var tag in listing.ControllerTags)
            sb.AppendLine(Format(tag.Name, tag));

        sb.AppendLine();
        sb.AppendLine("Program tags");
        sb.AppendLine("============");
        foreach (var (programName, programTags) in listing.ProgramTags)
        {
            sb.AppendLine(programName);
            foreach (var tag in programTags)
                sb.AppendLine("    " + Format(tag.Name, tag));
        }

        sb.AppendLine();
        sb.AppendLine("UDTs");
        sb.AppendLine("====");
        foreach (var udt in listing.Udts)
        {
            sb.AppendLine($"Id={udt.Id}  Name={udt.Name}  Fields={udt.NumFields}  Size={udt.Size}");
            foreach (var field in udt.Fields)
                sb.AppendLine($"    {field.Name,-24} offset={field.Offset,-5} type=0x{field.Type:X4}  {SymbolType.Describe(field.Type)}");
        }

        sb.AppendLine();
        sb.AppendLine("Atomic scalars, by type");
        sb.AppendLine("=======================");
        sb.AppendLine("(These are the tags a scalar read/write slice can target directly:");
        sb.AppendLine(" no structure, no array, no system flag.)");
        sb.AppendLine();

        var scalars = AtomicScalars(listing)
            .GroupBy(t => SymbolType.AtomicName(SymbolType.AtomicCode(t.Type)))
            .OrderBy(g => g.Key, StringComparer.Ordinal);

        var any = false;
        foreach (var group in scalars)
        {
            any = true;
            sb.AppendLine($"{group.Key}:");
            foreach (var (name, _) in group.Select(t => (t.FullName, t.Type)).OrderBy(t => t.FullName, StringComparer.Ordinal))
                sb.AppendLine($"    {name}");
        }

        if (!any)
            sb.AppendLine("(none — every tag on this controller is a structure, an array, or a system tag)");

        return sb.ToString();
    }

    private static string Format(string name, TagInfo tag)
    {
        var dims = tag.Dimensions is { Length: > 0 }
            ? $"  dims=[{string.Join(",", tag.Dimensions)}]"
            : string.Empty;

        return $"{name,-40} type=0x{tag.Type:X4}  {SymbolType.Describe(tag.Type),-20} len={tag.Length}{dims}";
    }

    /// <summary>Every atomic scalar tag in the listing, with its fully qualified name.</summary>
    internal static IEnumerable<(string FullName, ushort Type)> AtomicScalars(PlcTagListing listing)
    {
        foreach (var tag in listing.ControllerTags.Where(t => SymbolType.IsAtomicScalar(t.Type)))
            yield return (tag.Name, tag.Type);

        foreach (var (programName, programTags) in listing.ProgramTags)
            foreach (var tag in programTags.Where(t => SymbolType.IsAtomicScalar(t.Type)))
                yield return ($"{programName}.{tag.Name}", tag.Type);
    }
}
