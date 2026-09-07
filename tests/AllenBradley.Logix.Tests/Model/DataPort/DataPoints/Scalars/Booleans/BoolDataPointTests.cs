using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Scalars.Booleans;

public sealed class BoolDataPointTests
{
    [Fact]
    public void ItNamesItselfAsStudio5000SpellsIt()
    {
        // Arrange
        var dataPoint = new BoolDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var dataTypeName = dataPoint.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("BOOL"));
    }

    [Fact]
    public void ItAndItsConverterNameTheSameType()
    {
        // Arrange
        var dataPoint = new BoolDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var converter = DataPointConverterRegistry.GetConverter(dataPoint);

        // Assert
        converter.ExpectedTypeName.Value.Should().Be("BOOL");
    }

    [Fact]
    public void ABoolEngineValueIsCarriedThrough()
    {
        // Arrange
        var dataPoint = new BoolDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var conversion = dataPoint.ConvertValue(true);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().Be(true);
    }

    [Fact]
    public void ABoolValueIsInRange()
    {
        // Arrange
        var dataPoint = new BoolDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var value = dataPoint.CreateTypedValue(true);

        // Assert
        value.IsInValueRange().Should().BeTrue();
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange
        var dataPoint = new BoolDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var conversion = dataPoint.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(dataPoint);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagName.Value).And.Contain("BOOL");
    }

    [Fact]
    public void AnIntIsRefusedNamingBothTypes()
    {
        // Arrange
        // Nonzero is true on the wire, and nothing here extends that to the engine value: a 1 is an int.
        var dataPoint = new BoolDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var conversion = dataPoint.ConvertValue(1);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(bool).ToString()).And.Contain(typeof(int).ToString());
    }
}
