using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Arrays.Integers;

public sealed class DIntArrayDataPointTests
{
    private const int DeclaredElementCount = 10;

    private static readonly DIntArrayDataPoint Totals =
        new(DefaultTagPath, DefaultPollFrequency, NoChannels, new ElementCount(DeclaredElementCount));

    [Fact]
    public void ItNamesItselfAsStudio5000SpellsItWithoutTheDeclaredLength()
    {
        // Arrange

        // Act
        var dataTypeName = Totals.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("DINT[]"));
    }

    [Fact]
    public void AnIntArrayEngineValueIsCarriedThrough()
    {
        // Arrange
        int[] totals = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

        // Act
        var conversion = Totals.ConvertValue(totals);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().BeSameAs(totals);
    }

    [Fact]
    public void AnUnsignedArrayOfTheSameWidthIsRefusedRatherThanReinterpreted()
    {
        // Arrange
        // The CLR holds uint[] and int[] assignment-compatible, so this is the one engine value that
        // reaches a DINT[] point carrying elements of another type.
        uint[] unsignedTotals = [0, 1, 2, 3, 4, 5, 6, 7, 8, 3_000_000_000];

        // Act
        var conversion = Totals.ConvertValue(unsignedTotals);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(int[]).ToString()).And.Contain(typeof(uint[]).ToString());
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange

        // Act
        var conversion = Totals.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(Totals);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagAddress.Value).And.Contain("DINT[]");
    }

    [Theory]
    [InlineData(DeclaredElementCount, true)]
    [InlineData(DeclaredElementCount - 1, false)]
    [InlineData(DeclaredElementCount + 1, false)]
    public void AValueIsInRangeOnlyAtTheDeclaredLength(int length, bool expectedInRange)
    {
        // Arrange

        // Act
        var value = Totals.CreateTypedValue(new int[length]);

        // Assert
        value.IsInValueRange().Should().Be(expectedInRange);
    }
}
