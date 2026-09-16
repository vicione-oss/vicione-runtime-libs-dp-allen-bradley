using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Scalars.Integers;

public sealed class ULIntDataPointTests
{
    private static readonly ULIntDataPoint Cycles =
        new(DefaultTagAddress, DefaultPollFrequency, NoChannels);

    [Fact]
    public void ItNamesItselfAsStudio5000SpellsIt()
    {
        // Arrange

        // Act
        var dataTypeName = Cycles.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("ULINT"));
    }

    [Fact]
    public void ItAndItsConverterNameTheSameType()
    {
        // Arrange

        // Act
        var converter = DataPointConverterRegistry.GetConverter(Cycles);

        // Assert
        converter.ExpectedTypeName.Should().Be("ULINT");
    }

    [Fact]
    public void AULongEngineValueIsCarriedThrough()
    {
        // Arrange

        // Act
        var conversion = Cycles.ConvertValue(18446744073709551614ul);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().Be(18446744073709551614ul);
    }

    [Fact]
    public void AULongValueIsInRange()
    {
        // Arrange

        // Act
        var value = Cycles.CreateTypedValue(ulong.MaxValue);

        // Assert
        value.IsInValueRange().Should().BeTrue();
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange

        // Act
        var conversion = Cycles.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(Cycles);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagAddress.Value).And.Contain("ULINT");
    }

    [Fact]
    public void ALongIsRefusedNamingBothTypes()
    {
        // Arrange

        // Act
        var conversion = Cycles.ConvertValue(1234567890123L);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(ulong).ToString()).And.Contain(typeof(long).ToString());
    }
}
