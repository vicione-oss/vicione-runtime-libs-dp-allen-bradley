using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Scalars.FloatingPoints;

public sealed class RealDataPointTests
{
    private static readonly RealDataPoint Measurement =
        new(DefaultTagPath, DefaultPollFrequency, NoChannels);

    [Fact]
    public void ItNamesItselfAsStudio5000SpellsIt()
    {
        // Arrange

        // Act
        var dataTypeName = Measurement.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("REAL"));
    }

    [Fact]
    public void AFloatEngineValueIsCarriedThrough()
    {
        // Arrange

        // Act
        var conversion = Measurement.ConvertValue(1.5f);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().Be(1.5f);
    }

    [Fact]
    public void AFloatValueIsInRange()
    {
        // Arrange

        // Act
        var value = Measurement.CreateTypedValue(1.5f);

        // Assert
        value.IsInValueRange().Should().BeTrue();
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange

        // Act
        var conversion = Measurement.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(Measurement);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagAddress.Value).And.Contain("REAL");
    }

    [Fact]
    public void ADoubleIsRefusedNamingBothTypes()
    {
        // Arrange

        // Act
        var conversion = Measurement.ConvertValue(1.5d);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(float).ToString()).And.Contain(typeof(double).ToString());
    }
}
