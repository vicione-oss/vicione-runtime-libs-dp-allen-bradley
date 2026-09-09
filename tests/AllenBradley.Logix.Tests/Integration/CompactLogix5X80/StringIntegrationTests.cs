using System.Globalization;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80;

/// <summary>
/// The one type in the vocabulary that is a structure on the wire, that has a configured capacity, and
/// whose characters go through an encoding. Every case also asserts the declaration, because a round
/// trip of a short value says nothing about how much the tag holds.
/// </summary>
public sealed class StringIntegrationTests(ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(output)
{
    /// <summary>Each value written, with what Latin-1 storage gives back for it.</summary>
    public static TheoryData<string, string> ShortValues => new()
    {
        { "Hello World", "Hello World" }, // plain ASCII
        { "ÀÉÑÖß", "ÀÉÑÖß" }, // Latin-1 extended, preserved
        { "A€BДC中DשE", "A?B?C?D?E" }, // outside Latin-1 — one '?' per character, lossy by design
        { string.Empty, string.Empty }, // .LEN = 0, nothing in .DATA
    };

    [Theory]
    [MemberData(nameof(ShortValues))]
    public async Task AStringValueRoundTripsAndItsTagIsDeclaredWithItsCapacity(
        string valueToWrite, string expectedValue)
    {
        // Arrange

        // Act
        // Assert
        await AssertRoundTripAsync(
            StringTag(),
            valueToWrite,
            expectedValue,
            ExpectedTagDefinitions.StringScalar(TagAddresses.String, TagAddresses.StringCapacity));
    }

    [Fact]
    public async Task AStringFillingTheDeclaredCapacityRoundTrips()
    {
        // Arrange
        // Written from the configured capacity rather than a literal, so overriding the capacity moves
        // this case with it instead of turning it into a rejection.
        var valueToWrite = new string('X', TagAddresses.StringCapacity.Value);

        // Act
        // Assert
        await AssertRoundTripAsync(
            StringTag(),
            valueToWrite,
            ExpectedTagDefinitions.StringScalar(TagAddresses.String, TagAddresses.StringCapacity));
    }

    [Fact]
    public async Task AStringLongerThanTheDeclaredCapacityIsRefusedBeforeAnythingIsSent()
    {
        // Arrange
        // A truncating write would report success for a value the caller never asked for.
        var dataPoint = StringTag();
        var tooLong = new string('X', TagAddresses.StringCapacity.Value + 1);
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        var write = await Record.ExceptionAsync(
            () => Client.WriteAsync([dataPoint.CreateLogixValue(tooLong)], cancellationToken).AsTask());

        // Assert
        write.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Contain(TagAddresses.String)
            .And.Contain(TagAddresses.StringCapacity.Value.ToString(CultureInfo.InvariantCulture));

        ILogixDataPoint[] dataPoints = [dataPoint];
        var readResult = await Client.ReadAsync(
            new LogixDataPointGroup(DefaultPollFrequency, dataPoints), cancellationToken);
        readResult.Should().ContainSingle().Which.Value.Should().NotBe(tooLong);
    }

    private static StringDataPoint StringTag() =>
        new(new TagName(TagAddresses.String), DefaultPollFrequency, NoChannels, TagAddresses.StringCapacity);
}
