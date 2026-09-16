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
        new SIntDataPoint(new TagAddress(TagAddresses.USInt), DefaultPollFrequency, NoChannels),
        new DIntDataPoint(new TagAddress(TagAddresses.Int), DefaultPollFrequency, NoChannels),
        new IntDataPoint(new TagAddress(TagAddresses.Real), DefaultPollFrequency, NoChannels),
        new RealDataPoint(new TagAddress(TagAddresses.LReal), DefaultPollFrequency, NoChannels),
        new DIntDataPoint(new TagAddress(TagAddresses.String), DefaultPollFrequency, NoChannels),
        new StringDataPoint(
            new TagAddress(TagAddresses.DInt), DefaultPollFrequency, NoChannels, TagAddresses.StringCapacity),
        new IntDataPoint(new TagAddress(TagAddresses.IntArray), DefaultPollFrequency, NoChannels),
        new IntArrayDataPoint(
            new TagAddress(TagAddresses.Int), DefaultPollFrequency, NoChannels, TagAddresses.ArrayElementCount),
    ];

    private static IReadOnlyList<ILogixDataPoint> TheWholeVocabulary() =>
    [
        new BoolDataPoint(new TagAddress(TagAddresses.Bool), DefaultPollFrequency, NoChannels),
        new SIntDataPoint(new TagAddress(TagAddresses.SInt), DefaultPollFrequency, NoChannels),
        new IntDataPoint(new TagAddress(TagAddresses.Int), DefaultPollFrequency, NoChannels),
        new DIntDataPoint(new TagAddress(TagAddresses.DInt), DefaultPollFrequency, NoChannels),
        new LIntDataPoint(new TagAddress(TagAddresses.LInt), DefaultPollFrequency, NoChannels),
        new USIntDataPoint(new TagAddress(TagAddresses.USInt), DefaultPollFrequency, NoChannels),
        new UIntDataPoint(new TagAddress(TagAddresses.UInt), DefaultPollFrequency, NoChannels),
        new UDIntDataPoint(new TagAddress(TagAddresses.UDInt), DefaultPollFrequency, NoChannels),
        new ULIntDataPoint(new TagAddress(TagAddresses.ULInt), DefaultPollFrequency, NoChannels),
        new RealDataPoint(new TagAddress(TagAddresses.Real), DefaultPollFrequency, NoChannels),
        new LRealDataPoint(new TagAddress(TagAddresses.LReal), DefaultPollFrequency, NoChannels),
        new StringDataPoint(
            new TagAddress(TagAddresses.String), DefaultPollFrequency, NoChannels, TagAddresses.StringCapacity),
        new SIntArrayDataPoint(
            new TagAddress(TagAddresses.SIntArray), DefaultPollFrequency, NoChannels,
            TagAddresses.ArrayElementCount),
        new IntArrayDataPoint(
            new TagAddress(TagAddresses.IntArray), DefaultPollFrequency, NoChannels,
            TagAddresses.ArrayElementCount),
        new DIntArrayDataPoint(
            new TagAddress(TagAddresses.DIntArray), DefaultPollFrequency, NoChannels,
            TagAddresses.ArrayElementCount),
        new LIntArrayDataPoint(
            new TagAddress(TagAddresses.LIntArray), DefaultPollFrequency, NoChannels,
            TagAddresses.ArrayElementCount),
        new USIntArrayDataPoint(
            new TagAddress(TagAddresses.USIntArray), DefaultPollFrequency, NoChannels,
            TagAddresses.ArrayElementCount),
        new UIntArrayDataPoint(
            new TagAddress(TagAddresses.UIntArray), DefaultPollFrequency, NoChannels,
            TagAddresses.ArrayElementCount),
        new UDIntArrayDataPoint(
            new TagAddress(TagAddresses.UDIntArray), DefaultPollFrequency, NoChannels,
            TagAddresses.ArrayElementCount),
        new ULIntArrayDataPoint(
            new TagAddress(TagAddresses.ULIntArray), DefaultPollFrequency, NoChannels,
            TagAddresses.ArrayElementCount),
        new RealArrayDataPoint(
            new TagAddress(TagAddresses.RealArray), DefaultPollFrequency, NoChannels,
            TagAddresses.ArrayElementCount),
        new LRealArrayDataPoint(
            new TagAddress(TagAddresses.LRealArray), DefaultPollFrequency, NoChannels,
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
