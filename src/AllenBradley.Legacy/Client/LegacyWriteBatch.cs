using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Batches;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client;

/// <summary>
/// Writes Legacy values through the shared <see cref="WriteBatch{TDataPoint, TValue}"/>. When a value
/// cannot be encoded, nothing is sent. When an element is not written, it throws a
/// <see cref="LegacyTagException"/> that names each element that was not written.
/// </summary>
internal sealed class LegacyWriteBatch
{
    private readonly WriteBatch<ILegacyDataPoint, ILegacyDataPointValue> _batch;

    /// <exception cref="LegacyTagException">A value could not be encoded. Nothing was sent.</exception>
    internal LegacyWriteBatch(IReadOnlyList<ILegacyDataPointValue> values, LegacyTagSource tagSource)
    {
        _batch = new WriteBatch<ILegacyDataPoint, ILegacyDataPointValue>(
            values, value => value.DataPoint, tagSource, LegacyConverters.For);
        ThrowIfAnythingWouldNotEncode(_batch.EncodeFailures);
    }

    /// <exception cref="LegacyTagException">An element was not written. The other elements were written.</exception>
    internal async Task WriteAsync(CancellationToken cancellationToken) =>
        ThrowIfAnyFailed(await _batch.WriteAsync(cancellationToken).ConfigureAwait(false));

    // Nothing reached the controller, so the caller can send the whole group again.
    private static void ThrowIfAnythingWouldNotEncode(
        IReadOnlyList<WriteBatch<ILegacyDataPoint, ILegacyDataPointValue>.WriteFailure> failures)
    {
        if (failures.Count == 0)
        {
            return;
        }

        throw new LegacyTagException(
            $"Write failed for {WriteBatch<ILegacyDataPoint, ILegacyDataPointValue>.Describe(failures)}. " +
            "Nothing was sent to the controller.");
    }

    // ILegacyWriteClient.WriteAsync returns a plain ValueTask, so an exception is the only way to tell
    // the caller that a write failed.
    private static void ThrowIfAnyFailed(
        IReadOnlyList<WriteBatch<ILegacyDataPoint, ILegacyDataPointValue>.WriteFailure> failures)
    {
        if (failures.Count == 0)
        {
            return;
        }

        throw new LegacyTagException(
            $"Write failed for {WriteBatch<ILegacyDataPoint, ILegacyDataPointValue>.Describe(failures)}.");
    }
}
