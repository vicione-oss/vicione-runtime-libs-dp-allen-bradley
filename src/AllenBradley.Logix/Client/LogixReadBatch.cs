using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

// One batched read, resolved on construction and executed by ReadAsync. Constructing the batch pairs
// every data point with its converter and its tag, so a data point without a converter aborts before any
// I/O and holding a batch means holding a fully resolved one. ReadAsync then fans a read out per tag and
// decodes each buffer. The Multiple Service Packet that makes the fan-out fast is built by the libplctag
// core when the tags share a connection — this type only issues the concurrent reads.
//
// The batch is ours, not the caller's: data points are grouped by poll frequency to get them onto the
// wire in one Multiple Service Packet, and being read together carries no meaning beyond that. So one
// tag that will not read costs its own value and nothing else — the tags that did answer come home, and
// the failed ones are named so the client can log them. A batch where *nothing* read is a different
// thing: there is nothing to deliver and the controller is what failed, so that still throws and the
// polling job counts it.
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

    // A device failure rides home as an outcome rather than an exception. Awaiting Task.WhenAll rethrows
    // only the first exception of the set, so a thrown failure would lose the other tags' failures and
    // their values with them. Every tag still gets read — none of them stops its siblings — and the
    // outcomes are sorted into values and failures afterwards. Cancellation still throws, and is meant to.
    //
    // What the bytes are is not re-litigated per read. LogixConfigurationVerifier has already diffed every
    // configured data point against the controller's own declaration and aborted the connect on a
    // disagreement (ADR/2026-07-21-verifying-configuration-against-the-symbol-table.md), so a tag that is
    // being polled is a tag whose type already matched, and the decode reads the type it was configured
    // for.
    // Internal rather than private so the one entry's read-and-decode can be driven on its own: what it
    // turns into an outcome and what it lets travel is the rule the whole batch is built on, and testing
    // it through ReadAsync would only ever see it through the sorting that follows.
    internal static async Task<ReadOutcome> ReadEntryAsync(
        ReadEntry entry, CancellationToken cancellationToken)
    {
        var result = await entry.Tag.ReadAsync(cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            // A failed result carries its reason by construction — LogixTagReadResult.Succeeded is
            // defined as having none.
            return ReadOutcome.Failed(entry.DataPoint.TagName, result.Error!);
        }

        try
        {
            return ReadOutcome.Ok(entry.Converter.Decode(entry.DataPoint, result.Buffer.Span));
        }
        catch (ArgumentException ex)
        {
            // A reply too short for the type the data point was configured as. It should not happen on a
            // verified tag; caught narrowly so it is reported as this tag's failure with its name on it
            // rather than as a raw decode exception with no address in the message. Narrow on purpose —
            // this is the buffer being the wrong shape for the decode, and nothing else. A converter that
            // throws anything else is a bug in the converter, and it travels.
            return ReadOutcome.Failed(entry.DataPoint.TagName, ex.Message);
        }
    }

    // A batch that produced nothing has nothing to hand up, and returning an empty list would make a dead
    // controller look like a poll of a group that happens to be empty: no failed poll logged, and nothing
    // for the polling job's circuit breaker to count. So this one case still throws, and it names every
    // tag, because the reads are fanned out and none of them stops its siblings — a caller told only about
    // the first failure would re-drive the wrong set. An empty group is not a failure and does not throw.
    private static void ThrowIfNothingWasRead(BatchReadResult result)
    {
        if (result.Values.Count > 0 || result.Failures.Count == 0)
        {
            return;
        }

        throw new LogixTagException($"Read failed for {result.DescribeFailures()}.");
    }

    internal readonly record struct ReadEntry(
        ILogixDataPoint DataPoint, IDataPointConverter Converter, ILogixTag Tag);

    // What one batch came home with: a value per tag that answered, and the tags that did not, with the
    // reason each one gave. The two lists together always cover the group.
    internal readonly record struct BatchReadResult(
        IReadOnlyList<ILogixDataPointValue> Values, IReadOnlyList<ReadOutcome> Failures)
    {
        // Every failed tag with its own reason, in one line, for the caller that logs them.
        internal string DescribeFailures() =>
            string.Join("; ", Failures.Select(failure => $"{failure.TagName}: {failure.Error}"));
    }

    // The outcome of one entry's read and decode: the value it produced, or the tag that could not
    // produce one and why.
    internal readonly record struct ReadOutcome(TagName TagName, ILogixDataPointValue? Value, string? Error)
    {
        internal static ReadOutcome Ok(ILogixDataPointValue value) =>
            new(value.DataPoint.TagName, value, null);

        internal static ReadOutcome Failed(TagName tagName, string error) => new(tagName, null, error);
    }
}
