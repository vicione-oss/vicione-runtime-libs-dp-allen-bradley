using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Scalars.Integers;

public sealed class IntDataPointTests
{
    private static readonly IntDataPoint Counter =
        new(DefaultTagPath, DefaultPollFrequency, NoChannels);

    [Fact]
    public void ItNamesItselfAsStudio5000SpellsIt()
    {
        // Arrange

        // Act
        var dataTypeName = Counter.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("INT"));
    }

    [Fact]
    public void ItAndItsConverterNameTheSameType()
    {
        // Arrange

        // Act
        var converter = DataPointConverterRegistry.GetConverter(Counter);

        // Assert
        converter.ExpectedTypeName.Should().Be("INT");
    }

    [Fact]
    public void AShortEngineValueIsCarriedThrough()
    {
        // Arrange

        // Act
        var conversion = Counter.ConvertValue((short)4711);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().Be((short)4711);
    }

    [Fact]
    public void AShortValueIsInRange()
    {
        // Arrange

        // Act
        var value = Counter.CreateTypedValue(4711);

        // Assert
        value.IsInValueRange().Should().BeTrue();
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange

        // Act
        var conversion = Counter.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(Counter);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagAddress.Value).And.Contain("INT");
    }

    [Fact]
    public void AnIntIsRefusedNamingBothTypes()
    {
        // Arrange

        // Act
        var conversion = Counter.ConvertValue(4711);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(short).ToString()).And.Contain(typeof(int).ToString());
    }
}
