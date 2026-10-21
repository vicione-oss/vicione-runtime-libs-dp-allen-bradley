using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Counters;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Timers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;
using ViciOne.Suite.DataPort.Extensions.Verification;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.General;

/// <summary>
/// Checks that hold for the controller as a whole. Run
/// <see cref="EveryConfiguredTagPassesVerification"/> first against a newly provisioned
/// controller. It turns a wrong assumption in <see cref="TagAddresses"/> into a named disagreement.
/// </summary>
public abstract class GeneralIntegrationTests(
    TestControllerFixture fixture,
    ITestOutputHelper output,
    TagScope scope)
    : CompactLogix5X80IntegrationTestBase(fixture, output, scope)
{
    [Fact]
    public void TheClientIsConnected()
    {
        // Arrange
        // The collection fixture has already connected.

        // Act
        var isConnected = Client.IsConnected;

        // Assert
        isConnected.Should().BeTrue();
    }

    [Fact]
    public async Task EveryConfiguredTagPassesVerification()
    {
        // Arrange
        var dataPoints = TheWholeVocabulary();
        var verifier = new LogixConfigurationVerifier(Client);

        // Act
        var misconfigured = await verifier.Verify(dataPoints, TestContext.Current.CancellationToken);

        // Assert
        // Written out first, because a collection assertion truncates the reasons.
        Report(dataPoints.Count, misconfigured);
        misconfigured.Should().BeEmpty(
            "every tag this folder assumes should exist on the controller, declared as the type and shape "
            + "it is configured as");
    }

    [Fact]
    public async Task ATagOfTheWrongTypeIsReported()
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
    public async Task AReportNamesItsTag()
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

    [Fact]
    public async Task OnePollReadsTagsElementsAndMembers()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var tag = new DIntDataPoint(TagPath.Parse(Tags.DInt), DefaultPollFrequency, NoChannels);
        var element = new DIntDataPoint(
            TagPath.Parse(TagAddresses.ArrayElement(Tags.DIntArray, 3)), DefaultPollFrequency, NoChannels);
        var member = new DIntDataPoint(TagPath.Parse(Tags.Udt.DInt), DefaultPollFrequency, NoChannels);
        var nestedMember = new RealDataPoint(TagPath.Parse(Tags.Udt.Inner.Real), DefaultPollFrequency, NoChannels);
        var timer = new TimerDataPoint(TagPath.Parse(Tags.Timer), DefaultPollFrequency, NoChannels);
        ILogixDataPointValue[] written =
        [
            tag.CreateLogixValue(11),
            element.CreateLogixValue(22),
            member.CreateLogixValue(33),
            nestedMember.CreateLogixValue(44.5f),
            timer.CreateLogixValue(55)
        ];
        await Client.WriteAsync(written, cancellationToken);

        // Act
        var polled = await ReadAsync([tag, element, member, nestedMember, timer]);

        // Assert
        polled.Should().Equal(written);
    }

    // Every pairing is an existing tag configured as a type it does not have. A round trip cannot reach this
    // case, because the read decodes the bytes as the configured type and returns a plausible value. A SINT
    // on a USINT takes the width difference away, so every byte pattern is legal for both. The STRING on the
    // STRING_20 differs in capacity alone.
    private IReadOnlyList<ILogixDataPoint> TagsConfiguredAsTheWrongType() =>
    [
        new SIntDataPoint(TagPath.Parse(Tags.UsInt), DefaultPollFrequency, NoChannels),
        new DIntDataPoint(TagPath.Parse(Tags.Int), DefaultPollFrequency, NoChannels),
        new IntDataPoint(TagPath.Parse(Tags.Real), DefaultPollFrequency, NoChannels),
        new RealDataPoint(TagPath.Parse(Tags.LReal), DefaultPollFrequency, NoChannels),
        new DIntDataPoint(TagPath.Parse(Tags.String), DefaultPollFrequency, NoChannels),
        new StringDataPoint(
            TagPath.Parse(Tags.DInt), DefaultPollFrequency, NoChannels, TagAddresses.StringCapacity),
        new TimerDataPoint(TagPath.Parse(Tags.DInt), DefaultPollFrequency, NoChannels),
        new DIntDataPoint(TagPath.Parse(Tags.Timer), DefaultPollFrequency, NoChannels),
        new CounterDataPoint(TagPath.Parse(Tags.Timer), DefaultPollFrequency, NoChannels),
        new TimerDataPoint(TagPath.Parse(Tags.Counter), DefaultPollFrequency, NoChannels),
        new StringDataPoint(
            TagPath.Parse(Tags.String20), DefaultPollFrequency, NoChannels, TagAddresses.StringCapacity),
        new TimerDataPoint(
            TagPath.Parse(TagAddresses.ArrayElement(Tags.DIntArray, 3)), DefaultPollFrequency, NoChannels),
        new IntDataPoint(TagPath.Parse(Tags.IntArray), DefaultPollFrequency, NoChannels),
        new IntArrayDataPoint(
            TagPath.Parse(Tags.Int), DefaultPollFrequency, NoChannels, TagAddresses.ArrayElementCount)
    ];

    private IReadOnlyList<ILogixDataPoint> TheWholeVocabulary() =>
        new List<ILogixDataPoint>
        {
            new BoolDataPoint(TagPath.Parse(Tags.Bool), DefaultPollFrequency, NoChannels),
            new SIntDataPoint(TagPath.Parse(Tags.SInt), DefaultPollFrequency, NoChannels),
            new IntDataPoint(TagPath.Parse(Tags.Int), DefaultPollFrequency, NoChannels),
            new DIntDataPoint(TagPath.Parse(Tags.DInt), DefaultPollFrequency, NoChannels),
            new LIntDataPoint(TagPath.Parse(Tags.LInt), DefaultPollFrequency, NoChannels),
            new USIntDataPoint(TagPath.Parse(Tags.UsInt), DefaultPollFrequency, NoChannels),
            new UIntDataPoint(TagPath.Parse(Tags.UInt), DefaultPollFrequency, NoChannels),
            new UDIntDataPoint(TagPath.Parse(Tags.UdInt), DefaultPollFrequency, NoChannels),
            new ULIntDataPoint(TagPath.Parse(Tags.UlInt), DefaultPollFrequency, NoChannels),
            new RealDataPoint(TagPath.Parse(Tags.Real), DefaultPollFrequency, NoChannels),
            new LRealDataPoint(TagPath.Parse(Tags.LReal), DefaultPollFrequency, NoChannels),
            new StringDataPoint(
                TagPath.Parse(Tags.String), DefaultPollFrequency, NoChannels, TagAddresses.StringCapacity),
            new StringDataPoint(
                TagPath.Parse(Tags.String20), DefaultPollFrequency, NoChannels, TagAddresses.String20Capacity),
            new TimerDataPoint(TagPath.Parse(Tags.Timer), DefaultPollFrequency, NoChannels),
            new CounterDataPoint(TagPath.Parse(Tags.Counter), DefaultPollFrequency, NoChannels),
            new TimerDataPoint(
                TagPath.Parse(TagAddresses.ArrayElement(Tags.TimerArray, 3)), DefaultPollFrequency, NoChannels),
            new SIntArrayDataPoint(
                TagPath.Parse(Tags.SIntArray), DefaultPollFrequency, NoChannels,
                TagAddresses.ArrayElementCount),
            new IntArrayDataPoint(
                TagPath.Parse(Tags.IntArray), DefaultPollFrequency, NoChannels,
                TagAddresses.ArrayElementCount),
            new DIntArrayDataPoint(
                TagPath.Parse(Tags.DIntArray), DefaultPollFrequency, NoChannels,
                TagAddresses.ArrayElementCount),
            new LIntArrayDataPoint(
                TagPath.Parse(Tags.LIntArray), DefaultPollFrequency, NoChannels,
                TagAddresses.ArrayElementCount),
            new USIntArrayDataPoint(
                TagPath.Parse(Tags.UsIntArray), DefaultPollFrequency, NoChannels,
                TagAddresses.ArrayElementCount),
            new UIntArrayDataPoint(
                TagPath.Parse(Tags.UIntArray), DefaultPollFrequency, NoChannels,
                TagAddresses.ArrayElementCount),
            new UDIntArrayDataPoint(
                TagPath.Parse(Tags.UdIntArray), DefaultPollFrequency, NoChannels,
                TagAddresses.ArrayElementCount),
            new ULIntArrayDataPoint(
                TagPath.Parse(Tags.UlIntArray), DefaultPollFrequency, NoChannels,
                TagAddresses.ArrayElementCount),
            new RealArrayDataPoint(
                TagPath.Parse(Tags.RealArray), DefaultPollFrequency, NoChannels,
                TagAddresses.ArrayElementCount),
            new LRealArrayDataPoint(
                TagPath.Parse(Tags.LRealArray), DefaultPollFrequency, NoChannels,
                TagAddresses.ArrayElementCount)
        }.AsReadOnly();

    private void Report(
        int verifiedCount, IReadOnlyList<MisconfiguredDataPoint<ILogixDataPoint>> misconfigured)
    {
        Output.WriteLine($"Verified {verifiedCount} configured tags against the controller.");
        foreach (var reason in misconfigured.SelectMany(dataPoint =>
                     dataPoint.MismatchingConfigurations.Select(mismatch => mismatch.Value)))
        {
            Output.WriteLine($"  {reason}");
        }
    }

    public sealed class ControllerScope(TestControllerFixture fixture, ITestOutputHelper output)
        : GeneralIntegrationTests(fixture, output, TagScope.Controller);

    public sealed class ProgramScope(TestControllerFixture fixture, ITestOutputHelper output)
        : GeneralIntegrationTests(fixture, output, TagScope.Program);
}
