using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.LibPlcTag.TagListing;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.LibPlcTag;

/// <summary>
/// Carries the trait and the collection for the suites that drive libplctag on its own, against the
/// controller <see cref="BenchController"/> describes. Nothing here touches the addon.
/// </summary>
[Trait("Category", "Integration")]
[Collection(PlcCollection.Name)]
public abstract class LibPlcTagIntegrationTestBase
{
    /// <summary>A lister pointed at the controller under test.</summary>
    private protected static PlcTagLister Lister { get; } = new(
        BenchController.ClientInformation.ConnectionEndpoint.Value,
        BenchController.ClientInformation.CipRoutePath.Value,
        BenchController.ClientInformation.OperationTimeout.Value);
}
