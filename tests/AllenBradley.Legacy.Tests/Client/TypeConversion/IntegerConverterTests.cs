using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.LegacyDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Client.TypeConversion;

public sealed class IntegerConverterTests
{
    private static readonly IDataPointConverter<ILegacyDataPoint, ILegacyDataPointValue> Converter = new IntegerConverter();

    private static readonly ILegacyDataPoint Element = N7Element(0);

    [Fact]
    public void TwoStoredBytesDecodeToAShortWithTheLeastSignificantByteFirst()
    {
        // Arrange
        byte[] buffer = [0x67, 0x12];

        // Act
        var decoded = Converter.Decode(Element, buffer);

        // Assert
        decoded.Should().Be(N7Element(0).CreateLegacyValue(4711));
    }

    [Fact]
    public void AShortEncodesToTwoBytesWithTheLeastSignificantByteFirst()
    {
        // Arrange
        var value = N7Element(0).CreateLegacyValue(4711);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0x67, 0x12);
    }

    [Fact]
    public void ADataPointOfAnotherTypeIsRefusedWhenDecoding()
    {
        // Arrange
        var foreignDataPoint = Substitute.For<ILegacyDataPoint>();

        // Act
        var decoding = Converter.Invoking(converter => converter.Decode(foreignDataPoint, new byte[2]));

        // Assert
        decoding.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AValueOfAnotherTypeIsRefusedWhenEncoding()
    {
        // Arrange
        var foreignValue = Substitute.For<ILegacyDataPointValue>();
        foreignValue.DataPoint.Returns(Element);

        // Act
        var encoding = Converter.Invoking(converter => converter.Encode(foreignValue));

        // Assert
        encoding.Should().Throw<InvalidOperationException>();
    }
}
