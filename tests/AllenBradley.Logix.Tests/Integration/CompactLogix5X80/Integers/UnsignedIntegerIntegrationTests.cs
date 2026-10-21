using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Integers;

/// <summary>
/// The values above the signed maximum carry this suite. An unsigned type shares its width and its bytes
/// with its signed twin, so every value up to that maximum round-trips identically through the wrong codec.
/// </summary>
public abstract class UnsignedIntegerIntegrationTests(
    TestControllerFixture fixture,
    ITestOutputHelper output,
    TagScope scope)
    : CompactLogix5X80IntegrationTestBase(fixture, output, scope)
{
    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)42)]
    [InlineData((byte)128)]
    [InlineData((byte)200)]
    [InlineData(byte.MaxValue)]
    public async Task AusIntReadsBackAsWritten(byte valueToWrite)
    {
        // Arrange
        var dataPoint = new USIntDataPoint(TagPath.Parse(Tags.UsInt), DefaultPollFrequency, NoChannels);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedDeclaredType = new DeclaredType(dataPoint.TagAddress, AllenBradleyDataType.Usint, MaxLength: null,
            DimensionCount.Scalar, new ElementCount(1));
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, expectedDeclaredType),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData((ushort)0)]
    [InlineData((ushort)4242)]
    [InlineData((ushort)32768)]
    [InlineData((ushort)50000)]
    [InlineData(ushort.MaxValue)]
    public async Task AuIntReadsBackAsWritten(ushort valueToWrite)
    {
        // Arrange
        var dataPoint = new UIntDataPoint(TagPath.Parse(Tags.UInt), DefaultPollFrequency, NoChannels);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedDeclaredType = new DeclaredType(dataPoint.TagAddress, AllenBradleyDataType.Uint, MaxLength: null,
            DimensionCount.Scalar, new ElementCount(1));
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, expectedDeclaredType),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(123456u)]
    [InlineData(2147483648u)]
    [InlineData(4000000000u)]
    [InlineData(uint.MaxValue)]
    public async Task AudIntReadsBackAsWritten(uint valueToWrite)
    {
        // Arrange
        var dataPoint = new UDIntDataPoint(TagPath.Parse(Tags.UdInt), DefaultPollFrequency, NoChannels);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedDeclaredType = new DeclaredType(dataPoint.TagAddress, AllenBradleyDataType.Udint, MaxLength: null,
            DimensionCount.Scalar, new ElementCount(1));
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, expectedDeclaredType),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(0ul)]
    [InlineData(1234567890123ul)]
    [InlineData(9223372036854775808ul)]
    [InlineData(18446744073709551614ul)]
    [InlineData(ulong.MaxValue)]
    public async Task AulIntReadsBackAsWritten(ulong valueToWrite)
    {
        // Arrange
        var dataPoint = new ULIntDataPoint(TagPath.Parse(Tags.UlInt), DefaultPollFrequency, NoChannels);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedDeclaredType = new DeclaredType(dataPoint.TagAddress, AllenBradleyDataType.Ulint, MaxLength: null,
            DimensionCount.Scalar, new ElementCount(1));
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, expectedDeclaredType),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }

    public sealed class ControllerScope(TestControllerFixture fixture, ITestOutputHelper output)
        : UnsignedIntegerIntegrationTests(fixture, output, TagScope.Controller);

    public sealed class ProgramScope(TestControllerFixture fixture, ITestOutputHelper output)
        : UnsignedIntegerIntegrationTests(fixture, output, TagScope.Program);
}
