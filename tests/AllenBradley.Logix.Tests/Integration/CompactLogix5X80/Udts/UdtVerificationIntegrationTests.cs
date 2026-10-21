using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Counters;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Timers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Udts;

/// <summary>
/// What connect makes of the members of <see cref="TagAddresses.Udt"/>. Each is declared as its template
/// says, and each disagreement with the template is reported by name. Run
/// <see cref="EveryConfiguredMemberPassesVerification"/> first against a newly provisioned controller,
/// because it turns a wrong assumption about <c>UDT_Outer</c> or <c>UDT_Inner</c> into a named disagreement.
/// </summary>
public abstract class UdtVerificationIntegrationTests(
    TestControllerFixture fixture,
    ITestOutputHelper output,
    TagScope scope)
    : CompactLogix5X80IntegrationTestBase(fixture, output, scope)
{
    [Fact]
    public async Task EveryConfiguredMemberPassesVerification()
    {
        // Arrange
        var dataPoints = EveryMember();
        var verifier = new LogixConfigurationVerifier(Client);

        // Act
        var misconfigured = await verifier.Verify(dataPoints, TestContext.Current.CancellationToken);

        // Assert
        // The reasons are written out first, because a collection assertion truncates them.
        foreach (var reason in misconfigured.SelectMany(dataPoint => dataPoint.MismatchingConfigurations))
        {
            Output.WriteLine($"  {reason.Value}");
        }

        misconfigured.Should().BeEmpty(
            "every member this folder assumes should exist on the controller, declared as the type and shape it "
            + "is configured as");
    }

    [Fact]
    public async Task AMemberOfTheWrongTypeIsReported()
    {
        // Arrange
        var misconfigured = MembersConfiguredAsTheWrongType();
        var verifier = new LogixConfigurationVerifier(Client);

        // Act
        var reported = await verifier.Verify(misconfigured, TestContext.Current.CancellationToken);

        // Assert
        reported.Select(dataPoint => dataPoint.DataPoint).Should().BeEquivalentTo(misconfigured,
            "a member configured as the wrong type is caught at connect, not misread at poll time");
    }

    [Fact]
    public async Task AMissingMemberIsReported()
    {
        // Arrange
        var missing = new DIntDataPoint(TagPath.Parse(Tags.Udt.Missing), DefaultPollFrequency, NoChannels);
        var verifier = new LogixConfigurationVerifier(Client);

        // Act
        var reported = await verifier.Verify([missing], TestContext.Current.CancellationToken);

        // Assert
        var expected = $"RootTagName '{Tags.Udt.Missing}' was not found on the controller.";
        reported.Should().ContainSingle()
            .Which.MismatchingConfigurations.Should().ContainSingle()
            .Which.Value.Should().Be(expected);
    }

    [Fact]
    public async Task AnElementPastTheEndIsReported()
    {
        // Arrange
        var element = new DIntDataPoint(
            TagPath.Parse(TagAddresses.ArrayElement(Tags.Udt.DIntArray, (int)TagAddresses.ArrayElementCount.Value)),
            DefaultPollFrequency,
            NoChannels);
        var verifier = new LogixConfigurationVerifier(Client);

        // Act
        var reported = await verifier.Verify([element], TestContext.Current.CancellationToken);

        // Assert
        var expected = $"RootTagName '{element.TagAddress.Value}' was not found on the controller.";
        reported.Should().ContainSingle()
            .Which.MismatchingConfigurations.Should().ContainSingle()
            .Which.Value.Should().Be(expected);
    }

    private IReadOnlyList<ILogixDataPoint> MembersConfiguredAsTheWrongType() =>
    [
        new IntDataPoint(TagPath.Parse(Tags.Udt.DInt), DefaultPollFrequency, NoChannels),
        new DIntDataPoint(TagPath.Parse(Tags.Udt.DIntArray), DefaultPollFrequency, NoChannels),
        new StringDataPoint(
            TagPath.Parse(Tags.Udt.Timer), DefaultPollFrequency, NoChannels, TagAddresses.StringCapacity),
        new StringDataPoint(
            TagPath.Parse(Tags.Udt.String20), DefaultPollFrequency, NoChannels, TagAddresses.StringCapacity),
        new DIntDataPoint(TagPath.Parse(Tags.Udt.Inner.Address), DefaultPollFrequency, NoChannels),
        new BoolDataPoint(
            TagPath.Parse(TagAddresses.ArrayElement(Tags.Udt.Inner.IntArray, 3)), DefaultPollFrequency, NoChannels)
    ];

    private IReadOnlyList<ILogixDataPoint> EveryMember() =>
    [
        new BoolDataPoint(TagPath.Parse(Tags.Udt.Bool), DefaultPollFrequency, NoChannels),
        new BoolDataPoint(TagPath.Parse(Tags.Udt.BoolNeighbour), DefaultPollFrequency, NoChannels),
        new DIntDataPoint(TagPath.Parse(Tags.Udt.DInt), DefaultPollFrequency, NoChannels),
        new StringDataPoint(
            TagPath.Parse(Tags.Udt.String), DefaultPollFrequency, NoChannels, TagAddresses.StringCapacity),
        new StringDataPoint(
            TagPath.Parse(Tags.Udt.String20), DefaultPollFrequency, NoChannels, TagAddresses.String20Capacity),
        new DIntArrayDataPoint(
            TagPath.Parse(Tags.Udt.DIntArray), DefaultPollFrequency, NoChannels, TagAddresses.ArrayElementCount),
        new DIntDataPoint(
            TagPath.Parse(TagAddresses.ArrayElement(Tags.Udt.DIntArray, 3)), DefaultPollFrequency, NoChannels),
        new BoolArrayDataPoint(
            TagPath.Parse(Tags.Udt.BoolArray), DefaultPollFrequency, NoChannels, TagAddresses.BoolArrayElementCount),
        new TimerDataPoint(TagPath.Parse(Tags.Udt.Timer), DefaultPollFrequency, NoChannels),
        new TimerDataPoint(
            TagPath.Parse(TagAddresses.ArrayElement(Tags.Udt.TimerArray, 3)), DefaultPollFrequency, NoChannels),
        new CounterDataPoint(TagPath.Parse(Tags.Udt.Counter), DefaultPollFrequency, NoChannels),
        new BoolDataPoint(TagPath.Parse(Tags.Udt.Inner.Bool), DefaultPollFrequency, NoChannels),
        new RealDataPoint(TagPath.Parse(Tags.Udt.Inner.Real), DefaultPollFrequency, NoChannels),
        new IntArrayDataPoint(
            TagPath.Parse(Tags.Udt.Inner.IntArray), DefaultPollFrequency, NoChannels, TagAddresses.ArrayElementCount),
        new StringDataPoint(
            TagPath.Parse(Tags.Udt.Inner.String), DefaultPollFrequency, NoChannels, TagAddresses.StringCapacity)
    ];

    public sealed class ControllerScope(TestControllerFixture fixture, ITestOutputHelper output)
        : UdtVerificationIntegrationTests(fixture, output, TagScope.Controller);

    public sealed class ProgramScope(TestControllerFixture fixture, ITestOutputHelper output)
        : UdtVerificationIntegrationTests(fixture, output, TagScope.Program);
}
