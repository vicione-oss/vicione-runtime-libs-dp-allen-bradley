using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

/// <summary>
/// One batched write, the mirror of <see cref="LogixReadBatch"/>: constructing it resolves each value's
/// tag and encodes the value, so a value that will not encode fails before any tag is touched;
/// <see cref="WriteAsync"/> then fans the writes out
/// (ADR/2026-07-16-reading-and-writing-a-group-of-tags.md).
/// </summary>
internal sealed class LogixWriteBatch
{
    private readonly WriteEntry[] _entries;

    internal LogixWriteBatch(IReadOnlyList<ILogixDataPointValue> values, ILogixTagManager tagManager)
    {
        _entries = new WriteEntry[values.Count];
        var failures = new List<string>();

        for (var i = 0; i < values.Count; i++)
        {
            var outcome = EncodeEntry(values[i], tagManager);
            if (outcome.Failure is not null)
            {
                failures.Add(outcome.Failure);
                continue;
            }

            _entries[i] = outcome.Entry;
        }

        ThrowIfAnythingWouldNotEncode(failures);
    }

    internal async Task WriteAsync(CancellationToken cancellationToken)
    {
        var writes = new Task<WriteOutcome>[_entries.Length];
        for (var i = 0; i < _entries.Length; i++)
        {
            writes[i] = WriteEntryAsync(_entries[i], cancellationToken);
        }

        ThrowIfAnyFailed(await Task.WhenAll(writes).ConfigureAwait(false));
    }

    // A failure here rides home as an outcome rather than an exception so that all the bad values in a
    // batch can be named at once, not just the first.
    private static EncodeOutcome EncodeEntry(ILogixDataPointValue value, ILogixTagManager tagManager)
    {
        var dataPoint = value.DataPoint;
        var tag = tagManager.TagFor(dataPoint);
        var converter = DataPointConverterRegistry.GetConverter(dataPoint);

        try
        {
            return EncodeOutcome.Ok(new WriteEntry(dataPoint, tag, converter.Encode(value)));
        }
        catch (InvalidOperationException ex)
        {
            return EncodeOutcome.Failed($"{dataPoint.TagName}: {ex.Message}");
        }
    }

    // Said separately from the device failures because it is a different situation to recover from: the
    // controller has seen nothing, so the caller re-drives the whole group rather than the failed part.
    private static void ThrowIfAnythingWouldNotEncode(IReadOnlyList<string> failures)
    {
        if (failures.Count == 0)
        {
            return;
        }

        throw new LogixTagException(
            $"Write failed for {string.Join("; ", failures)}. Nothing was sent to the controller.");
    }

    // A device failure rides home as an outcome rather than an exception: awaiting Task.WhenAll rethrows
    // only the first exception of a set, and the other tags' failures would be lost with it. The bytes go
    // out as configured, verified at connect
    // (ADR/2026-07-21-verifying-configuration-against-the-symbol-table.md).
    private static async Task<WriteOutcome> WriteEntryAsync(
        WriteEntry entry, CancellationToken cancellationToken)
    {
        var result = await entry.Tag.WriteAsync(entry.Buffer, cancellationToken).ConfigureAwait(false);

        return new WriteOutcome(entry.DataPoint.TagName, result);
    }

    // ILogixWriteClient.WriteAsync hands the caller a bare ValueTask, so an exception is the only thing
    // that stops a dropped write from being silent.
    private static void ThrowIfAnyFailed(IReadOnlyList<WriteOutcome> outcomes)
    {
        var failures = outcomes.Where(outcome => !outcome.Result.Succeeded).ToList();
        if (failures.Count == 0)
        {
            return;
        }

        // Non-null by construction: LogixTagWriteResult.Succeeded is defined as having no error.
        var reasons = failures.Select(failure => $"{failure.TagName}: {failure.Result.Error}");
        throw new LogixTagException($"Write failed for {string.Join("; ", reasons)}.");
    }

    /// <summary>One value paired with its tag and the bytes that will be sent for it.</summary>
    private readonly record struct WriteEntry(ILogixDataPoint DataPoint, ILogixTag Tag, byte[] Buffer);

    /// <summary>
    /// The result of encoding one value: the entry it produced, or why it produced none. The failure
    /// already carries the tag name, since it is handed to the caller verbatim.
    /// </summary>
    private readonly record struct EncodeOutcome(WriteEntry Entry, string? Failure)
    {
        internal static EncodeOutcome Ok(WriteEntry entry) => new(entry, null);

        internal static EncodeOutcome Failed(string failure) => new(default, failure);
    }

    /// <summary>What the controller answered for one entry's write.</summary>
    private readonly record struct WriteOutcome(TagName TagName, LogixTagWriteResult Result);
}
