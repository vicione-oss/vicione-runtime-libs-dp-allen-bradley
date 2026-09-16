using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Integers;

/// <summary>
/// One element of each signed integer array as a data point of its own, addressed with a subscript. The
/// symbol table names no element, so an element resolves to its array's declaration. Every element value
/// is asymmetric in its bytes, so a mis-sized element cannot agree with the read-back.
/// </summary>
public sealed class IntegerArrayElementIntegrationTests(ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(output)
{
    private const int ElementIndex = 3;
    private const sbyte SIntElementValue = -56;
    private const short IntElementValue = 4711;
    private const int DIntElementValue = 123456;
    private const long LIntElementValue = 1234567890123;

    private static readonly SIntArrayDataPoint SIntArray = new(
        TagPath.Parse(TagAddresses.SIntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private static readonly SIntDataPoint SIntElement = new(
        TagPath.Parse(TagAddresses.ArrayElement(TagAddresses.SIntArray, ElementIndex)),
        DefaultPollFrequency,
        NoChannels);

    private static readonly IntArrayDataPoint IntArray = new(
        TagPath.Parse(TagAddresses.IntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private static readonly IntDataPoint IntElement = new(
        TagPath.Parse(TagAddresses.ArrayElement(TagAddresses.IntArray, ElementIndex)),
        DefaultPollFrequency,
        NoChannels);

    private static readonly DIntArrayDataPoint DIntArray = new(
        TagPath.Parse(TagAddresses.DIntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private static readonly DIntDataPoint DIntElement = new(
        TagPath.Parse(TagAddresses.ArrayElement(TagAddresses.DIntArray, ElementIndex)),
        DefaultPollFrequency,
        NoChannels);

    private static readonly LIntArrayDataPoint LIntArray = new(
        TagPath.Parse(TagAddresses.LIntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private static readonly LIntDataPoint LIntElement = new(
        TagPath.Parse(TagAddresses.ArrayElement(TagAddresses.LIntArray, ElementIndex)),
        DefaultPollFrequency,
        NoChannels);

    [Fact]
    public async Task AnSIntArrayElementRoundTripsAndResolvesToItsArraysDeclaration()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(SIntElement, SIntElementValue);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.SIntArray, AllenBradleyDataType.Sint, TagAddresses.ArrayElementCount);
        var expected = new RoundTripResult(
            new ResolvedDataPoint(SIntElement, expectedDefinition),
            SIntElement.CreateLogixValue(SIntElementValue));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task WritingAnSIntArrayElementLeavesTheOtherElementsAsTheyWere()
    {
        // Arrange

        // Act
        var arrayAfterWrite = await WriteOneElementOfAZeroedArrayAsync(SIntArray, SIntElement, SIntElementValue);

        // Assert
        sbyte[] expected = [0, 0, 0, SIntElementValue, 0, 0, 0, 0, 0, 0];

        arrayAfterWrite.Should().Equal(expected);
    }

    [Fact]
    public async Task AnIntArrayElementRoundTripsAndResolvesToItsArraysDeclaration()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(IntElement, IntElementValue);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.IntArray, AllenBradleyDataType.Int, TagAddresses.ArrayElementCount);
        var expected = new RoundTripResult(
            new ResolvedDataPoint(IntElement, expectedDefinition),
            IntElement.CreateLogixValue(IntElementValue));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task WritingAnIntArrayElementLeavesTheOtherElementsAsTheyWere()
    {
        // Arrange

        // Act
        var arrayAfterWrite = await WriteOneElementOfAZeroedArrayAsync(IntArray, IntElement, IntElementValue);

        // Assert
        short[] expected = [0, 0, 0, IntElementValue, 0, 0, 0, 0, 0, 0];

        arrayAfterWrite.Should().Equal(expected);
    }

    [Fact]
    public async Task ADIntArrayElementRoundTripsAndResolvesToItsArraysDeclaration()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(DIntElement, DIntElementValue);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.DIntArray, AllenBradleyDataType.Dint, TagAddresses.ArrayElementCount);
        var expected = new RoundTripResult(
            new ResolvedDataPoint(DIntElement, expectedDefinition),
            DIntElement.CreateLogixValue(DIntElementValue));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task WritingADIntArrayElementLeavesTheOtherElementsAsTheyWere()
    {
        // Arrange

        // Act
        var arrayAfterWrite = await WriteOneElementOfAZeroedArrayAsync(DIntArray, DIntElement, DIntElementValue);

        // Assert
        int[] expected = [0, 0, 0, DIntElementValue, 0, 0, 0, 0, 0, 0];

        arrayAfterWrite.Should().Equal(expected);
    }

    [Fact]
    public async Task AnLIntArrayElementRoundTripsAndResolvesToItsArraysDeclaration()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(LIntElement, LIntElementValue);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.LIntArray, AllenBradleyDataType.Lint, TagAddresses.ArrayElementCount);
        var expected = new RoundTripResult(
            new ResolvedDataPoint(LIntElement, expectedDefinition),
            LIntElement.CreateLogixValue(LIntElementValue));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task WritingAnLIntArrayElementLeavesTheOtherElementsAsTheyWere()
    {
        // Arrange

        // Act
        var arrayAfterWrite = await WriteOneElementOfAZeroedArrayAsync(LIntArray, LIntElement, LIntElementValue);

        // Assert
        long[] expected = [0, 0, 0, LIntElementValue, 0, 0, 0, 0, 0, 0];

        arrayAfterWrite.Should().Equal(expected);
    }
}
