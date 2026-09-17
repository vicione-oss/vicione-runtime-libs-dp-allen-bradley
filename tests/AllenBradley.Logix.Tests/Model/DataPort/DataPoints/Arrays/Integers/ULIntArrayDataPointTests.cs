using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Arrays.Integers;

public sealed class ULIntArrayDataPointTests
{
    private const int DeclaredElementCount = 10;

    private static readonly ULIntArrayDataPoint CycleCounts =
        new(DefaultTagPath, DefaultPollFrequency, NoChannels, new ElementCount(DeclaredElementCount));

    [Fact]
    public void ItNamesItselfAsStudio5000SpellsItWithoutTheDeclaredLength()
    {
        // Arrange

        // Act
        var dataTypeName = CycleCounts.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("ULINT[]"));
    }

    [Fact]
    public void AULongArrayEngineValueIsCarriedThrough()
    {
        // Arrange
        ulong[] cycleCounts = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

        // Act
        var conversion = CycleCounts.ConvertValue(cycleCounts);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().BeSameAs(cycleCounts);
    }

    [Fact]
    public void ASignedArrayOfTheSameWidthIsRefusedRatherThanReinterpreted()
    {
        // Arrange
        // The CLR holds long[] and ulong[] assignment-compatible, so this is the one engine value that
        // reaches a ULINT[] point carrying elements of another type.
        long[] signedCycleCounts = [0, 1, 2, 3, 4, 5, 6, 7, 8, -8_446_744_073_709_551_616];

        // Act
        var conversion = CycleCounts.ConvertValue(signedCycleCounts);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(ulong[]).ToString()).And.Contain(typeof(long[]).ToString());
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange

        // Act
        var conversion = CycleCounts.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(CycleCounts);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagAddress.Value).And.Contain("ULINT[]");
    }

    [Theory]
    [InlineData(DeclaredElementCount, true)]
    [InlineData(DeclaredElementCount - 1, false)]
    [InlineData(DeclaredElementCount + 1, false)]
    public void AValueIsInRangeOnlyAtTheDeclaredLength(int length, bool expectedInRange)
    {
        // Arrange

        // Act
        var value = CycleCounts.CreateTypedValue(new ulong[length]);

        // Assert
        value.IsInValueRange().Should().Be(expectedInRange);
    }
}
