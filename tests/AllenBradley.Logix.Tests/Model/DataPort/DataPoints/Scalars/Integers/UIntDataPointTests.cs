using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Scalars.Integers;

public sealed class UIntDataPointTests
{
    private static readonly UIntDataPoint Setpoint =
        new(DefaultTagName, DefaultPollFrequency, NoChannels);

    [Fact]
    public void ItNamesItselfAsStudio5000SpellsIt()
    {
        // Arrange

        // Act
        var dataTypeName = Setpoint.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("UINT"));
    }

    [Fact]
    public void ItAndItsConverterNameTheSameType()
    {
        // Arrange

        // Act
        var converter = DataPointConverterRegistry.GetConverter(Setpoint);

        // Assert
        converter.ExpectedTypeName.Should().Be("UINT");
    }

    [Fact]
    public void AUShortEngineValueIsCarriedThrough()
    {
        // Arrange

        // Act
        var conversion = Setpoint.ConvertValue((ushort)50000);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().Be((ushort)50000);
    }

    [Fact]
    public void AUShortValueIsInRange()
    {
        // Arrange

        // Act
        var value = Setpoint.CreateTypedValue(ushort.MaxValue);

        // Assert
        value.IsInValueRange().Should().BeTrue();
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange

        // Act
        var conversion = Setpoint.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(Setpoint);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagName.Value).And.Contain("UINT");
    }

    [Fact]
    public void AShortIsRefusedNamingBothTypes()
    {
        // Arrange

        // Act
        var conversion = Setpoint.ConvertValue((short)4242);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(ushort).ToString()).And.Contain(typeof(short).ToString());
    }
}
