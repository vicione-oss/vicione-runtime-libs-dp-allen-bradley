using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Testing.Logging;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.LegacyDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Integration;

/// <summary>
/// Builds the production client against <see cref="AbServer"/> and disposes it after each test. Disposing the
/// client is not optional: a libplctag handle left to its finalizer fail-fasts the process.
/// <para>
/// The client does not call <c>ConnectAsync</c>, because the connect reads the status file <c>S2:0</c> and
/// ab_server has no status file. ab_server knows only the data files in <see cref="AbServerDataFiles"/>. It has no
/// timer, counter, control, I/O or <c>A</c> files, and it refuses sub-elements such as <c>T4:0.ACC</c>. A test
/// that needs one of these stays a unit test.
/// </para>
/// <para>
/// The source, the checksums and the license of the ab_server binaries are in <c>binaries/README.md</c> beside
/// this file.
/// </para>
/// </summary>
[Trait("Category", "Integration")]
[Collection(AbServerCollection.Name)]
public abstract class AbServerIntegrationTestBase : IDisposable
{
    protected AbServerIntegrationTestBase(AbServer server)
    {
        var clientInformation = server.ClientInformation;
        Client = new LegacyClient(
            new LegacyTagSource(
                new LegacyTagAccessFactory(clientInformation),
                TestLogging.CreateLogger<LegacyTagSource>()),
            clientInformation,
            TestLogging.CreateLogger<LegacyClient>());
    }

    internal LegacyClient Client { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>Writes <paramref name="valueToWrite"/> to the element of the data point and reads the element back.</summary>
    private protected async Task<IReadOnlyList<ILegacyDataPointValue>> RoundTripAsync<TDomain>(
        LegacyDataPoint<TDomain> dataPoint, TDomain valueToWrite)
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await Client.WriteAsync([dataPoint.CreateLegacyValue(valueToWrite)], cancellationToken);
        return await Client.ReadAsync(new LegacyDataPointGroup(DefaultPollFrequency, [dataPoint]), cancellationToken);
    }
}
