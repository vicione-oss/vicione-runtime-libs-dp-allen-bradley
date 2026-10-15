using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.LegacyDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Client.TypeConversion;

public sealed class FloatConverterTests
{
    private static readonly IDataPointConverter<ILegacyDataPoint, ILegacyDataPointValue> Converter = new FloatConverter();

    /// <summary>The bytes of a float element as the controller sends them, with their value.</summary>
    public static TheoryData<byte[], float> FloatElements =>
        new()
        {
            { [0x00, 0x00, 0xC0, 0x3F], 1.5f },
            { [0x00, 0x00, 0xC0, 0xBF], -1.5f },
            { [0x00, 0x00, 0x00, 0x00], 0.0f },
            { [0x00, 0x00, 0x80, 0x7F], float.PositiveInfinity },
            { [0x00, 0x00, 0x80, 0xFF], float.NegativeInfinity },
            // The quiet NaN of .NET has the sign bit set.
            { [0x00, 0x00, 0xC0, 0xFF], float.NaN },
        };

    [Theory]
    [MemberData(nameof(FloatElements))]
    public void TheBytesOfAFloatElementDecodeToTheirValue(byte[] bytes, float expectedValue)
    {
        // Arrange
        var element = F8Element(12);

        // Act
        var value = Converter.Decode(element, bytes);

        // Assert
        value.Should().Be(element.CreateLegacyValue(expectedValue));
    }

    [Theory]
    [MemberData(nameof(FloatElements))]
    public void AFloatValueEncodesToTheBytesOfItsElement(byte[] expectedBytes, float value)
    {
        // Arrange
        var element = F8Element(12);

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
        foreignValue.DataPoint.Returns(F8Element(0));

        // Act
        var encoding = Converter.Invoking(converter => converter.Encode(foreignValue));

        // Assert
        encoding.Should().Throw<InvalidOperationException>();
    }
}
