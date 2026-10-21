using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;

/// <summary>
/// Gives each derived suite the client of the <see cref="TestControllerFixture"/>, which connects once for
/// the whole <see cref="TestControllerCollection"/>. A suite writes the tags of one
/// <see cref="TagScope"/> in <see cref="TagAddresses"/> and runs once per scope, through a nested class for each.
/// </summary>
[Trait("Category", "Integration")]
[Collection(TestControllerCollection.Name)]
public abstract class CompactLogix5X80IntegrationTestBase
{
    protected CompactLogix5X80IntegrationTestBase(
        TestControllerFixture fixture, ITestOutputHelper output, TagScope scope)
    {
        Client = fixture.Client;
        Output = output;
        Tags = new TagAddresses(scope);

        Output.WriteLine($"{TestControllerConfiguration.Description}, {Tags.Scope} scope");
    }

    internal LogixClient Client { get; }

    private protected TagAddresses Tags { get; }

    /// <summary>Where a suite reports what it saw on the device, for passing runs.</summary>
    protected ITestOutputHelper Output { get; }

    /// <summary>
    /// Resolves the tag, writes the value, reads it back, and hands both to the caller. Resolving is what
    /// makes the read-back say something. A value that survives a write and a read is consistent with the
    /// tag being almost anything of the right width.
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

    private protected async Task<ILogixDataPointValue> ReadAsync(ILogixDataPoint dataPoint) =>
        (await ReadAsync([dataPoint])).Single();

    /// <summary>Reads <paramref name="dataPoints"/> as one group and returns the values in their order.</summary>
    private protected async Task<IReadOnlyList<ILogixDataPointValue>> ReadAsync(
        IReadOnlyList<ILogixDataPoint> dataPoints) =>
        await Client.ReadAsync(
            new LogixDataPointGroup(DefaultPollFrequency, dataPoints), TestContext.Current.CancellationToken);

    /// <summary>
    /// Zeroes the whole array, writes one element through its own data point, and reads the whole array
    /// again. A round trip of the element alone cannot tell a write that reached the right element from one
    /// that reached its neighbour. The whole array can.
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
    /// A scalar value compares by content, so a test asserts the whole record. An array value compares by
    /// reference, so a test asserts the elements of <see cref="ReadValue"/> instead.
    /// </summary>
    private protected sealed record RoundTripResult(ResolvedDataPoint Resolved, ILogixDataPointValue ReadValue);
}
