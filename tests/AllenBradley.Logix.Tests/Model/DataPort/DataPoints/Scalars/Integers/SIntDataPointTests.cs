using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Scalars.Integers;

public sealed class SIntDataPointTests
{
    private static readonly SIntDataPoint Level =
        new(DefaultTagAddress, DefaultPollFrequency, NoChannels);

    [Fact]
    public void ItNamesItselfAsStudio5000SpellsIt()
    {
        // Arrange

        // Act
        var dataTypeName = Level.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("SINT"));
    }

    [Fact]
    public void ItAndItsConverterNameTheSameType()
    {
        // Arrange

        // Act
        var converter = DataPointConverterRegistry.GetConverter(Level);

        // Assert
        converter.ExpectedTypeName.Should().Be("SINT");
    }

    [Fact]
    public void AnSByteEngineValueIsCarriedThrough()
    {
        // Arrange

        // Act
        var conversion = Level.ConvertValue((sbyte)42);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().Be((sbyte)42);
    }

    [Fact]
    public void AnSByteValueIsInRange()
    {
        // Arrange

        // Act
        var value = Level.CreateTypedValue(42);

        // Assert
        value.IsInValueRange().Should().BeTrue();
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange

        // Act
        var conversion = Level.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(Level);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagAddress.Value).And.Contain("SINT");
    }

    [Fact]
    public void AByteIsRefusedNamingBothTypes()
    {
        // Arrange

        // Act
        var conversion = Level.ConvertValue((byte)42);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(sbyte).ToString()).And.Contain(typeof(byte).ToString());
    }
}
