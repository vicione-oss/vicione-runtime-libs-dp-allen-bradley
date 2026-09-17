using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

/// <summary>
/// The element codec alone, at the widest stride an element has. What every array converter shares — the
/// length guards on both directions — is on <c>AtomicArrayDataPointConverter</c> and is pinned by
/// <see cref="IntArrayConverterTests"/>.
/// </summary>
public sealed class LIntArrayConverterTests
{
    private const int DeclaredElementCount = 10;

    private static readonly IDataPointConverter Converter = new LIntArrayConverter();

    private static readonly LIntArrayDataPoint Timestamps = new(
        TagPath.Parse("lintArray1"), DefaultPollFrequency, NoChannels, new ElementCount(DeclaredElementCount));

    // Least significant byte first, holding both ends of the range and one value asymmetric in its bytes.
    private static readonly byte[] TenStoredLInts =
    [
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xCB, 0x04, 0xFB, 0x71, 0x1F, 0x01, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F,
        0x0A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x1E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    ];

    private static readonly long[] TenTimestamps =
        [0, 1, -1, 1234567890123, long.MinValue, long.MaxValue, 10, 20, 30, 40];

    [Fact]
    public void TheStoredBytesDecodeToTheDeclaredNumberOfLongsInIndexOrder()
    {
        // Arrange

        // Act
        var decoded = Converter.Decode(Timestamps, TenStoredLInts);

        // Assert
        decoded.Value.Should().BeOfType<long[]>()
            .Which.Should().Equal(TenTimestamps);
    }

    [Fact]
    public void TheDeclaredNumberOfLongsEncodesToEightBytesEachInIndexOrder()
    {
        // Arrange
        var value = Timestamps.CreateLogixValue(TenTimestamps);

        // Act
        var encoded = Converter.Encode(value);

        // Assert
        encoded.Should().Equal(TenStoredLInts);
    }
}
