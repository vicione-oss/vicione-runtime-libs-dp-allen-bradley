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
    private static async Task<ILogixDataPointValue> ReadEntryAsync(
        ReadEntry entry, CancellationToken cancellationToken)
    {
        var result = await entry.Tag.ReadAsync(cancellationToken).ConfigureAwait(false);

        // Gate on the controller's real CIP type before interpreting bytes (ADR-003): a tag whose
        // controller type disagrees with the configured converter degrades to a Bad value rather than a
        // misread one. When the type is unknown — the tag is absent from the flat symbol table, e.g. a
        // structure member — the byte-size backstop is the check.
        if (!result.Succeeded
            || entry.Converter.ConflictsWith(entry.Tag.Metadata)
            || result.Buffer.Length < entry.Converter.ByteSize.Value)
        {
            return new BadLogixDataPointValue(entry.DataPoint);
        }

        return entry.Converter.Decode(entry.DataPoint, result.Buffer.Span);
    }

    private readonly record struct ReadEntry(
        ILogixDataPoint DataPoint, IDataPointConverter Converter, ILogixTag Tag);
}
