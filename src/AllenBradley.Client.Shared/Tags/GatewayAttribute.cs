namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tags;

/// <summary>
/// What goes onto <c>RootTagName.Gateway</c>: the connection endpoint and the TCP port as one
/// <c>"10.0.0.1:44818"</c> string.
/// </summary>
internal readonly record struct GatewayAttribute(string Value)
{
    /// <summary>Joins the endpoint and the port the way libplctag wants them.</summary>
    internal static GatewayAttribute For(string connectionEndpoint, ushort tcpPort) =>
        new($"{connectionEndpoint}:{tcpPort}");
}
