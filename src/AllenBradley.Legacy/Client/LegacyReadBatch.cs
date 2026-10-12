using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Batches;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client;

/// <summary>
/// Reads a group of Legacy data points through the shared <see cref="ReadBatch{TDataPoint, TValue}"/>, and
/// throws a <see cref="LegacyTagException"/> when no element could be read. The constructor gets the tag
/// handles and the converters before anything is read.
/// </summary>
internal sealed class LegacyReadBatch(IReadOnlyList<ILegacyDataPoint> dataPoints, LegacyTagSource tagSource)
{
    private readonly ReadBatch<ILegacyDataPoint, ILegacyDataPointValue> _batch =
        new(dataPoints, tagSource, LegacyConverters.For);

    /// <exception cref="LegacyTagException">No element could be read.</exception>
    internal async Task<ReadBatch<ILegacyDataPoint, ILegacyDataPointValue>.BatchReadResult> ReadAsync(
        CancellationToken cancellationToken)
    {
        var result = await _batch.ReadAsync(cancellationToken).ConfigureAwait(false);
        ThrowIfNothingWasRead(result);

        return result;
    }

    // An empty list would look like the poll of an empty group. The polling job would never see the
    // controller fail, and its circuit breaker would never open.
    private static void ThrowIfNothingWasRead(ReadBatch<ILegacyDataPoint, ILegacyDataPointValue>.BatchReadResult result)
    {
        if (!result.NothingWasRead)
        {
            return;
        }

        throw new LegacyTagException($"Read failed for {result.DescribeFailures()}.");
    }
}
