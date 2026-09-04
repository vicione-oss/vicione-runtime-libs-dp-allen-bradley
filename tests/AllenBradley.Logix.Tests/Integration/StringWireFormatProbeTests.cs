using System.Globalization;
using System.Text;
using libplctag;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration;

/// <summary>
/// <b>A probe, not a test.</b> It asserts almost nothing; it prints the three device facts the
/// <c>STRING</c> work was specified against, so they can be read off a real controller and recorded in
/// TEST-DEVICE-SETUP.md. <b>Delete it once they are.</b>
/// </summary>
public class StringWireFormatProbeTests(ITestOutputHelper output) : LogixIntegrationTestBase
{
    private static readonly string ConnectionEndpoint = Environment.GetEnvironmentVariable("CIP_GATEWAY") ?? "192.168.0.100";
    private static readonly string CipRoutePath = Environment.GetEnvironmentVariable("CIP_PATH") ?? "1,0";

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    [Fact]
    public async Task ProbeStrValue1_PrintsTheRawBufferAndTheDecodedDeclaration()
    {
        // Arrange
        var dataPoint = new StringDataPoint(new TagName(LogixTagAddresses.StrValue1), DefaultPollFrequency, NoChannels, new StringMaxLength(StringMaxLength.Standard.Value));
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
        using var tag = new Tag();
        tag.Gateway = ConnectionEndpoint;
        tag.Path = CipRoutePath;
        tag.PlcType = PlcType.ControlLogix;
        tag.Protocol = Protocol.ab_eip;
        tag.Name = tagName;
        tag.Timeout = Timeout;

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
