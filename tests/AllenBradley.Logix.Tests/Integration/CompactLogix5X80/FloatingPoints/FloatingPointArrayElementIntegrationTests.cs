using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.FloatingPoints;

/// <summary>
/// The symbol table names no element, so an element resolves to its array's declaration. Every element value
/// has a full mantissa, so a mis-sized element cannot agree with the read-back.
/// </summary>
public abstract class FloatingPointArrayElementIntegrationTests(
    TestControllerFixture fixture,
    ITestOutputHelper output,
    TagScope scope)
    : CompactLogix5X80IntegrationTestBase(fixture, output, scope)
{
    private const int ElementIndex = 3;
    private const float RealElementValue = 3.14159f;
    private const double LRealElementValue = 3.141592653589793d;

    private RealArrayDataPoint RealArray => new(
        TagPath.Parse(Tags.RealArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private RealDataPoint RealElement => new(
        TagPath.Parse(TagAddresses.ArrayElement(Tags.RealArray, ElementIndex)),
        DefaultPollFrequency,
        NoChannels);

    private LRealArrayDataPoint LRealArray => new(
        TagPath.Parse(Tags.LRealArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private LRealDataPoint LRealElement => new(
        TagPath.Parse(TagAddresses.ArrayElement(Tags.LRealArray, ElementIndex)),
        DefaultPollFrequency,
        NoChannels);

    [Fact]
    public async Task ARealElementReadsBackAsWritten()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(RealElement, RealElementValue);

        // Assert
        var expectedDeclaredType = new DeclaredType(RealElement.TagAddress, AllenBradleyDataType.Real,
            MaxLength: null, DimensionCount.Scalar, new ElementCount(1));
        var expected = new RoundTripResult(
            new ResolvedDataPoint(RealElement, expectedDeclaredType),
            RealElement.CreateLogixValue(RealElementValue));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task WritingARealElementKeepsTheOthers()
    {
        // Arrange

        // Act
        var arrayAfterWrite = await WriteOneElementOfAZeroedArrayAsync(RealArray, RealElement, RealElementValue);

        // Assert
        float[] expected = [0f, 0f, 0f, RealElementValue, 0f, 0f, 0f, 0f, 0f, 0f];

        arrayAfterWrite.Should().Equal(expected);
    }

    [Fact]
    public async Task AnLRealElementReadsBackAsWritten()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(LRealElement, LRealElementValue);

        // Assert
        var expectedDeclaredType = new DeclaredType(LRealElement.TagAddress, AllenBradleyDataType.Lreal,
            MaxLength: null, DimensionCount.Scalar, new ElementCount(1));
        var expected = new RoundTripResult(
            new ResolvedDataPoint(LRealElement, expectedDeclaredType),
            LRealElement.CreateLogixValue(LRealElementValue));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task WritingAnLRealElementKeepsTheOthers()
    {
        // Arrange

        // Act
        var arrayAfterWrite = await WriteOneElementOfAZeroedArrayAsync(LRealArray, LRealElement, LRealElementValue);

        // Assert
        double[] expected = [0d, 0d, 0d, LRealElementValue, 0d, 0d, 0d, 0d, 0d, 0d];

        arrayAfterWrite.Should().Equal(expected);
    }

    public sealed class ControllerScope(TestControllerFixture fixture, ITestOutputHelper output)
        : FloatingPointArrayElementIntegrationTests(fixture, output, TagScope.Controller);

    public sealed class ProgramScope(TestControllerFixture fixture, ITestOutputHelper output)
        : FloatingPointArrayElementIntegrationTests(fixture, output, TagScope.Program);
}
