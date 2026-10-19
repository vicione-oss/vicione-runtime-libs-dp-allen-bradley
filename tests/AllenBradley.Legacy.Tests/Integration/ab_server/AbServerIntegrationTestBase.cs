using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Testing.Logging;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.LegacyDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Integration;

/// <summary>
/// Builds the production client against <see cref="AbServer"/> and disposes it after each test. The client does
/// not connect, because the connect reads the status file <c>S2</c> and ab_server has no status file. Disposing
/// the client is not optional: a libplctag handle left to its finalizer fail-fasts the process.
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
