using AwesomeAssertions;
using libplctag;

namespace ConnectivityTests;

/// <summary>
/// End-to-end integration tests that require a physical ControlLogix/CompactLogix device.
/// Marked with Category=E2E so they can be excluded from CI runs without device access:
///
///   dotnet test --filter "Category!=E2E"          (skip E2E)
///   dotnet test --filter "Category=E2E"           (run only E2E)
///
/// Configure the device below or via environment variables:
///   CIP_GATEWAY   – IP address of the PLC/gateway  (default: 192.168.0.100)
///   CIP_PATH      – Routing path                   (default: 1,0)
///   CIP_TAG_NAME  – Tag to write/read              (default: Program:MainProgram.strValue1)
/// </summary>
[Trait("Category", "E2E")]
public class PlcConnectivityTests
{
    // ── Connection configuration ──────────────────────────────────────────────
    private static readonly string Gateway  = Environment.GetEnvironmentVariable("CIP_GATEWAY")  ?? "192.168.0.100";
    private static readonly string Path     = Environment.GetEnvironmentVariable("CIP_PATH")     ?? "1,0";
    private static readonly string TagName  = Environment.GetEnvironmentVariable("CIP_TAG_NAME") ?? "Program:MainProgram.strValue1";

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void WriteAndReadStringTag_RoundTripPreservesValue()
    {
        // Arrange
        using var tag = new Tag();
        tag.Name     = TagName;
        tag.Gateway  = Gateway;
        tag.Path     = Path;
        tag.PlcType  = PlcType.ControlLogix;
        tag.Protocol = Protocol.ab_eip;
        tag.Timeout  = Timeout;
        tag.Read(); // initialize tag metadata before writing

        var expectedValue = $"TEST_{DateTime.UtcNow:HHmmss}";

        // Act
        tag.SetString(0, expectedValue);
        tag.Write();
        tag.Read();
        var actualValue = tag.GetString(0);

        // Assert
        actualValue.Should().Be(expectedValue);
    }

    [Fact]
    public async Task WriteAndReadStringTagAsync_RoundTripPreservesValue()
    {
        // Arrange
        using var tag = new Tag();
        tag.Name     = TagName;
        tag.Gateway  = Gateway;
        tag.Path     = Path;
        tag.PlcType  = PlcType.ControlLogix;
        tag.Protocol = Protocol.ab_eip;
        tag.Timeout  = Timeout;
        await tag.ReadAsync(); // initialize tag metadata before writing

        var expectedValue = $"TEST_{DateTime.UtcNow:HHmmss}";

        // Act
        tag.SetString(0, expectedValue);
        await tag.WriteAsync();
        await tag.ReadAsync();
        var actualValue = tag.GetString(0);

        // Assert
        actualValue.Should().Be(expectedValue);
    }
}
