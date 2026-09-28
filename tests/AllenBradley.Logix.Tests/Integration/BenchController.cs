using libplctag;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration;

/// <summary>
/// The borrowed CompactLogix L32E this folder's suites run against, and the only place that says how it
/// is reached; what lives on it is in <see cref="BenchControllerTags"/>.
/// </summary>
internal static class BenchController
{
    private const string EndpointVariable = "CIP_GATEWAY";
    private const string RoutePathVariable = "CIP_PATH";

    // The L32E as it is reached through the ifm demo cell's Link Manager tunnel; see
    // docs/AllenBradley.Documentation/test-bench/test-device-setup.md.
    private const string DefaultEndpoint = "192.168.0.100";

    private static readonly OperationTimeout Timeout = new(TimeSpan.FromSeconds(10));

    /// <summary>
    /// The same value the pool keys a production connection under, so these suites connect as a deployment
    /// does.
    /// </summary>
    internal static LogixClientInformation ClientInformation { get; } = new(
        new ConnectionEndpoint(Configured(EndpointVariable, DefaultEndpoint)),
        TcpPort.EtherNetIp,
        new CipRoutePath(Configured(RoutePathVariable, CipRoutePath.VirtualBackplane.Value)),
        Timeout);

    /// <summary>
    /// A raw libplctag handle on the same controller, for the suites that hold this dataport's decode
    /// against libplctag's own accessors. The caller owns it: a handle left to its finalizer fail-fasts
    /// the process (0xC0000602).
    /// </summary>
    internal static Tag RawTagFor(string tagName) => new()
    {
        Gateway = ClientInformation.ConnectionEndpoint.Value,
        Path = ClientInformation.CipRoutePath.Value,
        PlcType = PlcType.ControlLogix,
        Protocol = Protocol.ab_eip,
        Name = tagName,
        Timeout = ClientInformation.OperationTimeout.Value,
    };

    private static string Configured(string variable, string fallback)
    {
        var value = Environment.GetEnvironmentVariable(variable);

        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}
