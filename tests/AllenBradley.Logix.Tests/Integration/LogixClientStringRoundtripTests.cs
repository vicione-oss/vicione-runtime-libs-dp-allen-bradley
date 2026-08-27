using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration;

/// <summary>
/// A full write/read round trip of one scalar data point through the production client stack —
/// <c>LogixClient</c> → <c>LogixWriteBatch</c>/<c>LogixReadBatch</c> →
/// <c>DataPointConverterRegistry</c> → <c>LogixStringConverter</c> → <c>CachingLogixTagManager</c> →
/// <c>LogixTagAccess</c> → libplctag — against the real CompactLogix L32E.
/// </summary>
/// <remarks>
/// <para>
/// The target is a <c>STRING</c> rather than a DINT on purpose. <c>strValue1</c> is a program tag, so it
/// <em>is</em> in the <c>Program:MainProgram.@tags</c> listing and its metadata is non-null, which is what
/// the ADR-003 comparison needs. <c>Counter.PRE</c>, the read suite's target, is a structure member absent
/// from the flat listing, so nothing about its type can be checked at all.
/// </para>
/// <para>
/// Each case also asserts the declaration the controller reports, spelled out whole and compared as one
/// record so that every field is pinned — the declared capacity above all, which is what nothing in a
/// round trip of a shorter value would show.
/// </para>
/// <para>
/// The suite <b>writes</b> <c>strValue1</c>. That is what the tag is for; no save-and-restore is needed.
/// </para>
/// </remarks>
public class LogixClientStringRoundtripTests : LogixIntegrationTestBase
{
    // Exactly 82 characters — the built-in STRING's .DATA[82], filled to the last byte.
    private const string FullLengthValue =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor in.";

    public static TheoryData<string, string> StringEncodingTestData => new()
    {
        { "Hello World", "Hello World" }, // plain ASCII
        { "ÀÉÑÖß", "ÀÉÑÖß" }, // Latin-1 extended, preserved
        { "A€BДC中DשE", "A?B?C?D?E" }, // outside Latin-1 — one '?' per character
        { "Hello\r\nWorld", "Hello\r\nWorld" }, // CR+LF
        { "Hello\tWorld", "Hello\tWorld" }, // tab
        { "", "" }, // the empty string — .LEN = 0, nothing in .DATA
        { FullLengthValue, FullLengthValue }, // fills .DATA exactly
    };

    [Theory]
    [MemberData(nameof(StringEncodingTestData))]
    public async Task WriteAndReadBack_StringValue_RoundTripsThroughTheClientStack(
        string valueToWrite, string expectedValue)
    {
        // Arrange
        var dataPoint = CreateString(LogixTagAddresses.StrValue1);
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        // Asserting through the tag manager rather than the client, because the client seam has no
        // ResolveDataPoints yet; the manager already joins the controller's declaration onto every tag.
        var metadata = TagManager.TagFor(dataPoint).Metadata;
        await Client.WriteAsync([CreateValue(dataPoint, valueToWrite)], cancellationToken);
        var readResult = await Client.ReadAsync(CreateGroup(dataPoint), cancellationToken);

        // Assert
        metadata.Should().Be(new TagDefinition(
            new TagName(LogixTagAddresses.StrValue1),
            LogixTypeKind.Structure,
            AllenBradleyDataType.String,
            StringMaxLength.Standard,
            DimensionCount.Scalar,
            new ElementCount(1)));

        readResult.Should().ContainSingle();
        readResult[0].DataPoint.Should().Be(dataPoint);
        readResult[0].Quality.Should().Be(LogixQuality.Good);
        readResult[0].Value.Should().Be(expectedValue);
    }
}
