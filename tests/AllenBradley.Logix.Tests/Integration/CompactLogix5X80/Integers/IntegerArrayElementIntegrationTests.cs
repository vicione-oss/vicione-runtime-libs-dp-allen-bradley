using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Integers;

/// <summary>
/// The symbol table names no element, so an element resolves to its array's declaration. Every element
/// value is asymmetric in its bytes, so a mis-sized element cannot agree with the read-back.
/// </summary>
public abstract class IntegerArrayElementIntegrationTests(
    TestControllerFixture fixture,
    ITestOutputHelper output,
    TagScope scope)
    : CompactLogix5X80IntegrationTestBase(fixture, output, scope)
{
    private const int ElementIndex = 3;
    private const sbyte SIntElementValue = -56;
    private const short IntElementValue = 4711;
    private const int DIntElementValue = 123456;
    private const long LIntElementValue = 1234567890123;

    private SIntArrayDataPoint SIntArray => new(
        TagPath.Parse(Tags.SIntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private SIntDataPoint SIntElement => new(
        TagPath.Parse(TagAddresses.ArrayElement(Tags.SIntArray, ElementIndex)),
        DefaultPollFrequency,
        NoChannels);

    private IntArrayDataPoint IntArray => new(
        TagPath.Parse(Tags.IntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private IntDataPoint IntElement => new(
        TagPath.Parse(TagAddresses.ArrayElement(Tags.IntArray, ElementIndex)),
        DefaultPollFrequency,
        NoChannels);

    private DIntArrayDataPoint DIntArray => new(
        TagPath.Parse(Tags.DIntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private DIntDataPoint DIntElement => new(
        TagPath.Parse(TagAddresses.ArrayElement(Tags.DIntArray, ElementIndex)),
        DefaultPollFrequency,
        NoChannels);

    private LIntArrayDataPoint LIntArray => new(
        TagPath.Parse(Tags.LIntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private LIntDataPoint LIntElement => new(
        TagPath.Parse(TagAddresses.ArrayElement(Tags.LIntArray, ElementIndex)),
        DefaultPollFrequency,
        NoChannels);

    [Fact]
    public async Task AnSIntElementReadsBackAsWritten()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(SIntElement, SIntElementValue);

        // Assert
        var expectedDeclaredType = new DeclaredType(SIntElement.TagAddress, AllenBradleyDataType.Sint,
            MaxLength: null, DimensionCount.Scalar, new ElementCount(1));
        var expected = new RoundTripResult(
            new ResolvedDataPoint(SIntElement, expectedDeclaredType),
            SIntElement.CreateLogixValue(SIntElementValue));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task WritingAnSIntElementKeepsTheOthers()
    {
        // Arrange

        // Act
        var arrayAfterWrite = await WriteOneElementOfAZeroedArrayAsync(SIntArray, SIntElement, SIntElementValue);

        // Assert
        sbyte[] expected = [0, 0, 0, SIntElementValue, 0, 0, 0, 0, 0, 0];

        arrayAfterWrite.Should().Equal(expected);
    }

    [Fact]
    public async Task AnIntElementReadsBackAsWritten()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(IntElement, IntElementValue);

        // Assert
        var expectedDeclaredType = new DeclaredType(IntElement.TagAddress, AllenBradleyDataType.Int, MaxLength: null,
            DimensionCount.Scalar, new ElementCount(1));
        var expected = new RoundTripResult(
            new ResolvedDataPoint(IntElement, expectedDeclaredType),
            IntElement.CreateLogixValue(IntElementValue));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task WritingAnIntElementKeepsTheOthers()
    {
        // Arrange

        // Act
        var arrayAfterWrite = await WriteOneElementOfAZeroedArrayAsync(IntArray, IntElement, IntElementValue);

        // Assert
        short[] expected = [0, 0, 0, IntElementValue, 0, 0, 0, 0, 0, 0];

        arrayAfterWrite.Should().Equal(expected);
    }

    [Fact]
    public async Task AdIntElementReadsBackAsWritten()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(DIntElement, DIntElementValue);

        // Assert
        var expectedDeclaredType = new DeclaredType(DIntElement.TagAddress, AllenBradleyDataType.Dint,
            MaxLength: null, DimensionCount.Scalar, new ElementCount(1));
        var expected = new RoundTripResult(
            new ResolvedDataPoint(DIntElement, expectedDeclaredType),
            DIntElement.CreateLogixValue(DIntElementValue));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task WritingAdIntElementKeepsTheOthers()
    {
        // Arrange

        // Act
        var arrayAfterWrite = await WriteOneElementOfAZeroedArrayAsync(DIntArray, DIntElement, DIntElementValue);

        // Assert
        int[] expected = [0, 0, 0, DIntElementValue, 0, 0, 0, 0, 0, 0];

        arrayAfterWrite.Should().Equal(expected);
    }

    [Fact]
    public async Task AnLIntElementReadsBackAsWritten()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(LIntElement, LIntElementValue);

        // Assert
        var expectedDeclaredType = new DeclaredType(LIntElement.TagAddress, AllenBradleyDataType.Lint,
            MaxLength: null, DimensionCount.Scalar, new ElementCount(1));
        var expected = new RoundTripResult(
            new ResolvedDataPoint(LIntElement, expectedDeclaredType),
            LIntElement.CreateLogixValue(LIntElementValue));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task WritingAnLIntElementKeepsTheOthers()
    {
        // Arrange

        // Act
        var arrayAfterWrite = await WriteOneElementOfAZeroedArrayAsync(LIntArray, LIntElement, LIntElementValue);

        // Assert
        long[] expected = [0, 0, 0, LIntElementValue, 0, 0, 0, 0, 0, 0];

        arrayAfterWrite.Should().Equal(expected);
    }

    public sealed class ControllerScope(TestControllerFixture fixture, ITestOutputHelper output)
        : IntegerArrayElementIntegrationTests(fixture, output, TagScope.Controller);

    public sealed class ProgramScope(TestControllerFixture fixture, ITestOutputHelper output)
        : IntegerArrayElementIntegrationTests(fixture, output, TagScope.Program);
}
