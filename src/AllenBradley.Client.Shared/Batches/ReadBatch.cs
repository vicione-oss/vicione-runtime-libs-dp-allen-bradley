using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Batches;

/// <summary>
/// One batched read, resolved on construction and executed by <see cref="ReadAsync"/>: every data point
/// is paired with its converter and tag before any I/O, then a read is fanned out per tag and each buffer
/// decoded (ADR/2026-07-16-reading-and-writing-a-group-of-tags.md). A tag that fails is reported in the
/// result, so each dataport decides what a batch with failures means to its caller.
/// </summary>
internal sealed class ReadBatch<TDataPoint, TValue>
    where TDataPoint : IDataPoint
    where TValue : class
{
    private readonly ReadEntry[] _entries;

    /// <exception cref="InvalidOperationException">
    /// <paramref name="converterFor"/> has no converter for one of the data points.
    /// </exception>
    internal ReadBatch(
        IReadOnlyList<TDataPoint> dataPoints,
        ITagSource<TDataPoint, ITagAccess> tagSource,
        Func<TDataPoint, IDataPointConverter<TDataPoint, TValue>> converterFor)
    {
        _entries = new ReadEntry[dataPoints.Count];
        for (var i = 0; i < dataPoints.Count; i++)
        {
            var dataPoint = dataPoints[i];
            _entries[i] = new ReadEntry(dataPoint, converterFor(dataPoint), tagSource.TagFor(dataPoint));
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

        var values = new List<TValue>(outcomes.Length);
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

        return new BatchReadResult(values, failures);
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
            // Non-null by construction: TagReadResult.Succeeded is defined as having no error.
            return ReadOutcome.Failed(entry.DataPoint, result.Error!);
        }

        try
        {
            return ReadOutcome.Ok(entry.DataPoint, entry.Converter.Decode(entry.DataPoint, result.Buffer.Span));
        }
        catch (Exception ex) when (ex is ArgumentException or DecodeException)
        {
            // Narrow on purpose: a buffer the wrong shape for the decode is named as this tag's failure,
            // while anything else a converter throws is a converter bug and travels.
            return ReadOutcome.Failed(entry.DataPoint, ex.Message);
        }
    }

    /// <summary>One data point with everything the read needs for it resolved.</summary>
    internal readonly record struct ReadEntry(
        TDataPoint DataPoint, IDataPointConverter<TDataPoint, TValue> Converter, ITagAccess Tag);

    /// <summary>
    /// What one batch came home with. The two lists together always cover the group.
    /// </summary>
    internal readonly record struct BatchReadResult(IReadOnlyList<TValue> Values, IReadOnlyList<ReadOutcome> Failures)
    {
        /// <summary>
        /// Whether the batch produced no value at all although it had data points to read. An empty
        /// group is not this case.
        /// </summary>
        internal bool NothingWasRead => Values.Count == 0 && Failures.Count > 0;

        /// <summary>Every failed tag with its own reason, in one line, for the caller that logs them.</summary>
        internal string DescribeFailures() =>
            string.Join("; ", Failures.Select(failure => $"{failure.DataPoint.Identifier.Value}: {failure.Error}"));
    }

    /// <summary>
    /// The outcome of one entry's read and decode: the value it produced, or why it produced none.
    /// </summary>
    internal readonly record struct ReadOutcome(TDataPoint DataPoint, TValue? Value, string? Error)
    {
        internal static ReadOutcome Ok(TDataPoint dataPoint, TValue value) => new(dataPoint, value, null);

        internal static ReadOutcome Failed(TDataPoint dataPoint, string error) => new(dataPoint, null, error);
    }
}
