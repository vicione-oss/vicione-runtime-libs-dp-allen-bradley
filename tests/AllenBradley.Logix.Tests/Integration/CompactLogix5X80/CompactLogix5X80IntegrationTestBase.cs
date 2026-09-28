using Microsoft.Extensions.Logging.Abstractions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access.LibPlcTag;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80;

/// <summary>
/// Builds the production client stack against the controller <see cref="TestController"/> describes and
/// tears it down; every derived suite writes the tags in <see cref="TagAddresses"/>. Disposing the client
/// is not optional: a libplctag handle left to its finalizer fail-fasts the process with <c>0xC0000602</c>.
/// </summary>
[Trait("Category", "Integration")]
[Collection(PlcCollection.Name)]
public abstract class CompactLogix5X80IntegrationTestBase : IAsyncLifetime
{
    protected CompactLogix5X80IntegrationTestBase(ITestOutputHelper output)
    {
        Output = output;

        var clientInformation = TestController.ClientInformation;
        var accessFactory = new LogixTagAccessFactory(clientInformation);
        var tagManager = new CachingLogixTagManager(
            accessFactory,
            new SymbolTableLoader(accessFactory),
            NullLogger<CachingLogixTagManager>.Instance);

        Client = new LogixClient(tagManager, clientInformation, NullLogger<LogixClient>.Instance);
    }

    /// <summary>The client under test — both directions, over one shared tag manager it also owns.</summary>
    internal LogixClient Client { get; }

    /// <summary>Where a suite reports what it saw on the device, for the runs that pass.</summary>
    protected ITestOutputHelper Output { get; }

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        Output.WriteLine(TestController.Description);
        await Client.ConnectAsync(TestContext.Current.CancellationToken);
    }

    /// <inheritdoc />
    public virtual ValueTask DisposeAsync()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// The one shape every round trip has: resolve the tag, write the value, read it back, and hand both to
    /// the caller. Resolving is what makes the read-back say something: a value that survives a write and a
    /// read is consistent with the tag being almost anything of the right width.
    /// </summary>
    private protected async Task<RoundTripResult> RoundTripAsync<TDomain>(
        LogixDataPoint<TDomain> dataPoint, TDomain valueToWrite)
    {
        ILogixDataPoint[] dataPoints = [dataPoint];
        var group = new LogixDataPointGroup(DefaultPollFrequency, dataPoints);

        var resolved = await Client.ResolveDataPoints(dataPoints, TestContext.Current.CancellationToken);
        await Client.WriteAsync([dataPoint.CreateLogixValue(valueToWrite)], TestContext.Current.CancellationToken);
        var readResult = await Client.ReadAsync(group, TestContext.Current.CancellationToken);

        return new RoundTripResult(resolved.Single(), readResult.Single());
    }

    /// <summary>
    /// The one shape every per-element write has: zero the array whole, write one element through its own
    /// data point, and read the array whole again. A round trip of the element alone cannot tell a write
    /// that reached the right element from one that reached its neighbour; the whole array can.
    /// </summary>
    private protected async Task<TElement[]> WriteOneElementOfAZeroedArrayAsync<TElement>(
        LogixArrayDataPoint<TElement> wholeArray, LogixDataPoint<TElement> element, TElement valueToWrite)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var zeros = new TElement[wholeArray.ElementCount.Value];
        var group = new LogixDataPointGroup(DefaultPollFrequency, [wholeArray]);

        await Client.WriteAsync([wholeArray.CreateLogixValue(zeros)], cancellationToken);
        await Client.WriteAsync([element.CreateLogixValue(valueToWrite)], cancellationToken);
        var readResult = await Client.ReadAsync(group, cancellationToken);

        return ((ILogixDataPointValue<TElement[]>)readResult.Single()).TypedValue;
    }

    /// <summary>
    /// A scalar value compares by content, so a test asserts the whole record; an array value compares by
    /// reference, so a test asserts the elements of <see cref="ReadValue"/> instead.
    /// </summary>
    private protected sealed record RoundTripResult(ResolvedDataPoint Resolved, ILogixDataPointValue ReadValue);
}
