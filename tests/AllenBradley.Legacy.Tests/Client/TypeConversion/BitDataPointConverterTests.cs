using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.LegacyDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Client.TypeConversion;

/// <summary>The byte layout has no test here, because the codec tests pin it.</summary>
public sealed class BitDataPointConverterTests
{
    private static readonly IDataPointConverter<ILegacyDataPoint, ILegacyDataPointValue> Converter =
        new BitDataPointConverter();

    [Fact]
    public void ADataPointOfAnotherTypeIsRefusedWhenDecoding()
    {
        // Arrange
        var foreignDataPoint = N7Element(0);

        // Act
        var decoding = Converter.Invoking(converter => converter.Decode(foreignDataPoint, new byte[1]));

        // Assert
        decoding.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AValueOfAnotherTypeIsRefusedWhenEncoding()
    {
        // Arrange
        var foreignValue = Substitute.For<ILegacyDataPointValue>();
        foreignValue.DataPoint.Returns(B3Bit(0, 0));

        // Act
        var encoding = Converter.Invoking(converter => converter.Encode(foreignValue));

        // Assert
        encoding.Should().Throw<InvalidOperationException>();
    }
}
