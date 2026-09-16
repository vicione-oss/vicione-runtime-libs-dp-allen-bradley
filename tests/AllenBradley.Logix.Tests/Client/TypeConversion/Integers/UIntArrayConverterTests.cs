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
public sealed class UIntArrayConverterTests
{
    private const int DeclaredElementCount = 10;

    private static readonly IDataPointConverter Converter = new UIntArrayConverter();

    private static readonly UIntArrayDataPoint Speeds = new(
        TagPath.Parse("uintArray1"), DefaultPollFrequency, NoChannels, new ElementCount(DeclaredElementCount));

    // Least significant byte first, holding both ends of the range, a value asymmetric in its bytes and two
    // above the signed maximum, which is where a UINT and the INT beside it disagree.
    private static readonly byte[] TenStoredUInts =
    [
        0x00, 0x00, 0x01, 0x00, 0x92, 0x10, 0x00, 0x80, 0x50, 0xC3,
        0xFF, 0xFF, 0x0A, 0x00, 0x14, 0x00, 0x1E, 0x00, 0x28, 0x00,
    ];

    private static readonly ushort[] TenSpeeds =
        [0, 1, 4242, 32768, 50000, ushort.MaxValue, 10, 20, 30, 40];

    [Fact]
    public void TheConverterExpectsTheControllerToDeclareTheElementsUInts()
    {
        // Arrange

        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        expectedDataType.Should().Be(AllenBradleyDataType.Uint);
    }

    [Fact]
    public void TheStoredBytesDecodeToTheDeclaredNumberOfUShortsInIndexOrder()
    {
        // Arrange

        // Act
        var decoded = Converter.Decode(Speeds, TenStoredUInts);

        // Assert
        decoded.Value.Should().BeOfType<ushort[]>()
            .Which.Should().Equal(TenSpeeds);
    }

    [Fact]
    public void TheDeclaredNumberOfUShortsEncodesToTwoBytesEachInIndexOrder()
    {
        // Arrange
        var value = Speeds.CreateLogixValue(TenSpeeds);

        // Act
        var encoded = Converter.Encode(value);

        // Assert
        encoded.Should().Equal(TenStoredUInts);
    }
}
