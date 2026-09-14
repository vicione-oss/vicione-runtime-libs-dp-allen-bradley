using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Booleans.BoolArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Booleans.BoolArray.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Arrays.Booleans.BoolArray.
    Mapping;

public sealed class BoolArrayNodePropertyValidatorTests
{
    private const int DefaultPollFrequency = 100;

    private readonly BoolArrayNodePropertyValidator _validator = new();

    /// <summary>Each declared count Studio 5000 would accept for a BOOL array.</summary>
    public static TheoryData<uint> CountsThatFillWholeWords => [32, 64, 320];

    [Theory]
    [MemberData(nameof(CountsThatFillWholeWords))]
    public void ACountThatFillsWholeWordsIsAccepted(uint declaredBitCount)
    {
        // Arrange
        var node = BoolArrayNodeWith(
            CreateTagName("Flags"),
            CreatePollFrequency(DefaultPollFrequency),
            CreateElementCount(declaredBitCount));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(1u, "a single BOOL still occupies a whole word")]
    [InlineData(10u, "an ARRAY[0..9] OF BOOL is not a declaration Studio 5000 makes")]
    [InlineData(33u, "one bit past a word needs a second one the tag has not got")]
    public void ACountThatLeavesAWordPartlyOwnedIsRefused(uint declaredBitCount, string invalidBecause)
    {
        // Arrange
        var node = BoolArrayNodeWith(
            CreateTagName("Flags"),
            CreatePollFrequency(DefaultPollFrequency),
            CreateElementCount(declaredBitCount));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse(invalidBecause);
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(LogixArrayNode.ElementCountPropertyName);
    }

    [Fact]
    public void ACountOfZeroIsRefusedOnceAsEmptyRatherThanTwiceAsEmptyAndUnaligned()
    {
        // Arrange
        var node = BoolArrayNodeWith(
            CreateTagName("Flags"), CreatePollFrequency(DefaultPollFrequency), CreateElementCount(0u));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(LogixArrayNode.ElementCountPropertyName);
    }

    [Fact]
    public void AMissingElementCountIsRefusedWithoutTheWordRuleThrowingOnIt()
    {
        // Arrange
        var node = BoolArrayNodeWith(CreateTagName("Flags"), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validating = _validator.Invoking(validator => validator.Validate(node));

        // Assert
        validating.Should().NotThrow().Which.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(LogixArrayNode.ElementCountPropertyName);
    }

    [Fact]
    public void ATagNameStudio5000WouldNotDeclareIsRefusedOnABoolArrayTagToo()
    {
        // Arrange
        var node = BoolArrayNodeWith(
            CreateTagName("1Flags"), CreatePollFrequency(DefaultPollFrequency), CreateElementCount(32u));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixTagNode.TagNamePropertyName);
    }

    private static LinkedNode BoolArrayNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(BoolArrayNode.LinkedNodeTypeId, "Flags", properties);
}
