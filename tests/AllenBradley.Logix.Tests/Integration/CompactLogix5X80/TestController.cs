using System.Globalization;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80;

/// <summary>
/// The CompactLogix 5069-L306ER this folder's suites run against — our own controller, a CompactLogix
/// 5380 and so a 5X80 — and the <b>only</b> place that says how it is reached. Nothing else here names
/// an address, a port or a route path.
/// </summary>
/// <remarks>
/// The device is on hand but not commissioned, so every default below is an assumption rather than a
/// fact, and each one is overridable from the environment — see the table in
/// <c>docs/AllenBradley.Documentation/context/TEST-DEVICE-SETUP.md</c>. A direct connection is assumed:
/// the endpoint is the controller itself, on the EtherNet/IP port, over the virtual backplane a
/// DIN-rail controller always presents.
/// <para>
/// If it turns out to sit behind a tunnel or a bridge instead, this file is what changes — a fixture
/// that opens the tunnel, and an endpoint pointing at its local end, the way the sibling S7 repo's
/// <c>SshPortForwardingFixture</c> does. The suites are written against
/// <see cref="ClientInformation"/> and cannot tell the difference.
/// </para>
/// <para>
/// The variables are prefixed <c>CIP_5X80_</c> rather than sharing the <c>CIP_</c> ones: those point at
/// the 5X70 L32E that <c>Integration/CompactLogix5X70</c> targets, and pointing this suite at that
/// controller would fail on the <c>LREAL</c> it has not got — for confusing reasons.
/// </para>
/// </remarks>
internal static class TestController
{
    /// <summary>
    /// The controller kind every assumption in this folder rests on. A 5X80 is what has the whole type
    /// vocabulary the addon implements: on a 5X70 the <c>LREAL</c> suite addresses a type the controller
    /// cannot resolve. Not passed to the client — libplctag opens every Logix the same way — but it is
    /// what makes this folder a folder, so it is written to the test output on every run.
    /// </summary>
    internal static LogixControllerKind Kind => LogixControllerKind.CompactLogix5X80;

    private const string EndpointVariable = "CIP_5X80_GATEWAY";
    private const string PortVariable = "CIP_5X80_PORT";
    private const string RoutePathVariable = "CIP_5X80_PATH";
    private const string TimeoutVariable = "CIP_5X80_TIMEOUT_SECONDS";

    // Where the L306ER will sit is not decided yet, so this is a placeholder: 192.168.0.102 is the
    // second CompactLogix on the lab subnet — labelled AB_CompactLogix and not configured at the time
    // of writing (TEST-DEVICE-SETUP.md) — chosen because it is at least a real address on the network
    // the tunnel routes, rather than an invented one.
    private const string DefaultEndpoint = "192.168.0.102";

    private const int DefaultTimeoutSeconds = 10;

    /// <summary>
    /// Which controller to reach and how long one operation against it may take — the same value the
    /// pool keys a production connection under, so these suites connect exactly as a deployment does.
    /// </summary>
    internal static LogixClientInformation ClientInformation { get; } = new(
        new ConnectionEndpoint(Configured(EndpointVariable, DefaultEndpoint)),
        new TcpPort(ConfiguredPort()),
        new CipRoutePath(Configured(RoutePathVariable, CipRoutePath.VirtualBackplane.Value)),
        new OperationTimeout(TimeSpan.FromSeconds(ConfiguredTimeoutSeconds())));

    /// <summary>
    /// One line naming the controller a run talked to, written to every test's output so that a result
    /// file says which device produced it rather than leaving it to be inferred from when it was run.
    /// </summary>
    internal static string Description =>
        $"{Kind.Family} {Kind.Generation} at {ClientInformation.ConnectionEndpoint.Value}:" +
        $"{ClientInformation.TcpPort.Value} via CIP route path {ClientInformation.CipRoutePath.Value}";

    private static string Configured(string variable, string fallback)
    {
        var value = Environment.GetEnvironmentVariable(variable);
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    // A malformed override is refused rather than quietly falling back: a typo that silently restores
    // the default is a connection to the wrong place, diagnosed as a timeout half an hour later.
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
