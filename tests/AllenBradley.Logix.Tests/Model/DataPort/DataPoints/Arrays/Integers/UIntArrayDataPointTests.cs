using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Arrays.Integers;

public sealed class UIntArrayDataPointTests
{
    private const int DeclaredElementCount = 10;

    private static readonly UIntArrayDataPoint Speeds =
        new(DefaultTagAddress, DefaultPollFrequency, NoChannels, new ElementCount(DeclaredElementCount));

    [Fact]
    public void ItNamesItselfAsStudio5000SpellsItWithoutTheDeclaredLength()
    {
        // Arrange

        // Act
        var dataTypeName = Speeds.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("UINT[]"));
    }

    [Fact]
    public void ItAndItsConverterNameTheSameType()
    {
        // Arrange

        // Act
        var converter = DataPointConverterRegistry.GetConverter(Speeds);

        // Assert
        converter.ExpectedTypeName.Should().Be("UINT[]");
    }

    [Fact]
    public void AUShortArrayEngineValueIsCarriedThrough()
    {
        // Arrange
        ushort[] speeds = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

        // Act
        var conversion = Speeds.ConvertValue(speeds);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().BeSameAs(speeds);
    }

    [Fact]
    public void ASignedArrayOfTheSameWidthIsRefusedRatherThanReinterpreted()
    {
        // Arrange
        // The CLR holds short[] and ushort[] assignment-compatible, so this is the one engine value that
        // reaches a UINT[] point carrying elements of another type.
        short[] signedSpeeds = [0, 1, 2, 3, 4, 5, 6, 7, 8, -25536];

        // Act
        var conversion = Speeds.ConvertValue(signedSpeeds);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(ushort[]).ToString()).And.Contain(typeof(short[]).ToString());
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange

        // Act
        var conversion = Speeds.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(Speeds);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagAddress.Value).And.Contain("UINT[]");
    }

    [Theory]
    [InlineData(DeclaredElementCount, true)]
    [InlineData(DeclaredElementCount - 1, false)]
    [InlineData(DeclaredElementCount + 1, false)]
    public void AValueIsInRangeOnlyAtTheDeclaredLength(int length, bool expectedInRange)
    {
        // Arrange

        // Act
        var value = Speeds.CreateTypedValue(new ushort[length]);

        // Assert
        value.IsInValueRange().Should().Be(expectedInRange);
    }
}
