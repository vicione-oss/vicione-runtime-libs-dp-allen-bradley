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
/// Base class for the CompactLogix 5X80 round-trip suites. Builds the production client stack against
/// the controller <see cref="TestController"/> describes — <c>LogixTagAccessFactory</c> →
/// <c>TagDefinitionsLoader</c> → <c>CachingLogixTagManager</c> → <c>LogixClient</c> — connects it, and
/// tears it down. Every layer a derived suite exercises is the shipping code; this only supplies the
/// connection settings.
/// </summary>
/// <remarks>
/// The client is built directly rather than taken from <c>LogixClientPool</c>: these tests own their
/// connection, and a refcounted instance shared with other holders is not something a suite can
/// connect and dispose on its own terms.
/// <para>
/// Disposing the client is not optional. Under MTP the test assembly <em>is</em> the process, and a
/// libplctag handle left to its finalizer frees a native handle after CLR teardown — which fail-fasts
/// the process with <c>0xC0000602</c> on an otherwise green run.
/// </para>
/// </remarks>
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
    /// <remarks>
    /// Shared rather than spelled out per type because the types differ in exactly two things — the
    /// value and the declaration — and both are arguments here. What is left in a suite is the part
    /// worth reading: which values a type is exercised with, and what its tag must be declared as.
    /// <para>
    /// Resolving as well as reading is what makes a round trip say something. A value that survives a
    /// write and a read is consistent with the tag being almost anything of the right width; the
    /// declaration is what says it is the type that was configured, a scalar, and — for a
    /// <c>STRING</c> — as wide as it was declared to be.
    /// </para>
    /// </remarks>
    /// <param name="dataPoint">The configured point, which decides the type the value is encoded as.</param>
    /// <param name="valueToWrite">The value written, and the value expected back.</param>
    /// <param name="expectedDefinition">The declaration the controller must report for the tag.</param>
    /// <typeparam name="TDomain">The .NET type the point exchanges — <c>int</c> for a <c>DINT</c>.</typeparam>
    private protected Task AssertRoundTripAsync<TDomain>(
        LogixDataPoint<TDomain> dataPoint, TDomain valueToWrite, TagDefinition expectedDefinition) =>
        AssertRoundTripAsync(dataPoint, valueToWrite, valueToWrite, expectedDefinition);

    /// <summary>
    /// A round trip whose read-back is not the value that went in. Only the <c>STRING</c> needs it: a
    /// character outside Latin-1 has no byte to be stored as and comes back as <c>'?'</c>, which is
    /// lossy by design. Every elementary type returns exactly what it was given, and uses the overload
    /// that says so.
    /// </summary>
    /// <inheritdoc cref="AssertRoundTripAsync{TDomain}(LogixDataPoint{TDomain}, TDomain, TagDefinition)"
    ///     path="/remarks"/>
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

        readResult.Should().ContainSingle();
        readResult[0].DataPoint.Should().Be(dataPoint);
        readResult[0].Value.Should().BeOfType<TDomain>();
        readResult[0].Value.Should().Be(expectedValue);
    }
}
