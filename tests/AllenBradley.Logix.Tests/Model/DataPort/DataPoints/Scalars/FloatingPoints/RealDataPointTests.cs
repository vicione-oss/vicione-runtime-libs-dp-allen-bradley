using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Scalars.FloatingPoints;

public sealed class RealDataPointTests
{
    [Fact]
    public void ItNamesItselfAsStudio5000SpellsIt()
    {
        // Arrange
        var dataPoint = new RealDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var dataTypeName = dataPoint.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("REAL"));
    }

    [Fact]
    public void ItAndItsConverterNameTheSameType()
    {
        // Arrange
        var dataPoint = new RealDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var converter = DataPointConverterRegistry.GetConverter(dataPoint);

        // Assert
        converter.ExpectedTypeName.Value.Should().Be("REAL");
    }

    [Fact]
    public void AFloatEngineValueIsCarriedThrough()
    {
        // Arrange
        var dataPoint = new RealDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var conversion = dataPoint.ConvertValue(1.5f);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().Be(1.5f);
    }

    [Fact]
    public void AFloatValueIsInRange()
    {
        // Arrange
        var dataPoint = new RealDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var value = dataPoint.CreateTypedValue(1.5f);

        // Assert
        value.IsInValueRange().Should().BeTrue();
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange
        var dataPoint = new RealDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var conversion = dataPoint.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(dataPoint);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagName.Value).And.Contain("REAL");
    }

    [Fact]
    public void ADoubleIsRefusedNamingBothTypes()
    {
        // Arrange
        var dataPoint = new RealDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var conversion = dataPoint.ConvertValue(1.5d);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(float).ToString()).And.Contain(typeof(double).ToString());
    }
}
