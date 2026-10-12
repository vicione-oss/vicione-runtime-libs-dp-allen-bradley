using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Batches;

/// <summary>
/// One batched write, the mirror of <see cref="ReadBatch{TDataPoint, TValue}"/>: constructing it resolves
/// each value's tag and encodes the value, and <see cref="WriteAsync"/> then fans the writes out
/// (ADR/2026-07-16-reading-and-writing-a-group-of-tags.md). Values that will not encode and tags that
/// will not take their value are both reported as failures, so each dataport decides what they mean to
/// its caller.
/// </summary>
internal sealed class WriteBatch<TDataPoint, TValue>
    where TDataPoint : IDataPoint
{
    private readonly WriteEntry[] _entries;

    /// <param name="values">The values to write, each carrying the data point it is written to.</param>
    /// <param name="dataPointOf">The data point a value is written to.</param>
    /// <param name="tagSource">Resolves the tag for each data point.</param>
    /// <param name="converterFor">The converter that encodes a value for its data point.</param>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="converterFor"/> has no converter for one of the data points.
    /// </exception>
    internal WriteBatch(
        IReadOnlyList<TValue> values,
        Func<TValue, TDataPoint> dataPointOf,
        ITagSource<TDataPoint, ITagAccess> tagSource,
        Func<TDataPoint, IDataPointConverter<TDataPoint, TValue>> converterFor)
    {
        var entries = new List<WriteEntry>(values.Count);
        var encodeFailures = new List<WriteFailure>();

        foreach (var value in values)
        {
            var dataPoint = dataPointOf(value);
            var tag = tagSource.TagFor(dataPoint);
            var converter = converterFor(dataPoint);

            // A failure here is collected rather than thrown so that all the bad values in a batch can be
            // named at once, not just the first.
            try
            {
                entries.Add(new WriteEntry(dataPoint, tag, converter.Encode(value)));
            }
            catch (InvalidOperationException ex)
            {
                encodeFailures.Add(new WriteFailure(dataPoint, ex.Message));
            }
        }

        _entries = [.. entries];
        EncodeFailures = encodeFailures;
    }

    /// <summary>
    /// The values that would not encode. <see cref="WriteAsync"/> sends the others regardless, so a
    /// caller that wants all or nothing checks this first.
    /// </summary>
    internal IReadOnlyList<WriteFailure> EncodeFailures { get; }

    /// <summary>
    /// Writes every value that encoded and returns the tags that would not take theirs, in the order the
    /// values were given. Only cancellation throws.
    /// </summary>
    internal async Task<IReadOnlyList<WriteFailure>> WriteAsync(CancellationToken cancellationToken)
    {
        var writes = new Task<WriteFailure?>[_entries.Length];
        for (var i = 0; i < _entries.Length; i++)
        {
            writes[i] = WriteEntryAsync(_entries[i], cancellationToken);
        }

        var outcomes = await Task.WhenAll(writes).ConfigureAwait(false);
        return [.. outcomes.OfType<WriteFailure>()];
    }

    /// <summary>Every failed tag with its own reason, in one line, for the caller that reports them.</summary>
    internal static string Describe(IReadOnlyList<WriteFailure> failures) =>
        string.Join("; ", failures.Select(failure => $"{failure.DataPoint.Identifier.Value}: {failure.Error}"));

    // A device failure rides home as a value rather than an exception: awaiting Task.WhenAll rethrows
    // only the first exception of a set, and the other tags' failures would be lost with it. The bytes go
    // out as configured, verified at connect
    // (ADR/2026-07-21-verifying-configuration-against-the-symbol-table.md).
    private static async Task<WriteFailure?> WriteEntryAsync(
        WriteEntry entry, CancellationToken cancellationToken)
    {
        var result = await entry.Tag.WriteAsync(entry.Buffer, cancellationToken).ConfigureAwait(false);

        // Non-null by construction: TagWriteResult.Succeeded is defined as having no error.
        return result.Succeeded ? null : new WriteFailure(entry.DataPoint, result.Error!);
    }

    /// <summary>Why one value did not reach its tag.</summary>
    internal readonly record struct WriteFailure(TDataPoint DataPoint, string Error);

    /// <summary>One value paired with its tag and the bytes that will be sent for it.</summary>
    private readonly record struct WriteEntry(TDataPoint DataPoint, ITagAccess Tag, byte[] Buffer);
}
