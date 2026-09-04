using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

/// <summary>
/// Constructs the <see cref="LogixClientInformation"/> the client and pool suites are driven with.
/// </summary>
internal static class LogixClientTestDataFactory
{
    internal static readonly OperationTimeout DefaultOperationTimeout = new(TimeSpan.FromSeconds(5));
    internal static readonly ConnectionEndpoint DefaultConnectionEndpoint = new("10.0.0.1");

    internal static readonly TcpPort DefaultTcpPort = new(44818);
    internal static readonly CipRoutePath DefaultCipRoutePath = new("1,0");


    internal static LogixClientInformation DefaultClientInformation() =>
        new(DefaultConnectionEndpoint,
            DefaultTcpPort,
            DefaultCipRoutePath,
            DefaultOperationTimeout);
}
