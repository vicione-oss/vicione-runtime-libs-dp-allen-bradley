using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Booleans;

/// <summary>
/// Both states are tested, because a <c>BOOL</c> that reads <c>true</c> whatever was written would pass a
/// one-value test.
/// </summary>
public abstract class BooleanIntegrationTests(
    TestControllerFixture fixture,
    ITestOutputHelper output,
    TagScope scope)
    : CompactLogix5X80IntegrationTestBase(fixture, output, scope)
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ABoolReadsBackAsWritten(bool valueToWrite)
    {
        // Arrange
        var dataPoint = new BoolDataPoint(TagPath.Parse(Tags.Bool), DefaultPollFrequency, NoChannels);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedDeclaredType = new DeclaredType(dataPoint.TagAddress, AllenBradleyDataType.Bool, MaxLength: null,
            DimensionCount.Scalar, new ElementCount(1));
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, expectedDeclaredType),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }

    public sealed class ControllerScope(TestControllerFixture fixture, ITestOutputHelper output)
        : BooleanIntegrationTests(fixture, output, TagScope.Controller);

    public sealed class ProgramScope(TestControllerFixture fixture, ITestOutputHelper output)
        : BooleanIntegrationTests(fixture, output, TagScope.Program);
}
