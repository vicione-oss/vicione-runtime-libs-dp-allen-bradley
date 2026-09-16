using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

/// <summary>
/// The element codec alone, at the widest stride an integer element has. What every array converter
/// shares — the length guards on both directions — is on <c>AtomicArrayDataPointConverter</c> and is
/// pinned by <see cref="IntArrayConverterTests"/>.
/// </summary>
public sealed class ULIntArrayConverterTests
{
    private const int DeclaredElementCount = 10;

    private static readonly IDataPointConverter Converter = new ULIntArrayConverter();

    private static readonly ULIntArrayDataPoint CycleCounts = new(
        TagPath.Parse("ulintArray1"), DefaultPollFrequency, NoChannels, new ElementCount(DeclaredElementCount));

    // Least significant byte first, holding both ends of the range, a value asymmetric in its bytes and two
    // above the signed maximum, which is where a ULINT and the LINT beside it disagree.
    private static readonly byte[] TenStoredULInts =
    [
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0xCB, 0x04, 0xFB, 0x71, 0x1F, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80,
        0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0x0A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x1E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    ];

    private static readonly ulong[] TenCycleCounts =
    [
        0, 1, 1234567890123, 9223372036854775808, 18446744073709551614, ulong.MaxValue, 10, 20, 30, 40,
    ];

    [Fact]
    public void TheConverterExpectsTheControllerToDeclareTheElementsULInts()
    {
        // Arrange

        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        expectedDataType.Should().Be(AllenBradleyDataType.Ulint);
    }

    [Fact]
    public void TheStoredBytesDecodeToTheDeclaredNumberOfULongsInIndexOrder()
    {
        // Arrange

        // Act
        var decoded = Converter.Decode(CycleCounts, TenStoredULInts);

        // Assert
        decoded.Value.Should().BeOfType<ulong[]>()
            .Which.Should().Equal(TenCycleCounts);
    }

    [Fact]
    public void TheDeclaredNumberOfULongsEncodesToEightBytesEachInIndexOrder()
    {
        // Arrange
        var value = CycleCounts.CreateLogixValue(TenCycleCounts);

        // Act
        var encoded = Converter.Encode(value);

        // Assert
        encoded.Should().Equal(TenStoredULInts);
    }
}
