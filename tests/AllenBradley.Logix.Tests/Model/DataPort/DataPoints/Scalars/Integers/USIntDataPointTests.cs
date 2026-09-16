using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Scalars.Integers;

public sealed class USIntDataPointTests
{
    private static readonly USIntDataPoint Level =
        new(DefaultTagName, DefaultPollFrequency, NoChannels);

    [Fact]
    public void ItNamesItselfAsStudio5000SpellsIt()
    {
        // Arrange

        // Act
        var dataTypeName = Level.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("USINT"));
    }

    [Fact]
    public void ItAndItsConverterNameTheSameType()
    {
        // Arrange

        // Act
        var converter = DataPointConverterRegistry.GetConverter(Level);

        // Assert
        converter.ExpectedTypeName.Should().Be("USINT");
    }

    [Fact]
    public void AByteEngineValueIsCarriedThrough()
    {
        // Arrange

        // Act
        var conversion = Level.ConvertValue((byte)200);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().Be((byte)200);
    }

    [Fact]
    public void AByteValueIsInRange()
    {
        // Arrange

        // Act
        var value = Level.CreateTypedValue(byte.MaxValue);

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
        failure.Details.Should().Contain(DefaultTagName.Value).And.Contain("USINT");
    }

    [Fact]
    public void AnSByteIsRefusedNamingBothTypes()
    {
        // Arrange

        // Act
        var conversion = Level.ConvertValue((sbyte)42);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(byte).ToString()).And.Contain(typeof(sbyte).ToString());
    }
}
