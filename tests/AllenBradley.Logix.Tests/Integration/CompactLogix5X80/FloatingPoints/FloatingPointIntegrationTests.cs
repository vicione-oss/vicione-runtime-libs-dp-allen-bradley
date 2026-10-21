using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.FloatingPoints;

/// <summary>
/// Values are compared for exact equality on purpose. A controller returns the bytes it was given, so a
/// round trip that loses a bit is a codec defect. A tolerance would hide it.
/// </summary>
public abstract class FloatingPointIntegrationTests(
    TestControllerFixture fixture,
    ITestOutputHelper output,
    TagScope scope)
    : CompactLogix5X80IntegrationTestBase(fixture, output, scope)
{
    [Theory]
    [InlineData(0f)]
    [InlineData(3.14159f)]
    [InlineData(-1.5f)]
    [InlineData(float.MinValue)]
    [InlineData(float.MaxValue)]
    public async Task ARealReadsBackAsWritten(float valueToWrite)
    {
        // Arrange
        var dataPoint = new RealDataPoint(TagPath.Parse(Tags.Real), DefaultPollFrequency, NoChannels);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedDeclaredType = new DeclaredType(dataPoint.TagAddress, AllenBradleyDataType.Real, MaxLength: null,
            DimensionCount.Scalar, new ElementCount(1));
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, expectedDeclaredType),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(0d)]
    [InlineData(Math.PI)]
    [InlineData(-2.5d)]
    [InlineData(double.MinValue)]
    [InlineData(double.MaxValue)]
    public async Task AnLRealReadsBackAsWritten(double valueToWrite)
    {
        // Arrange
        // Math.PI cannot be represented in a REAL, so it catches an LREAL encoded through the narrower codec.
        var dataPoint = new LRealDataPoint(TagPath.Parse(Tags.LReal), DefaultPollFrequency, NoChannels);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedDeclaredType = new DeclaredType(dataPoint.TagAddress, AllenBradleyDataType.Lreal, MaxLength: null,
            DimensionCount.Scalar, new ElementCount(1));
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, expectedDeclaredType),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }

    public sealed class ControllerScope(TestControllerFixture fixture, ITestOutputHelper output)
        : FloatingPointIntegrationTests(fixture, output, TagScope.Controller);

    public sealed class ProgramScope(TestControllerFixture fixture, ITestOutputHelper output)
        : FloatingPointIntegrationTests(fixture, output, TagScope.Program);
}
