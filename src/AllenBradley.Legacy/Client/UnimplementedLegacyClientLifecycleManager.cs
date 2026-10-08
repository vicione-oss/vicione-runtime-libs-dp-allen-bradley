using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Client;
using ViciOne.Suite.DataPort.Extensions.Exceptions;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client;

/// <summary>
/// Stands in for the client pool until the Legacy client exists, so a deployed port reports why it cannot
/// connect instead of publishing nothing without a reason.
/// </summary>
internal sealed class UnimplementedLegacyClientLifecycleManager
    : IClientLifecycleManager<ILegacyClient, LegacyClientInformation>
{
    /// <inheritdoc />
    /// <exception cref="ConnectionFailureException">Always, because the Legacy client is not implemented yet.</exception>
    public ValueTask<ILegacyClient> AcquireConnectedAsync(
        LegacyClientInformation clientInformation, CancellationToken cancellationToken) =>
        throw new ConnectionFailureException("The Legacy client is not implemented yet.");

    /// <inheritdoc />
    public ValueTask ReleaseAsync(LegacyClientInformation clientInformation, CancellationToken cancellationToken) =>
        ValueTask.CompletedTask;
}
