using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.LegacyDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Client.TypeConversion;

/// <summary>
/// The elements are built by hand from the documented layout — a length word, then 82 characters with the
/// two characters of each word swapped — so no decode test agrees with the encoder by construction.
/// </summary>
public sealed class StringConverterTests
{
    private const int ElementSize = 84;

    private static readonly IDataPointConverter<ILegacyDataPoint, ILegacyDataPointValue> Converter = new StringConverter();

    /// <summary>The first bytes of a string element as the controller sends them, with the value they hold.</summary>
    public static TheoryData<byte[], string> StringElements =>
        new()
        {
            { [0x00, 0x00], "" },
            { [0x02, 0x00, 0x69, 0x48], "Hi" },
            { [0x05, 0x00, 0x65, 0x48, 0x6C, 0x6C, 0x00, 0x6F], "Hello" },
        };

    [Theory]
    [MemberData(nameof(StringElements))]
    public void TheBytesOfAStringElementDecodeToTheirValue(byte[] leadingBytes, string expectedValue)
    {
        // Arrange
        var element = ST9Element(1);

        // Act
        var value = Converter.Decode(element, ElementStartingWith(leadingBytes));

        // Assert
        value.Should().Be(element.CreateLegacyValue(expectedValue));
    }

    [Theory]
    [MemberData(nameof(StringElements))]
    public void AStringValueEncodesToTheBytesOfItsElement(byte[] expectedLeadingBytes, string value)
    {
        // Arrange
        var element = ST9Element(1);

        // Act
        var bytes = Converter.Encode(element.CreateLegacyValue(value));

        // Assert
        bytes.Should().Equal(ElementStartingWith(expectedLeadingBytes));
    }

    [Fact]
    public void OnlyTheFirstLengthCharactersAreDecoded()
    {
        // Arrange
        // A shorter value written over a longer one leaves the end of the old one in the element.
        var element = ST9Element(1);
        var bytes = ElementStartingWith([0x02, 0x00, 0x69, 0x48, 0x61, 0x6C, 0x65, 0x74]);

        // Act
        var value = Converter.Decode(element, bytes);

        // Assert
        value.Should().Be(element.CreateLegacyValue("Hi"));
    }

    [Fact]
    public void ALengthAboveTheCapacityDecodesToTheWholeElement()
    {
        // Arrange
        var element = ST9Element(1);
        var bytes = Enumerable.Repeat((byte)'x', ElementSize).ToArray();
        bytes[0] = 0xFF;
        bytes[1] = 0x00;

        // Act
        var value = Converter.Decode(element, bytes);

        // Assert
        value.Should().Be(element.CreateLegacyValue(new string('x', 82)));
    }

    [Fact]
    public void AReplyShorterThanAStringElementIsRefused()
    {
        // Arrange
        var element = ST9Element(1);

        // Act
        var decoding = Converter.Invoking(converter => converter.Decode(element, new byte[ElementSize - 1]));

        // Assert
        decoding.Should().Throw<DecodeException>();
    }

    [Fact]
    public void AValueOf83CharactersIsRefusedWhenEncoding()
    {
        // Arrange
        var element = ST9Element(1);
        var value = element.CreateLegacyValue(new string('x', 83));

        // Act
        var encoding = Converter.Invoking(converter => converter.Encode(value));

        // Assert
        encoding.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AValueWithACharacterOutsideAsciiIsRefusedWhenEncoding()
    {
        // Arrange
        var element = ST9Element(1);
        var value = element.CreateLegacyValue("Grüße");

        // Act
        var encoding = Converter.Invoking(converter => converter.Encode(value));

        // Assert
        encoding.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ADataPointOfAnotherTypeIsRefusedWhenDecoding()
    {
        // Arrange
        var foreignDataPoint = N7Element(0);

        // Act
        var decoding = Converter.Invoking(converter => converter.Decode(foreignDataPoint, new byte[ElementSize]));

        // Assert
        decoding.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AValueOfAnotherTypeIsRefusedWhenEncoding()
    {
        // Arrange
        var foreignValue = Substitute.For<ILegacyDataPointValue>();
        foreignValue.DataPoint.Returns(ST9Element(1));

        // Act
        var encoding = Converter.Invoking(converter => converter.Encode(foreignValue));

        // Assert
        encoding.Should().Throw<InvalidOperationException>();
    }

    private static byte[] ElementStartingWith(byte[] leadingBytes)
    {
        var element = new byte[ElementSize];
        leadingBytes.CopyTo(element, 0);
        return element;
    }
}
