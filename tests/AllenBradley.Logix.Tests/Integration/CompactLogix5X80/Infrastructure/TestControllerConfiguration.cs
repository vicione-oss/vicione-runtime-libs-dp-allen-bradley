using System.Globalization;
using libplctag;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;

/// <summary>
/// The CompactLogix 5069-L306ER this folder's suites run against, and the <b>only</b> place that says how
/// it is reached. The environment can override every default below. See
/// <c>docs/AllenBradley.Logix.Documentation/how-to/run-the-integration-tests.md</c>.
/// </summary>
internal static class TestControllerConfiguration
{
    /// <summary>
    /// The controller kind every assumption in this folder rests on. libplctag opens every Logix the same
    /// way, so the client never receives it. Every run writes it to the test output.
    /// </summary>
    internal static LogixControllerKind Kind => LogixControllerKind.CompactLogix5X80;

    private const string EndpointVariable = "CIP_5X80_GATEWAY";
    private const string PortVariable = "CIP_5X80_PORT";
    private const string RoutePathVariable = "CIP_5X80_PATH";
    private const string TimeoutVariable = "CIP_5X80_TIMEOUT_SECONDS";

    private const string DefaultEndpoint = "10.100.11.234";

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
    /// One line naming the controller a run talked to. It goes into every test's output, so a result file
    /// says which device produced it.
    /// </summary>
    internal static string Description =>
        $"{Kind.Family} {Kind.Generation} at {ClientInformation.ConnectionEndpoint.Value}:" +
        $"{ClientInformation.TcpPort.Value} via CIP route path {ClientInformation.CipRoutePath.Value}";

    /// <summary>
    /// A raw libplctag handle on the same controller, for the suites that hold this dataport against
    /// libplctag's own accessors. The caller owns it. A handle left to its finalizer fail-fasts the
    /// process (0xC0000602).
    /// </summary>
    internal static Tag RawTagFor(string tagName) => new()
    {
        Gateway = $"{ClientInformation.ConnectionEndpoint.Value}:{ClientInformation.TcpPort.Value}",
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

    // A malformed override is refused instead of falling back to the default. A fallback would connect to
    // the wrong place, and the failure would show up as a timeout half an hour later.
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
