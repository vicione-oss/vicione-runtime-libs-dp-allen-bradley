using System.Globalization;
using System.Text;
using libplctag;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration;

/// <summary>
/// <b>A probe, not a test.</b> It asserts almost nothing; it prints the three device facts the
/// <c>STRING</c> work was specified against, so they can be read off a real controller and recorded in
/// TEST-DEVICE-SETUP.md. <b>Delete it once they are.</b>
/// </summary>
/// <remarks>
/// <para>Two of the three were settled from libplctag's own documented Logix string layout — count word
/// 4 bytes at offset 0, capacity 82, 2 pad bytes, 88 total — which says the library strips the
/// <c>A0 02 HH HH</c> abbreviated-structure prefix into its own type-info store and hands the caller
/// only the member bytes. This prints the buffer so that claim is checked against the wire rather than
/// believed.</para>
/// <list type="number">
/// <item>Does the buffer start at <c>.LEN</c>, or at the <c>A0 02 HH HH</c> prefix? Every offset in
/// <c>LogixStringConverter</c> hangs off this.</item>
/// <item>What <c>ElementLength</c> does the listing report for <c>strValue1</c> — 86 (the members) or
/// 88 (padded)? <c>TagsDecoder</c> assumes 86 and subtracts the 4-byte <c>.LEN</c> to get the
/// capacity, so 88 would decode every built-in <c>STRING</c> as <c>.DATA[84]</c> and degrade its
/// every read. This is the fact with the most riding on it.</item>
/// <item>Is <c>strValue1</c> a built-in <c>STRING</c> (<c>.DATA[82]</c>) or a custom string type? Sets
/// the expected <c>StringMaxLength</c>.</item>
/// </list>
/// </remarks>
public class StringWireFormatProbeTests(ITestOutputHelper output) : LogixIntegrationTestBase
{
    private static readonly string ConnectionEndpoint = Environment.GetEnvironmentVariable("CIP_GATEWAY") ?? "192.168.0.100";
    private static readonly string CipRoutePath = Environment.GetEnvironmentVariable("CIP_PATH") ?? "1,0";

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    [Fact]
    public async Task ProbeStrValue1_PrintsTheRawBufferAndTheDecodedDeclaration()
    {
        // Arrange
        var dataPoint = CreateString(LogixTagAddresses.StrValue1);
        var tag = TagManager.TagFor(dataPoint);

        // Act
        var read = await tag.ReadAsync(TestContext.Current.CancellationToken);

        // Assert
        read.Succeeded.Should().BeTrue(read.Error);

        var buffer = read.Buffer.Span;
        output.WriteLine($"Tag                : {LogixTagAddresses.StrValue1}");
        output.WriteLine($"Buffer length      : {buffer.Length}   (88 = the padded .LEN + .DATA[82] template)");
        output.WriteLine($"First 16 bytes     : {Hex(buffer[..Math.Min(16, buffer.Length)])}");
        output.WriteLine(string.Empty);
        output.WriteLine("Fact 1 — where the buffer starts");
        output.WriteLine("  A0 02 .. ..  => the abbreviated-structure prefix is present; every converter offset shifts by 4.");
        output.WriteLine("  LL 00 00 00  => the buffer starts at .LEN, which is what LogixStringConverter assumes.");
        output.WriteLine(string.Empty);
        output.WriteLine("Fact 2 — the listing's declaration for the tag");
        output.WriteLine($"  {tag.Metadata?.ToString() ?? "(absent from the symbol table)"}");
        output.WriteLine(string.Empty);
        output.WriteLine("Fact 3 — what libplctag itself makes of the tag");
        output.WriteLine($"  {DescribeWithRawLibplctag(LogixTagAddresses.StrValue1)}");
    }

    // libplctag's own view of the tag, for the cross-check: its string accessors apply the Logix layout
    // (count word at offset 0, capacity 82, 88 total) that this converter is built on, so a disagreement
    // between its GetString and our decode is the interesting signal.
    private static string DescribeWithRawLibplctag(string tagName)
    {
        using var tag = new Tag
        {
            Gateway = ConnectionEndpoint,
            Path = CipRoutePath,
            PlcType = PlcType.ControlLogix,
            Protocol = Protocol.ab_eip,
            Name = tagName,
            Timeout = Timeout,
        };

        tag.Read();

        return $"GetSize()={tag.GetSize()}  ElementSize={tag.ElementSize}  ElementCount={tag.ElementCount}  " +
               $"GetString(0)='{tag.GetString(0)}'  GetStringLength(0)={tag.GetStringLength(0)}";
    }

    private static string Hex(ReadOnlySpan<byte> bytes)
    {
        var text = new StringBuilder(bytes.Length * 3);
        foreach (var b in bytes)
        {
            text.Append(CultureInfo.InvariantCulture, $"{b:X2} ");
        }

        return text.ToString().TrimEnd();
    }
}
