using Microsoft.Extensions.Logging.Abstractions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access.LibPlcTag;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;

/// <summary>
/// The production client stack against the controller <see cref="TestControllerConfiguration"/> describes, connected from
/// the first test of <see cref="TestControllerCollection"/> to the last. The client must be disposed,
/// because a libplctag handle left to its finalizer fail-fasts the process with <c>0xC0000602</c>.
/// </summary>
public sealed class TestControllerFixture : IAsyncLifetime
{
    public TestControllerFixture()
    {
        var clientInformation = TestControllerConfiguration.ClientInformation;
        var accessFactory = new LogixTagAccessFactory(clientInformation);
        var tagManager = new CachingLogixTagManager(
            accessFactory,
            new SymbolTableLoader(accessFactory),
            NullLogger<CachingLogixTagManager>.Instance);

        TagManager = tagManager;
        Client = new LogixClient(tagManager, clientInformation, NullLogger<LogixClient>.Instance);
    }

    /// <summary>The client under test, for both directions, over one shared tag manager that it also owns.</summary>
    internal LogixClient Client { get; }

    /// <summary>The tag manager under <see cref="Client"/>, which hands out the tag each data point reads.</summary>
    internal ILogixTagManager TagManager { get; }

    /// <inheritdoc />
    public async ValueTask InitializeAsync() =>
        await Client.ConnectAsync(TestContext.Current.CancellationToken);

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        Client.Dispose();
        return ValueTask.CompletedTask;
    }
}
