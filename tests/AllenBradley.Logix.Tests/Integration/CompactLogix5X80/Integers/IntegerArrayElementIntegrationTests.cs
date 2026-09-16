using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Integers;

/// <summary>
/// One element of <see cref="TagAddresses.IntArray"/> as a data point of its own, addressed with a
/// subscript. The symbol table names no element, so an element resolves to its array's declaration.
/// </summary>
public sealed class IntegerArrayElementIntegrationTests(ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(output)
{
    private const int ElementIndex = 3;
    private const short ElementValue = 4711;

    private static readonly IntArrayDataPoint WholeArray = new(
        TagPath.Parse(TagAddresses.IntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private static readonly IntDataPoint Element = new(
        TagPath.Parse(TagAddresses.IntArrayElement(ElementIndex)),
        DefaultPollFrequency,
        NoChannels);

    [Fact]
    public async Task AnIntArrayElementRoundTripsAndResolvesToItsArraysDeclaration()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(Element, ElementValue);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.IntArray, AllenBradleyDataType.Int, TagAddresses.ArrayElementCount);
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(Element, expectedDefinition),
            Element.CreateLogixValue(ElementValue));

        roundTripResult.Should().Be(expectedResult);
    }

    [Fact]
    public async Task WritingAnIntArrayElementLeavesTheOtherElementsAsTheyWere()
    {
        // Arrange
        short[] tenZeros = new short[TagAddresses.ArrayElementCount.Value];
        await Client.WriteAsync([WholeArray.CreateLogixValue(tenZeros)], TestContext.Current.CancellationToken);
        var group = new LogixDataPointGroup(DefaultPollFrequency, [WholeArray]);

        // Act
        await Client.WriteAsync([Element.CreateLogixValue(ElementValue)], TestContext.Current.CancellationToken);
        var arrayAfterWrite = await Client.ReadAsync(group, TestContext.Current.CancellationToken);

        // Assert
        short[] expected = [0, 0, 0, ElementValue, 0, 0, 0, 0, 0, 0];
        arrayAfterWrite.Should().ContainSingle().Which.Value.Should().BeOfType<short[]>().Which.Should().Equal(expected);
    }
}
