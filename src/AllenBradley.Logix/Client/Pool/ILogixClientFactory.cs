using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Pool;

/// <summary>
/// Builds the <see cref="ILogixClient"/>s <see cref="LogixClientPool"/> hands out — one per controller,
/// disconnected, for the pool to connect.
/// </summary>
/// <remarks>
/// A seam rather than a <c>new</c> in the pool: it is what lets the pool's tests drive reference
/// counting, racing connects and disposal without a controller. Nothing per-client is held by an
/// implementation, so one factory serves a whole pool.
/// </remarks>
internal interface ILogixClientFactory
{
    ILogixClient Create(LogixClientInformation clientInformation);
}
