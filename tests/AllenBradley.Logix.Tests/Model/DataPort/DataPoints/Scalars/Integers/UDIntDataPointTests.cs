using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Scalars.Integers;

public sealed class UDIntDataPointTests
{
    private static readonly UDIntDataPoint Runtime =
        new(DefaultTagPath, DefaultPollFrequency, NoChannels);

    [Fact]
    public void ItNamesItselfAsStudio5000SpellsIt()
    {
        // Arrange

        // Act
        var dataTypeName = Runtime.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("UDINT"));
    }

    [Fact]
    public void ItAndItsConverterNameTheSameType()
    {
        // Arrange

        // Act
        var converter = DataPointConverterRegistry.GetConverter(Runtime);

        // Assert
        converter.ExpectedTypeName.Should().Be("UDINT");
    }

    [Fact]
    public void AUIntEngineValueIsCarriedThrough()
    {
        // Arrange

        // Act
        var conversion = Runtime.ConvertValue(4000000000u);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().Be(4000000000u);
    }

    [Fact]
    public void AUIntValueIsInRange()
    {
        // Arrange

        // Act
        var value = Runtime.CreateTypedValue(uint.MaxValue);

        // Assert
        value.IsInValueRange().Should().BeTrue();
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange

        // Act
        var conversion = Runtime.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(Runtime);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagAddress.Value).And.Contain("UDINT");
    }

    [Fact]
    public void AnIntIsRefusedNamingBothTypes()
    {
        // Arrange

        // Act
        var conversion = Runtime.ConvertValue(123456);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(uint).ToString()).And.Contain(typeof(int).ToString());
    }
}
