using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.LegacyDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Client.TypeConversion;

/// <summary>The byte layout has no test here, because the codec tests pin it.</summary>
public sealed class TimerConverterTests
{
    private static readonly IDataPointConverter<ILegacyDataPoint, ILegacyDataPointValue> Converter = new TimerConverter();

    [Fact]
    public void AnAccumulatedValueIsReadBackAsItWasWritten()
    {
        // Arrange
        var dataPoint = T4Element(0);
        var written = dataPoint.CreateLegacyValue(1234);

        // Act
        var read = Converter.Decode(dataPoint, Converter.Encode(written));

        // Assert
        read.Value.Should().Be((short)1234);
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
        foreignValue.DataPoint.Returns(T4Element(0));

        // Act
        var encoding = Converter.Invoking(converter => converter.Encode(foreignValue));

        // Assert
        encoding.Should().Throw<InvalidOperationException>();
    }
}
