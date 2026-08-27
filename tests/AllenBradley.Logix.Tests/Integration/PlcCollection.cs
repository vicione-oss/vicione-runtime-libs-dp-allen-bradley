namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration;

/// <summary>
/// Groups every test that talks to the physical PLC into one collection that never runs in parallel —
/// with itself or with any other collection. They all share one controller reached over one libplctag
/// session, and concurrent operations on that session collide: at best the controller answers
/// <c>ErrorBusy</c>, at worst a wedged operation hangs and (under MTP, where the assembly is the process)
/// takes the whole run down. Serializing them is what makes <c>dotnet test -p:test-suite=integration</c>
/// a suite rather than a race.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class PlcCollection
{
    public const string Name = "PLC";
}
