using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.FloatingPoints;

/// <summary>
/// Every element set holds both ends of the range and a value with a full mantissa, so a mis-sized element
/// cannot agree with the read-back.
/// </summary>
public sealed class FloatingPointArrayIntegrationTests(ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(output)
{
    [Fact]
    public async Task ARealArrayIsWrittenWholeAndReadsBackAsTheElementsThatWentIn()
    {
        // Arrange
        RealArrayDataPoint temperatures = new(
            new TagName(TagAddresses.RealArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.ArrayElementCount);
        float[] tenTemperatures = [0f, 1f, -1f, 3.14159f, float.MinValue, float.MaxValue, 10.5f, -20.25f, 30.75f, 40f];

        // Act
        var roundTripResult = await RoundTripAsync(temperatures, tenTemperatures);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.RealArray, AllenBradleyDataType.Real, TagAddresses.ArrayElementCount);
        var expectedResolved = new ResolvedDataPoint(temperatures, expectedDefinition);

        roundTripResult.Resolved.Should().Be(expectedResolved);
        roundTripResult.ReadValue.Value.Should().BeOfType<float[]>().Which.Should().Equal(tenTemperatures);
    }

    [Fact]
    public async Task AnLRealArrayIsWrittenWholeAndReadsBackAsTheElementsThatWentIn()
    {
        // Arrange
        LRealArrayDataPoint positions = new(
            new TagName(TagAddresses.LRealArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.ArrayElementCount);
        double[] tenPositions =
            [0d, 1d, -1d, 3.141592653589793d, double.MinValue, double.MaxValue, 10.5d, -20.25d, 30.75d, 40d];

        // Act
        var roundTripResult = await RoundTripAsync(positions, tenPositions);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.LRealArray, AllenBradleyDataType.Lreal, TagAddresses.ArrayElementCount);
        var expectedResolved = new ResolvedDataPoint(positions, expectedDefinition);

        roundTripResult.Resolved.Should().Be(expectedResolved);
        roundTripResult.ReadValue.Value.Should().BeOfType<double[]>().Which.Should().Equal(tenPositions);
    }
}
