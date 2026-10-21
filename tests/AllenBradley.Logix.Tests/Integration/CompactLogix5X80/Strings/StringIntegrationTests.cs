using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Strings;

/// <summary>
/// A string is a structure on the wire, has a configured capacity, and encodes its characters. Every case
/// also asserts the declaration, because a round trip of a short value does not show how much the tag holds.
/// </summary>
public abstract class StringIntegrationTests(
    TestControllerFixture fixture,
    ITestOutputHelper output,
    TagScope scope)
    : CompactLogix5X80IntegrationTestBase(fixture, output, scope)
{
    public static TheoryData<string, string> ShortValues => new()
    {
        { "Hello World", "Hello World" },
        { "ÀÉÑÖß", "ÀÉÑÖß" },
        // Each character outside Latin-1 becomes one '?'. The loss is by design.
        { "A€BДC中DשE", "A?B?C?D?E" },
        { string.Empty, string.Empty }
    };

    [Theory]
    [MemberData(nameof(ShortValues))]
    public async Task AStringReadsBackAsLatin1(
        string valueToWrite, string expectedValue)
    {
        // Arrange
        var dataPoint = StringTag();

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedDeclaredType = new DeclaredType(dataPoint.TagAddress, AllenBradleyDataType.String,
            TagAddresses.StringCapacity, DimensionCount.Scalar, new ElementCount(1));
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, expectedDeclaredType),
            dataPoint.CreateLogixValue(expectedValue));

        roundTripResult.Should().Be(expectedResult);
    }

    [Fact]
    public async Task AFullStringReadsBackAsWritten()
    {
        // Arrange
        var dataPoint = StringTag();
        var valueToWrite = new string('X', TagAddresses.StringCapacity.Value);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedDeclaredType = new DeclaredType(dataPoint.TagAddress, AllenBradleyDataType.String,
            TagAddresses.StringCapacity, DimensionCount.Scalar, new ElementCount(1));
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, expectedDeclaredType),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }

    [Fact]
    public async Task AFullString20ReadsBackAsWritten()
    {
        // Arrange
        var dataPoint = new StringDataPoint(
            TagPath.Parse(Tags.String20), DefaultPollFrequency, NoChannels, TagAddresses.String20Capacity);
        var valueToWrite = new string('X', TagAddresses.String20Capacity.Value);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedDeclaredType = new DeclaredType(dataPoint.TagAddress, AllenBradleyDataType.String,
            TagAddresses.String20Capacity, DimensionCount.Scalar, new ElementCount(1));
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, expectedDeclaredType),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }

    [Fact]
    public async Task AStringLongerThanItsCapacityIsRefused()
    {
        // Arrange
        var dataPoint = StringTag();
        var tooLong = new string('X', TagAddresses.StringCapacity.Value + 1);

        // Act
        var writing = Client.Awaiting(client =>
            client.WriteAsync([dataPoint.CreateLogixValue(tooLong)], TestContext.Current.CancellationToken).AsTask());

        // Assert
        await writing.Should().ThrowAsync<LogixTagException>();
    }

    private StringDataPoint StringTag() =>
        new(TagPath.Parse(Tags.String), DefaultPollFrequency, NoChannels, TagAddresses.StringCapacity);

    public sealed class ControllerScope(TestControllerFixture fixture, ITestOutputHelper output)
        : StringIntegrationTests(fixture, output, TagScope.Controller);

    public sealed class ProgramScope(TestControllerFixture fixture, ITestOutputHelper output)
        : StringIntegrationTests(fixture, output, TagScope.Program);
}
