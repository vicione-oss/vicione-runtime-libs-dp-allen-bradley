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
public sealed class SIntArrayConverterTests
{
    private const int DeclaredElementCount = 10;

    private static readonly IDataPointConverter Converter = new SIntArrayConverter();

    private static readonly SIntArrayDataPoint Samples = new(
        new TagName("sintArray1"), DefaultPollFrequency, NoChannels, new ElementCount(DeclaredElementCount));

    // Both ends of the range and a zero, with no two elements sharing a bit pattern, so an element read
    // at the wrong offset cannot agree with the one that belongs there.
    private static readonly byte[] TenStoredSInts =
        [0x00, 0x01, 0xFF, 0x2A, 0x80, 0x7F, 0x0A, 0x14, 0x1E, 0x28];

    private static readonly sbyte[] TenSamples =
        [0, 1, -1, 42, sbyte.MinValue, sbyte.MaxValue, 10, 20, 30, 40];

    [Fact]
    public void TheConverterExpectsTheControllerToDeclareTheElementsSInts()
    {
        // Arrange

        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        expectedDataType.Should().Be(AllenBradleyDataType.Sint);
    }

    [Fact]
    public void TheStoredBytesDecodeToTheDeclaredNumberOfSBytesInIndexOrder()
    {
        // Arrange

        // Act
        var decoded = Converter.Decode(Samples, TenStoredSInts);

        // Assert
        decoded.Value.Should().BeOfType<sbyte[]>()
            .Which.Should().Equal(TenSamples);
    }

    [Fact]
    public void TheDeclaredNumberOfSBytesEncodesToOneByteEachInIndexOrder()
    {
        // Arrange
        var value = Samples.CreateLogixValue(TenSamples);

        // Act
        var encoded = Converter.Encode(value);

        // Assert
        encoded.Should().Equal(TenStoredSInts);
    }
}
