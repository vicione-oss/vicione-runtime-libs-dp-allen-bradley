using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.FloatingPoints;

/// <summary>
/// Every element set holds both ends of the range and a value with a full mantissa. A mis-sized element
/// then cannot agree with the read-back.
/// </summary>
public abstract class FloatingPointArrayIntegrationTests(
    TestControllerFixture fixture,
    ITestOutputHelper output,
    TagScope scope)
    : CompactLogix5X80IntegrationTestBase(fixture, output, scope)
{
    [Fact]
    public async Task ARealArrayReadsBackAsWritten()
    {
        // Arrange
        RealArrayDataPoint temperatures = new(
            TagPath.Parse(Tags.RealArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.ArrayElementCount);
        float[] tenTemperatures = [0f, 1f, -1f, 3.14159f, float.MinValue, float.MaxValue, 10.5f, -20.25f, 30.75f, 40f];

        // Act
        var roundTripResult = await RoundTripAsync(temperatures, tenTemperatures);

        // Assert
        var expectedDeclaredType = new DeclaredType(temperatures.TagAddress, AllenBradleyDataType.Real,
            MaxLength: null, DimensionCount.OneDimensional, TagAddresses.ArrayElementCount);
        var expectedResolved = new ResolvedDataPoint(temperatures, expectedDeclaredType);

        roundTripResult.Resolved.Should().Be(expectedResolved);
        roundTripResult.ReadValue.Value.Should().BeOfType<float[]>().Which.Should().Equal(tenTemperatures);
    }

    [Fact]
    public async Task AnLRealArrayReadsBackAsWritten()
    {
        // Arrange
        LRealArrayDataPoint positions = new(
            TagPath.Parse(Tags.LRealArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.ArrayElementCount);
        double[] tenPositions =
            [0d, 1d, -1d, 3.141592653589793d, double.MinValue, double.MaxValue, 10.5d, -20.25d, 30.75d, 40d];

        // Act
        var roundTripResult = await RoundTripAsync(positions, tenPositions);

        // Assert
        var expectedDeclaredType = new DeclaredType(positions.TagAddress, AllenBradleyDataType.Lreal, MaxLength: null,
            DimensionCount.OneDimensional, TagAddresses.ArrayElementCount);
        var expectedResolved = new ResolvedDataPoint(positions, expectedDeclaredType);

        roundTripResult.Resolved.Should().Be(expectedResolved);
        roundTripResult.ReadValue.Value.Should().BeOfType<double[]>().Which.Should().Equal(tenPositions);
    }

    public sealed class ControllerScope(TestControllerFixture fixture, ITestOutputHelper output)
        : FloatingPointArrayIntegrationTests(fixture, output, TagScope.Controller);

    public sealed class ProgramScope(TestControllerFixture fixture, ITestOutputHelper output)
        : FloatingPointArrayIntegrationTests(fixture, output, TagScope.Program);
}
