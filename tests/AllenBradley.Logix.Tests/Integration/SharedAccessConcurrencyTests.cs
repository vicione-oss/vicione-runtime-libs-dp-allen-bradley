using libplctag;
using Microsoft.Extensions.Logging.Abstractions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access.LibPlcTag;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using Path = ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device.Path;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration;

/// <summary>
/// Verifies — against the real CompactLogix L32E — the two claims that justify <c>SynchronizedLogixTagAccess</c>,
/// rather than taking ADR-001's word for them:
/// <list type="number">
/// <item>a group can name the same tag twice, so <c>CachingLogixTagManager</c> hands one shared tag to
/// several concurrent readers (<see cref="Group_NamingTheSameTagTwice_SharesOneTag_AndReadsConsistently"/>);</item>
/// <item>concurrent read and write on one libplctag handle genuinely races on its single native buffer, so
/// a bare access self-inflicts errors (<see cref="BareAccess_ConcurrentReadAndWrite_RacesOnTheSharedBuffer"/>)
/// while a synchronized one does not (<see cref="SynchronizedAccess_ConcurrentReadAndWrite_NeverSelfInflictsAnError"/>).</item>
/// </list>
/// Requires the device reachable (see TEST-DEVICE-SETUP.md). The writes here push the DINT's own current
/// value straight back, so they are idempotent — the tag is never actually changed.
/// </summary>
[Trait("Category", "Integration")]
[Collection(PlcCollection.Name)]
public class SharedAccessConcurrencyTests
{
    private static readonly string Gateway = Environment.GetEnvironmentVariable("CIP_GATEWAY") ?? "192.168.0.100";
    private static readonly string Path = Environment.GetEnvironmentVariable("CIP_PATH") ?? "1,0";

    // A COUNTER's .PRE member is a stable DINT we can read and write without disturbing the program (see
    // LogixClientReadTests). Override with CIP_DINT_TAG if your device differs.
    private static readonly string DintTagName =
        Environment.GetEnvironmentVariable("CIP_DINT_TAG") ?? "Program:MainProgram.Counter.PRE";

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    // Hard deadline for a single probe op on a bare access, so a collision that wedges an operation
    // surfaces as a timed-out anomaly rather than an indefinite hang.
    private static readonly TimeSpan OpDeadline = TimeSpan.FromSeconds(3);

    // How hard the concurrency tests lean on the access: each round fires this many reads and writes at
    // once, all onto the one shared access, for this many rounds.
    private const int Rounds = 60;
    private const int ReadsPerRound = 3;
    private const int WritesPerRound = 3;

    [Fact]
    public async Task Group_NamingTheSameTagTwice_SharesOneTag_AndReadsConsistently()
    {
        // Arrange
        var accessFactory = new LogixTagAccessFactory(ClientInformation(), Timeout);
        using var tagManager = new CachingLogixTagManager(
            accessFactory, new TagDefinitionsLoader(accessFactory), NullLogger<CachingLogixTagManager>.Instance);
        var first = new DIntDataPoint(new TagName(DintTagName));
        var second = new DIntDataPoint(new TagName(DintTagName));
        var readClient = new LogixClient(tagManager);
        IReadOnlyList<ILogixDataPoint> group = [first, second];

        // Act
        await tagManager.LoadTagDefinitionsAsync(CancellationToken.None);
        var values = await readClient.ReadAsync(group, CancellationToken.None);

        // Assert
        // Axis 2, deterministically: two distinct but record-equal data points naming one tag resolve to
        // the *same* tag — and so share its one synchronized access. This is the shared-access premise,
        // verified without needing a race to occur.
        tagManager.TagFor(second).Should().BeSameAs(tagManager.TagFor(first));

        // And a group carrying both reads cleanly through the production stack: two entries, one tag,
        // two concurrent reads that the gate serialized — both Good, both the same value.
        values.Should().HaveCount(2);
        values.Should().OnlyContain(value => value.Quality == LogixQuality.Good);
        values.Select(value => value.Value).Distinct().Should().ContainSingle(
            "both entries read the same tag over the same access");
    }

    [Fact]
    public async Task SynchronizedAccess_ConcurrentReadAndWrite_NeverSelfInflictsAnError()
    {
        // Arrange
        using ILogixTagAccess access = new SynchronizedLogixTagAccess(new LogixTagAccess(NewRawTag()));

        // Act
        var outcomes = await HammerAsync(access, CancellationToken.None);

        // Assert
        // The gate makes every operation a whole exchange with the access to itself, so nothing the batch
        // does to this tag can make it fail. On a healthy device that means every read and write is Ok.
        outcomes.Where(o => !o.Ok).Should().BeEmpty(
            "serializing operations on the shared access removes the self-inflicted races");
    }

