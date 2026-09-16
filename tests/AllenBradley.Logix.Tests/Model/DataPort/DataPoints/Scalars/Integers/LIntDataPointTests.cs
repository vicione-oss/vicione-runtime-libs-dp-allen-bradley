using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Scalars.Integers;

public sealed class LIntDataPointTests
{
    private static readonly LIntDataPoint Ticks =
        new(DefaultTagAddress, DefaultPollFrequency, NoChannels);

    [Fact]
    public void ItNamesItselfAsStudio5000SpellsIt()
    {
        // Arrange

        // Act
        var dataTypeName = Ticks.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("LINT"));
    }

    [Fact]
    public void ItAndItsConverterNameTheSameType()
    {
        // Arrange

        // Act
        var converter = DataPointConverterRegistry.GetConverter(Ticks);

        // Assert
        converter.ExpectedTypeName.Should().Be("LINT");
    }

    [Fact]
    public void ALongEngineValueIsCarriedThrough()
    {
        // Arrange

        // Act
        var conversion = Ticks.ConvertValue(9223372036854775806L);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().Be(9223372036854775806L);
    }

    [Fact]
    public void ALongValueIsInRange()
    {
        // Arrange

        // Act
        var value = Ticks.CreateTypedValue(long.MaxValue);

        // Assert
        value.IsInValueRange().Should().BeTrue();
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange

        // Act
        var conversion = Ticks.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(Ticks);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagAddress.Value).And.Contain("LINT");
    }

    [Fact]
    public void AnIntIsRefusedNamingBothTypes()
    {
        // Arrange

        // Act
        var conversion = Ticks.ConvertValue(4711);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(long).ToString()).And.Contain(typeof(int).ToString());
    }
}
