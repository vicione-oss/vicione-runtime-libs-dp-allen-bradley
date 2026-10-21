using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Integers;

/// <summary>
/// Each element set holds zero, the maximum, and two values above the signed maximum. A signed misreading
/// of the same bytes gets those two wrong.
/// </summary>
public abstract class UnsignedIntegerArrayIntegrationTests(
    TestControllerFixture fixture,
    ITestOutputHelper output,
    TagScope scope)
    : CompactLogix5X80IntegrationTestBase(fixture, output, scope)
{
    [Fact]
    public async Task AusIntArrayReadsBackAsWritten()
    {
        // Arrange
        USIntArrayDataPoint pressures = new(
            TagPath.Parse(Tags.UsIntArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.ArrayElementCount);
        byte[] tenPressures = [0, 1, 42, 128, 200, byte.MaxValue, 10, 20, 30, 40];

        // Act
        var roundTripResult = await RoundTripAsync(pressures, tenPressures);

        // Assert
        var expectedDeclaredType = new DeclaredType(pressures.TagAddress, AllenBradleyDataType.Usint, MaxLength: null,
            DimensionCount.OneDimensional, TagAddresses.ArrayElementCount);
        var expectedResolved = new ResolvedDataPoint(pressures, expectedDeclaredType);

        roundTripResult.Resolved.Should().Be(expectedResolved);
        roundTripResult.ReadValue.Value.Should().BeOfType<byte[]>().Which.Should().Equal(tenPressures);
    }

    [Fact]
    public async Task AuIntArrayReadsBackAsWritten()
    {
        // Arrange
        UIntArrayDataPoint speeds = new(
            TagPath.Parse(Tags.UIntArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.ArrayElementCount);
        ushort[] tenSpeeds = [0, 1, 4242, 32768, 50000, ushort.MaxValue, 10, 20, 30, 40];

        // Act
        var roundTripResult = await RoundTripAsync(speeds, tenSpeeds);

        // Assert
        var expectedDeclaredType = new DeclaredType(speeds.TagAddress, AllenBradleyDataType.Uint, MaxLength: null,
            DimensionCount.OneDimensional, TagAddresses.ArrayElementCount);
        var expectedResolved = new ResolvedDataPoint(speeds, expectedDeclaredType);

        roundTripResult.Resolved.Should().Be(expectedResolved);
        roundTripResult.ReadValue.Value.Should().BeOfType<ushort[]>().Which.Should().Equal(tenSpeeds);
    }

    [Fact]
    public async Task AudIntArrayReadsBackAsWritten()
    {
        // Arrange
        UDIntArrayDataPoint runtimes = new(
            TagPath.Parse(Tags.UdIntArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.ArrayElementCount);
        uint[] tenRuntimes = [0, 1, 123456, 2147483648, 4000000000, uint.MaxValue, 10, 20, 30, 40];

        // Act
        var roundTripResult = await RoundTripAsync(runtimes, tenRuntimes);

        // Assert
        var expectedDeclaredType = new DeclaredType(runtimes.TagAddress, AllenBradleyDataType.Udint, MaxLength: null,
            DimensionCount.OneDimensional, TagAddresses.ArrayElementCount);
        var expectedResolved = new ResolvedDataPoint(runtimes, expectedDeclaredType);

        roundTripResult.Resolved.Should().Be(expectedResolved);
        roundTripResult.ReadValue.Value.Should().BeOfType<uint[]>().Which.Should().Equal(tenRuntimes);
    }

    [Fact]
    public async Task AulIntArrayReadsBackAsWritten()
    {
        // Arrange
        ULIntArrayDataPoint cycleCounts = new(
            TagPath.Parse(Tags.UlIntArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.ArrayElementCount);
        ulong[] tenCycleCounts =
            [0, 1, 1234567890123, 9223372036854775808, 18446744073709551614, ulong.MaxValue, 10, 20, 30, 40];

        // Act
        var roundTripResult = await RoundTripAsync(cycleCounts, tenCycleCounts);

        // Assert
        var expectedDeclaredType = new DeclaredType(cycleCounts.TagAddress, AllenBradleyDataType.Ulint,
            MaxLength: null, DimensionCount.OneDimensional, TagAddresses.ArrayElementCount);
        var expectedResolved = new ResolvedDataPoint(cycleCounts, expectedDeclaredType);

        roundTripResult.Resolved.Should().Be(expectedResolved);
        roundTripResult.ReadValue.Value.Should().BeOfType<ulong[]>().Which.Should().Equal(tenCycleCounts);
    }

    public sealed class ControllerScope(TestControllerFixture fixture, ITestOutputHelper output)
        : UnsignedIntegerArrayIntegrationTests(fixture, output, TagScope.Controller);

    public sealed class ProgramScope(TestControllerFixture fixture, ITestOutputHelper output)
        : UnsignedIntegerArrayIntegrationTests(fixture, output, TagScope.Program);
}
