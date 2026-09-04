using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Scalars.Strings;

public sealed class StringDataPointTests
{
    private const int DeclaredCapacity = 82;

    [Fact]
    public void ItNamesItselfAsStudio5000SpellsIt()
    {
        // Arrange
        var dataPoint = new StringDataPoint(
            DefaultTagName, DefaultPollFrequency, NoChannels, new StringMaxLength(DeclaredCapacity));

        // Act
        var dataTypeName = dataPoint.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("STRING"));
    }

    [Fact]
    public void ItAndItsConverterNameTheSameType()
    {
        // Arrange
        var dataPoint = new StringDataPoint(
            DefaultTagName, DefaultPollFrequency, NoChannels, new StringMaxLength(DeclaredCapacity));

        // Act
        var converter = DataPointConverterRegistry.GetConverter(dataPoint);

        // Assert
        converter.ExpectedTypeName.Value.Should().Be("STRING");
    }

    [Fact]
    public void AStringEngineValueIsCarriedThrough()
    {
        // Arrange
        var dataPoint = new StringDataPoint(
            DefaultTagName, DefaultPollFrequency, NoChannels, new StringMaxLength(DeclaredCapacity));

        // Act
        var conversion = dataPoint.ConvertValue("Hi");

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().Be("Hi");
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange
        var dataPoint = new StringDataPoint(
            DefaultTagName, DefaultPollFrequency, NoChannels, new StringMaxLength(DeclaredCapacity));

        // Act
        var conversion = dataPoint.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(dataPoint);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagName.Value).And.Contain("STRING");
    }

    [Fact]
    public void AnIntegerIsRefusedNamingBothTypes()
    {
        // Arrange
        var dataPoint = new StringDataPoint(
            DefaultTagName, DefaultPollFrequency, NoChannels, new StringMaxLength(DeclaredCapacity));

        // Act
        var conversion = dataPoint.ConvertValue(42);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(string).ToString()).And.Contain(typeof(int).ToString());
    }

    [Theory]
    [InlineData(DeclaredCapacity, true)]
    [InlineData(DeclaredCapacity + 1, false)]
    public void AValueIsInRangeWhileItFitsTheDeclaredCapacity(int length, bool expected)
    {
        // Arrange
        var dataPoint = new StringDataPoint(
            DefaultTagName, DefaultPollFrequency, NoChannels, new StringMaxLength(DeclaredCapacity));

        // Act
        var value = dataPoint.CreateTypedValue(new string('X', length));

        // Assert
        value.IsInValueRange().Should().Be(expected);
    }
}
