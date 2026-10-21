using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Udts;

/// <summary>
/// The atomic members of <see cref="TagAddresses.Udt"/>: a <c>DINT</c> reached through the tag's template, and
/// two <c>BOOL</c>s that the controller packs into one hidden byte.
/// </summary>
public abstract class UdtAtomicMemberIntegrationTests(
    TestControllerFixture fixture, ITestOutputHelper output, TagScope scope)
    : CompactLogix5X80IntegrationTestBase(fixture, output, scope)
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ABoolMemberReadsBackAsWritten(bool valueToWrite)
    {
        // Arrange
        var dataPoint = new BoolDataPoint(TagPath.Parse(Tags.Udt.Bool), DefaultPollFrequency, NoChannels);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, new DeclaredType(dataPoint.TagAddress, AllenBradleyDataType.Bool, MaxLength: null, DimensionCount.Scalar, new ElementCount(1))),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WritingABoolMemberKeepsItsNeighbour(bool neighbourValue)
    {
        // Arrange
        // Both bits start equal. The write sets the first to the opposite of its neighbour.
        var cancellationToken = TestContext.Current.CancellationToken;
        var written = new BoolDataPoint(TagPath.Parse(Tags.Udt.Bool), DefaultPollFrequency, NoChannels);
        var neighbour = new BoolDataPoint(TagPath.Parse(Tags.Udt.BoolNeighbour), DefaultPollFrequency, NoChannels);
        await Client.WriteAsync(
            [written.CreateLogixValue(neighbourValue), neighbour.CreateLogixValue(neighbourValue)], cancellationToken);

        // Act
        await Client.WriteAsync([written.CreateLogixValue(!neighbourValue)], cancellationToken);
        var bothBitsAfterwards = await ReadAsync([written, neighbour]);

        // Assert
        bothBitsAfterwards.Should().Equal(
            written.CreateLogixValue(!neighbourValue), neighbour.CreateLogixValue(neighbourValue));
    }

    [Fact]
    public async Task AdIntMemberReadsBackAsWritten()
    {
        // Arrange
        const int valueToWrite = 123456;
        var dataPoint = new DIntDataPoint(TagPath.Parse(Tags.Udt.DInt), DefaultPollFrequency, NoChannels);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, new DeclaredType(dataPoint.TagAddress, AllenBradleyDataType.Dint, MaxLength: null, DimensionCount.Scalar, new ElementCount(1))),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }

    public sealed class ControllerScope(TestControllerFixture fixture, ITestOutputHelper output)
        : UdtAtomicMemberIntegrationTests(fixture, output, TagScope.Controller);

    public sealed class ProgramScope(TestControllerFixture fixture, ITestOutputHelper output)
        : UdtAtomicMemberIntegrationTests(fixture, output, TagScope.Program);
}
