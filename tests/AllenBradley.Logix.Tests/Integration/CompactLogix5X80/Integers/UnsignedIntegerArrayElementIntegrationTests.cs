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
/// value is above the signed maximum, so a signed misreading of the same bytes gets it wrong.
/// </summary>
public abstract class UnsignedIntegerArrayElementIntegrationTests(
    TestControllerFixture fixture,
    ITestOutputHelper output,
    TagScope scope)
    : CompactLogix5X80IntegrationTestBase(fixture, output, scope)
{
    private const int ElementIndex = 3;
    private const byte UsIntElementValue = 200;
    private const ushort UIntElementValue = 50000;
    private const uint UdIntElementValue = 4000000000;
    private const ulong UlIntElementValue = 18446744073709551614;

    private USIntArrayDataPoint UsIntArray => new(
        TagPath.Parse(Tags.UsIntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private USIntDataPoint UsIntElement => new(
        TagPath.Parse(TagAddresses.ArrayElement(Tags.UsIntArray, ElementIndex)),
        DefaultPollFrequency,
        NoChannels);

    private UIntArrayDataPoint UIntArray => new(
        TagPath.Parse(Tags.UIntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private UIntDataPoint UIntElement => new(
        TagPath.Parse(TagAddresses.ArrayElement(Tags.UIntArray, ElementIndex)),
        DefaultPollFrequency,
        NoChannels);

    private UDIntArrayDataPoint UdIntArray => new(
        TagPath.Parse(Tags.UdIntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private UDIntDataPoint UdIntElement => new(
        TagPath.Parse(TagAddresses.ArrayElement(Tags.UdIntArray, ElementIndex)),
        DefaultPollFrequency,
        NoChannels);

    private ULIntArrayDataPoint UlIntArray => new(
        TagPath.Parse(Tags.UlIntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private ULIntDataPoint UlIntElement => new(
        TagPath.Parse(TagAddresses.ArrayElement(Tags.UlIntArray, ElementIndex)),
        DefaultPollFrequency,
        NoChannels);

    [Fact]
    public async Task AusIntElementReadsBackAsWritten()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(UsIntElement, UsIntElementValue);

        // Assert
        var expectedDeclaredType = new DeclaredType(UsIntElement.TagAddress, AllenBradleyDataType.Usint,
            MaxLength: null, DimensionCount.Scalar, new ElementCount(1));
        var expected = new RoundTripResult(
            new ResolvedDataPoint(UsIntElement, expectedDeclaredType),
            UsIntElement.CreateLogixValue(UsIntElementValue));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task WritingAusIntElementKeepsTheOthers()
    {
        // Arrange

        // Act
        var arrayAfterWrite = await WriteOneElementOfAZeroedArrayAsync(UsIntArray, UsIntElement, UsIntElementValue);

        // Assert
        byte[] expected = [0, 0, 0, UsIntElementValue, 0, 0, 0, 0, 0, 0];

        arrayAfterWrite.Should().Equal(expected);
    }

    [Fact]
    public async Task AuIntElementReadsBackAsWritten()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(UIntElement, UIntElementValue);

        // Assert
        var expectedDeclaredType = new DeclaredType(UIntElement.TagAddress, AllenBradleyDataType.Uint,
            MaxLength: null, DimensionCount.Scalar, new ElementCount(1));
        var expected = new RoundTripResult(
            new ResolvedDataPoint(UIntElement, expectedDeclaredType),
            UIntElement.CreateLogixValue(UIntElementValue));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task WritingAuIntElementKeepsTheOthers()
    {
        // Arrange

        // Act
        var arrayAfterWrite = await WriteOneElementOfAZeroedArrayAsync(UIntArray, UIntElement, UIntElementValue);

        // Assert
        ushort[] expected = [0, 0, 0, UIntElementValue, 0, 0, 0, 0, 0, 0];

        arrayAfterWrite.Should().Equal(expected);
    }

    [Fact]
    public async Task AudIntElementReadsBackAsWritten()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(UdIntElement, UdIntElementValue);

        // Assert
        var expectedDeclaredType = new DeclaredType(UdIntElement.TagAddress, AllenBradleyDataType.Udint,
            MaxLength: null, DimensionCount.Scalar, new ElementCount(1));
        var expected = new RoundTripResult(
            new ResolvedDataPoint(UdIntElement, expectedDeclaredType),
            UdIntElement.CreateLogixValue(UdIntElementValue));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task WritingAudIntElementKeepsTheOthers()
    {
        // Arrange

        // Act
        var arrayAfterWrite = await WriteOneElementOfAZeroedArrayAsync(UdIntArray, UdIntElement, UdIntElementValue);

        // Assert
        uint[] expected = [0, 0, 0, UdIntElementValue, 0, 0, 0, 0, 0, 0];

        arrayAfterWrite.Should().Equal(expected);
    }

    [Fact]
    public async Task AulIntElementReadsBackAsWritten()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(UlIntElement, UlIntElementValue);

        // Assert
        var expectedDeclaredType = new DeclaredType(UlIntElement.TagAddress, AllenBradleyDataType.Ulint,
            MaxLength: null, DimensionCount.Scalar, new ElementCount(1));
        var expected = new RoundTripResult(
            new ResolvedDataPoint(UlIntElement, expectedDeclaredType),
            UlIntElement.CreateLogixValue(UlIntElementValue));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task WritingAulIntElementKeepsTheOthers()
    {
        // Arrange

        // Act
        var arrayAfterWrite = await WriteOneElementOfAZeroedArrayAsync(UlIntArray, UlIntElement, UlIntElementValue);

        // Assert
        ulong[] expected = [0, 0, 0, UlIntElementValue, 0, 0, 0, 0, 0, 0];

        arrayAfterWrite.Should().Equal(expected);
    }

    public sealed class ControllerScope(TestControllerFixture fixture, ITestOutputHelper output)
        : UnsignedIntegerArrayElementIntegrationTests(fixture, output, TagScope.Controller);

    public sealed class ProgramScope(TestControllerFixture fixture, ITestOutputHelper output)
        : UnsignedIntegerArrayElementIntegrationTests(fixture, output, TagScope.Program);
}
