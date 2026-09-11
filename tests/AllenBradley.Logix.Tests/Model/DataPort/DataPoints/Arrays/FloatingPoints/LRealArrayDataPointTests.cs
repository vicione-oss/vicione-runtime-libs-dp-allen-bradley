using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Arrays.FloatingPoints;

public sealed class LRealArrayDataPointTests
{
    private const int DeclaredElementCount = 10;

    private static readonly LRealArrayDataPoint Positions =
        new(DefaultTagName, DefaultPollFrequency, NoChannels, new ElementCount(DeclaredElementCount));

    [Fact]
    public void ItNamesItselfAsStudio5000SpellsItWithoutTheDeclaredLength()
    {
        // Arrange

        // Act
        var dataTypeName = Positions.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("LREAL[]"));
    }

    [Fact]
    public void ItAndItsConverterNameTheSameType()
    {
        // Arrange

        // Act
        var converter = DataPointConverterRegistry.GetConverter(Positions);

        // Assert
        converter.ExpectedTypeName.Value.Should().Be("LREAL[]");
    }

    [Fact]
    public void ADoubleArrayEngineValueIsCarriedThrough()
    {
        // Arrange
        double[] positions = [0d, 1.5d, 2d, 3d, 4d, 5d, 6d, 7d, 8d, 9d];

        // Act
        var conversion = Positions.ConvertValue(positions);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().BeSameAs(positions);
    }

    [Fact]
    public void ALongArrayOfTheSameWidthIsRefusedNamingBothTypes()
    {
        // Arrange
        // No array is assignment-compatible with double[], so the width the two share buys a long[]
        // nothing here; every integer array is turned away by the same rule.
        long[] rawPositions = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

        // Act
        var conversion = Positions.ConvertValue(rawPositions);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(double[]).ToString()).And.Contain(typeof(long[]).ToString());
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange

        // Act
        var conversion = Positions.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(Positions);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagName.Value).And.Contain("LREAL[]");
    }

    [Theory]
    [InlineData(DeclaredElementCount, true)]
    [InlineData(DeclaredElementCount - 1, false)]
    [InlineData(DeclaredElementCount + 1, false)]
    public void AValueIsInRangeOnlyAtTheDeclaredLength(int length, bool expectedInRange)
    {
        // Arrange

        // Act
        var value = Positions.CreateTypedValue(new double[length]);

        // Assert
        value.IsInValueRange().Should().Be(expectedInRange);
    }

    [Fact]
    public void AnAbsentArrayIsOutOfRangeRatherThanACrash()
    {
        // Arrange
        // ConvertValue turns away a null engine value, so only a caller inside the model reaches here.

        // Act
        var value = Positions.CreateTypedValue(null!);

        // Assert
        value.IsInValueRange().Should().BeFalse();
    }
}
