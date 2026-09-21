using Microsoft.Extensions.Logging.Abstractions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access.LibPlcTag;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X70;

/// <summary>
/// Builds the production client stack against the controller <see cref="BenchController"/> describes and
/// tears it down. Disposing the client is not optional: under MTP a libplctag handle left to its finalizer
/// fail-fasts the process with <c>0xC0000602</c> on otherwise green tests.
/// </summary>
[Trait("Category", "Integration")]
[Collection(PlcCollection.Name)]
public abstract class LogixIntegrationTestBase : IAsyncLifetime
{
    private readonly CachingLogixTagManager _tagManager;

    protected LogixIntegrationTestBase()
    {
        var clientInformation = BenchController.ClientInformation;
        var accessFactory = new LogixTagAccessFactory(clientInformation);

        _tagManager = new CachingLogixTagManager(
            accessFactory,
            new SymbolTableLoader(accessFactory),
            NullLogger<CachingLogixTagManager>.Instance);

        Client = new LogixClient(_tagManager, clientInformation, NullLogger<LogixClient>.Instance);
    }

    /// <summary>The client under test — both directions, over the one shared tag manager.</summary>
    internal LogixClient Client { get; }

    /// <summary>
    /// The tag manager, which joins the controller's declaration onto every tag — how a suite asserts what
    /// the controller says a tag <em>is</em>.
    /// </summary>
    internal ILogixTagManager TagManager => _tagManager;

    /// <inheritdoc />
    public async ValueTask InitializeAsync() =>
        await Client.ConnectAsync(TestContext.Current.CancellationToken);

    /// <inheritdoc />
    public virtual ValueTask DisposeAsync()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
