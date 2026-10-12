using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Batches;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

/// <summary>
/// A <see cref="WriteBatch{TDataPoint, TValue}"/> over Logix values. It sends nothing when any value would
/// not encode, and throws a <see cref="LogixTagException"/> naming every value that did not reach its tag.
/// </summary>
internal sealed class LogixWriteBatch
{
    private readonly WriteBatch<ILogixDataPoint, ILogixDataPointValue> _batch;

    /// <exception cref="LogixTagException">A value would not encode; nothing was sent.</exception>
    internal LogixWriteBatch(IReadOnlyList<ILogixDataPointValue> values, ILogixTagManager tagManager)
    {
        _batch = new WriteBatch<ILogixDataPoint, ILogixDataPointValue>(
            values, value => value.DataPoint, tagManager, DataPointConverterRegistry.GetConverter);
        ThrowIfAnythingWouldNotEncode(_batch.EncodeFailures);
    }

    /// <exception cref="LogixTagException">A tag would not take its value; the others were written.</exception>
    internal async Task WriteAsync(CancellationToken cancellationToken) =>
        ThrowIfAnyFailed(await _batch.WriteAsync(cancellationToken).ConfigureAwait(false));

    // Said separately from the device failures because it is a different situation to recover from: the
    // controller has seen nothing, so the caller re-drives the whole group rather than the failed part.
    private static void ThrowIfAnythingWouldNotEncode(
        IReadOnlyList<WriteBatch<ILogixDataPoint, ILogixDataPointValue>.WriteFailure> failures)
    {
        if (failures.Count == 0)
        {
            return;
        }

        throw new LogixTagException(
            $"Write failed for {WriteBatch<ILogixDataPoint, ILogixDataPointValue>.Describe(failures)}. " +
            "Nothing was sent to the controller.");
    }

    // ILogixWriteClient.WriteAsync hands the caller a bare ValueTask, so an exception is the only thing
    // that stops a dropped write from being silent.
    private static void ThrowIfAnyFailed(
        IReadOnlyList<WriteBatch<ILogixDataPoint, ILogixDataPointValue>.WriteFailure> failures)
    {
        if (failures.Count == 0)
        {
            return;
        }

        throw new LogixTagException(
            $"Write failed for {WriteBatch<ILogixDataPoint, ILogixDataPointValue>.Describe(failures)}.");
    }
}
