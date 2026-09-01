using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

/// <summary>
/// Constructs the <see cref="LogixClientInformation"/> the client and pool suites are driven with.
/// </summary>
/// <remarks>
/// The pool keys on this record, so a test that wants a second controller changes the gateway and gets a
/// second entry; one that wants the same controller calls the factory twice and gets an equal value.
/// </remarks>
internal static class LogixClientTestDataFactory
{
    /// <summary>The controller every value the factory makes points at, unless a test names another.</summary>
    internal const string DefaultGateway = "10.0.0.1";

    internal static LogixClientInformation CreateClientInformation(string gateway = DefaultGateway) =>
        new(new Gateway(gateway),
            new CipRoutePath("1,0"),
            new OperationTimeout(TimeSpan.FromSeconds(5)));
}
