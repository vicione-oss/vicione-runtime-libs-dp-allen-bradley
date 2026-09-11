using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Arrays.Integers;

public sealed class IntArrayDataPointTests
{
    private const int DeclaredElementCount = 10;

    private static readonly IntArrayDataPoint Readings =
        new(DefaultTagName, DefaultPollFrequency, NoChannels, new ElementCount(DeclaredElementCount));

    [Fact]
    public void ItNamesItselfAsStudio5000SpellsItWithoutTheDeclaredLength()
    {
        // Arrange

        // Act
        var dataTypeName = Readings.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("INT[]"));
    }

    [Fact]
    public void ItAndItsConverterNameTheSameType()
    {
        // Arrange

        // Act
        var converter = DataPointConverterRegistry.GetConverter(Readings);

        // Assert
        converter.ExpectedTypeName.Value.Should().Be("INT[]");
    }

    [Fact]
    public void AShortArrayEngineValueIsCarriedThrough()
    {
        // Arrange
        short[] readings = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

        // Act
        var conversion = Readings.ConvertValue(readings);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().BeSameAs(readings);
    }

    [Fact]
    public void AnUnsignedArrayOfTheSameWidthIsRefusedRatherThanReinterpreted()
    {
        // Arrange
        // The CLR holds ushort[] and short[] assignment-compatible, so this is the one engine value that
        // reaches an INT[] point carrying elements of another type.
        ushort[] unsignedReadings = [0, 1, 2, 3, 4, 5, 6, 7, 8, 40000];

        // Act
        var conversion = Readings.ConvertValue(unsignedReadings);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(short[]).ToString()).And.Contain(typeof(ushort[]).ToString());
    }

    [Fact]
    public void ASingleShortIsRefusedNamingBothTypes()
    {
        // Arrange

        // Act
        var conversion = Readings.ConvertValue((short)4711);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(short[]).ToString()).And.Contain(typeof(short).ToString());
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange

        // Act
        var conversion = Readings.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(Readings);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagName.Value).And.Contain("INT[]");
    }

    [Theory]
    [InlineData(DeclaredElementCount, true)]
    [InlineData(DeclaredElementCount - 1, false)]
    [InlineData(DeclaredElementCount + 1, false)]
    public void AValueIsInRangeOnlyAtTheDeclaredLength(int length, bool expectedInRange)
    {
        // Arrange

        // Act
        var value = Readings.CreateTypedValue(new short[length]);

        // Assert
        value.IsInValueRange().Should().Be(expectedInRange);
    }
}
