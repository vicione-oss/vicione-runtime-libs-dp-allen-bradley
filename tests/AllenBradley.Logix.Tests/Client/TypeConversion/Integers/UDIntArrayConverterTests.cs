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
public sealed class UDIntArrayConverterTests
{
    private const int DeclaredElementCount = 10;

    private static readonly IDataPointConverter Converter = new UDIntArrayConverter();

    private static readonly UDIntArrayDataPoint Runtimes = new(
        new TagName("udintArray1"), DefaultPollFrequency, NoChannels, new ElementCount(DeclaredElementCount));

    // Least significant byte first, holding both ends of the range, a value asymmetric in its bytes and two
    // above the signed maximum, which is where a UDINT and the DINT beside it disagree.
    private static readonly byte[] TenStoredUDInts =
    [
        0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x40, 0xE2, 0x01, 0x00, 0x00, 0x00, 0x00, 0x80,
        0x00, 0x28, 0x6B, 0xEE, 0xFF, 0xFF, 0xFF, 0xFF, 0x0A, 0x00, 0x00, 0x00, 0x14, 0x00, 0x00, 0x00,
        0x1E, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00,
    ];

    private static readonly uint[] TenRuntimes =
        [0, 1, 123456, 2147483648, 4000000000, uint.MaxValue, 10, 20, 30, 40];

    [Fact]
    public void TheConverterExpectsTheControllerToDeclareTheElementsUDInts()
    {
        // Arrange

        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        expectedDataType.Should().Be(AllenBradleyDataType.Udint);
    }

    [Fact]
    public void TheStoredBytesDecodeToTheDeclaredNumberOfUIntsInIndexOrder()
    {
        // Arrange

        // Act
        var decoded = Converter.Decode(Runtimes, TenStoredUDInts);

        // Assert
        decoded.Value.Should().BeOfType<uint[]>()
            .Which.Should().Equal(TenRuntimes);
    }

    [Fact]
    public void TheDeclaredNumberOfUIntsEncodesToFourBytesEachInIndexOrder()
    {
        // Arrange
        var value = Runtimes.CreateLogixValue(TenRuntimes);

        // Act
        var encoded = Converter.Encode(value);

        // Assert
        encoded.Should().Equal(TenStoredUDInts);
    }
}
