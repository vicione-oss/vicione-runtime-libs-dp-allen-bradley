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
/// Builds the production client stack against the controller <see cref="TestController"/> describes,
/// connects it and tears it down; every derived suite writes the tags in <see cref="TagAddresses"/>.
/// Disposing the client is not optional: under MTP a libplctag handle left to its finalizer frees a
/// native handle after CLR teardown, which fail-fasts the process with <c>0xC0000602</c> on an otherwise
/// green run.
/// </summary>
[Trait("Category", "Integration")]
[Collection(PlcCollection.Name)]
public abstract class CompactLogix5X80IntegrationTestBase : IAsyncLifetime
{
    /// <param name="output">Where the suite writes what it saw on the device.</param>
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
        // Every test names the controller it ran against, so a result file identifies itself.
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
    /// The one shape every scalar round trip has: resolve the tag, write the value, read it back, and
    /// check both what the controller says the tag <em>is</em> and what came back out of it.
    /// </summary>
    /// <param name="dataPoint">The configured point, which decides the type the value is encoded as.</param>
    /// <param name="valueToWrite">The value written, and the value expected back.</param>
    /// <param name="expectedDefinition">The declaration the controller must report for the tag.</param>
    /// <typeparam name="TDomain">The .NET type the point exchanges — <c>int</c> for a <c>DINT</c>.</typeparam>
    private protected Task AssertRoundTripAsync<TDomain>(
        LogixDataPoint<TDomain> dataPoint, TDomain valueToWrite, TagDefinition expectedDefinition) =>
        AssertRoundTripAsync(dataPoint, valueToWrite, valueToWrite, expectedDefinition);

    /// <summary>
    /// A round trip whose read-back is not the value that went in, which only the <c>STRING</c> needs: a
    /// character outside Latin-1 has no byte to be stored as and comes back as <c>'?'</c>.
    /// </summary>
    /// <param name="dataPoint">The configured point, which decides the type the value is encoded as.</param>
    /// <param name="valueToWrite">The value written.</param>
    /// <param name="expectedValue">What the read must come back with, which is not what went in.</param>
    /// <param name="expectedDefinition">The declaration the controller must report for the tag.</param>
    /// <typeparam name="TDomain">The .NET type the point exchanges — <c>int</c> for a <c>DINT</c>.</typeparam>
    private protected async Task AssertRoundTripAsync<TDomain>(
        LogixDataPoint<TDomain> dataPoint,
        TDomain valueToWrite,
        TDomain expectedValue,
        TagDefinition expectedDefinition)
    {
        // Arrange
        // Resolving as well as reading is what makes a round trip say something: a value that survives a
        // write and a read is consistent with the tag being almost anything of the right width.
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
