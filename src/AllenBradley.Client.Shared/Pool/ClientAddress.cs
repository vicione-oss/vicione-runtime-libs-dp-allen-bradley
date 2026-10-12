namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Pool;

/// <summary>What the pool's log messages name a controller by.</summary>
/// <param name="ConnectionEndpoint">The IP address or host name the session is opened to.</param>
/// <param name="CipRoutePath">The hops from that endpoint to the controller.</param>
internal readonly record struct ClientAddress(string ConnectionEndpoint, string CipRoutePath);
