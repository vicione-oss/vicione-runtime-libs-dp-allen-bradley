using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Scalars.Integers;

public sealed class UIntDataPointTests
{
    [Fact]
    public void ItNamesItselfAsStudio5000SpellsIt()
    {
        // Arrange
        var dataPoint = new UIntDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var dataTypeName = dataPoint.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("UINT"));
    }

    [Fact]
    public void ItAndItsConverterNameTheSameType()
    {
        // Arrange
        var dataPoint = new UIntDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var converter = DataPointConverterRegistry.GetConverter(dataPoint);

        // Assert
        converter.ExpectedTypeName.Value.Should().Be("UINT");
    }

    [Fact]
    public void AUShortEngineValueIsCarriedThrough()
    {
        // Arrange
        var dataPoint = new UIntDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var conversion = dataPoint.ConvertValue((ushort)50000);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().Be((ushort)50000);
    }

    [Fact]
    public void AUShortValueIsInRange()
    {
        // Arrange
        var dataPoint = new UIntDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var value = dataPoint.CreateTypedValue(ushort.MaxValue);

        // Assert
        value.IsInValueRange().Should().BeTrue();
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange
        var dataPoint = new UIntDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var conversion = dataPoint.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(dataPoint);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagName.Value).And.Contain("UINT");
    }

    [Fact]
    public void AShortIsRefusedNamingBothTypes()
    {
        // Arrange
        // The signed twin of this point's own type, and the one substitution that would otherwise pass
        // unnoticed: both occupy two bytes, so nothing downstream would object.
        var dataPoint = new UIntDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var conversion = dataPoint.ConvertValue((short)4242);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(ushort).ToString()).And.Contain(typeof(short).ToString());
    }
}
