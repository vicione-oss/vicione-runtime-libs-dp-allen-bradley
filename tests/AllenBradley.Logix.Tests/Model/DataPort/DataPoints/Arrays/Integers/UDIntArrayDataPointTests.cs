using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Arrays.Integers;

public sealed class UDIntArrayDataPointTests
{
    private const int DeclaredElementCount = 10;

    private static readonly UDIntArrayDataPoint Runtimes =
        new(DefaultTagName, DefaultPollFrequency, NoChannels, new ElementCount(DeclaredElementCount));

    [Fact]
    public void ItNamesItselfAsStudio5000SpellsItWithoutTheDeclaredLength()
    {
        // Arrange

        // Act
        var dataTypeName = Runtimes.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("UDINT[]"));
    }

    [Fact]
    public void ItAndItsConverterNameTheSameType()
    {
        // Arrange

        // Act
        var converter = DataPointConverterRegistry.GetConverter(Runtimes);

        // Assert
        converter.ExpectedTypeName.Value.Should().Be("UDINT[]");
    }

    [Fact]
    public void AUIntArrayEngineValueIsCarriedThrough()
    {
        // Arrange
        uint[] runtimes = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

        // Act
        var conversion = Runtimes.ConvertValue(runtimes);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().BeSameAs(runtimes);
    }

    [Fact]
    public void ASignedArrayOfTheSameWidthIsRefusedRatherThanReinterpreted()
    {
        // Arrange
        // The CLR holds int[] and uint[] assignment-compatible, so this is the one engine value that
        // reaches a UDINT[] point carrying elements of another type.
        int[] signedRuntimes = [0, 1, 2, 3, 4, 5, 6, 7, 8, -294967296];

        // Act
        var conversion = Runtimes.ConvertValue(signedRuntimes);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(uint[]).ToString()).And.Contain(typeof(int[]).ToString());
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange

        // Act
        var conversion = Runtimes.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(Runtimes);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagName.Value).And.Contain("UDINT[]");
    }

    [Theory]
    [InlineData(DeclaredElementCount, true)]
    [InlineData(DeclaredElementCount - 1, false)]
    [InlineData(DeclaredElementCount + 1, false)]
    public void AValueIsInRangeOnlyAtTheDeclaredLength(int length, bool expectedInRange)
    {
        // Arrange

        // Act
        var value = Runtimes.CreateTypedValue(new uint[length]);

        // Assert
        value.IsInValueRange().Should().Be(expectedInRange);
    }

    [Fact]
    public void AnAbsentArrayIsOutOfRangeRatherThanACrash()
    {
        // Arrange
        // ConvertValue turns away a null engine value, so only a caller inside the model reaches here.

        // Act
        var value = Runtimes.CreateTypedValue(null!);

        // Assert
        value.IsInValueRange().Should().BeFalse();
    }
}
