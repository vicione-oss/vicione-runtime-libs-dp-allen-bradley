using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.FloatingPoints;

/// <summary>
/// The element codec alone. What every array converter shares — the length guards on both directions —
/// is on <c>AtomicArrayDataPointConverter</c> and is pinned by <c>IntArrayConverterTests</c>.
/// </summary>
public sealed class LRealArrayConverterTests
{
    private const int DeclaredElementCount = 10;

    private static readonly IDataPointConverter Converter = new LRealArrayConverter();

    private static readonly LRealArrayDataPoint Positions = new(
        TagPath.Parse("lrealArray1"), DefaultPollFrequency, NoChannels, new ElementCount(DeclaredElementCount));

    // Least significant byte first, holding both ends of the range, a negative, a fraction and values whose
    // eight bytes differ, so a swapped element cannot agree with the one that belongs there.
    private static readonly byte[] TenStoredLReals =
    [
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0xBF, 0x18, 0x2D, 0x44, 0x54, 0xFB, 0x21, 0x09, 0x40,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xEF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xEF, 0x7F,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x25, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x34, 0xC0,
        0x00, 0x00, 0x00, 0x00, 0x00, 0xC0, 0x3E, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x44, 0x40,
    ];

    private static readonly double[] TenPositions =
    [
        0d, 1d, -1d, 3.141592653589793d, double.MinValue, double.MaxValue, 10.5d, -20.25d, 30.75d, 40d,
    ];

    [Fact]
    public void TheStoredBytesDecodeToTheDeclaredNumberOfDoublesInIndexOrder()
    {
        // Arrange

        // Act
        var decoded = Converter.Decode(Positions, TenStoredLReals);

        // Assert
        decoded.Value.Should().BeOfType<double[]>()
            .Which.Should().Equal(TenPositions);
    }

    [Fact]
    public void TheDeclaredNumberOfDoublesEncodesToEightBytesEachInIndexOrder()
    {
        // Arrange
        var value = Positions.CreateLogixValue(TenPositions);

        // Act
        var encoded = Converter.Encode(value);

        // Assert
        encoded.Should().Equal(TenStoredLReals);
    }
}
