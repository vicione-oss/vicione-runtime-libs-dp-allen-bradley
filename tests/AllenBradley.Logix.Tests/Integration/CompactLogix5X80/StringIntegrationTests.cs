using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80;

/// <summary>
/// Write and read back the <c>STRING</c> tag on the CompactLogix 5X80, through the production client
/// stack. The one type in the vocabulary that is a structure on the wire, that has a configured
/// capacity, and whose characters go through an encoding.
/// </summary>
/// <remarks>
/// The round trip alone would leave the capacity unchecked — writing a short value and reading it back
/// says nothing about how much the tag holds — so every case here also asserts the declaration, which
/// carries it. Two cases go further and put the capacity itself under load: one fills the tag to its
/// last character, and one asks for a character more than it was declared to hold.
/// <para>
/// The exhaustive encoding matrix — every Latin-1 corner, control characters, the lot — is in
/// <c>Integration/CompactLogix5X70/LogixClientStringRoundtripTests</c>. Encoding is a fact about the
/// codec rather than about a controller generation, so it is proved once; what is repeated here is the
/// part that is a fact about <em>this</em> controller.
/// </para>
/// </remarks>
public sealed class StringIntegrationTests(ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(output)
{
    public static TheoryData<string, string> ShortValues => new()
    {
        { "Hello World", "Hello World" }, // plain ASCII
        { "ÀÉÑÖß", "ÀÉÑÖß" }, // Latin-1 extended, preserved
        { "A€BДC中DשE", "A?B?C?D?E" }, // outside Latin-1 — one '?' per character, lossy by design
        { string.Empty, string.Empty }, // .LEN = 0, nothing in .DATA
    };

    [Theory]
    [MemberData(nameof(ShortValues))]
    public async Task WriteAndReadBack_StringValue_RoundTripsAndTheTagIsDeclaredWithItsCapacity(
        string valueToWrite, string expectedValue) =>
        await AssertRoundTripAsync(
            StringTag(),
            valueToWrite,
            expectedValue,
            ExpectedTagDefinitions.StringScalar(TagAddresses.String, TagAddresses.StringCapacity));

    /// <summary>
    /// The tag filled to its last character. Written from the configured capacity rather than a literal,
    /// so that overriding the capacity moves this case with it instead of turning it into a rejection.
    /// </summary>
    [Fact]
    public async Task WriteAndReadBack_StringValue_FillingTheDeclaredCapacity_RoundTrips()
    {
        // Arrange
        var valueToWrite = new string('X', TagAddresses.StringCapacity.Value);

        // Act
        // Assert
        await AssertRoundTripAsync(
            StringTag(),
            valueToWrite,
            ExpectedTagDefinitions.StringScalar(TagAddresses.String, TagAddresses.StringCapacity));
    }

    /// <summary>
    /// One character too many is refused, not truncated — and refused while the batch is being encoded,
    /// so nothing reaches the controller. A truncating write would report success for a value the caller
    /// never asked for.
    /// </summary>
    [Fact]
    public async Task WriteAsync_StringValue_LongerThanTheDeclaredCapacity_IsRefusedBeforeAnythingIsSent()
    {
        // Arrange
        var dataPoint = StringTag();
        var tooLong = new string('X', TagAddresses.StringCapacity.Value + 1);
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        var write = async () =>
            await Client.WriteAsync([dataPoint.CreateLogixValue(tooLong)], cancellationToken);

        // Assert
        (await write.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage($"*{TagAddresses.String}*")
            .And.Message.Should().Contain(TagAddresses.StringCapacity.Value.ToString(
                System.Globalization.CultureInfo.InvariantCulture));

        // And the tag is untouched: a readable tag holding something other than the rejected value.
        ILogixDataPoint[] dataPoints = [dataPoint];
        var readResult = await Client.ReadAsync(
            new LogixDataPointGroup(DefaultPollFrequency, dataPoints), cancellationToken);

        readResult.Should().ContainSingle();
        readResult[0].Value.Should().NotBe(tooLong);
    }

    private static StringDataPoint StringTag() =>
        new(new TagName(TagAddresses.String), DefaultPollFrequency, NoChannels, TagAddresses.StringCapacity);
}
