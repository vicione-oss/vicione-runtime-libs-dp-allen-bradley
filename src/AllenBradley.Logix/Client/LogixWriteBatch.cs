using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

// One batched write, the mirror of LogixReadBatch: resolved on construction and executed by WriteAsync.
// Constructing the batch encodes each value into a right-sized buffer via its converter and resolves its
// tag, so holding a batch means holding a fully encoded one. WriteAsync then fans the writes out.
internal sealed class LogixWriteBatch
{
    private readonly WriteEntry[] _entries;

    internal LogixWriteBatch(IReadOnlyList<ILogixDataPointValue> values, ILogixTagManager tagManager)
    {
        _entries = new WriteEntry[values.Count];
        for (var i = 0; i < values.Count; i++)
        {
            var value = values[i];
            var converter = DataPointConverterRegistry.GetConverter(value.DataPoint);
            var buffer = new byte[converter.ByteSize.Value];
            converter.Encode(value, buffer);
            _entries[i] = new WriteEntry(
                value.DataPoint.TagName, converter, tagManager.TagFor(value.DataPoint), buffer);
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
    private static async Task<WriteOutcome> WriteEntryAsync(
        WriteEntry entry, CancellationToken cancellationToken)
    {
        // Gate on the controller's real CIP type before writing (ADR-003): encoding a DINT onto a REAL tag
        // would corrupt it. A contradiction fails the tag by name without issuing the write; an unknown
        // type (null metadata) is not a contradiction and lets the write through, its size fixed by the
        // converter.
        if (entry.Converter.ConflictsWith(entry.Tag.Metadata))
        {
            return new WriteOutcome(
                entry.TagName,
                LogixTagWriteResult.Failed(
                    $"the controller's type for the tag does not match the configured {entry.Converter.ExpectedType}"));
        }

        var result = await entry.Tag.WriteAsync(entry.Buffer, cancellationToken).ConfigureAwait(false);

        return new WriteOutcome(entry.TagName, result);
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

    private readonly record struct WriteEntry(
        TagName TagName, IDataPointConverter Converter, ILogixTag Tag, byte[] Buffer);

    private readonly record struct WriteOutcome(TagName TagName, LogixTagWriteResult Result);
}
