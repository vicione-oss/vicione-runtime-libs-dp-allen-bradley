using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.LegacyDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Client.TypeConversion;

/// <summary>
/// The elements are built by hand from the documented layout — one word with its two characters
/// swapped — so no decode test agrees with the encoder by construction.
/// </summary>
public sealed class AsciiConverterTests
{
    private const int ElementSize = 2;

    private static readonly IDataPointConverter<ILegacyDataPoint, ILegacyDataPointValue> Converter = new AsciiConverter();

    /// <summary>An ASCII element as the controller sends it, with the value it holds.</summary>
    public static TheoryData<byte[], string> AsciiElements =>
        new()
        {
            { [0x00, 0x00], "" },
            { [0x00, 0x48], "H" },
            { [0x69, 0x48], "Hi" },
        };

    [Theory]
    [MemberData(nameof(AsciiElements))]
    public void TheBytesOfAnAsciiElementDecodeToTheirValue(byte[] element, string expectedValue)
    {
        // Arrange
        var dataPoint = A9Element(0);

        // Act
        var value = Converter.Decode(dataPoint, element);

        // Assert
        value.Should().Be(dataPoint.CreateLegacyValue(expectedValue));
    }

    [Theory]
    [MemberData(nameof(AsciiElements))]
    public void AnAsciiValueEncodesToTheBytesOfItsElement(byte[] expectedElement, string value)
    {
        // Arrange
        var dataPoint = A9Element(0);

        // Act
        var bytes = Converter.Encode(dataPoint.CreateLegacyValue(value));

        // Assert
        bytes.Should().Equal(expectedElement);
    }

    [Fact]
    public void ANulCharacterBeforeACharacterIsPartOfTheValue()
    {
        // Arrange
        var dataPoint = A9Element(0);

        // Act
        var value = Converter.Decode(dataPoint, [0x48, 0x00]);

        // Assert
        value.Should().Be(dataPoint.CreateLegacyValue("\0H"));
    }

    [Fact]
    public void AReplyShorterThanAnAsciiElementIsRefused()
    {
        // Arrange
        var dataPoint = A9Element(0);

        // Act
        var decoding = Converter.Invoking(converter => converter.Decode(dataPoint, new byte[ElementSize - 1]));

        // Assert
        decoding.Should().Throw<DecodeException>();
    }

    [Fact]
    public void AValueOf3CharactersIsRefusedWhenEncoding()
    {
        // Arrange
        var value = A9Element(0).CreateLegacyValue("abc");

        // Act
        var encoding = Converter.Invoking(converter => converter.Encode(value));

        // Assert
        encoding.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AValueWithACharacterOutsideAsciiIsRefusedWhenEncoding()
    {
        // Arrange
        var value = A9Element(0).CreateLegacyValue("ü");

        // Act
        var encoding = Converter.Invoking(converter => converter.Encode(value));

        // Assert
        encoding.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ADataPointOfAnotherTypeIsRefusedWhenDecoding()
    {
        // Arrange
        var foreignDataPoint = ST9Element(1);

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
        foreignValue.DataPoint.Returns(A9Element(0));

        // Act
        var encoding = Converter.Invoking(converter => converter.Encode(foreignValue));

        // Assert
        encoding.Should().Throw<InvalidOperationException>();
    }
}
