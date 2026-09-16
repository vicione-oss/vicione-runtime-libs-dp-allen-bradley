using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Integers;

/// <summary>
/// Every element set holds zero, the maximum, and two values above the signed maximum, which are what a
/// signed misreading of the same bytes gets wrong.
/// </summary>
public sealed class UnsignedIntegerArrayIntegrationTests(ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(output)
{
    [Fact]
    public async Task AUSIntArrayIsWrittenWholeAndReadsBackAsTheElementsThatWentIn()
    {
        // Arrange
        USIntArrayDataPoint pressures = new(
            TagPath.Parse(TagAddresses.USIntArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.ArrayElementCount);
        byte[] tenPressures = [0, 1, 42, 128, 200, byte.MaxValue, 10, 20, 30, 40];

        // Act
        var roundTripResult = await RoundTripAsync(pressures, tenPressures);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.USIntArray, AllenBradleyDataType.Usint, TagAddresses.ArrayElementCount);
        var expectedResolved = new ResolvedDataPoint(pressures, expectedDefinition);

        roundTripResult.Resolved.Should().Be(expectedResolved);
        roundTripResult.ReadValue.Value.Should().BeOfType<byte[]>().Which.Should().Equal(tenPressures);
    }

    [Fact]
    public async Task AUIntArrayIsWrittenWholeAndReadsBackAsTheElementsThatWentIn()
    {
        // Arrange
        UIntArrayDataPoint speeds = new(
            TagPath.Parse(TagAddresses.UIntArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.ArrayElementCount);
        ushort[] tenSpeeds = [0, 1, 4242, 32768, 50000, ushort.MaxValue, 10, 20, 30, 40];

        // Act
        var roundTripResult = await RoundTripAsync(speeds, tenSpeeds);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.UIntArray, AllenBradleyDataType.Uint, TagAddresses.ArrayElementCount);
        var expectedResolved = new ResolvedDataPoint(speeds, expectedDefinition);

        roundTripResult.Resolved.Should().Be(expectedResolved);
        roundTripResult.ReadValue.Value.Should().BeOfType<ushort[]>().Which.Should().Equal(tenSpeeds);
    }

    [Fact]
    public async Task AUDIntArrayIsWrittenWholeAndReadsBackAsTheElementsThatWentIn()
    {
        // Arrange
        UDIntArrayDataPoint runtimes = new(
            TagPath.Parse(TagAddresses.UDIntArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.ArrayElementCount);
        uint[] tenRuntimes = [0, 1, 123456, 2147483648, 4000000000, uint.MaxValue, 10, 20, 30, 40];

        // Act
        var roundTripResult = await RoundTripAsync(runtimes, tenRuntimes);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.UDIntArray, AllenBradleyDataType.Udint, TagAddresses.ArrayElementCount);
        var expectedResolved = new ResolvedDataPoint(runtimes, expectedDefinition);

        roundTripResult.Resolved.Should().Be(expectedResolved);
        roundTripResult.ReadValue.Value.Should().BeOfType<uint[]>().Which.Should().Equal(tenRuntimes);
    }

    [Fact]
    public async Task AULIntArrayIsWrittenWholeAndReadsBackAsTheElementsThatWentIn()
    {
        // Arrange
        ULIntArrayDataPoint cycleCounts = new(
            TagPath.Parse(TagAddresses.ULIntArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.ArrayElementCount);
        ulong[] tenCycleCounts =
            [0, 1, 1234567890123, 9223372036854775808, 18446744073709551614, ulong.MaxValue, 10, 20, 30, 40];

        // Act
        var roundTripResult = await RoundTripAsync(cycleCounts, tenCycleCounts);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.ULIntArray, AllenBradleyDataType.Ulint, TagAddresses.ArrayElementCount);
        var expectedResolved = new ResolvedDataPoint(cycleCounts, expectedDefinition);

        roundTripResult.Resolved.Should().Be(expectedResolved);
        roundTripResult.ReadValue.Value.Should().BeOfType<ulong[]>().Which.Should().Equal(tenCycleCounts);
    }
}
