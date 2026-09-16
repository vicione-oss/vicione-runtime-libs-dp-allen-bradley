using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Booleans;

/// <summary>
/// A <c>BOOL</c> array is packed 32 to a word on the controller, so the elements are chosen to catch a bit
/// read at the wrong shift or out of the wrong byte.
/// </summary>
public sealed class BooleanArrayIntegrationTests(ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(output)
{
    [Fact]
    public async Task ABoolArrayIsWrittenWholeAndReadsBackAsTheBitsThatWentIn()
    {
        // Arrange
        BoolArrayDataPoint flags = new(
            new TagAddress(TagAddresses.BoolArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.BoolArrayElementCount);
        // Both ends of every byte of the word, grouped eight to a line as the controller packs them.
        bool[] thirtyTwoFlags =
        [
            true, true, false, false, false, false, false, true,
            true, false, false, false, false, false, false, true,
            true, false, false, false, false, false, false, false,
            false, false, false, false, false, false, true, true,
        ];

        // Act
        var roundTripResult = await RoundTripAsync(flags, thirtyTwoFlags);

        // Assert
        // The controller declares the two halves of this differently from every other array: the type
        // as DWORD, which decodes to Bool, and the extent as the one word its 32 bits fill.
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.BoolArray, AllenBradleyDataType.Bool, TagAddresses.BoolArrayElementCount);
        var expectedResolved = new ResolvedDataPoint(flags, expectedDefinition);

        roundTripResult.Resolved.Should().Be(expectedResolved);
        roundTripResult.ReadValue.Value.Should().BeOfType<bool[]>().Which.Should().Equal(thirtyTwoFlags);
    }
}
