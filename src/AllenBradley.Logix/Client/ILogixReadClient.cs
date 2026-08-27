using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

/// <summary>
/// Reads a group of Logix data points in one batched operation and returns a typed value per point.
/// </summary>
public interface ILogixReadClient
{
    /// <summary>
    /// Reads every data point in <paramref name="dataPoints"/> and returns the results in the same order.
    /// A point whose read fails comes back with <see cref="LogixQuality.Bad"/> rather than sinking
    /// the whole group.
    /// </summary>
    /// <param name="dataPoints">The data points to read.</param>
    /// <param name="cancellationToken">Cancels the batched read.</param>
    ValueTask<IReadOnlyList<ILogixDataPointValue>> ReadAsync(
        IReadOnlyList<ILogixDataPoint> dataPoints, CancellationToken cancellationToken);
}
