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
// The group is the unit of delivery: it comes home whole or not at all, the same contract the sibling
// Siemens S7 addon's batch has (Siemens.S7.Absolute/Client/S7NetPlusClient.cs). A group that lost a tag
// is a group the engine cannot use, and a half-filled poll published as if it were complete is worse
// than no poll at all — the polling job logs the failure and the next tick tries again.
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

    internal async Task<IReadOnlyList<ILogixDataPointValue>> ReadAsync(CancellationToken cancellationToken)
    {
        var reads = new Task<ReadOutcome>[_entries.Length];
        for (var i = 0; i < _entries.Length; i++)
        {
            reads[i] = ReadEntryAsync(_entries[i], cancellationToken);
        }

        var outcomes = await Task.WhenAll(reads).ConfigureAwait(false);
        ThrowIfAnyFailed(outcomes);

        var values = new ILogixDataPointValue[outcomes.Length];
        for (var i = 0; i < outcomes.Length; i++)
        {
            // Non-null by construction: ThrowIfAnyFailed has already left if any outcome carried a reason.
            values[i] = outcomes[i].Value!;
        }

        return values;
    }

    // A device failure rides home as an outcome rather than an exception, for the same reason the write
    // side does it: awaiting Task.WhenAll rethrows only the first exception of the set, and a caller told
    // about one failed tag out of five would go looking in the wrong place. Every tag still gets read —
    // none of them stops its siblings — and the failures are combined afterwards. Cancellation still
    // throws, and is meant to.
    //
    // What the bytes are is not re-litigated per read. LogixConfigurationVerifier has already diffed every
    // configured data point against the controller's own declaration and aborted the connect on a
    // disagreement (ADR/2026-07-21-verifying-configuration-against-the-symbol-table.md), so a tag that is
    // being polled is a tag whose type already matched, and the decode reads the type it was configured
    // for.
    // Internal rather than private so the one entry's read-and-decode can be driven on its own: what it
    // turns into an outcome and what it lets travel is the rule the whole batch is built on, and testing
    // it through ReadAsync would only ever see it through an aggregated message.
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

    // Every failed tag is named, because the reads are fanned out and none of them stops its siblings: a
    // caller told only about the first failure would re-drive the wrong set.
    private static void ThrowIfAnyFailed(IReadOnlyList<ReadOutcome> outcomes)
    {
        var failures = outcomes.Where(outcome => outcome.Error is not null).ToList();
        if (failures.Count == 0)
        {
            return;
        }

        var reasons = failures.Select(failure => $"{failure.TagName}: {failure.Error}");
        throw new LogixTagException($"Read failed for {string.Join("; ", reasons)}.");
    }

    internal readonly record struct ReadEntry(
        ILogixDataPoint DataPoint, IDataPointConverter Converter, ILogixTag Tag);

    // The outcome of one entry's read and decode: the value it produced, or the tag that could not
    // produce one and why.
    internal readonly record struct ReadOutcome(TagName TagName, ILogixDataPointValue? Value, string? Error)
    {
        internal static ReadOutcome Ok(ILogixDataPointValue value) =>
            new(value.DataPoint.TagName, value, null);

        internal static ReadOutcome Failed(TagName tagName, string error) => new(tagName, null, error);
    }
}
