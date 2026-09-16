using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Arrays.Booleans;

public sealed class BoolArrayDataPointTests
{
    private const int DeclaredBitCount = 32;

    private static readonly BoolArrayDataPoint Flags =
        new(DefaultTagPath, DefaultPollFrequency, NoChannels, new ElementCount(DeclaredBitCount));

    [Fact]
    public void ItNamesItselfAsStudio5000SpellsItWithoutTheDeclaredLength()
    {
        // Arrange

        // Act
        var dataTypeName = Flags.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("BOOL[]"));
    }

    [Fact]
    public void ItAndItsConverterNameTheSameType()
    {
        // Arrange

        // Act
        var converter = DataPointConverterRegistry.GetConverter(Flags);

        // Assert
        converter.ExpectedTypeName.Should().Be("BOOL[]");
    }

    /// <summary>Each declared bit count with the number of 32-bit words the controller packs it into.</summary>
    public static TheoryData<uint, uint> BitCountsAndTheirWords =>
        new()
        {
            { 32, 1 },
            { 64, 2 },
            { 320, 10 },
        };

    [Theory]
    [MemberData(nameof(BitCountsAndTheirWords))]
    public void ItCountsTheWordsItsBitsArePackedInto(uint declaredBitCount, uint expectedWordCount)
    {
        // Arrange
        var flags = Flags with { ElementCount = new ElementCount(declaredBitCount) };

        // Act
        var wordCount = flags.WordCount;

        // Assert
        wordCount.Should().Be(new ElementCount(expectedWordCount));
    }

    [Fact]
    public void ABoolArrayEngineValueIsCarriedThrough()
    {
        // Arrange
        var flags = new bool[DeclaredBitCount];

        // Act
        var conversion = Flags.ConvertValue(flags);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().BeSameAs(flags);
    }

    [Fact]
    public void AByteArrayOfOneEntryPerBitIsRefusedRatherThanReadAsFlags()
    {
        // Arrange
        var flagsAsBytes = new byte[DeclaredBitCount];

        // Act
        var conversion = Flags.ConvertValue(flagsAsBytes);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(bool[]).ToString()).And.Contain(typeof(byte[]).ToString());
    }

    [Fact]
    public void ASingleBoolIsRefusedNamingBothTypes()
    {
        // Arrange

        // Act
        var conversion = Flags.ConvertValue(true);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(typeof(bool[]).ToString()).And.Contain(typeof(bool).ToString());
    }

    [Fact]
    public void ANullEngineValueIsRefusedNamingThePointAndItsType()
    {
        // Arrange

        // Act
        var conversion = Flags.ConvertValue(null);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(Flags);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(DefaultTagAddress.Value).And.Contain("BOOL[]");
    }

    [Theory]
    [InlineData(DeclaredBitCount, true)]
    [InlineData(DeclaredBitCount - 1, false)]
    [InlineData(DeclaredBitCount + 1, false)]
    public void AValueIsInRangeOnlyAtTheDeclaredLength(int length, bool expectedInRange)
    {
        // Arrange

        // Act
        var value = Flags.CreateTypedValue(new bool[length]);

        // Assert
        value.IsInValueRange().Should().Be(expectedInRange);
    }
}
