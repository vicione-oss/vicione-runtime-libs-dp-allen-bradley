using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

/// <summary>
/// The controller client: reads and writes Logix data points over one shared tag manager.
/// </summary>
/// <remarks>
/// One class serves both directions so the incoming and outgoing dataports — created separately but
/// targeting the same controller — share its connection; the <see cref="ILogixReadClient"/> /
/// <see cref="ILogixWriteClient"/> seams stay split so each dataport depends only on the direction
/// it uses (ADR-002).
/// </remarks>
/// <param name="tagManager">Resolves the tag for each data point this client reads or writes.</param>
internal sealed class LogixClient(ILogixTagManager tagManager) : ILogixReadClient, ILogixWriteClient
{
    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<ILogixDataPointValue>> ReadAsync(
        IReadOnlyList<ILogixDataPoint> dataPoints, CancellationToken cancellationToken) =>
        await new LogixReadBatch(dataPoints, tagManager).ReadAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async ValueTask WriteAsync(
        IReadOnlyList<ILogixDataPointValue> values, CancellationToken cancellationToken) =>
        await new LogixWriteBatch(values, tagManager).WriteAsync(cancellationToken).ConfigureAwait(false);
}
