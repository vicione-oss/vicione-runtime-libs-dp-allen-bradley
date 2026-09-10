using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

/// <summary>
/// One batched read, resolved on construction and executed by <see cref="ReadAsync"/>: every data point
/// is paired with its converter and tag before any I/O, then a read is fanned out per tag and each buffer
/// decoded (ADR/2026-07-16-reading-and-writing-a-group-of-tags.md).
/// </summary>
internal sealed class LogixReadBatch
{
    private readonly ReadEntry[] _entries;

    internal LogixReadBatch(IReadOnlyList<ILogixDataPoint> dataPoints, ILogixTagManager tagManager)
    {
        _entries = new ReadEntry[dataPoints.Count];
        for (var i = 0; i < dataPoints.Count; i++)
        {
            var dataPoint = dataPoints[i];
            _entries[i] = new ReadEntry(
                dataPoint, DataPointConverterRegistry.GetConverter(dataPoint), tagManager.TagFor(dataPoint));
        }
    }

    internal async Task<BatchReadResult> ReadAsync(CancellationToken cancellationToken)
    {
        var reads = new Task<ReadOutcome>[_entries.Length];
        for (var i = 0; i < _entries.Length; i++)
        {
            reads[i] = ReadEntryAsync(_entries[i], cancellationToken);
        }

        var outcomes = await Task.WhenAll(reads).ConfigureAwait(false);

        var values = new List<ILogixDataPointValue>(outcomes.Length);
        var failures = new List<ReadOutcome>();
        foreach (var outcome in outcomes)
        {
            if (outcome.Error is null)
            {
                // Non-null by construction: an outcome without a reason is one that carries a value.
                values.Add(outcome.Value!);
            }
            else
            {
                failures.Add(outcome);
            }
        }

        var result = new BatchReadResult(values, failures);
        ThrowIfNothingWasRead(result);

        return result;
    }

    // A device failure rides home as an outcome rather than an exception: awaiting Task.WhenAll rethrows
    // only the first exception of a set, so the other tags' failures and values would be lost with it.
    // The decode trusts the configured type, verified at connect
    // (ADR/2026-07-21-verifying-configuration-against-the-symbol-table.md).
    // Internal rather than private so one entry's read-and-decode can be driven without the sorting that
    // ReadAsync does afterwards.
    internal static async Task<ReadOutcome> ReadEntryAsync(
        ReadEntry entry, CancellationToken cancellationToken)
    {
        var result = await entry.Tag.ReadAsync(cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            // Non-null by construction: LogixTagReadResult.Succeeded is defined as having no error.
            return ReadOutcome.Failed(entry.DataPoint.TagName, result.Error!);
        }

        try
        {
            return ReadOutcome.Ok(entry.Converter.Decode(entry.DataPoint, result.Buffer.Span));
        }
        catch (Exception ex) when (ex is ArgumentException or LogixDecodeException)
        {
            // Narrow on purpose: a buffer the wrong shape for the decode is named as this tag's failure,
            // while anything else a converter throws is a converter bug and travels.
            return ReadOutcome.Failed(entry.DataPoint.TagName, ex.Message);
        }
    }

    // Returning an empty list would make a dead controller look like a poll of an empty group, leaving the
    // polling job's circuit breaker nothing to count
    // (ADR/2026-07-16-reading-and-writing-a-group-of-tags.md). An empty group is not that case.
    private static void ThrowIfNothingWasRead(BatchReadResult result)
    {
        if (result.Values.Count > 0 || result.Failures.Count == 0)
        {
            return;
        }

        throw new LogixTagException($"Read failed for {result.DescribeFailures()}.");
    }

    /// <summary>One data point with everything the read needs for it resolved.</summary>
    internal readonly record struct ReadEntry(
        ILogixDataPoint DataPoint, IDataPointConverter Converter, ILogixTag Tag);

    /// <summary>
    /// What one batch came home with. The two lists together always cover the group.
    /// </summary>
    internal readonly record struct BatchReadResult(
        IReadOnlyList<ILogixDataPointValue> Values, IReadOnlyList<ReadOutcome> Failures)
    {
        /// <summary>Every failed tag with its own reason, in one line, for the caller that logs them.</summary>
        internal string DescribeFailures() =>
            string.Join("; ", Failures.Select(failure => $"{failure.TagName}: {failure.Error}"));
    }

    /// <summary>
    /// The outcome of one entry's read and decode: the value it produced, or why it produced none.
    /// </summary>
    internal readonly record struct ReadOutcome(TagName TagName, ILogixDataPointValue? Value, string? Error)
    {
        internal static ReadOutcome Ok(ILogixDataPointValue value) =>
            new(value.DataPoint.TagName, value, null);

        internal static ReadOutcome Failed(TagName tagName, string error) => new(tagName, null, error);
    }
}
