using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.ClientStack;

/// <summary>
/// The client stack against libplctag's own typed accessors on the same tag, one direction per test. The
/// expectation never comes from the codec under test, because a round trip through the client alone passes
/// when the encode and the decode share a wrong layout. The suite writes the scope's <c>DINT</c>.
/// </summary>
public abstract class LibplctagCrossCheckIntegrationTests(
    TestControllerFixture fixture,
    ITestOutputHelper output,
    TagScope scope)
    : CompactLogix5X80IntegrationTestBase(fixture, output, scope)
{
    // Four distinct bytes and the sign bit, so a wrong byte order or width cannot give the same value.
    private const int DistinctBytesValue = -123456789;

    private DIntDataPoint DInt => new(TagPath.Parse(Tags.DInt), DefaultPollFrequency, NoChannels);

    [Fact]
    public async Task AdIntThatLibplctagWroteReadsBackThroughTheClientAsTheSameValue()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using (var rawTag = TestControllerConfiguration.RawTagFor(Tags.DInt))
        {
            await rawTag.InitializeAsync(cancellationToken);
            rawTag.SetInt32(0, DistinctBytesValue);
            await rawTag.WriteAsync(cancellationToken);
        }

        // Act
        var readValue = await ReadAsync(DInt);

        // Assert
        readValue.Should().Be(DInt.CreateLogixValue(DistinctBytesValue));
    }

    [Fact]
    public async Task AdIntThatTheClientWroteReadsBackThroughLibplctagAsTheSameValue()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        await Client.WriteAsync([DInt.CreateLogixValue(DistinctBytesValue)], cancellationToken);

        // Act
        var readValue = await ReadWithRawLibplctagAsync(Tags.DInt, cancellationToken);

        // Assert
        readValue.Should().Be(DistinctBytesValue);
    }

    private static async Task<int> ReadWithRawLibplctagAsync(string tagName, CancellationToken cancellationToken)
    {
        using var rawTag = TestControllerConfiguration.RawTagFor(tagName);
        await rawTag.ReadAsync(cancellationToken);

        return rawTag.GetInt32(0);
    }

    public sealed class ControllerScope(TestControllerFixture fixture, ITestOutputHelper output)
        : LibplctagCrossCheckIntegrationTests(fixture, output, TagScope.Controller);

    public sealed class ProgramScope(TestControllerFixture fixture, ITestOutputHelper output)
        : LibplctagCrossCheckIntegrationTests(fixture, output, TagScope.Program);
}
