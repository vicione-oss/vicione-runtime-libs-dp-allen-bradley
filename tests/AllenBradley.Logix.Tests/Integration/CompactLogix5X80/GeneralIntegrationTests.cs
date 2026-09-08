using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80;

/// <summary>
/// What holds for the CompactLogix 5X80 as a whole rather than for one type: that it can be connected
/// to, that every tag this folder assumes exists and is declared as assumed, and that a tag configured
/// as the wrong type is caught at connect instead of misread at poll time.
/// </summary>
/// <remarks>
/// <see cref="Verify_EveryTypeInTheVocabulary_ReportsNoMisconfiguration"/> is the test to run first
/// against a newly provisioned controller. The tag names in <see cref="TagAddresses"/> are assumptions
/// until a real device confirms them, and this is what turns a wrong one into a named, readable
/// disagreement — <em>which</em> tag, and whether it is missing, the wrong type or the wrong shape —
/// rather than into a round-trip failure per type that all say a read came back Bad.
/// </remarks>
public sealed class GeneralIntegrationTests(ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(output)
{
    [Fact]
    public void Connect_ToTheControllerUnderTest_Succeeds() =>
        // Arrange — the base class connected in InitializeAsync
        // Act
        // Assert
        Client.IsConnected.Should().BeTrue();

    [Fact]
    public async Task Verify_EveryTypeInTheVocabulary_ReportsNoMisconfiguration()
    {
        // Arrange
        var dataPoints = TheWholeVocabulary();
        var verifier = new LogixConfigurationVerifier(Client);

        // Act
        var misconfigured = await verifier.Verify(dataPoints, TestContext.Current.CancellationToken);

        // Assert
        // Written out before the assertion, because the message a collection assertion renders truncates
        // and the reasons are the whole point: each one names its tag and says what disagreed.
        Output.WriteLine($"Verified {dataPoints.Count} configured tags against the controller.");
        foreach (var reason in misconfigured.SelectMany(
                     dataPoint => dataPoint.MismatchingConfigurations.Select(mismatch => mismatch.Value)))
        {
            Output.WriteLine($"  {reason}");
        }

        misconfigured.Should().BeEmpty(
            "every tag this folder assumes should exist on the controller, declared as the type and shape "
            + "it is configured as");
    }

    [Fact]
    public async Task Verify_TagsConfiguredAsTheWrongType_AreEachReported()
    {
        // Arrange
        // Every pairing is a tag that exists, configured as a type it is not — which is the case a round
        // trip cannot reach: the read would decode the bytes as whatever was configured and hand back a
        // plausible value. The last two cross the atomic/structure line, the mismatch that is not merely
        // a different width.
        //
        // SINT-on-a-USINT is the same mismatch with the width taken away. Configuring a type wider than
        // the tag has a backstop even without verification — the decode runs out of buffer and the write
        // is refused as out of bounds — but two types of equal width have none in either direction, and
        // every byte pattern is legal for both. Writing 200 and reading -56 back is all that is left.
        ILogixDataPoint[] misconfigured =
        [
            new SIntDataPoint(new TagName(TagAddresses.USInt), DefaultPollFrequency, NoChannels),
            new DIntDataPoint(new TagName(TagAddresses.Int), DefaultPollFrequency, NoChannels),
            new IntDataPoint(new TagName(TagAddresses.Real), DefaultPollFrequency, NoChannels),
            new RealDataPoint(new TagName(TagAddresses.LReal), DefaultPollFrequency, NoChannels),
            new DIntDataPoint(new TagName(TagAddresses.String), DefaultPollFrequency, NoChannels),
            new StringDataPoint(
                new TagName(TagAddresses.DInt), DefaultPollFrequency, NoChannels, TagAddresses.StringCapacity),
        ];
        var verifier = new LogixConfigurationVerifier(Client);

        // Act
        var reported = await verifier.Verify(misconfigured, TestContext.Current.CancellationToken);

        // Assert
        foreach (var reason in reported.SelectMany(
                     dataPoint => dataPoint.MismatchingConfigurations.Select(mismatch => mismatch.Value)))
        {
            Output.WriteLine(reason);
        }

        reported.Select(dataPoint => dataPoint.DataPoint).Should().BeEquivalentTo(misconfigured,
            "a scalar configured as the wrong type is caught at connect, not misread at poll time");

        // The reason travels with the point: a connect that aborts names the tag, not a count.
        reported.Should().AllSatisfy(dataPoint =>
            dataPoint.MismatchingConfigurations.Should().ContainSingle()
                .Which.Value.Should().Contain(dataPoint.DataPoint.TagName.Value));
    }

    // The whole configured vocabulary, exactly as the round-trip suites configure it — one point per
    // type, at the address configured for that type, with the STRING at its configured capacity.
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
    ];
}
