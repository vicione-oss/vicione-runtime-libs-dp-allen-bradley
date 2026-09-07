using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

// One batched write, the mirror of LogixReadBatch: resolved on construction and executed by WriteAsync.
// Constructing the batch resolves each value's tag and has the converter encode the value, so holding a
// batch means holding a fully encoded one. WriteAsync then fans the writes out.
//
// The bytes are the value's, not the tag's: the converter sizes them from the type or the configured
// capacity and knows nothing of the handle. How wide the tag is on the controller is libplctag's fact,
// and it is enforced where the two meet — SetBuffer refuses a payload longer than the handle before
// anything is sent, and that comes home as a failed outcome naming the tag like any other device
// failure (see LogixTagAccess).
internal sealed class LogixWriteBatch
{
    private readonly WriteEntry[] _entries;

    internal LogixWriteBatch(IReadOnlyList<ILogixDataPointValue> values, ILogixTagManager tagManager)
    {
        _entries = new WriteEntry[values.Count];
        for (var i = 0; i < values.Count; i++)
        {
            var value = values[i];
            var tag = tagManager.TagFor(value.DataPoint);
            var converter = DataPointConverterRegistry.GetConverter(value.DataPoint);
            _entries[i] = new WriteEntry(value.DataPoint, tag, converter.Encode(value));
        }
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

    // A device failure rides home as an outcome rather than an exception so that one tag's failure
    // cannot hide another's: awaiting Task.WhenAll rethrows only the first exception of the set, and
    // the rest would be lost. Cancellation still throws, and is meant to.
    //
    // The bytes go out as the configuration says they should. LogixConfigurationVerifier has already
    // diffed every configured data point against the controller's own declaration and aborted the connect
    // on a disagreement (ADR/2026-07-21-verifying-configuration-against-the-symbol-table.md), so a tag
    // that is being written is a tag whose type already matched.
    private static async Task<WriteOutcome> WriteEntryAsync(
        WriteEntry entry, CancellationToken cancellationToken)
    {
        var result = await entry.Tag.WriteAsync(entry.Buffer, cancellationToken).ConfigureAwait(false);

        return new WriteOutcome(entry.DataPoint.TagName, result);
    }

    // Unlike a read, whose failure rides home on its value's quality, ILogixWriteClient.WriteAsync
    // hands the caller a bare ValueTask, so an exception is the only thing that stops a dropped write
    // from being silent. Every failed tag is named, because the writes are fanned out and none of them
    // stops its siblings: a caller told only about the first failure would re-drive the wrong set.
    private static void ThrowIfAnyFailed(IReadOnlyList<WriteOutcome> outcomes)
    {
        var failures = outcomes.Where(outcome => !outcome.Result.Succeeded).ToList();
        if (failures.Count == 0)
        {
            return;
        }

        // A failed result carries its reason by construction — LogixTagWriteResult.Succeeded is defined
        // as having none.
        var reasons = failures.Select(failure => $"{failure.TagName}: {failure.Result.Error}");
        throw new LogixTagException($"Write failed for {string.Join("; ", reasons)}.");
    }

    private readonly record struct WriteEntry(ILogixDataPoint DataPoint, ILogixTag Tag, byte[] Buffer);

    private readonly record struct WriteOutcome(TagName TagName, LogixTagWriteResult Result);
}
