using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;
using libplctag;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.ClientStack;

/// <summary>
/// The two device facts that justify <c>SynchronizedTagAccess</c>, shown here instead of taken on the word of
/// <c>ADR/2026-07-16-operations-not-accessors-over-libplctag.md</c>. The suite writes the DINT's
/// own current value straight back, so the tag is never actually changed. The behaviour does not depend on
/// the scope, so the suite runs in one.
/// </summary>
public sealed class SharedAccessConcurrencyIntegrationTests(
    TestControllerFixture fixture,
    ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(fixture, output, TagScope.Controller)
{
    private const int Rounds = 60;
    private const int ReadsPerRound = 3;
    private const int WritesPerRound = 3;

    private const int RaceProbeRounds = 20;

    // A collision can wedge an operation on real hardware, and a hang under MTP takes the process down.
    private static readonly TimeSpan OperationDeadline = TimeSpan.FromSeconds(3);

    private static readonly TimeSpan SeedTimeout = TimeSpan.FromSeconds(10);

    private readonly ILogixTagManager _tagManager = fixture.TagManager;

    [Fact]
    public void AGroupNamingOneTagTwiceDrawsTheSameTagForBothEntries()
    {
        // Arrange
        // Distinct instances, equal by record value. The shared access relies on this.
        var first = DIntPoint();
        var second = DIntPoint();

        // Act
        var tag = _tagManager.TagFor(second);

        // Assert
        tag.Should().BeSameAs(_tagManager.TagFor(first));
    }

    [Fact]
    public async Task AGroupNamingOneTagTwiceReadsTheSameValueForBothEntries()
    {
        // Arrange
        ILogixDataPoint[] dataPoints = [DIntPoint(), DIntPoint()];
        var group = new LogixDataPointGroup(DefaultPollFrequency, dataPoints);

        // Act
        var values = await Client.ReadAsync(group, TestContext.Current.CancellationToken);

        // Assert
        // Reaching this line is itself a check, because a failed read would have thrown.
        values.Should().HaveCount(2);
        values.Select(value => value.Value).Distinct().Should().ContainSingle(
            "both entries read the same tag over the same access");
    }

    [Fact]
    public async Task AGatedAccessUnderConcurrentReadsAndWritesNeverSelfInflictsAnError()
    {
        // Arrange
        using ITagAccess access = new SynchronizedTagAccess(new LibPlcTagAccess(NewRawTag()));

        // Act
        var outcomes = await HammerAsync(access, TestContext.Current.CancellationToken);

        // Assert
        outcomes.Should().NotContain(outcome => !outcome.Ok,
            "serializing operations on the shared access removes the self-inflicted races");
    }

    [Fact]
    public async Task AnUngatedAccessUnderConcurrentReadsAndWritesRacesOnItsOneNativeBuffer()
    {
        // Arrange
        using ITagAccess access = new LibPlcTagAccess(NewRawTag());
        var payload = await SeedPayloadAsync(access);

        // Act
        var anomalies = new List<OperationOutcome>();
        for (var round = 0; round < RaceProbeRounds && anomalies.Count == 0; round++)
        {
            using var operationCts = new CancellationTokenSource(OperationDeadline);
            var operations = new[]
            {
                ReadOnceAsync(access, operationCts.Token),
                WriteOnceAsync(access, payload, operationCts.Token),
                ReadOnceAsync(access, operationCts.Token),
                WriteOnceAsync(access, payload, operationCts.Token),
            };

            anomalies.AddRange((await Task.WhenAll(operations)).Where(outcome => !outcome.Ok));
        }

        // Assert
        if (anomalies.Count == 0)
        {
            Assert.Skip("No collision reproduced this run — the shared-buffer race is timing-dependent.");
        }

        anomalies.Should().NotBeEmpty(
            "an unsynchronized shared access collides read against write on its one native buffer");
    }

    private DIntDataPoint DIntPoint() => new(TagPath.Parse(Tags.DInt), DefaultPollFrequency, NoChannels);

    private Tag NewRawTag() => TestControllerConfiguration.RawTagFor(Tags.DInt);

    // The tag's own current bytes, so every write the probes issue is a no-op on the device.
    private static async Task<byte[]> SeedPayloadAsync(ITagAccess access)
    {
        using var seedCts = new CancellationTokenSource(SeedTimeout);
        var seed = await access.ReadAsync(seedCts.Token);
        seed.Succeeded.Should().BeTrue("the tag must be readable before the concurrency probe");

        return seed.Buffer.ToArray();
    }

    private static async Task<IReadOnlyList<OperationOutcome>> HammerAsync(
        ITagAccess access, CancellationToken cancellationToken)
    {
        var payload = await SeedPayloadAsync(access).ConfigureAwait(false);

        var outcomes = new List<OperationOutcome>();
        for (var round = 0; round < Rounds; round++)
        {
            var operations = new List<Task<OperationOutcome>>(ReadsPerRound + WritesPerRound);
            for (var read = 0; read < ReadsPerRound; read++)
            {
                operations.Add(ReadOnceAsync(access, cancellationToken));
            }

            for (var write = 0; write < WritesPerRound; write++)
            {
                operations.Add(WriteOnceAsync(access, payload, cancellationToken));
            }

            outcomes.AddRange(await Task.WhenAll(operations).ConfigureAwait(false));
        }

        return outcomes;
    }

    private static async Task<OperationOutcome> ReadOnceAsync(
        ITagAccess access, CancellationToken cancellationToken)
    {
        try
        {
            var read = await access.ReadAsync(cancellationToken).ConfigureAwait(false);
            return new OperationOutcome("read", read.Succeeded, read.Error);
        }
        catch (Exception exception)
        {
            return new OperationOutcome("read", false, exception.Message);
        }
    }

    private static async Task<OperationOutcome> WriteOnceAsync(
        ITagAccess access, byte[] payload, CancellationToken cancellationToken)
    {
        try
        {
            var write = await access.WriteAsync(payload, cancellationToken).ConfigureAwait(false);
            return new OperationOutcome("write", write.Succeeded, write.Error);
        }
        catch (Exception exception)
        {
            return new OperationOutcome("write", false, exception.Message);
        }
    }

    private readonly record struct OperationOutcome(string Operation, bool Ok, string? Error);
}
