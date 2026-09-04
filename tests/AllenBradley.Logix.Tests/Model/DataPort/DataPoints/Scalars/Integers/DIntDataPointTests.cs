using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Scalars.Integers;

public sealed class DIntDataPointTests
{
    [Fact]
    public void ItNamesItselfAsStudio5000SpellsIt()
    {
        // Arrange
        var dataPoint = new DIntDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var dataTypeName = dataPoint.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("DINT"));
    }

    [Fact]
    public void ItAndItsConverterNameTheSameType()
    {
        // Arrange
        var dataPoint = new DIntDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var converter = DataPointConverterRegistry.GetConverter(dataPoint);

        // Assert
        converter.ExpectedTypeName.Value.Should().Be("DINT");
    }

    [Fact]
    public void AnIntegerEngineValueIsCarriedThrough()
    {
        // Arrange
        var dataPoint = new DIntDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var conversion = dataPoint.ConvertValue(42);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().Be(42);
    }

    [Fact]
    public void AnIntegerValueIsInRange()
    {
        // Arrange
        var dataPoint = new DIntDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var value = dataPoint.CreateTypedValue(42);

        // Assert
        value.IsInValueRange().Should().BeTrue();
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange
        var dataPoint = new DIntDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var conversion = dataPoint.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(dataPoint);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagName.Value).And.Contain("DINT");
    }

    [Fact]
    public void AStringIsRefusedNamingBothTypes()
    {
        // Arrange
        var dataPoint = new DIntDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var conversion = dataPoint.ConvertValue("42");

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(int).ToString()).And.Contain(typeof(string).ToString());
    }
}
