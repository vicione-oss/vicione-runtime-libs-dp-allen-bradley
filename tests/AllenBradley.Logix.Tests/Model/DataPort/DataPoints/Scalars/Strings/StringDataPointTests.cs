using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Scalars.Strings;

public sealed class StringDataPointTests
{
    private const int DeclaredCapacity = 82;

    private static readonly StringDataPoint Label =
        new(DefaultTagPath, DefaultPollFrequency, NoChannels, new StringMaxLength(DeclaredCapacity));

    [Fact]
    public void ItNamesItselfAsStudio5000SpellsIt()
    {
        // Arrange

        // Act
        var dataTypeName = Label.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("STRING"));
    }

    [Fact]
    public void AStringEngineValueIsCarriedThrough()
    {
        // Arrange

        // Act
        var conversion = Label.ConvertValue("Hi");

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().Be("Hi");
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange

        // Act
        var conversion = Label.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(Label);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagAddress.Value).And.Contain("STRING");
    }

    [Fact]
    public void AnIntegerIsRefusedNamingBothTypes()
    {
        // Arrange

        // Act
        var conversion = Label.ConvertValue(42);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(string).ToString()).And.Contain(typeof(int).ToString());
    }

    [Theory]
    [InlineData(DeclaredCapacity, true)]
    [InlineData(DeclaredCapacity + 1, false)]
    public void AValueIsInRangeWhileItFitsTheDeclaredCapacity(int length, bool expectedInRange)
    {
        // Arrange

        // Act
        var value = Label.CreateTypedValue(new string('X', length));

        // Assert
        value.IsInValueRange().Should().Be(expectedInRange);
    }
}
