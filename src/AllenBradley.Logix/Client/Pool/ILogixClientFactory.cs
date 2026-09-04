using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Pool;

/// <summary>
/// Builds the <see cref="ILogixClient"/>s <see cref="LogixClientPool"/> hands out — one per controller,
/// disconnected, for the pool to connect.
/// </summary>
internal interface ILogixClientFactory
{
    ILogixClient Create(LogixClientInformation clientInformation);
}
