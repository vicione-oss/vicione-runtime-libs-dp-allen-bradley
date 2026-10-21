using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Addressing;

/// <summary>
/// Logix matches tag, program and member names without regard to case, so the lookup must too. Each
/// address here is configured upper-cased, which no name on the controller is.
/// </summary>
public abstract class CaseInsensitivityIntegrationTests(
    TestControllerFixture fixture,
    ITestOutputHelper output,
    TagScope scope)
    : CompactLogix5X80IntegrationTestBase(fixture, output, scope)
{
    [Fact]
    public async Task AnUpperCaseTagNameIsFound()
    {
        // Arrange
        var dataPoint = new DIntDataPoint(
            TagPath.Parse(TagAddresses.InUpperCase(Tags.DInt)), DefaultPollFrequency, NoChannels);
        const int valueToWrite = 123456;

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expected = new RoundTripResult(
            new ResolvedDataPoint(dataPoint,
                new DeclaredType(dataPoint.TagAddress, AllenBradleyDataType.Dint, MaxLength: null,
                    DimensionCount.Scalar, new ElementCount(1))),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task AnUpperCaseMemberNameIsFound()
    {
        // Arrange
        var dataPoint = new RealDataPoint(
            TagPath.Parse(TagAddresses.InUpperCase(Tags.Udt.Inner.Real)), DefaultPollFrequency, NoChannels);
        const float valueToWrite = 123.456f;

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expected = new RoundTripResult(
            new ResolvedDataPoint(dataPoint,
                new DeclaredType(dataPoint.TagAddress, AllenBradleyDataType.Real, MaxLength: null,
                    DimensionCount.Scalar, new ElementCount(1))),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expected);
    }

    public sealed class ControllerScope(TestControllerFixture fixture, ITestOutputHelper output)
        : CaseInsensitivityIntegrationTests(fixture, output, TagScope.Controller);

    public sealed class ProgramScope(TestControllerFixture fixture, ITestOutputHelper output)
        : CaseInsensitivityIntegrationTests(fixture, output, TagScope.Program);
}
