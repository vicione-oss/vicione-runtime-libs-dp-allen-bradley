using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion;

/// <summary>
/// The type rule itself, read against a real converter rather than a stand-in.
/// </summary>
/// <remarks>
/// <c>LogixConfigurationVerifierTests</c> covers the same rule through the messages it renders. This is
/// where the ordering — shape before type — is pinned, because those messages cannot show it.
/// </remarks>
public class LogixTypeComparisonTests
{
    private static readonly IDataPointConverter DIntCodec = new DIntConverter();

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

    [Fact]
    public void Compare_WhenTheTagIsAbsentFromTheSymbolTable_ReportsNoMismatch()
    {
        // Arrange

        // Act
        // Nothing to compare against is not a contradiction. The verifier reports the tag as absent
        // before it ever asks here.
        var mismatch = LogixTypeComparison.Compare(DIntCodec, declaration: null);

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.None);
    }

    [Fact]
    public void Compare_AnElementaryTagOfTheExpectedType_ReportsNoMismatch()
    {
        // Arrange

        // Act
        var mismatch = LogixTypeComparison.Compare(DIntCodec, Atomic(AllenBradleyDataType.Dint));

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.None);
    }

    [Fact]
    public void Compare_WhenTheControllerReportsAnotherElementaryType_ReportsAtomicType()
    {
        // Arrange

        // Act
        var mismatch = LogixTypeComparison.Compare(DIntCodec, Atomic(AllenBradleyDataType.Real));

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.AtomicType);
    }

    [Fact]
    public void Compare_WhenTheControllerReportsAStructureForAnElementaryType_ReportsStructure()
    {
        // Arrange

        // Act
        var mismatch = LogixTypeComparison.Compare(DIntCodec, Structure());

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.Structure);
    }

    [Fact]
    public void Compare_WhenTheControllerReportsAnArrayOfTheExpectedType_ReportsArrayBeforeAnythingElse()
    {
        // Arrange

        // Act
        // Shape is settled first: the elements are the type that was configured, and it is still an
        // array where a scalar was asked for.
        var mismatch = LogixTypeComparison.Compare(
            DIntCodec, Atomic(AllenBradleyDataType.Dint, dimensionCount: 1));

        // Assert
        mismatch.Should().Be(LogixTypeMismatch.Array);
    }
}
