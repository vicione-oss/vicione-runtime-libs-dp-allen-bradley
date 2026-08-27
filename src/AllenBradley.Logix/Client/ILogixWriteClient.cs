using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

/// <summary>
/// Writes a batch of Logix data point values to the controller.
/// </summary>
public interface ILogixWriteClient
{
    /// <summary>Encodes and writes every value in <paramref name="values"/>.</summary>
    /// <param name="values">The values to write.</param>
    /// <param name="cancellationToken">Cancels the batched write.</param>
    /// <exception cref="LogixTagException">One or more values could not be written; the message names
    /// every failed tag and its reason. The values it does not name were written — a failing tag does
    /// not stop the rest of the batch.</exception>
    ValueTask WriteAsync(IReadOnlyList<ILogixDataPointValue> values, CancellationToken cancellationToken);
}
