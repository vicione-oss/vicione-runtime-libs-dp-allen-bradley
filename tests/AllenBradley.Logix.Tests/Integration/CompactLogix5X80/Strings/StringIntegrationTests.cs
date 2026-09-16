using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Strings;

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
        { "Hello World", "Hello World" },
        { "ÀÉÑÖß", "ÀÉÑÖß" },
        // Outside Latin-1 — one '?' per character, lossy by design.
        { "A€BДC中DשE", "A?B?C?D?E" },
        { string.Empty, string.Empty },
    };

    [Theory]
    [MemberData(nameof(ShortValues))]
    public async Task AStringValueRoundTripsAndItsTagIsDeclaredWithItsCapacity(
        string valueToWrite, string expectedValue)
    {
        // Arrange
        var dataPoint = StringTag();

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.StringScalar(TagAddresses.String, TagAddresses.StringCapacity);
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, expectedDefinition),
            dataPoint.CreateLogixValue(expectedValue));

        roundTripResult.Should().Be(expectedResult);
    }

    [Fact]
    public async Task AStringFillingTheDeclaredCapacityRoundTrips()
    {
        // Arrange
        var dataPoint = StringTag();
        var valueToWrite = new string('X', TagAddresses.StringCapacity.Value);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.StringScalar(TagAddresses.String, TagAddresses.StringCapacity);
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, expectedDefinition),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }

    [Fact]
    public async Task AStringLongerThanTheDeclaredCapacityIsRefused()
    {
        // Arrange
        var dataPoint = StringTag();
        var tooLong = new string('X', TagAddresses.StringCapacity.Value + 1);

        // Act
        var writing = Client.Awaiting(client =>
            client.WriteAsync([dataPoint.CreateLogixValue(tooLong)], TestContext.Current.CancellationToken).AsTask());

        // Assert
        await writing.Should().ThrowAsync<InvalidOperationException>();
    }

    private static StringDataPoint StringTag() =>
        new(TagPath.Parse(TagAddresses.String), DefaultPollFrequency, NoChannels, TagAddresses.StringCapacity);
}
