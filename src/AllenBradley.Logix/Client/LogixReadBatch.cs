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
        var reads = new Task<ILogixDataPointValue>[_entries.Length];
        for (var i = 0; i < _entries.Length; i++)
        {
            reads[i] = ReadEntryAsync(_entries[i], cancellationToken);
        }

        return await Task.WhenAll(reads).ConfigureAwait(false);
    }

    // Each tag is read and decoded independently: a failing tag degrades to a Bad value for its
    // own data point instead of failing the whole group. A failed read is an ordinary result here,
    // not an exception — a tag that will not read is expected in a group polled on an interval.
    //
    // What the bytes are is not re-litigated per read. LogixConfigurationVerifier has already diffed every
    // configured data point against the controller's own declaration and aborted the connect on a
    // disagreement (ADR-003), so a tag that is being polled is a tag whose type already matched, and the
    // decode reads the type it was configured for.
    private static async Task<ILogixDataPointValue> ReadEntryAsync(
        ReadEntry entry, CancellationToken cancellationToken)
    {
        var result = await entry.Tag.ReadAsync(cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            return new BadLogixDataPointValue(entry.DataPoint);
        }

        try
        {
            return entry.Converter.Decode(entry.DataPoint, result.Buffer.Span);
        }
        catch (ArgumentException)
        {
            // A reply too short for the type the data point was configured as. It should not happen on a
            // verified tag, and it is caught anyway for the same reason a failed read is a Bad value
            // rather than an exception: one tag must not sink the group it is polled in (ADR-004).
            // Narrow on purpose — this is the buffer being the wrong shape for the decode, and nothing
            // else. A converter that throws anything else is a bug in the converter, and it travels.
            return new BadLogixDataPointValue(entry.DataPoint);
        }
    }

    private readonly record struct ReadEntry(
        ILogixDataPoint DataPoint, IDataPointConverter Converter, ILogixTag Tag);
}
