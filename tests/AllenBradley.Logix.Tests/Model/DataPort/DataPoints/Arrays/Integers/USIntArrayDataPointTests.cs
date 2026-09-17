using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Arrays.Integers;

public sealed class USIntArrayDataPointTests
{
    private const int DeclaredElementCount = 10;

    private static readonly USIntArrayDataPoint Pressures =
        new(DefaultTagPath, DefaultPollFrequency, NoChannels, new ElementCount(DeclaredElementCount));

    [Fact]
    public void ItNamesItselfAsStudio5000SpellsItWithoutTheDeclaredLength()
    {
        // Arrange

        // Act
        var dataTypeName = Pressures.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("USINT[]"));
    }

    [Fact]
    public void AByteArrayEngineValueIsCarriedThrough()
    {
        // Arrange
        byte[] pressures = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

        // Act
        var conversion = Pressures.ConvertValue(pressures);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().BeSameAs(pressures);
    }

    [Fact]
    public void ASignedArrayOfTheSameWidthIsRefusedRatherThanReinterpreted()
    {
        // Arrange
        // The CLR holds sbyte[] and byte[] assignment-compatible, so this is the one engine value that
        // reaches a USINT[] point carrying elements of another type.
        sbyte[] signedPressures = [0, 1, 2, 3, 4, 5, 6, 7, 8, -56];

        // Act
        var conversion = Pressures.ConvertValue(signedPressures);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(byte[]).ToString()).And.Contain(typeof(sbyte[]).ToString());
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange

        // Act
        var conversion = Pressures.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(Pressures);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagAddress.Value).And.Contain("USINT[]");
    }

    [Theory]
    [InlineData(DeclaredElementCount, true)]
    [InlineData(DeclaredElementCount - 1, false)]
    [InlineData(DeclaredElementCount + 1, false)]
    public void AValueIsInRangeOnlyAtTheDeclaredLength(int length, bool expectedInRange)
    {
        // Arrange

        // Act
        var value = Pressures.CreateTypedValue(new byte[length]);

        // Assert
        value.IsInValueRange().Should().Be(expectedInRange);
    }
}
