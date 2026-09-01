using libplctag;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration;

/// <summary>
/// End-to-end read through the production client stack — <c>LogixTagAccessFactory</c> →
/// <c>CachingLogixTagManager</c> → <c>LogixClient</c> → <c>LogixReadBatch</c> →
/// <c>DataPointConverterRegistry</c> → <c>DIntConverter</c> → <c>LogixTagAccess</c>
/// → libplctag — against the real CompactLogix L32E. Every layer under test is the shipping code; the
/// test only supplies a raw-libplctag cross-check. Requires the device reachable (see
/// TEST-DEVICE-SETUP.md).
/// </summary>
public class LogixClientReadTests : LogixIntegrationTestBase
{
    private static readonly string ConnectionEndpoint = Environment.GetEnvironmentVariable("CIP_GATEWAY") ?? "192.168.0.100";
    private static readonly string CipRoutePath = Environment.GetEnvironmentVariable("CIP_PATH") ?? "1,0";

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    [Fact]
    public async Task ReadAsync_Dint_DecodesTheSameValueLibplctagReadsRaw()
    {
        // Arrange
        // Read the DINT with a raw libplctag handle to establish the expected value. Cross-checking
        // against libplctag's own typed getter proves our raw-buffer decode is byte-correct.
        var expected = ReadDintWithRawLibplctag(LogixTagAddresses.CounterPreset);
        var group = CreateGroup(CreateDInt(LogixTagAddresses.CounterPreset));

        // Act
        // Read through the production client + converter stack. The target is a COUNTER member, absent
        // from the flat symbol table, so nothing checked its type: the decode reads the bytes as the
        // DINT the configuration says they are.
        var values = await Client.ReadAsync(group, TestContext.Current.CancellationToken);

        // Assert
        values.Should().ContainSingle();
        var value = values[0];
        value.Quality.Should().Be(LogixQuality.Good);
        value.Value.Should().Be(expected);
    }

    private static int ReadDintWithRawLibplctag(string tagName)
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
        return tag.GetInt32(0);
    }
}
