using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Arrays.Integers;

public sealed class LIntArrayDataPointTests
{
    private const int DeclaredElementCount = 10;

    private static readonly LIntArrayDataPoint Timestamps =
        new(DefaultTagPath, DefaultPollFrequency, NoChannels, new ElementCount(DeclaredElementCount));

    [Fact]
    public void ItNamesItselfAsStudio5000SpellsItWithoutTheDeclaredLength()
    {
        // Arrange

        // Act
        var dataTypeName = Timestamps.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("LINT[]"));
    }

    [Fact]
    public void ALongArrayEngineValueIsCarriedThrough()
    {
        // Arrange
        long[] timestamps = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

        // Act
        var conversion = Timestamps.ConvertValue(timestamps);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().BeSameAs(timestamps);
    }

    [Fact]
    public void AnUnsignedArrayOfTheSameWidthIsRefusedRatherThanReinterpreted()
    {
        // Arrange
        // The CLR holds ulong[] and long[] assignment-compatible, so this is the one engine value that
        // reaches a LINT[] point carrying elements of another type.
        ulong[] unsignedTimestamps = [0, 1, 2, 3, 4, 5, 6, 7, 8, 10_000_000_000_000_000_000];

        // Act
        var conversion = Timestamps.ConvertValue(unsignedTimestamps);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(long[]).ToString()).And.Contain(typeof(ulong[]).ToString());
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange

        // Act
        var conversion = Timestamps.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(Timestamps);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagAddress.Value).And.Contain("LINT[]");
    }

    [Theory]
    [InlineData(DeclaredElementCount, true)]
    [InlineData(DeclaredElementCount - 1, false)]
    [InlineData(DeclaredElementCount + 1, false)]
    public void AValueIsInRangeOnlyAtTheDeclaredLength(int length, bool expectedInRange)
    {
        // Arrange

        // Act
        var value = Timestamps.CreateTypedValue(new long[length]);

        // Assert
        value.IsInValueRange().Should().Be(expectedInRange);
    }
}
