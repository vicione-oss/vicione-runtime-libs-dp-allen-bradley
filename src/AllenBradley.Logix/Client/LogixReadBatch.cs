using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Batches;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

/// <summary>
/// A <see cref="ReadBatch{TDataPoint, TValue}"/> over Logix data points that throws a
/// <see cref="LogixTagException"/> when no tag produced a value. Constructing it resolves every tag and
/// converter, as the shared batch does.
/// </summary>
internal sealed class LogixReadBatch(IReadOnlyList<ILogixDataPoint> dataPoints, ILogixTagManager tagManager)
{
    private readonly ReadBatch<ILogixDataPoint, ILogixDataPointValue> _batch =
        new(dataPoints, tagManager, DataPointConverterRegistry.GetConverter);

    internal async Task<ReadBatch<ILogixDataPoint, ILogixDataPointValue>.BatchReadResult> ReadAsync(
        CancellationToken cancellationToken)
    {
        var result = await _batch.ReadAsync(cancellationToken).ConfigureAwait(false);
        ThrowIfNothingWasRead(result);

        return result;
    }

    // Returning an empty list would make a dead controller look like a poll of an empty group, leaving the
    // polling job's circuit breaker nothing to count
    // (ADR/2026-07-16-reading-and-writing-a-group-of-tags.md).
    private static void ThrowIfNothingWasRead(ReadBatch<ILogixDataPoint, ILogixDataPointValue>.BatchReadResult result)
    {
        if (!result.NothingWasRead)
        {
            return;
        }

        throw new LogixTagException($"Read failed for {result.DescribeFailures()}.");
    }
}
