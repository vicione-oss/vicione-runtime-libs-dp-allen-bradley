using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

// One batched write, the mirror of LogixReadBatch: resolved on construction and executed by WriteAsync.
// Constructing the batch resolves each value's tag and has the converter encode the value, so holding a
// batch means holding a fully encoded one. WriteAsync then fans the writes out.
//
// Where the conversion sits is the one place the two directions differ. A decode needs the reply, so it
// happens per entry; an encode needs nothing from the device, so it happens here, and a value that will
// not encode fails before any tag is touched instead of leaving half the batch written. How the failure
// is reported does not differ: both loops collect their failures and raise one LogixTagException naming
// every tag that could not be served.
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

    // One value paired with its tag and turned into the bytes that will be sent. An encode needs nothing
    // from the device, so this is all that has to happen before WriteAsync — and a failure here rides home
    // as an outcome rather than an exception for the same reason a failed device write does: a caller told
    // about the first bad value out of three would fix one and be back.
    private static EncodeOutcome EncodeEntry(ILogixDataPointValue value, ILogixTagManager tagManager)
    {
        var dataPoint = value.DataPoint;
        var tag = tagManager.TagFor(dataPoint);
        var converter = DataPointConverterRegistry.GetConverter(dataPoint);

        try
        {
            return EncodeOutcome.Ok(new WriteEntry(dataPoint, tag, converter.Encode(value)));
        }
        // A value the converter will not encode — too long for its tag, or not the value its data point
        // makes. Nothing is skipped by collecting it; the batch has touched no tag yet either way.
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

    // The result of encoding one value: the entry it produced, or why it produced none. Simpler than the
    // read side's ReadOutcome because a failed encode has nothing but its reason to carry — the tag name
    // is already in the message, since that message is what the caller is given.
    private readonly record struct EncodeOutcome(WriteEntry Entry, string? Failure)
    {
        internal static EncodeOutcome Ok(WriteEntry entry) => new(entry, null);

        internal static EncodeOutcome Failed(string failure) => new(default, failure);
    }

    private readonly record struct WriteOutcome(TagName TagName, LogixTagWriteResult Result);
}
