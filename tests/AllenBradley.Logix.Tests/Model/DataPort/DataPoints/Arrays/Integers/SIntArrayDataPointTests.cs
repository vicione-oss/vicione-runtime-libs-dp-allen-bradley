using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Arrays.Integers;

public sealed class SIntArrayDataPointTests
{
    private const int DeclaredElementCount = 10;

    private static readonly SIntArrayDataPoint Samples =
        new(DefaultTagPath, DefaultPollFrequency, NoChannels, new ElementCount(DeclaredElementCount));

    [Fact]
    public void ItNamesItselfAsStudio5000SpellsItWithoutTheDeclaredLength()
    {
        // Arrange

        // Act
        var dataTypeName = Samples.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("SINT[]"));
    }

    [Fact]
    public void AnSByteArrayEngineValueIsCarriedThrough()
    {
        // Arrange
        sbyte[] samples = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

        // Act
        var conversion = Samples.ConvertValue(samples);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().BeSameAs(samples);
    }

    [Fact]
    public void AnUnsignedArrayOfTheSameWidthIsRefusedRatherThanReinterpreted()
    {
        // Arrange
        // The CLR holds byte[] and sbyte[] assignment-compatible, so this is the one engine value that
        // reaches a SINT[] point carrying elements of another type.
        byte[] unsignedSamples = [0, 1, 2, 3, 4, 5, 6, 7, 8, 200];

        // Act
        var conversion = Samples.ConvertValue(unsignedSamples);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(sbyte[]).ToString()).And.Contain(typeof(byte[]).ToString());
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange

        // Act
        var conversion = Samples.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(Samples);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagAddress.Value).And.Contain("SINT[]");
    }

    [Theory]
    [InlineData(DeclaredElementCount, true)]
    [InlineData(DeclaredElementCount - 1, false)]
    [InlineData(DeclaredElementCount + 1, false)]
    public void AValueIsInRangeOnlyAtTheDeclaredLength(int length, bool expectedInRange)
    {
        // Arrange

        // Act
        var value = Samples.CreateTypedValue(new sbyte[length]);

        // Assert
        value.IsInValueRange().Should().Be(expectedInRange);
    }
}
