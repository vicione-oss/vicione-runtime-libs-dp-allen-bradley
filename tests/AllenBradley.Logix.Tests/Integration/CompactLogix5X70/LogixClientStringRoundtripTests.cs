using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X70;

/// <summary>
/// A full write/read round trip of one <c>STRING</c> point through the production client stack. The
/// suite writes <c>strValue1</c>; that is what the tag is for, so nothing is restored.
/// </summary>
public sealed class LogixClientStringRoundtripTests : LogixIntegrationTestBase
{
    // Exactly 82 characters — the built-in STRING's .DATA[82], filled to the last byte.
    private const string FullLengthValue =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor in.";

    /// <summary>Each value written, with what Latin-1 storage gives back for it.</summary>
    public static TheoryData<string, string> StringEncodingTestData => new()
    {
        { "Hello World", "Hello World" },
        { "ÀÉÑÖß", "ÀÉÑÖß" },
        // Outside Latin-1 — one '?' per character.
        { "A€BДC中DשE", "A?B?C?D?E" },
        { "Hello\r\nWorld", "Hello\r\nWorld" },
        { "Hello\tWorld", "Hello\tWorld" },
        { "", "" },
        { FullLengthValue, FullLengthValue },
    };

    [Theory]
    [MemberData(nameof(StringEncodingTestData))]
    public async Task AWrittenStringComesBackThroughTheClientStackAsLatin1Stored(
        string valueToWrite, string expectedValue)
    {
        // Arrange
        var dataPoint = StringTag();
        var cancellationToken = TestContext.Current.CancellationToken;
        ILogixDataPoint[] dataPoints = [dataPoint];

        // Act
        await Client.WriteAsync([dataPoint.CreateLogixValue(valueToWrite)], cancellationToken);
        var readResult = await Client.ReadAsync(
            new LogixDataPointGroup(DefaultPollFrequency, dataPoints), cancellationToken);

        // Assert
        readResult.Should().ContainSingle().Which.Should().Be(dataPoint.CreateLogixValue(expectedValue));
    }

    [Fact]
    public void TheControllerDeclaresTheStringTagAsAScalarStructureOfEightyTwoCharacters()
    {
        // Arrange
        var dataPoint = StringTag();

        // Act
        var metadata = TagManager.TagFor(dataPoint).Metadata;

        // Assert
        var expected = new TagDefinition(
            new TagAddress(BenchControllerTags.StrValue1),
            AllenBradleyDataType.String,
            StringMaxLength.Standard,
            DimensionCount.Scalar,
            new ElementCount(1));
        metadata.Should().Be(expected);
    }

    private static StringDataPoint StringTag() =>
        new(TagPath.Parse(BenchControllerTags.StrValue1), DefaultPollFrequency, NoChannels, StringMaxLength.Standard);
}
