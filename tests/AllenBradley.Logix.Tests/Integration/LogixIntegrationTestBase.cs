using Microsoft.Extensions.Logging.Abstractions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access.LibPlcTag;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using Path = ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device.Path;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration;

/// <summary>
/// Builds the production client stack against the real CompactLogix L32E and tears it down —
/// <c>LogixTagAccessFactory</c> → <c>LogixSchemaBrowser</c> → <c>CachingLogixTagManager</c> →
/// <c>LogixClient</c>. Every layer a derived suite exercises is the shipping code; this only supplies
/// the connection settings. Requires the device reachable (see TEST-DEVICE-SETUP.md).
/// </summary>
/// <remarks>
/// <para>
/// The stack is assembled here rather than taken from <c>LogixClientFactory</c> so that
/// <see cref="TagManager"/> stays reachable — a suite needs the controller's declaration for a tag, and
/// the factory's product hides it. It is the same chain the factory builds, which is what makes that
/// shape worth trusting.
/// </para>
/// <para>
/// <see cref="InitializeAsync"/> connects, which is the browse the whole stack is gated on:
/// <c>TagFor</c> throws until it has run, and it doubles as the session warm-up.
/// </para>
/// <para>
/// <b>Disposing the client is not optional.</b> Under MTP the test assembly <em>is</em> the process, so
/// its exit code is the suite's. A libplctag handle left to its finalizer is freed after CLR teardown
/// and fail-fasts the run with <c>0xC0000602</c> — on otherwise green tests. The client owns the tag
/// manager, which owns every handle, so disposing it here is what frees them.
/// </para>
/// </remarks>
[Trait("Category", "Integration")]
[Collection(PlcCollection.Name)]
public abstract class LogixIntegrationTestBase : IAsyncLifetime
{
    private static readonly string Gateway = Environment.GetEnvironmentVariable("CIP_GATEWAY") ?? "192.168.0.100";
    private static readonly string Path = Environment.GetEnvironmentVariable("CIP_PATH") ?? "1,0";

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    private readonly CachingLogixTagManager _tagManager;

    protected LogixIntegrationTestBase()
    {
        var clientInformation = new LogixClientInformation(
            new Gateway(Gateway),
            new Path(Path),
            LogixControllerType.ControlLogix,
            new OperationTimeout(Timeout));

        var accessFactory = new LogixTagAccessFactory(clientInformation);

        _tagManager = new CachingLogixTagManager(
            accessFactory,
            new TagDefinitionsLoader(accessFactory),
            NullLogger<CachingLogixTagManager>.Instance);

        Client = new LogixClient(_tagManager, clientInformation, NullLogger<LogixClient>.Instance);
    }

    /// <summary>The client under test — both directions, over the one shared tag manager.</summary>
    internal LogixClient Client { get; }

    /// <summary>
    /// The tag manager, for the facts that are not values: it joins the controller's declaration onto
    /// every tag, which is how a suite asserts what the controller says a tag <em>is</em>. This is the
    /// CIP analogue of S7's <c>Client.ResolveDataPoints</c>, which has no equivalent on the client seam
    /// here yet.
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
