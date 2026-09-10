using Microsoft.Extensions.Logging.Abstractions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access.LibPlcTag;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
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
            new TagDefinitionsLoader(accessFactory),
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
    /// The one shape every scalar round trip has: resolve the tag, write the value, read it back, and check
    /// both what the controller says the tag <em>is</em> and what came back out of it.
    /// </summary>
    private protected Task AssertRoundTripAsync<TDomain>(
        LogixDataPoint<TDomain> dataPoint, TDomain valueToWrite, TagDefinition expectedDefinition) =>
        AssertRoundTripAsync(dataPoint, valueToWrite, valueToWrite, expectedDefinition);

    /// <summary>
    /// A round trip whose read-back is not the value that went in, which only the <c>STRING</c> needs: a
    /// character outside Latin-1 has no byte to be stored as and comes back as <c>'?'</c>.
    /// </summary>
    private protected async Task AssertRoundTripAsync<TDomain>(
        LogixDataPoint<TDomain> dataPoint,
        TDomain valueToWrite,
        TDomain expectedValue,
        TagDefinition expectedDefinition)
    {
        // Arrange
        // Resolving as well as reading is what makes this say something: a value that survives a write and
        // a read is consistent with the tag being almost anything of the right width.
        var cancellationToken = TestContext.Current.CancellationToken;
        ILogixDataPoint[] dataPoints = [dataPoint];
        var group = new LogixDataPointGroup(DefaultPollFrequency, dataPoints);

        // Act
        var resolved = await Client.ResolveDataPoints(dataPoints, cancellationToken);
        await Client.WriteAsync([dataPoint.CreateLogixValue(valueToWrite)], cancellationToken);
        var readResult = await Client.ReadAsync(group, cancellationToken);

        // Assert
        resolved.Should().ContainSingle()
            .Which.Should().Be(new ResolvedDataPoint(dataPoint, expectedDefinition));
        readResult.Should().ContainSingle()
            .Which.Should().Be(dataPoint.CreateLogixValue(expectedValue));
    }
}
