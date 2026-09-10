using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;
using ViciOne.Suite.DataPort.Extensions.Verification;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80;

/// <summary>
/// What holds for the controller as a whole rather than for one type. Run
/// <see cref="EveryTypeInTheVocabularyIsDeclaredAsItIsConfigured"/> first against a newly provisioned
/// controller: it turns a wrong assumption in <see cref="TagAddresses"/> into a named disagreement.
/// </summary>
public sealed class GeneralIntegrationTests(ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(output)
{
    [Fact]
    public void TheControllerUnderTestIsConnected()
    {
        // Arrange
        // The base class connected in InitializeAsync.

        // Act
        var isConnected = Client.IsConnected;

        // Assert
        isConnected.Should().BeTrue();
    }

    [Fact]
    public async Task EveryTypeInTheVocabularyIsDeclaredAsItIsConfigured()
    {
        // Arrange
        var dataPoints = TheWholeVocabulary();
        var verifier = new LogixConfigurationVerifier(Client);

        // Act
        var misconfigured = await verifier.Verify(dataPoints, TestContext.Current.CancellationToken);

        // Assert
        // Written out before the assertion, because a collection assertion truncates the reasons.
        Report(dataPoints.Count, misconfigured);
        misconfigured.Should().BeEmpty(
            "every tag this folder assumes should exist on the controller, declared as the type and shape "
            + "it is configured as");
    }

    [Fact]
    public async Task EveryTagConfiguredAsTheWrongTypeIsReported()
    {
        // Arrange
        var misconfigured = TagsConfiguredAsTheWrongType();
        var verifier = new LogixConfigurationVerifier(Client);

        // Act
        var reported = await verifier.Verify(misconfigured, TestContext.Current.CancellationToken);

        // Assert
        Report(misconfigured.Count, reported);
        reported.Select(dataPoint => dataPoint.DataPoint).Should().BeEquivalentTo(misconfigured,
            "a scalar configured as the wrong type is caught at connect, not misread at poll time");
    }

    [Fact]
    public async Task EveryReportedMismatchNamesTheTagItIsAbout()
    {
        // Arrange
        var verifier = new LogixConfigurationVerifier(Client);

        // Act
        var reported = await verifier.Verify(
            TagsConfiguredAsTheWrongType(), TestContext.Current.CancellationToken);

        // Assert
        reported.Should().AllSatisfy(dataPoint =>
            dataPoint.MismatchingConfigurations.Should().ContainSingle()
                .Which.Value.Should().Contain(dataPoint.DataPoint.TagName.Value));
    }

    // Every pairing is a tag that exists, configured as a type it is not — the case a round trip cannot
    // reach, because the read decodes the bytes as whatever was configured and hands back a plausible
    // value. SINT-on-a-USINT is that with the width taken away: every byte pattern is legal for both.
    private static IReadOnlyList<ILogixDataPoint> TagsConfiguredAsTheWrongType() =>
    [
        new SIntDataPoint(new TagName(TagAddresses.USInt), DefaultPollFrequency, NoChannels),
        new DIntDataPoint(new TagName(TagAddresses.Int), DefaultPollFrequency, NoChannels),
        new IntDataPoint(new TagName(TagAddresses.Real), DefaultPollFrequency, NoChannels),
        new RealDataPoint(new TagName(TagAddresses.LReal), DefaultPollFrequency, NoChannels),
        new DIntDataPoint(new TagName(TagAddresses.String), DefaultPollFrequency, NoChannels),
        new StringDataPoint(
            new TagName(TagAddresses.DInt), DefaultPollFrequency, NoChannels, TagAddresses.StringCapacity),
        new IntDataPoint(new TagName(TagAddresses.IntArray), DefaultPollFrequency, NoChannels),
        new IntArrayDataPoint(
            new TagName(TagAddresses.Int), DefaultPollFrequency, NoChannels, TagAddresses.ArrayElementCount),
    ];

    private static IReadOnlyList<ILogixDataPoint> TheWholeVocabulary() =>
    [
        new BoolDataPoint(new TagName(TagAddresses.Bool), DefaultPollFrequency, NoChannels),
        new SIntDataPoint(new TagName(TagAddresses.SInt), DefaultPollFrequency, NoChannels),
        new IntDataPoint(new TagName(TagAddresses.Int), DefaultPollFrequency, NoChannels),
        new DIntDataPoint(new TagName(TagAddresses.DInt), DefaultPollFrequency, NoChannels),
        new LIntDataPoint(new TagName(TagAddresses.LInt), DefaultPollFrequency, NoChannels),
        new USIntDataPoint(new TagName(TagAddresses.USInt), DefaultPollFrequency, NoChannels),
        new UIntDataPoint(new TagName(TagAddresses.UInt), DefaultPollFrequency, NoChannels),
        new UDIntDataPoint(new TagName(TagAddresses.UDInt), DefaultPollFrequency, NoChannels),
        new ULIntDataPoint(new TagName(TagAddresses.ULInt), DefaultPollFrequency, NoChannels),
        new RealDataPoint(new TagName(TagAddresses.Real), DefaultPollFrequency, NoChannels),
        new LRealDataPoint(new TagName(TagAddresses.LReal), DefaultPollFrequency, NoChannels),
        new StringDataPoint(
            new TagName(TagAddresses.String), DefaultPollFrequency, NoChannels, TagAddresses.StringCapacity),
        new SIntArrayDataPoint(
            new TagName(TagAddresses.SIntArray), DefaultPollFrequency, NoChannels,
            TagAddresses.ArrayElementCount),
        new IntArrayDataPoint(
            new TagName(TagAddresses.IntArray), DefaultPollFrequency, NoChannels,
            TagAddresses.ArrayElementCount),
    ];

    private void Report(
        int verifiedCount, IReadOnlyList<MisconfiguredDataPoint<ILogixDataPoint>> misconfigured)
    {
        Output.WriteLine($"Verified {verifiedCount} configured tags against the controller.");
        foreach (var reason in misconfigured.SelectMany(
                     dataPoint => dataPoint.MismatchingConfigurations.Select(mismatch => mismatch.Value)))
        {
            Output.WriteLine($"  {reason}");
        }
    }
}
