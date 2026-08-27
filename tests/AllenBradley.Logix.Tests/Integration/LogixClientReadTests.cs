using libplctag;
using Microsoft.Extensions.Logging.Abstractions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access.LibPlcTag;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using Path = ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device.Path;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration;

/// <summary>
/// End-to-end read through the production client stack — <c>LogixTagAccessFactory</c> →
/// <c>CachingLogixTagManager</c> → <c>LogixClient</c> → <c>LogixReadBatch</c> →
/// <c>DataPointConverterRegistry</c> → <c>DIntConverter</c> → <c>LogixTagAccess</c>
/// → libplctag — against the real CompactLogix L32E. Every layer under test is the shipping code; the
/// test only supplies connection settings and a raw-libplctag cross-check. Requires the device
/// reachable (see TEST-DEVICE-SETUP.md).
/// </summary>
[Trait("Category", "Integration")]
[Collection(PlcCollection.Name)]
public class LogixClientReadTests
{
    private static readonly string Gateway = Environment.GetEnvironmentVariable("CIP_GATEWAY") ?? "192.168.0.100";
    private static readonly string Path = Environment.GetEnvironmentVariable("CIP_PATH") ?? "1,0";

    // The device exposes no plain DINT tag, but a COUNTER's .PRE member is a stable, side-effect-free
    // DINT — a safe read target for DIntConverter. Override with CIP_DINT_TAG if your device differs.
    private static readonly string DintTagName =
        Environment.GetEnvironmentVariable("CIP_DINT_TAG") ?? "Program:MainProgram.Counter.PRE";

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    [Fact]
    public async Task ReadAsync_Dint_DecodesTheSameValueLibplctagReadsRaw()
    {
        // Arrange
        // Read the DINT with a raw libplctag handle to establish the expected value. Cross-checking
        // against libplctag's own typed getter proves our raw-buffer decode is byte-correct.
        var expected = ReadDintWithRawLibplctag(DintTagName);

        var accessFactory =
            new LogixTagAccessFactory(
                new LogixClientInformation(new Gateway(Gateway), new Path(Path), LogixControllerType.ControlLogix),
                Timeout);
        using var tagManager = new CachingLogixTagManager(
            accessFactory, new TagDefinitionsLoader(accessFactory), NullLogger<CachingLogixTagManager>.Instance);
        var readClient = new LogixClient(tagManager);
        IReadOnlyList<ILogixDataPoint> group = [CreateDInt(DintTagName)];

        // Act
        // Load the schema (the connect precondition), then read through the production client + converter
        // stack. The target is a COUNTER member, absent from the flat symbol table, so its metadata is null
        // and the byte-size backstop is what gates the decode.
        await tagManager.LoadTagDefinitionsAsync(CancellationToken.None);
        var values = await readClient.ReadAsync(group, CancellationToken.None);

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
            Gateway = Gateway,
            Path = Path,
            PlcType = PlcType.ControlLogix,
            Protocol = Protocol.ab_eip,
            Name = tagName,
            Timeout = Timeout,
        };

        tag.Read();
        return tag.GetInt32(0);
    }
}
