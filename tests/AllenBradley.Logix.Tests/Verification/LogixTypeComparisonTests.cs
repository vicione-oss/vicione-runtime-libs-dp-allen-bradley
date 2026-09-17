using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TagDefinitionTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Verification;

/// <summary>
/// The rule read against real data points rather than a stand-in: a scalar, a string, an array and an
/// element are the four shapes it has to serve.
/// </summary>
public sealed class LogixTypeComparisonTests
{
    private static readonly DIntDataPoint Speed = new(TagPath.Parse("Motor.Speed"), DefaultPollFrequency, NoChannels);

    private static readonly StringDataPoint Label =
        new(TagPath.Parse("Line.Label"), DefaultPollFrequency, NoChannels, StringMaxLength.Standard);

    private static readonly IntArrayDataPoint Readings =
        new(TagPath.Parse("Tank.Readings"), DefaultPollFrequency, NoChannels, TenElements);

    private static readonly IntDataPoint ThirdReading =
        new(TagPath.Parse("Tank.Readings[3]"), DefaultPollFrequency, NoChannels);

    private static readonly IntDataPoint EleventhReading =
        new(TagPath.Parse("Tank.Readings[10]"), DefaultPollFrequency, NoChannels);

    private static readonly DIntDataPoint ThirdReadingAsDInt =
        new(TagPath.Parse("Tank.Readings[3]"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void ATagAbsentFromTheSymbolTableIsNoMismatch()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(Speed, TagDefinition: null);

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.None);
    }

