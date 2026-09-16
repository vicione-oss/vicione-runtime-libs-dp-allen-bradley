using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.FloatingPoints;

/// <summary>
/// One element of each floating-point array as a data point of its own, addressed with a subscript. The
/// symbol table names no element, so an element resolves to its array's declaration. Every element value
/// has a full mantissa, so a mis-sized element cannot agree with the read-back.
/// </summary>
public sealed class FloatingPointArrayElementIntegrationTests(ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(output)
{
    private const int ElementIndex = 3;
    private const float RealElementValue = 3.14159f;
    private const double LRealElementValue = 3.141592653589793d;

    private static readonly RealArrayDataPoint RealArray = new(
        TagPath.Parse(TagAddresses.RealArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private static readonly RealDataPoint RealElement = new(
        TagPath.Parse(TagAddresses.ArrayElement(TagAddresses.RealArray, ElementIndex)),
        DefaultPollFrequency,
        NoChannels);

    private static readonly LRealArrayDataPoint LRealArray = new(
        TagPath.Parse(TagAddresses.LRealArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private static readonly LRealDataPoint LRealElement = new(
        TagPath.Parse(TagAddresses.ArrayElement(TagAddresses.LRealArray, ElementIndex)),
        DefaultPollFrequency,
        NoChannels);

    [Fact]
    public async Task ARealArrayElementRoundTripsAndResolvesToItsArraysDeclaration()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(RealElement, RealElementValue);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.RealArray, AllenBradleyDataType.Real, TagAddresses.ArrayElementCount);
        var expected = new RoundTripResult(
            new ResolvedDataPoint(RealElement, expectedDefinition),
            RealElement.CreateLogixValue(RealElementValue));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task WritingARealArrayElementLeavesTheOtherElementsAsTheyWere()
    {
        // Arrange

        // Act
        var arrayAfterWrite = await WriteOneElementOfAZeroedArrayAsync(RealArray, RealElement, RealElementValue);

        // Assert
        float[] expected = [0f, 0f, 0f, RealElementValue, 0f, 0f, 0f, 0f, 0f, 0f];

        arrayAfterWrite.Should().Equal(expected);
    }

    [Fact]
    public async Task AnLRealArrayElementRoundTripsAndResolvesToItsArraysDeclaration()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(LRealElement, LRealElementValue);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.LRealArray, AllenBradleyDataType.Lreal, TagAddresses.ArrayElementCount);
        var expected = new RoundTripResult(
            new ResolvedDataPoint(LRealElement, expectedDefinition),
            LRealElement.CreateLogixValue(LRealElementValue));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task WritingAnLRealArrayElementLeavesTheOtherElementsAsTheyWere()
    {
        // Arrange

        // Act
        var arrayAfterWrite = await WriteOneElementOfAZeroedArrayAsync(LRealArray, LRealElement, LRealElementValue);

        // Assert
        double[] expected = [0d, 0d, 0d, LRealElementValue, 0d, 0d, 0d, 0d, 0d, 0d];

        arrayAfterWrite.Should().Equal(expected);
    }
}
