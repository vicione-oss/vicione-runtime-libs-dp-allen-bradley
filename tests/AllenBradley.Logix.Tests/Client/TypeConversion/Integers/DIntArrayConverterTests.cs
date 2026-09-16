using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

/// <summary>
/// The element codec alone. What every array converter shares — the length guards on both directions —
/// is on <c>AtomicArrayDataPointConverter</c> and is pinned by <see cref="IntArrayConverterTests"/>.
/// </summary>
public sealed class DIntArrayConverterTests
{
    private const int DeclaredElementCount = 10;

    private static readonly IDataPointConverter Converter = new DIntArrayConverter();

    private static readonly DIntArrayDataPoint Totals = new(
        new TagAddress("dintArray1"), DefaultPollFrequency, NoChannels, new ElementCount(DeclaredElementCount));

    // Least significant byte first, holding both ends of the range and one value asymmetric in its bytes.
    private static readonly byte[] TenStoredDInts =
    [
        0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x40, 0xE2, 0x01, 0x00,
        0x00, 0x00, 0x00, 0x80, 0xFF, 0xFF, 0xFF, 0x7F, 0x0A, 0x00, 0x00, 0x00, 0x14, 0x00, 0x00, 0x00,
        0x1E, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00,
    ];

    private static readonly int[] TenTotals =
        [0, 1, -1, 123456, int.MinValue, int.MaxValue, 10, 20, 30, 40];

    [Fact]
    public void TheConverterExpectsTheControllerToDeclareTheElementsDInts()
    {
        // Arrange

        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        expectedDataType.Should().Be(AllenBradleyDataType.Dint);
    }

    [Fact]
    public void TheStoredBytesDecodeToTheDeclaredNumberOfIntsInIndexOrder()
    {
        // Arrange

        // Act
        var decoded = Converter.Decode(Totals, TenStoredDInts);

        // Assert
        decoded.Value.Should().BeOfType<int[]>()
            .Which.Should().Equal(TenTotals);
    }

    [Fact]
    public void TheDeclaredNumberOfIntsEncodesToFourBytesEachInIndexOrder()
    {
        // Arrange
        var value = Totals.CreateLogixValue(TenTotals);

        // Act
        var encoded = Converter.Encode(value);

        // Assert
        encoded.Should().Equal(TenStoredDInts);
    }
}
