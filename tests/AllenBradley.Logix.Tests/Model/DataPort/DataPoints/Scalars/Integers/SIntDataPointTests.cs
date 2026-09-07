using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Scalars.Integers;

public sealed class SIntDataPointTests
{
    [Fact]
    public void ItNamesItselfAsStudio5000SpellsIt()
    {
        // Arrange
        var dataPoint = new SIntDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var dataTypeName = dataPoint.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("SINT"));
    }

    [Fact]
    public void ItAndItsConverterNameTheSameType()
    {
        // Arrange
        var dataPoint = new SIntDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var converter = DataPointConverterRegistry.GetConverter(dataPoint);

        // Assert
        converter.ExpectedTypeName.Value.Should().Be("SINT");
    }

    [Fact]
    public void AnSByteEngineValueIsCarriedThrough()
    {
        // Arrange
        var dataPoint = new SIntDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var conversion = dataPoint.ConvertValue((sbyte)42);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().Be((sbyte)42);
    }

    [Fact]
    public void AnSByteValueIsInRange()
    {
        // Arrange
        var dataPoint = new SIntDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var value = dataPoint.CreateTypedValue(42);

        // Assert
        value.IsInValueRange().Should().BeTrue();
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange
        var dataPoint = new SIntDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var conversion = dataPoint.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(dataPoint);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagName.Value).And.Contain("SINT");
    }

    [Fact]
    public void AByteIsRefusedNamingBothTypes()
    {
        // Arrange
        var dataPoint = new SIntDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var conversion = dataPoint.ConvertValue((byte)42);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(sbyte).ToString()).And.Contain(typeof(byte).ToString());
    }
}
