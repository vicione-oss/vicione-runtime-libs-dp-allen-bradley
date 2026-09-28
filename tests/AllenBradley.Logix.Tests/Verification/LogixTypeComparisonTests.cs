using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Timers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.DeclaredTypeTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Verification;

/// <summary>
/// The rule read against real data points rather than a stand-in: a scalar, a string and an array are
/// the three shapes it has to serve. An element arrives as the scalar the lookup declared it.
/// </summary>
public sealed class LogixTypeComparisonTests
{
    private static readonly DIntDataPoint Speed = new(TagPath.Parse("Motor.Speed"), DefaultPollFrequency, NoChannels);

    private static readonly StringDataPoint Label =
        new(TagPath.Parse("Line.Label"), DefaultPollFrequency, NoChannels, StringMaxLength.Standard);

    private static readonly IntArrayDataPoint Readings =
        new(TagPath.Parse("Tank.Readings"), DefaultPollFrequency, NoChannels, TenElements);

    private static readonly TimerDataPoint Delay = new(TagPath.Parse("Line.Delay"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void ATagAbsentFromTheSymbolTableIsNoMismatch()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(Speed, DeclaredType: null);

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.None);
    }

    [Fact]
    public void AnElementaryTagOfTheExpectedTypeIsNoMismatch()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(Speed, DefaultAtomicDeclaredType());

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.None);
    }

    [Fact]
    public void AnElementaryTagOfAnotherTypeIsADataTypeMismatch()
    {
        // Arrange
        var declaration = DefaultAtomicDeclaredType() with { DataType = AllenBradleyDataType.Real };
        var resolved = new ResolvedDataPoint(Speed, declaration);

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.DataType);
    }

    [Fact]
    public void AStringWhereAnElementaryTypeWasConfiguredIsADataTypeMismatch()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(Speed, DefaultStringDeclaredType());

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.DataType);
    }

    [Fact]
    public void AnElementaryTypeWhereAStringWasConfiguredIsADataTypeMismatch()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(Label, DefaultAtomicDeclaredType());

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.DataType);
    }

    [Fact]
    public void ATimerWhereATimerWasConfiguredIsNoMismatch()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(Delay, TimerDeclaredType());

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.None);
    }

    /// <summary>What the controller may declare at a tag that is no timer.</summary>
    public static TheoryData<DeclaredType> DeclaredTypesThatAreNoTimer =>
    [
        DefaultAtomicDeclaredType(),
        DefaultAtomicDeclaredType() with { DataType = AllenBradleyDataType.Structure },
    ];

    [Theory]
    [MemberData(nameof(DeclaredTypesThatAreNoTimer))]
    public void AnythingButATimerWhereATimerWasConfiguredIsADataTypeMismatch(DeclaredType declaration)
    {
        // Arrange
        var resolved = new ResolvedDataPoint(Delay, declaration);

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.DataType);
    }

    [Fact]
    public void AStringOfTheConfiguredCapacityIsNoMismatch()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(Label, DefaultStringDeclaredType());

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.None);
    }

    [Fact]
    public void ASmallerDeclaredCapacityIsAStringCapacityMismatch()
    {
        // Arrange
        var declaration = DefaultStringDeclaredType() with { MaxLength = new StringMaxLength(20) };
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
        var declaration = DefaultStringDeclaredType() with { MaxLength = new StringMaxLength(100) };
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
        var declaration = DefaultAtomicDeclaredType() with { DimensionCount = DimensionCount.OneDimensional };
        var resolved = new ResolvedDataPoint(Speed, declaration);

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.Rank);
    }

    [Fact]
    public void AnArrayOfStringsIsARankMismatchBeforeItsCapacity()
    {
        // Arrange
        var declaration = DefaultStringDeclaredType() with
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
        var resolved = new ResolvedDataPoint(Readings, DefaultIntArrayDeclaredType());

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.None);
    }

    [Fact]
    public void AScalarWhereAnArrayWasConfiguredIsARankMismatch()
    {
        // Arrange
        var declaration = DefaultIntArrayDeclaredType() with
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
        var declaration = DefaultIntArrayDeclaredType() with
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
        var declaration = DefaultIntArrayDeclaredType() with
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
        var declaration = DefaultIntArrayDeclaredType() with
        {
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
        var declaration = DefaultIntArrayDeclaredType() with
        {
            DataType = AllenBradleyDataType.Bool,
            ElementCount = new ElementCount(32),
        };
        var resolved = new ResolvedDataPoint(flags, declaration);

        // Act
        var mismatch = LogixTypeComparison.Compare(resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.ElementCount);
    }

    private static DeclaredType TimerDeclaredType() =>
        DefaultAtomicDeclaredType() with { DataType = AllenBradleyDataType.Timer };
}