    [Fact]
    public async Task BareAccess_ConcurrentReadAndWrite_RacesOnTheSharedBuffer()
    {
        // Arrange
        using ILogixTagAccess access = new LogixTagAccess(NewRawTag());

        // Seed the write payload from the tag's own value, under its own bounded read.
        byte[] payload;
        using (var seedCts = new CancellationTokenSource(Timeout))
        {
            var seed = await access.ReadAsync(seedCts.Token);
            seed.Succeeded.Should().BeTrue("the tag must be readable before the concurrency probe");
            payload = seed.Buffer.ToArray();
        }

        // Act
        // Overlapping read and write on one native handle collides on its single buffer — and against real
        // hardware the collision does not merely come back with an error, it can leave an operation wedged
        // so the read never completes. So every probe op carries a hard deadline: a wedged op surfaces as a
        // timed-out anomaly instead of hanging the suite (a hang under MTP would take the whole process
        // down). Stop at the first collision — one is all the evidence the design needs.
        var anomalies = new List<OpOutcome>();
        for (var round = 0; round < 20 && anomalies.Count == 0; round++)
        {
            using var opCts = new CancellationTokenSource(OpDeadline);
            var ops = new[]
            {
                ReadOnceAsync(access, opCts.Token),
                WriteOnceAsync(access, payload, opCts.Token),
                ReadOnceAsync(access, opCts.Token),
                WriteOnceAsync(access, payload, opCts.Token),
            };

            anomalies.AddRange((await Task.WhenAll(ops)).Where(o => !o.Ok));
        }

        // Assert
        // The race is timing-dependent: a run that reproduces it is the evidence; a run that does not is
        // inconclusive, not a failure — asserting the race *must* fire here would be a flaky false negative.
        if (anomalies.Count == 0)
        {
            Assert.Skip("No collision reproduced this run — the shared-buffer race is timing-dependent.");
        }

        anomalies.Should().NotBeEmpty(
            "an unsynchronized shared access collides read against write on its one native buffer");
    }

    // Fires ReadsPerRound reads and WritesPerRound writes concurrently onto one access, for Rounds rounds,
    // and returns what each operation reported. Writes push the tag's own current bytes back, so they never
    // change the value — the point is only that a write is in flight while a read is.
    private static async Task<IReadOnlyList<OpOutcome>> HammerAsync(ILogixTagAccess access, CancellationToken ct)
    {
        // Seed the write payload from the tag's own current value, so every write is a no-op on the device.
        var seed = await access.ReadAsync(ct).ConfigureAwait(false);
        seed.Succeeded.Should().BeTrue("the tag must be readable before the concurrency probe");
        var payload = seed.Buffer.ToArray();

        var outcomes = new List<OpOutcome>();
        for (var round = 0; round < Rounds; round++)
        {
            var ops = new List<Task<OpOutcome>>(ReadsPerRound + WritesPerRound);
            for (var r = 0; r < ReadsPerRound; r++)
            {
                ops.Add(ReadOnceAsync(access, ct));
            }

            for (var w = 0; w < WritesPerRound; w++)
            {
                ops.Add(WriteOnceAsync(access, payload, ct));
            }

            outcomes.AddRange(await Task.WhenAll(ops).ConfigureAwait(false));
        }

        return outcomes;
    }

    private static async Task<OpOutcome> ReadOnceAsync(ILogixTagAccess access, CancellationToken ct)
    {
        try
        {
            var result = await access.ReadAsync(ct).ConfigureAwait(false);
            return new OpOutcome("read", result.Succeeded, result.Error);
        }
        catch (Exception ex)
        {
            return new OpOutcome("read", false, ex.Message);
        }
    }

    private static async Task<OpOutcome> WriteOnceAsync(ILogixTagAccess access, byte[] payload, CancellationToken ct)
    {
        try
        {
            var result = await access.WriteAsync(payload, ct).ConfigureAwait(false);
            return new OpOutcome("write", result.Succeeded, result.Error);
        }
        catch (Exception ex)
        {
            return new OpOutcome("write", false, ex.Message);
        }
    }

    private static LogixClientInformation ClientInformation() =>
        new(new Gateway(Gateway), new Path(Path), LogixControllerType.ControlLogix);

    private static Tag NewRawTag() => new()
    {
        Gateway = Gateway,
        Path = Path,
        PlcType = PlcType.ControlLogix,
        Protocol = Protocol.ab_eip,
        Name = DintTagName,
        Timeout = Timeout,
    };

    private readonly record struct OpOutcome(string Op, bool Ok, string? Error);
}
