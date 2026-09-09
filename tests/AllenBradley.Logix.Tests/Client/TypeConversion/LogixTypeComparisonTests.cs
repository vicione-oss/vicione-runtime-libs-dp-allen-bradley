using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TagDefinitionTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion;

/// <summary>
/// The rule is read against real converters rather than a stand-in, because an elementary type and a
/// structure are the two sets of constants it has to serve and they are opposites of each other.
/// </summary>
public sealed class LogixTypeComparisonTests
{
    private static readonly IDataPointConverter DIntCodec = new DIntConverter();

    private static readonly IDataPointConverter StringCodec = new LogixStringConverter();

    private static readonly DIntDataPoint Speed = new(new TagName("Motor.Speed"), DefaultPollFrequency, NoChannels);

    private static readonly StringDataPoint Label =
        new(new TagName("Line.Label"), DefaultPollFrequency, NoChannels, StringMaxLength.Standard);

    [Fact]
    public void ATagAbsentFromTheSymbolTableIsNoMismatch()
    {
        // Arrange
        // The verifier reports an absent tag as absent before it asks, so this only pins that the rule
        // does not invent a mismatch out of a null.
        var resolved = new ResolvedDataPoint(Speed, TagDefinition: null);

        // Act
        var mismatch = LogixTypeComparison.Compare(DIntCodec, resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.None);
    }

    [Fact]
    public void AnElementaryTagOfTheExpectedTypeIsNoMismatch()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(Speed, DefaultAtomicTagDefinition());

        // Act
        var mismatch = LogixTypeComparison.Compare(DIntCodec, resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.None);
    }

    [Fact]
    public void AnElementaryTagOfAnotherTypeIsAnAtomicTypeMismatch()
    {
        // Arrange
        var declaration = DefaultAtomicTagDefinition() with { DataType = AllenBradleyDataType.Real };
        var resolved = new ResolvedDataPoint(Speed, declaration);

        // Act
        var mismatch = LogixTypeComparison.Compare(DIntCodec, resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.AtomicType);
    }

    [Fact]
    public void AStructureWhereAnElementaryTypeWasConfiguredIsAStructureMismatch()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(Speed, DefaultStringTagDefinition());

        // Act
        var mismatch = LogixTypeComparison.Compare(DIntCodec, resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.Structure);
    }

    [Fact]
    public void AnElementaryTypeWhereAStructureWasConfiguredIsAnAtomicMismatch()
    {
        // Arrange
        // The inverse of the case above, and what a STRING configured onto a DINT tag looks like.
        var resolved = new ResolvedDataPoint(Label, DefaultAtomicTagDefinition());

        // Act
        var mismatch = LogixTypeComparison.Compare(StringCodec, resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.Atomic);
    }

    [Fact]
    public void AScalarStructureOfTheConfiguredCapacityIsNoMismatch()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(Label, DefaultStringTagDefinition());

        // Act
        var mismatch = LogixTypeComparison.Compare(StringCodec, resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.None);
    }

    [Fact]
    public void ASmallerDeclaredCapacityIsAStringCapacityMismatch()
    {
        // Arrange
        // A STRING configured onto a STRING_20: the shape agrees, the capacity does not, and nothing in a
        // round trip of a short value would show it.
        var declaration = DefaultStringTagDefinition() with { MaxLength = new StringMaxLength(20) };
        var resolved = new ResolvedDataPoint(Label, declaration);

        // Act
        var mismatch = LogixTypeComparison.Compare(StringCodec, resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.StringCapacity);
    }

    [Fact]
    public void ALargerDeclaredCapacityIsAStringCapacityMismatch()
    {
        // Arrange
        // A capacity is an equality, not a bound: a STRING configured onto a STRING_100 sizes every write
        // buffer 18 bytes short of the tag.
        var declaration = DefaultStringTagDefinition() with { MaxLength = new StringMaxLength(100) };
        var resolved = new ResolvedDataPoint(Label, declaration);

        // Act
        var mismatch = LogixTypeComparison.Compare(StringCodec, resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.StringCapacity);
    }

    [Fact]
    public void AnArrayOfTheExpectedTypeIsReportedAsAnArrayBeforeAnythingElse()
    {
        // Arrange
        // An array is the wrong shape whatever its elements hold, and this ordering is the one the
        // verifier's messages cannot show.
        var declaration = DefaultAtomicTagDefinition() with { DimensionCount = new DimensionCount(1) };
        var resolved = new ResolvedDataPoint(Speed, declaration);

        // Act
        var mismatch = LogixTypeComparison.Compare(DIntCodec, resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.Array);
    }

    [Fact]
    public void AnArrayOfStructuresIsReportedAsAnArrayBeforeItsCapacity()
    {
        // Arrange
        var declaration = DefaultStringTagDefinition() with
        {
            MaxLength = new StringMaxLength(20),
            DimensionCount = new DimensionCount(1),
        };
        var resolved = new ResolvedDataPoint(Label, declaration);

        // Act
        var mismatch = LogixTypeComparison.Compare(StringCodec, resolved);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.Array);
    }
}
