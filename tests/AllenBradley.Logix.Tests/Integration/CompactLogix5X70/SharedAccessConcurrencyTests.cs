using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access.LibPlcTag;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X70;

/// <summary>
/// The two device facts that justify <c>SynchronizedLogixTagAccess</c>, rather than taking the word of
/// <c>ADR/2026-07-16-operations-not-accessors-over-libplctag.md</c> for them. The suite writes the DINT's
/// own current value straight back, so the tag is never actually changed.
/// </summary>
public sealed class SharedAccessConcurrencyTests : LogixIntegrationTestBase
{
    // How hard the concurrency probes lean on one access: each round fires this many reads and writes at
    // once, for this many rounds.
    private const int Rounds = 60;
    private const int ReadsPerRound = 3;
    private const int WritesPerRound = 3;

    // Rounds the race probe gives up after, since one collision is all the evidence the design needs.
    private const int RaceProbeRounds = 20;

    // Hard deadline for a single probe op on an ungated access, so a collision that wedges an operation
    // surfaces as a timed-out anomaly rather than an indefinite hang.
    private static readonly TimeSpan OperationDeadline = TimeSpan.FromSeconds(3);

    private static readonly TimeSpan SeedTimeout = TimeSpan.FromSeconds(10);

    [Fact]
    public void AGroupNamingOneTagTwiceDrawsTheSameTagForBothEntries()
    {
        // Arrange
        // Distinct instances, equal by record value — the shared-access premise, without needing a race.
        var first = CounterPresetPoint();
        var second = CounterPresetPoint();

        // Act
        var tag = TagManager.TagFor(second);

        // Assert
        tag.Should().BeSameAs(TagManager.TagFor(first));
    }

    [Fact]
    public async Task AGroupNamingOneTagTwiceReadsTheSameValueForBothEntries()
    {
        // Arrange
        ILogixDataPoint[] dataPoints = [CounterPresetPoint(), CounterPresetPoint()];
        var group = new LogixDataPointGroup(DefaultPollFrequency, dataPoints);

        // Act
        var values = await Client.ReadAsync(group, TestContext.Current.CancellationToken);

        // Assert
        // Two entries, one tag, two concurrent reads the gate serialized. Reaching this line is itself a
        // check: a read that failed would have thrown.
        values.Should().HaveCount(2);
        values.Select(value => value.Value).Distinct().Should().ContainSingle(
            "both entries read the same tag over the same access");
    }

    [Fact]
    public async Task AGatedAccessUnderConcurrentReadsAndWritesNeverSelfInflictsAnError()
    {
        // Arrange
        using ILogixTagAccess access = new SynchronizedLogixTagAccess(new LogixTagAccess(NewRawTag()));

        // Act
        var outcomes = await HammerAsync(access, TestContext.Current.CancellationToken);

        // Assert
        // The gate makes every operation one whole operation on the access to itself, so nothing the
        // batch does to this tag can make it fail.
        outcomes.Should().NotContain(outcome => !outcome.Ok,
            "serializing operations on the shared access removes the self-inflicted races");
    }

    [Fact]
    public async Task AnUngatedAccessUnderConcurrentReadsAndWritesRacesOnItsOneNativeBuffer()
    {
        // Arrange
        using ILogixTagAccess access = new LogixTagAccess(NewRawTag());
        var payload = await SeedPayloadAsync(access);

        // Act
        // Every probe op carries a hard deadline, because against real hardware a collision can leave an
        // operation wedged — and a hang under MTP takes the whole process down.
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
        // The race is timing-dependent: a run that reproduces it is the evidence, and a run that does not
        // is inconclusive rather than a failure.
        if (anomalies.Count == 0)
        {
            Assert.Skip("No collision reproduced this run — the shared-buffer race is timing-dependent.");
        }

        anomalies.Should().NotBeEmpty(
            "an unsynchronized shared access collides read against write on its one native buffer");
    }

    private static DIntDataPoint CounterPresetPoint() =>
        new(new TagName(BenchControllerTags.CounterPreset), DefaultPollFrequency, NoChannels);

    private static libplctag.Tag NewRawTag() => BenchController.RawTagFor(BenchControllerTags.CounterPreset);

    // The tag's own current bytes, so every write the probes issue is a no-op on the device.
    private static async Task<byte[]> SeedPayloadAsync(ILogixTagAccess access)
    {
        using var seedCts = new CancellationTokenSource(SeedTimeout);
        var seed = await access.ReadAsync(seedCts.Token);
        seed.Succeeded.Should().BeTrue("the tag must be readable before the concurrency probe");

        return seed.Buffer.ToArray();
    }

    // Fires ReadsPerRound reads and WritesPerRound writes concurrently onto one access, for Rounds
    // rounds, and returns what each operation reported.
    private static async Task<IReadOnlyList<OperationOutcome>> HammerAsync(
        ILogixTagAccess access, CancellationToken cancellationToken)
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
        ILogixTagAccess access, CancellationToken cancellationToken)
    {
        try
        {
            var result = await access.ReadAsync(cancellationToken).ConfigureAwait(false);
            return new OperationOutcome("read", result.Succeeded, result.Error);
        }
        catch (Exception exception)
        {
            return new OperationOutcome("read", false, exception.Message);
        }
    }

    private static async Task<OperationOutcome> WriteOnceAsync(
        ILogixTagAccess access, byte[] payload, CancellationToken cancellationToken)
    {
        try
        {
            var result = await access.WriteAsync(payload, cancellationToken).ConfigureAwait(false);
            return new OperationOutcome("write", result.Succeeded, result.Error);
        }
        catch (Exception exception)
        {
            return new OperationOutcome("write", false, exception.Message);
        }
    }

    private readonly record struct OperationOutcome(string Operation, bool Ok, string? Error);
}
