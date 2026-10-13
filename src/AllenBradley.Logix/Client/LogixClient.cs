using Microsoft.Extensions.Logging;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Batches;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Pool;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

/// <summary>
/// The controller client: reads and writes Logix data points over one shared tag manager, and owns that
/// manager's lifetime.
/// </summary>
internal sealed class LogixClient(
    ILogixTagManager tagManager,
    LogixClientInformation clientInformation,
    ILogger<LogixClient> logger) : ILogixClient
{
    private readonly string _connectionEndpoint = clientInformation.ConnectionEndpoint.Value;
    private readonly string _cipRoutePath = clientInformation.CipRoutePath.Value;

    private readonly ClientLifetime _lifetime = new(
        logger,
        new ClientAddress(clientInformation.ConnectionEndpoint.Value, clientInformation.CipRoutePath.Value));

    private readonly LoggedBatches _batches = new(logger, clientInformation.ConnectionEndpoint.Value);

    /// <inheritdoc />
    public bool IsConnected => _lifetime.IsConnected;

    /// <inheritdoc />
    public Task ConnectAsync(CancellationToken cancellationToken) =>
        _lifetime.ConnectAsync(LoadSymbolTableAsync, cancellationToken);

    // The tag manager's own gate is what makes a concurrent second connect free.
    private Task LoadSymbolTableAsync(CancellationToken cancellationToken)
    {
        logger.LoadingSymbolTable(_connectionEndpoint, _cipRoutePath);
        return tagManager.LoadSymbolTableAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task DisconnectAsync(CancellationToken cancellationToken)
    {
        _lifetime.Disconnect(tagManager.Drain);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ResolvedDataPoint>> ResolveDataPoints(
        IReadOnlyList<ILogixDataPoint> dataPoints, CancellationToken cancellationToken)
    {
        // Refuses an unconnected client rather than browsing on its behalf, which would hide a caller that
        // has broken the lifecycle.
        _lifetime.ThrowIfNotConnected(
            $"The client for '{_connectionEndpoint}' via CIP route path '{_cipRoutePath}' must be connected before its data " +
            "points can be resolved; connect is what browses the symbol table they resolve against.");

        var resolved = new ResolvedDataPoint[dataPoints.Count];
        for (var i = 0; i < dataPoints.Count; i++)
        {
            // Deliberately the very tag the polls will run against, not a second lookup of it.
            resolved[i] = tagManager.TagFor(dataPoints[i]).Resolved;
        }

        return Task.FromResult<IReadOnlyList<ResolvedDataPoint>>(resolved);
    }

    /// <inheritdoc />
    public ValueTask<IReadOnlyList<ILogixDataPointValue>> ReadAsync(
        LogixDataPointGroup dataPointGroup, CancellationToken cancellationToken) =>
        _batches.ReadAsync(
            dataPointGroup.DataPoints.Count,
            token => new LogixReadBatch(dataPointGroup.DataPoints, tagManager).ReadAsync(token),
            cancellationToken);

    /// <inheritdoc />
    public ValueTask WriteAsync(
        IReadOnlyList<ILogixDataPointValue> values, CancellationToken cancellationToken) =>
        _batches.WriteAsync(
            values.Count,
            token => new LogixWriteBatch(values, tagManager).WriteAsync(token),
            cancellationToken);

    /// <summary>
    /// Ends the client and the tag manager with it. Disposing is what frees the native handles.
    /// </summary>
    public void Dispose() => _lifetime.End(tagManager.Dispose);
}
