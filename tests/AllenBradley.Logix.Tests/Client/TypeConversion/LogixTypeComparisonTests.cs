using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion;

/// <summary>
/// The type rule itself, read against real converters rather than a stand-in: an elementary type and a
/// structure are the two sets of constants it has to serve, and they are opposites of each other.
/// </summary>
public class LogixTypeComparisonTests
{
    private static readonly IDataPointConverter DIntCodec = new DIntConverter();

    private static readonly IDataPointConverter StringCodec = new LogixStringConverter();

    private static TagDefinition Atomic(AllenBradleyDataType type, int dimensionCount = 0) =>
        new(
            new TagName("Tag"),
            LogixTypeKind.Atomic,
            type,
            MaxLength: null,
            new DimensionCount(dimensionCount),
            new ElementCount(1));

    private static TagDefinition Structure(int maxLength = 82, int dimensionCount = 0) =>
        new(
            new TagName("Tag"),
            LogixTypeKind.Structure,
            AllenBradleyDataType.String,
            new StringMaxLength(maxLength),
            new DimensionCount(dimensionCount),
            new ElementCount(1));

    private static LogixTypeMismatch Compare(
        IDataPointConverter converter, ILogixDataPoint dataPoint, TagDefinition? declaration) =>
        LogixTypeComparison.Compare(converter, new ResolvedDataPoint(dataPoint, declaration));

    [Fact]
    public void Compare_WhenTheTagIsAbsentFromTheSymbolTable_ReportsNoMismatch()
    {
        // Arrange
        // Nothing to compare is not a contradiction. The verifier reports an absent tag as absent before
        // it asks, so this only pins that the rule itself does not invent a mismatch out of a null.

        // Act
        var mismatch = Compare(DIntCodec, new DIntDataPoint(new TagName("Motor.Speed"), DefaultPollFrequency, NoChannels), declaration: null);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.None);
    }

    [Fact]
    public void Compare_AnElementaryTagOfTheExpectedType_ReportsNoMismatch()
    {
        // Arrange

        // Act
        var mismatch = Compare(DIntCodec, new DIntDataPoint(new TagName("Motor.Speed"), DefaultPollFrequency, NoChannels), Atomic(AllenBradleyDataType.Dint));

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.None);
    }

    [Fact]
    public void Compare_WhenTheControllerReportsAnotherElementaryType_ReportsAtomicType()
    {
        // Arrange

        // Act
        var mismatch = Compare(DIntCodec, new DIntDataPoint(new TagName("Motor.Speed"), DefaultPollFrequency, NoChannels), Atomic(AllenBradleyDataType.Real));

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.AtomicType);
    }

    [Fact]
    public void Compare_WhenTheControllerReportsAStructureForAnElementaryType_ReportsStructure()
    {
        // Arrange

        // Act
        var mismatch = Compare(DIntCodec, new DIntDataPoint(new TagName("Motor"), DefaultPollFrequency, NoChannels), Structure());

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.Structure);
    }

    [Fact]
    public void Compare_WhenTheControllerReportsAnElementaryTypeForAStructure_ReportsAtomic()
    {
        // Arrange
        // The inverse of the case above, and what a STRING configured onto a DINT tag looks like.

        // Act
        var mismatch = Compare(StringCodec, new StringDataPoint(new TagName("Label"), DefaultPollFrequency, NoChannels, new StringMaxLength(StringMaxLength.Standard.Value)), Atomic(AllenBradleyDataType.Dint));

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.Atomic);
    }

    [Fact]
    public void Compare_AScalarStructureOfTheConfiguredCapacity_ReportsNoMismatch()
    {
        // Arrange

        // Act
        var mismatch = Compare(StringCodec, new StringDataPoint(new TagName("Label"), DefaultPollFrequency, NoChannels, new StringMaxLength(StringMaxLength.Standard.Value)), Structure());

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.None);
    }

    [Fact]
    public void Compare_WhenTheDeclaredCapacityIsSmaller_ReportsStringCapacity()
    {
        // Arrange
        // A STRING configured onto a STRING_20: the shape agrees, the capacity does not, and nothing in
        // a round trip of a short value would show it.

        // Act
        var mismatch = Compare(StringCodec, new StringDataPoint(new TagName("Label"), DefaultPollFrequency, NoChannels, new StringMaxLength(StringMaxLength.Standard.Value)), Structure(maxLength: 20));

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.StringCapacity);
    }

    [Fact]
    public void Compare_WhenTheDeclaredCapacityIsLarger_ReportsStringCapacity()
    {
        // Arrange
        // A capacity is an equality, not a bound: a STRING configured onto a STRING_100 sizes every
        // write buffer 18 bytes short of the tag, which is a misconfiguration in the same way.

        // Act
        var mismatch = Compare(StringCodec, new StringDataPoint(new TagName("Label"), DefaultPollFrequency, NoChannels, new StringMaxLength(StringMaxLength.Standard.Value)), Structure(maxLength: 100));

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.StringCapacity);
    }

    [Fact]
    public void Compare_WhenTheControllerReportsAnArrayOfTheExpectedType_ReportsArrayBeforeAnythingElse()
    {
        // Arrange
        // An array is the wrong shape whatever its elements hold, so the element's own type is not the
        // interesting fact — and this is the one ordering the verifier's messages cannot show.

        // Act
        var mismatch = Compare(
            DIntCodec, new DIntDataPoint(new TagName("Counts"), DefaultPollFrequency, NoChannels), Atomic(AllenBradleyDataType.Dint, dimensionCount: 1));

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.Array);
    }

    [Fact]
    public void Compare_WhenTheControllerReportsAnArrayOfStructures_ReportsArrayBeforeCapacity()
    {
        // Arrange

        // Act
        var mismatch = Compare(
            StringCodec, new StringDataPoint(new TagName("Labels"), DefaultPollFrequency, NoChannels, new StringMaxLength(StringMaxLength.Standard.Value)), Structure(maxLength: 20, dimensionCount: 1));

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.Array);
    }
}
