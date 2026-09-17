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
public sealed class USIntArrayConverterTests
{
    private const int DeclaredElementCount = 10;

    private static readonly IDataPointConverter Converter = new USIntArrayConverter();

    private static readonly USIntArrayDataPoint Pressures = new(
        TagPath.Parse("usintArray1"), DefaultPollFrequency, NoChannels, new ElementCount(DeclaredElementCount));

    // Both ends of the range and two elements above the signed maximum, with no two elements sharing a bit
    // pattern, so neither an element read at the wrong offset nor one read as a SINT can agree.
    private static readonly byte[] TenStoredUSInts =
        [0x00, 0x01, 0x2A, 0x80, 0xC8, 0xFF, 0x0A, 0x14, 0x1E, 0x28];

    private static readonly byte[] TenPressures =
        [0, 1, 42, 128, 200, byte.MaxValue, 10, 20, 30, 40];

    [Fact]
    public void TheStoredBytesDecodeToTheDeclaredNumberOfBytesInIndexOrder()
    {
        // Arrange

        // Act
        var decoded = Converter.Decode(Pressures, TenStoredUSInts);

        // Assert
        decoded.Value.Should().BeOfType<byte[]>()
            .Which.Should().Equal(TenPressures);
    }

    [Fact]
    public void TheDeclaredNumberOfBytesEncodesToOneByteEachInIndexOrder()
    {
        // Arrange
        var value = Pressures.CreateLogixValue(TenPressures);

        // Act
        var encoded = Converter.Encode(value);

        // Assert
        encoded.Should().Equal(TenStoredUSInts);
    }
}
