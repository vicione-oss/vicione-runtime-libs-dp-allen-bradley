using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Scalars.Booleans;

public sealed class BoolDataPointTests
{
    private static readonly BoolDataPoint Flag =
        new(DefaultTagPath, DefaultPollFrequency, NoChannels);

    [Fact]
    public void ItNamesItselfAsStudio5000SpellsIt()
    {
        // Arrange

        // Act
        var dataTypeName = Flag.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("BOOL"));
    }

    [Fact]
    public void ItAndItsConverterNameTheSameType()
    {
        // Arrange

        // Act
        var converter = DataPointConverterRegistry.GetConverter(Flag);

        // Assert
        converter.ExpectedTypeName.Should().Be("BOOL");
    }

    [Fact]
    public void ABoolEngineValueIsCarriedThrough()
    {
        // Arrange

        // Act
        var conversion = Flag.ConvertValue(true);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().Be(true);
    }

    [Fact]
    public void ABoolValueIsInRange()
    {
        // Arrange

        // Act
        var value = Flag.CreateTypedValue(true);

        // Assert
        value.IsInValueRange().Should().BeTrue();
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange

        // Act
        var conversion = Flag.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(Flag);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagAddress.Value).And.Contain("BOOL");
    }

    [Fact]
    public void AnIntIsRefusedNamingBothTypes()
    {
        // Arrange

        // Act
        var conversion = Flag.ConvertValue(1);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(bool).ToString()).And.Contain(typeof(int).ToString());
    }
}
