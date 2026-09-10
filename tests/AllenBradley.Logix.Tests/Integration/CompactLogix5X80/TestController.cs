using System.Globalization;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80;

/// <summary>
/// The CompactLogix 5069-L306ER this folder's suites run against, and the <b>only</b> place that says how
/// it is reached. The device is on hand but not commissioned, so every default below is an assumption,
/// each overridable from the environment; see
/// <c>docs/AllenBradley.Documentation/context/TEST-DEVICE-SETUP.md</c>.
/// </summary>
internal static class TestController
{
    /// <summary>
    /// The controller kind every assumption in this folder rests on. Not passed to the client — libplctag
    /// opens every Logix the same way — but written to the test output on every run.
    /// </summary>
    internal static LogixControllerKind Kind => LogixControllerKind.CompactLogix5X80;

    private const string EndpointVariable = "CIP_5X80_GATEWAY";
    private const string PortVariable = "CIP_5X80_PORT";
    private const string RoutePathVariable = "CIP_5X80_PATH";
    private const string TimeoutVariable = "CIP_5X80_TIMEOUT_SECONDS";

    // A placeholder until the L306ER has an address of its own: the unconfigured second CompactLogix on
    // the lab subnet (TEST-DEVICE-SETUP.md).
    private const string DefaultEndpoint = "192.168.0.102";

    private const int DefaultTimeoutSeconds = 10;

    /// <summary>
    /// The same value the pool keys a production connection under, so these suites connect as a deployment
    /// does.
    /// </summary>
    internal static LogixClientInformation ClientInformation { get; } = new(
        new ConnectionEndpoint(Configured(EndpointVariable, DefaultEndpoint)),
        new TcpPort(ConfiguredPort()),
        new CipRoutePath(Configured(RoutePathVariable, CipRoutePath.VirtualBackplane.Value)),
        new OperationTimeout(TimeSpan.FromSeconds(ConfiguredTimeoutSeconds())));

    /// <summary>
    /// One line naming the controller a run talked to, written to every test's output so a result file
    /// says which device produced it.
    /// </summary>
    internal static string Description =>
        $"{Kind.Family} {Kind.Generation} at {ClientInformation.ConnectionEndpoint.Value}:" +
        $"{ClientInformation.TcpPort.Value} via CIP route path {ClientInformation.CipRoutePath.Value}";

    private static string Configured(string variable, string fallback)
    {
        var value = Environment.GetEnvironmentVariable(variable);
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    // A malformed override is refused rather than quietly falling back on the default, which would be a
    // connection to the wrong place diagnosed as a timeout half an hour later.
    private static ushort ConfiguredPort()
    {
        var configured = Configured(PortVariable, string.Empty);
        if (configured.Length == 0)
        {
            return TcpPort.EtherNetIp.Value;
        }

        return ushort.TryParse(configured, CultureInfo.InvariantCulture, out var port)
            ? port
            : throw new InvalidOperationException(
                $"'{PortVariable}' is set to '{configured}', which is not a TCP port number.");
    }

    /// <inheritdoc cref="ConfiguredPort"/>
    private static int ConfiguredTimeoutSeconds()
    {
        var configured = Configured(TimeoutVariable, string.Empty);
        if (configured.Length == 0)
        {
            return DefaultTimeoutSeconds;
        }

        return int.TryParse(configured, CultureInfo.InvariantCulture, out var seconds) && seconds > 0
            ? seconds
            : throw new InvalidOperationException(
                $"'{TimeoutVariable}' is set to '{configured}', which is not a positive number of seconds.");
    }
}
