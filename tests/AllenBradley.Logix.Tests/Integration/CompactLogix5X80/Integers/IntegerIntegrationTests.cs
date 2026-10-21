using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Integers;

/// <summary>
/// Each type is driven to both ends of its range and through the middle. Width is all that distinguishes
/// these four, and a decode that reads the right bytes at the wrong width agrees with a narrow test value.
/// </summary>
public abstract class IntegerIntegrationTests(
    TestControllerFixture fixture,
    ITestOutputHelper output,
    TagScope scope)
    : CompactLogix5X80IntegrationTestBase(fixture, output, scope)
{
    [Theory]
    [InlineData((sbyte)0)]
    [InlineData((sbyte)42)]
    [InlineData((sbyte)-1)]
    [InlineData(sbyte.MinValue)]
    [InlineData(sbyte.MaxValue)]
    public async Task AnSIntReadsBackAsWritten(sbyte valueToWrite)
    {
        // Arrange
        var dataPoint = new SIntDataPoint(TagPath.Parse(Tags.SInt), DefaultPollFrequency, NoChannels);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedDeclaredType = new DeclaredType(dataPoint.TagAddress, AllenBradleyDataType.Sint, MaxLength: null,
            DimensionCount.Scalar, new ElementCount(1));
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, expectedDeclaredType),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData((short)0)]
    [InlineData((short)4242)]
    [InlineData((short)-1)]
    [InlineData(short.MinValue)]
    [InlineData(short.MaxValue)]
    public async Task AnIntReadsBackAsWritten(short valueToWrite)
    {
        // Arrange
        var dataPoint = new IntDataPoint(TagPath.Parse(Tags.Int), DefaultPollFrequency, NoChannels);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedDeclaredType = new DeclaredType(dataPoint.TagAddress, AllenBradleyDataType.Int, MaxLength: null,
            DimensionCount.Scalar, new ElementCount(1));
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, expectedDeclaredType),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(123456)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public async Task AdIntReadsBackAsWritten(int valueToWrite)
    {
        // Arrange
        var dataPoint = new DIntDataPoint(TagPath.Parse(Tags.DInt), DefaultPollFrequency, NoChannels);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedDeclaredType = new DeclaredType(dataPoint.TagAddress, AllenBradleyDataType.Dint, MaxLength: null,
            DimensionCount.Scalar, new ElementCount(1));
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, expectedDeclaredType),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(1234567890123L)]
    [InlineData(-1L)]
    [InlineData(long.MinValue)]
    [InlineData(long.MaxValue)]
    public async Task AnLIntReadsBackAsWritten(long valueToWrite)
    {
        // Arrange
        var dataPoint = new LIntDataPoint(TagPath.Parse(Tags.LInt), DefaultPollFrequency, NoChannels);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedDeclaredType = new DeclaredType(dataPoint.TagAddress, AllenBradleyDataType.Lint, MaxLength: null,
            DimensionCount.Scalar, new ElementCount(1));
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, expectedDeclaredType),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }

    public sealed class ControllerScope(TestControllerFixture fixture, ITestOutputHelper output)
        : IntegerIntegrationTests(fixture, output, TagScope.Controller);

    public sealed class ProgramScope(TestControllerFixture fixture, ITestOutputHelper output)
        : IntegerIntegrationTests(fixture, output, TagScope.Program);
}
