using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Integers;

/// <summary>
/// One element of each unsigned integer array as a data point of its own, addressed with a subscript. The
/// symbol table names no element, so an element resolves to its array's declaration. Every element value
/// is above the signed maximum, which is what a signed misreading of the same bytes gets wrong.
/// </summary>
public sealed class UnsignedIntegerArrayElementIntegrationTests(ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(output)
{
    private const int ElementIndex = 3;
    private const byte USIntElementValue = 200;
    private const ushort UIntElementValue = 50000;
    private const uint UDIntElementValue = 4000000000;
    private const ulong ULIntElementValue = 18446744073709551614;

    private static readonly USIntArrayDataPoint USIntArray = new(
        TagPath.Parse(TagAddresses.USIntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private static readonly USIntDataPoint USIntElement = new(
        TagPath.Parse(TagAddresses.ArrayElement(TagAddresses.USIntArray, ElementIndex)),
        DefaultPollFrequency,
        NoChannels);

    private static readonly UIntArrayDataPoint UIntArray = new(
        TagPath.Parse(TagAddresses.UIntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private static readonly UIntDataPoint UIntElement = new(
        TagPath.Parse(TagAddresses.ArrayElement(TagAddresses.UIntArray, ElementIndex)),
        DefaultPollFrequency,
        NoChannels);

    private static readonly UDIntArrayDataPoint UDIntArray = new(
        TagPath.Parse(TagAddresses.UDIntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private static readonly UDIntDataPoint UDIntElement = new(
        TagPath.Parse(TagAddresses.ArrayElement(TagAddresses.UDIntArray, ElementIndex)),
        DefaultPollFrequency,
        NoChannels);

    private static readonly ULIntArrayDataPoint ULIntArray = new(
        TagPath.Parse(TagAddresses.ULIntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private static readonly ULIntDataPoint ULIntElement = new(
        TagPath.Parse(TagAddresses.ArrayElement(TagAddresses.ULIntArray, ElementIndex)),
        DefaultPollFrequency,
        NoChannels);

    [Fact]
    public async Task AUSIntArrayElementRoundTripsAndResolvesToItsArraysDeclaration()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(USIntElement, USIntElementValue);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.USIntArray, AllenBradleyDataType.Usint, TagAddresses.ArrayElementCount);
        var expected = new RoundTripResult(
            new ResolvedDataPoint(USIntElement, expectedDefinition),
            USIntElement.CreateLogixValue(USIntElementValue));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task WritingAUSIntArrayElementLeavesTheOtherElementsAsTheyWere()
    {
        // Arrange

        // Act
        var arrayAfterWrite = await WriteOneElementOfAZeroedArrayAsync(USIntArray, USIntElement, USIntElementValue);

        // Assert
        byte[] expected = [0, 0, 0, USIntElementValue, 0, 0, 0, 0, 0, 0];

        arrayAfterWrite.Should().Equal(expected);
    }

    [Fact]
    public async Task AUIntArrayElementRoundTripsAndResolvesToItsArraysDeclaration()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(UIntElement, UIntElementValue);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.UIntArray, AllenBradleyDataType.Uint, TagAddresses.ArrayElementCount);
        var expected = new RoundTripResult(
            new ResolvedDataPoint(UIntElement, expectedDefinition),
            UIntElement.CreateLogixValue(UIntElementValue));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task WritingAUIntArrayElementLeavesTheOtherElementsAsTheyWere()
    {
        // Arrange

        // Act
        var arrayAfterWrite = await WriteOneElementOfAZeroedArrayAsync(UIntArray, UIntElement, UIntElementValue);

        // Assert
        ushort[] expected = [0, 0, 0, UIntElementValue, 0, 0, 0, 0, 0, 0];

        arrayAfterWrite.Should().Equal(expected);
    }

    [Fact]
    public async Task AUDIntArrayElementRoundTripsAndResolvesToItsArraysDeclaration()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(UDIntElement, UDIntElementValue);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.UDIntArray, AllenBradleyDataType.Udint, TagAddresses.ArrayElementCount);
        var expected = new RoundTripResult(
            new ResolvedDataPoint(UDIntElement, expectedDefinition),
            UDIntElement.CreateLogixValue(UDIntElementValue));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task WritingAUDIntArrayElementLeavesTheOtherElementsAsTheyWere()
    {
        // Arrange

        // Act
        var arrayAfterWrite = await WriteOneElementOfAZeroedArrayAsync(UDIntArray, UDIntElement, UDIntElementValue);

        // Assert
        uint[] expected = [0, 0, 0, UDIntElementValue, 0, 0, 0, 0, 0, 0];

        arrayAfterWrite.Should().Equal(expected);
    }

    [Fact]
    public async Task AULIntArrayElementRoundTripsAndResolvesToItsArraysDeclaration()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(ULIntElement, ULIntElementValue);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.ULIntArray, AllenBradleyDataType.Ulint, TagAddresses.ArrayElementCount);
        var expected = new RoundTripResult(
            new ResolvedDataPoint(ULIntElement, expectedDefinition),
            ULIntElement.CreateLogixValue(ULIntElementValue));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task WritingAULIntArrayElementLeavesTheOtherElementsAsTheyWere()
    {
        // Arrange

        // Act
        var arrayAfterWrite = await WriteOneElementOfAZeroedArrayAsync(ULIntArray, ULIntElement, ULIntElementValue);

        // Assert
        ulong[] expected = [0, 0, 0, ULIntElementValue, 0, 0, 0, 0, 0, 0];

        arrayAfterWrite.Should().Equal(expected);
    }
}
