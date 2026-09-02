using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access.LibPlcTag;

/// <summary>
/// What goes onto <c>Tag.Gateway</c>: the connection endpoint and the TCP port as one
/// <c>"10.0.0.1:44818"</c> string.
/// </summary>
/// <remarks>
/// It lives here rather than on <see cref="LogixClientInformation"/> because the joining is libplctag's
/// rule, not the controller's. The library has no port attribute and splits the colon out in its native
/// core, so this is the one place the two stop being separate values
/// (<c>docs/AllenBradley.Documentation/libPlcTag/the-port-in-the-gateway-attribute.md</c>).
/// </remarks>
internal readonly record struct GatewayAttribute(string Value)
{
    /// <summary>Joins the endpoint and port of <paramref name="clientInformation"/> the way libplctag wants them.</summary>
    internal static GatewayAttribute For(LogixClientInformation clientInformation) =>
        new($"{clientInformation.ConnectionEndpoint.Value}:{clientInformation.TcpPort.Value}");
}
