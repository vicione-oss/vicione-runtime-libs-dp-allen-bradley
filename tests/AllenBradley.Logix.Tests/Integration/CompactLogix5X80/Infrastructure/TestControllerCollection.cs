namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;

/// <summary>
/// Groups every test that talks to the <see cref="TestControllerConfiguration"/>. The tests share one
/// <see cref="TestControllerFixture"/> and never run in parallel, because they share one libplctag session
/// and concurrent operations on that session collide.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class TestControllerCollection : ICollectionFixture<TestControllerFixture>
{
    public const string Name = "5X80 test controller";
}
