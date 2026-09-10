using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Arrays.FloatingPoints;

public sealed class RealArrayDataPointTests
{
    private const int DeclaredElementCount = 10;

    private static readonly RealArrayDataPoint Temperatures =
        new(DefaultTagName, DefaultPollFrequency, NoChannels, new ElementCount(DeclaredElementCount));

    [Fact]
    public void ItNamesItselfAsStudio5000SpellsItWithoutTheDeclaredLength()
    {
        // Arrange

        // Act
        var dataTypeName = Temperatures.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("REAL[]"));
    }

    [Fact]
    public void ItAndItsConverterNameTheSameType()
    {
        // Arrange

        // Act
        var converter = DataPointConverterRegistry.GetConverter(Temperatures);

        // Assert
        converter.ExpectedTypeName.Value.Should().Be("REAL[]");
    }

    [Fact]
    public void AFloatArrayEngineValueIsCarriedThrough()
    {
        // Arrange
        float[] temperatures = [0f, 1.5f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f];

        // Act
        var conversion = Temperatures.ConvertValue(temperatures);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().BeSameAs(temperatures);
    }

    [Fact]
    public void AnIntArrayOfTheSameWidthIsRefusedNamingBothTypes()
    {
        // Arrange
        // No array is assignment-compatible with float[], so the width the two share buys an int[]
        // nothing here; every integer array is turned away by the same rule.
        int[] rawTemperatures = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

        // Act
        var conversion = Temperatures.ConvertValue(rawTemperatures);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(float[]).ToString()).And.Contain(typeof(int[]).ToString());
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange

        // Act
        var conversion = Temperatures.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(Temperatures);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagName.Value).And.Contain("REAL[]");
    }

    [Theory]
    [InlineData(DeclaredElementCount, true)]
    [InlineData(DeclaredElementCount - 1, false)]
    [InlineData(DeclaredElementCount + 1, false)]
    public void AValueIsInRangeOnlyAtTheDeclaredLength(int length, bool expectedInRange)
    {
        // Arrange

        // Act
        var value = Temperatures.CreateTypedValue(new float[length]);

        // Assert
        value.IsInValueRange().Should().Be(expectedInRange);
    }

    [Fact]
    public void AnAbsentArrayIsOutOfRangeRatherThanACrash()
    {
        // Arrange
        // ConvertValue turns away a null engine value, so only a caller inside the model reaches here.

        // Act
        var value = Temperatures.CreateTypedValue(null!);

        // Assert
        value.IsInValueRange().Should().BeFalse();
    }
}