    [Fact]
    public void AnElementaryTagOfTheExpectedTypeIsNoMismatch()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(Speed, DefaultAtomicTagDefinition());

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.None);
    }

    [Fact]
    public void AnElementaryTagOfAnotherTypeIsADataTypeMismatch()
    {
        // Arrange
        var declaration = DefaultAtomicTagDefinition() with { DataType = AllenBradleyDataType.Real };
        var resolved = new ResolvedDataPoint(Speed, declaration);

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.DataType);
    }

    [Fact]
    public void AStructureWhereAnElementaryTypeWasConfiguredIsADataTypeMismatch()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(Speed, DefaultStringTagDefinition());

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.DataType);
    }

    [Fact]
    public void AnElementaryTypeWhereAStructureWasConfiguredIsADataTypeMismatch()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(Label, DefaultAtomicTagDefinition());

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.DataType);
    }

    [Fact]
    public void AScalarStructureOfTheConfiguredCapacityIsNoMismatch()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(Label, DefaultStringTagDefinition());

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.None);
    }

    [Fact]
    public void ASmallerDeclaredCapacityIsAStringCapacityMismatch()
    {
        // Arrange
        var declaration = DefaultStringTagDefinition() with { MaxLength = new StringMaxLength(20) };
        var resolved = new ResolvedDataPoint(Label, declaration);

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.StringCapacity);
    }

    [Fact]
    public void ALargerDeclaredCapacityIsAStringCapacityMismatch()
    {
        // Arrange
        var declaration = DefaultStringTagDefinition() with { MaxLength = new StringMaxLength(100) };
        var resolved = new ResolvedDataPoint(Label, declaration);

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.StringCapacity);
    }

    [Fact]
    public void AnArrayWhereAScalarWasConfiguredIsARankMismatchBeforeAnythingElse()
    {
        // Arrange
        var declaration = DefaultAtomicTagDefinition() with { DimensionCount = DimensionCount.OneDimensional };
        var resolved = new ResolvedDataPoint(Speed, declaration);

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.Rank);
    }

    [Fact]
    public void AnArrayOfStructuresIsARankMismatchBeforeItsCapacity()
    {
        // Arrange
        var declaration = DefaultStringTagDefinition() with
        {
            MaxLength = new StringMaxLength(20),
            DimensionCount = DimensionCount.OneDimensional,
        };
        var resolved = new ResolvedDataPoint(Label, declaration);

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.Rank);
    }

    [Fact]
    public void AnArrayOfTheConfiguredTypeAndCountIsNoMismatch()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(Readings, DefaultIntArrayTagDefinition());

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.None);
    }

    [Fact]
    public void AnElementWithinTheDeclaredArrayIsComparedAsAScalarOfItsElementType()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(ThirdReading, DefaultIntArrayTagDefinition());

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.None);
    }

    [Fact]
    public void AnElementOfAnotherTypeThanTheArrayIsADataTypeMismatch()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(ThirdReadingAsDInt, DefaultIntArrayTagDefinition());

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.DataType);
    }

    [Fact]
    public void AnElementPastTheDeclaredCountIsOutOfRange()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(EleventhReading, DefaultIntArrayTagDefinition());

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.ElementIndexOutOfRange);
    }

    [Fact]
    public void AnElementOfATagDeclaredScalarIsAnElementOfScalarMismatch()
    {
        // Arrange
        var declaration = DefaultIntArrayTagDefinition() with
        {
            DimensionCount = DimensionCount.Scalar,
            ElementCount = OneElement,
        };
        var resolved = new ResolvedDataPoint(ThirdReading, declaration);

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.ElementOfScalar);
    }

    [Fact]
    public void AScalarWhereAnArrayWasConfiguredIsARankMismatch()
    {
        // Arrange
        var declaration = DefaultIntArrayTagDefinition() with
        {
            DimensionCount = DimensionCount.Scalar,
            ElementCount = OneElement,
        };
        var resolved = new ResolvedDataPoint(Readings, declaration);

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.Rank);
    }

    [Fact]
    public void AnArrayOfAnotherElementTypeIsADataTypeMismatchBeforeItsCount()
    {
        // Arrange
        var declaration = DefaultIntArrayTagDefinition() with
        {
            DataType = AllenBradleyDataType.Dint,
            ElementCount = new ElementCount(20),
        };
        var resolved = new ResolvedDataPoint(Readings, declaration);

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.DataType);
    }

    [Theory]
    [InlineData(20u)]
    [InlineData(5u)]
    public void AnArrayOfAnotherLengthIsAnElementCountMismatch(uint declaredElementCount)
    {
        // Arrange
        var declaration = DefaultIntArrayTagDefinition() with
        {
            ElementCount = new ElementCount(declaredElementCount),
        };
        var resolved = new ResolvedDataPoint(Readings, declaration);

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.ElementCount);
    }

    [Fact]
    public void ABoolArrayIsComparedInBitsOnBothSidesRatherThanBitsAgainstWords()
    {
        // Arrange
        // TagsDecoder has already turned the controller's two DWORDs into the 64 bits they hold, so the
        // comparison never sees the packing.
        var flags = new BoolArrayDataPoint(
            TagPath.Parse("Line.Flags"), DefaultPollFrequency, NoChannels, new ElementCount(64));
        var declaration = DefaultIntArrayTagDefinition() with
        {
            TagAddress = new TagAddress("Line.Flags"),
            DataType = AllenBradleyDataType.Bool,
            ElementCount = new ElementCount(64),
        };
        var resolved = new ResolvedDataPoint(flags, declaration);

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.None);
    }

    [Fact]
    public void ABoolArrayDeclaredWithAnotherNumberOfWordsIsAnElementCountMismatch()
    {
        // Arrange
        var flags = new BoolArrayDataPoint(
            TagPath.Parse("Line.Flags"), DefaultPollFrequency, NoChannels, new ElementCount(64));
        var declaration = DefaultIntArrayTagDefinition() with
        {
            TagAddress = new TagAddress("Line.Flags"),
            DataType = AllenBradleyDataType.Bool,
            ElementCount = new ElementCount(32),
        };
        var resolved = new ResolvedDataPoint(flags, declaration);

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.ElementCount);
    }
}
