using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.FloatingPoints;

/// <summary>
/// The element codec alone. What every array converter shares — the length guards on both directions —
/// is on <c>AtomicArrayDataPointConverter</c> and is pinned by <c>IntArrayConverterTests</c>.
/// </summary>
public sealed class RealArrayConverterTests
{
    private const int DeclaredElementCount = 10;

    private static readonly IDataPointConverter Converter = new RealArrayConverter();

    private static readonly RealArrayDataPoint Temperatures = new(
        TagPath.Parse("realArray1"), DefaultPollFrequency, NoChannels, new ElementCount(DeclaredElementCount));

    // Least significant byte first, holding both ends of the range, a negative, a fraction and values
    // whose four bytes differ, so a swapped element cannot agree with the one that belongs there.
    private static readonly byte[] TenStoredReals =
    [
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0xBF, 0xD0, 0x0F, 0x49, 0x40,
        0xFF, 0xFF, 0x7F, 0xFF, 0xFF, 0xFF, 0x7F, 0x7F, 0x00, 0x00, 0x28, 0x41, 0x00, 0x00, 0xA2, 0xC1,
        0x00, 0x00, 0xF6, 0x41, 0x00, 0x00, 0x20, 0x42,
    ];

    private static readonly float[] TenTemperatures =
        [0f, 1f, -1f, 3.14159f, float.MinValue, float.MaxValue, 10.5f, -20.25f, 30.75f, 40f];

    [Fact]
    public void TheStoredBytesDecodeToTheDeclaredNumberOfFloatsInIndexOrder()
    {
        // Arrange

        // Act
        var decoded = Converter.Decode(Temperatures, TenStoredReals);

        // Assert
        decoded.Value.Should().BeOfType<float[]>()
            .Which.Should().Equal(TenTemperatures);
    }

    [Fact]
    public void TheDeclaredNumberOfFloatsEncodesToFourBytesEachInIndexOrder()
    {
        // Arrange
        var value = Temperatures.CreateLogixValue(TenTemperatures);

        // Act
        var encoded = Converter.Encode(value);

        // Assert
        encoded.Should().Equal(TenStoredReals);
    }
}
