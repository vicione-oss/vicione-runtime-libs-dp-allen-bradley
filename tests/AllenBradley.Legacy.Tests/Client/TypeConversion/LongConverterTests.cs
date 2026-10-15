using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.LegacyDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Client.TypeConversion;

public sealed class LongConverterTests
{
    private static readonly IDataPointConverter<ILegacyDataPoint, ILegacyDataPointValue> Converter = new LongConverter();

    /// <summary>The bytes of a long element as the controller sends them, with their value.</summary>
    public static TheoryData<byte[], int> LongElements =>
        new()
        {
            { [0x00, 0x00, 0x00, 0x00], 0 },
            { [0xA0, 0x86, 0x01, 0x00], 100_000 },
            { [0xFF, 0xFF, 0xFF, 0xFF], -1 },
            { [0xFF, 0xFF, 0xFF, 0x7F], int.MaxValue },
            { [0x00, 0x00, 0x00, 0x80], int.MinValue },
        };

    [Theory]
    [MemberData(nameof(LongElements))]
    public void TheBytesOfALongElementDecodeToTheirValue(byte[] bytes, int expectedValue)
    {
        // Arrange
        var element = L9Element(0);

        // Act
        var value = Converter.Decode(element, bytes);

        // Assert
        value.Should().Be(element.CreateLegacyValue(expectedValue));
    }

    [Theory]
    [MemberData(nameof(LongElements))]
    public void ALongValueEncodesToTheBytesOfItsElement(byte[] expectedBytes, int value)
    {
        // Arrange
        var element = L9Element(0);

        // Act
        var bytes = Converter.Encode(element.CreateLegacyValue(value));

        // Assert
        bytes.Should().Equal(expectedBytes);
    }

    [Fact]
    public void ADataPointOfAnotherTypeIsRefusedWhenDecoding()
    {
        // Arrange
        var foreignDataPoint = N7Element(0);

        // Act
        var decoding = Converter.Invoking(converter => converter.Decode(foreignDataPoint, new byte[4]));

        // Assert
        decoding.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AValueOfAnotherTypeIsRefusedWhenEncoding()
    {
        // Arrange
        var foreignValue = Substitute.For<ILegacyDataPointValue>();
        foreignValue.DataPoint.Returns(L9Element(0));

        // Act
        var encoding = Converter.Invoking(converter => converter.Encode(foreignValue));

        // Assert
        encoding.Should().Throw<InvalidOperationException>();
    }
}
