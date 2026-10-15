using Microsoft.Extensions.Logging;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Batches;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Pool;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Exceptions;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client;

/// <summary>
/// Reads and writes the data points of one Legacy controller. It gets its tag handles from a
/// <see cref="LegacyTagSource"/> and disposes that source when it is disposed.
/// </summary>
internal sealed class LegacyClient(
    LegacyTagSource tagSource,
    LegacyClientInformation clientInformation,
    ILogger<LegacyClient> logger) : ILegacyClient
{
    // The current libplctag version has no request to check a connection. Every PCCC controller has the status file S2.
    // We read it once to verify that the plc is reachable.
    internal static readonly DataFileAddress StatusFile = new(DataFileType.Status, new FileNumber(2),
        new ElementNumber(0));

    private readonly ClientLifetime _lifetime = new(logger,
        new ClientAddress(clientInformation.ConnectionEndpoint.Value, clientInformation.CipRoutePath.Value));

    private readonly LoggedBatches _batches = new(logger, clientInformation.ConnectionEndpoint.Value);

    public bool IsConnected => _lifetime.IsConnected;

    public Task ConnectAsync(CancellationToken cancellationToken) =>
        _lifetime.ConnectAsync(CheckThatTheControllerIsReachable, cancellationToken);

    private async Task CheckThatTheControllerIsReachable(CancellationToken cancellationToken)
    {
        using var statusWordAccess = tagSource.CreateUncachedAccess(StatusFile);
        var statusWord = await statusWordAccess.ReadAsync(cancellationToken).ConfigureAwait(false);
        if (!statusWord.Succeeded)
        {
            throw new DataRetrievalException($"Read failed for {StatusFile}: {statusWord.Error}");
        }
    }

    public Task DisconnectAsync(CancellationToken cancellationToken)
    {
        _lifetime.Disconnect(tagSource.Drain);
        return Task.CompletedTask;
    }

    public ValueTask<IReadOnlyList<ILegacyDataPointValue>> ReadAsync(
        LegacyDataPointGroup dataPointGroup, CancellationToken cancellationToken) =>
        _batches.ReadAsync(
            dataPointGroup.DataPoints.Count,
            token => new LegacyReadBatch(dataPointGroup.DataPoints, tagSource).ReadAsync(token),
            cancellationToken);

    public ValueTask WriteAsync(IReadOnlyList<ILegacyDataPointValue> values, CancellationToken cancellationToken) =>
        _batches.WriteAsync(
            values.Count,
            token => new LegacyWriteBatch(values, tagSource).WriteAsync(token),
            cancellationToken);

    /// <summary>Disposes the tag source, which disposes every tag handle.</summary>
    public void Dispose() => _lifetime.End(tagSource.Dispose);
}
