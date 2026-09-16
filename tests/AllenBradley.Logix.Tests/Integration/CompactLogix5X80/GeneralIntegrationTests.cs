using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.FloatingPoints;
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
                .Which.Value.Should().Contain(dataPoint.DataPoint.TagAddress.Value));
    }

    // Every pairing is a tag that exists, configured as a type it is not — the case a round trip cannot
    // reach, because the read decodes the bytes as whatever was configured and hands back a plausible
    // value. SINT-on-a-USINT is that with the width taken away: every byte pattern is legal for both.
    private static IReadOnlyList<ILogixDataPoint> TagsConfiguredAsTheWrongType() =>
    [
        new SIntDataPoint(TagPath.Parse(TagAddresses.USInt), DefaultPollFrequency, NoChannels),
        new DIntDataPoint(TagPath.Parse(TagAddresses.Int), DefaultPollFrequency, NoChannels),
        new IntDataPoint(TagPath.Parse(TagAddresses.Real), DefaultPollFrequency, NoChannels),
        new RealDataPoint(TagPath.Parse(TagAddresses.LReal), DefaultPollFrequency, NoChannels),
        new DIntDataPoint(TagPath.Parse(TagAddresses.String), DefaultPollFrequency, NoChannels),
        new StringDataPoint(
            TagPath.Parse(TagAddresses.DInt), DefaultPollFrequency, NoChannels, TagAddresses.StringCapacity),
        new IntDataPoint(TagPath.Parse(TagAddresses.IntArray), DefaultPollFrequency, NoChannels),
        new IntArrayDataPoint(
            TagPath.Parse(TagAddresses.Int), DefaultPollFrequency, NoChannels, TagAddresses.ArrayElementCount),
    ];

    private static IReadOnlyList<ILogixDataPoint> TheWholeVocabulary() =>
    [
        new BoolDataPoint(TagPath.Parse(TagAddresses.Bool), DefaultPollFrequency, NoChannels),
        new SIntDataPoint(TagPath.Parse(TagAddresses.SInt), DefaultPollFrequency, NoChannels),
        new IntDataPoint(TagPath.Parse(TagAddresses.Int), DefaultPollFrequency, NoChannels),
        new DIntDataPoint(TagPath.Parse(TagAddresses.DInt), DefaultPollFrequency, NoChannels),
        new LIntDataPoint(TagPath.Parse(TagAddresses.LInt), DefaultPollFrequency, NoChannels),
        new USIntDataPoint(TagPath.Parse(TagAddresses.USInt), DefaultPollFrequency, NoChannels),
        new UIntDataPoint(TagPath.Parse(TagAddresses.UInt), DefaultPollFrequency, NoChannels),
        new UDIntDataPoint(TagPath.Parse(TagAddresses.UDInt), DefaultPollFrequency, NoChannels),
        new ULIntDataPoint(TagPath.Parse(TagAddresses.ULInt), DefaultPollFrequency, NoChannels),
        new RealDataPoint(TagPath.Parse(TagAddresses.Real), DefaultPollFrequency, NoChannels),
        new LRealDataPoint(TagPath.Parse(TagAddresses.LReal), DefaultPollFrequency, NoChannels),
        new StringDataPoint(
            TagPath.Parse(TagAddresses.String), DefaultPollFrequency, NoChannels, TagAddresses.StringCapacity),
        new SIntArrayDataPoint(
            TagPath.Parse(TagAddresses.SIntArray), DefaultPollFrequency, NoChannels,
            TagAddresses.ArrayElementCount),
        new IntArrayDataPoint(
            TagPath.Parse(TagAddresses.IntArray), DefaultPollFrequency, NoChannels,
            TagAddresses.ArrayElementCount),
        new DIntArrayDataPoint(
            TagPath.Parse(TagAddresses.DIntArray), DefaultPollFrequency, NoChannels,
            TagAddresses.ArrayElementCount),
        new LIntArrayDataPoint(
            TagPath.Parse(TagAddresses.LIntArray), DefaultPollFrequency, NoChannels,
            TagAddresses.ArrayElementCount),
        new USIntArrayDataPoint(
            TagPath.Parse(TagAddresses.USIntArray), DefaultPollFrequency, NoChannels,
            TagAddresses.ArrayElementCount),
        new UIntArrayDataPoint(
            TagPath.Parse(TagAddresses.UIntArray), DefaultPollFrequency, NoChannels,
            TagAddresses.ArrayElementCount),
        new UDIntArrayDataPoint(
            TagPath.Parse(TagAddresses.UDIntArray), DefaultPollFrequency, NoChannels,
            TagAddresses.ArrayElementCount),
        new ULIntArrayDataPoint(
            TagPath.Parse(TagAddresses.ULIntArray), DefaultPollFrequency, NoChannels,
            TagAddresses.ArrayElementCount),
        new RealArrayDataPoint(
            TagPath.Parse(TagAddresses.RealArray), DefaultPollFrequency, NoChannels,
            TagAddresses.ArrayElementCount),
        new LRealArrayDataPoint(
            TagPath.Parse(TagAddresses.LRealArray), DefaultPollFrequency, NoChannels,
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
