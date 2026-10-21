using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Udts;

/// <summary>
/// The members of the <c>UDT_Inner</c> nested in <see cref="TagAddresses.Udt"/>, two templates deep. The
/// lookup reads the outer template for the nested member and the inner one for what is under it.
/// </summary>
public abstract class NestedUdtIntegrationTests(
    TestControllerFixture fixture,
    ITestOutputHelper output,
    TagScope scope)
    : CompactLogix5X80IntegrationTestBase(fixture, output, scope)
{
    [Fact]
    public async Task ARealMemberReadsBackAsWritten()
    {
        // Arrange
        var dataPoint = new RealDataPoint(TagPath.Parse(Tags.Udt.Inner.Real), DefaultPollFrequency, NoChannels);
        const float valueToWrite = 123.456f;

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expected = new RoundTripResult(
            new ResolvedDataPoint(dataPoint,
                new DeclaredType(dataPoint.TagAddress, AllenBradleyDataType.Real, MaxLength: null,
                    DimensionCount.Scalar, new ElementCount(1))),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expected);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ABoolMemberReadsBackAsWritten(bool valueToWrite)
    {
        // Arrange
        var dataPoint = new BoolDataPoint(TagPath.Parse(Tags.Udt.Inner.Bool), DefaultPollFrequency, NoChannels);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expected = new RoundTripResult(
            new ResolvedDataPoint(dataPoint,
                new DeclaredType(dataPoint.TagAddress, AllenBradleyDataType.Bool, MaxLength: null,
                    DimensionCount.Scalar, new ElementCount(1))),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task AnIntElementReadsBackAsWritten()
    {
        // Arrange
        var element = new IntDataPoint(
            TagPath.Parse(TagAddresses.ArrayElement(Tags.Udt.Inner.IntArray, 3)), DefaultPollFrequency, NoChannels);
        const short valueToWrite = 4711;

        // Act
        var roundTripResult = await RoundTripAsync(element, valueToWrite);

        // Assert
        var expected = new RoundTripResult(
            new ResolvedDataPoint(element,
                new DeclaredType(element.TagAddress, AllenBradleyDataType.Int, MaxLength: null, DimensionCount.Scalar,
                    new ElementCount(1))),
            element.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task AStringMemberReadsBackAsWritten()
    {
        // Arrange
        var dataPoint = new StringDataPoint(
            TagPath.Parse(Tags.Udt.Inner.String), DefaultPollFrequency, NoChannels, TagAddresses.StringCapacity);
        const string valueToWrite = "Nested";

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expected = new RoundTripResult(
            new ResolvedDataPoint(dataPoint,
                new DeclaredType(dataPoint.TagAddress, AllenBradleyDataType.String, TagAddresses.StringCapacity,
                    DimensionCount.Scalar, new ElementCount(1))),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expected);
    }

    public sealed class ControllerScope(TestControllerFixture fixture, ITestOutputHelper output)
        : NestedUdtIntegrationTests(fixture, output, TagScope.Controller);

    public sealed class ProgramScope(TestControllerFixture fixture, ITestOutputHelper output)
        : NestedUdtIntegrationTests(fixture, output, TagScope.Program);
}
