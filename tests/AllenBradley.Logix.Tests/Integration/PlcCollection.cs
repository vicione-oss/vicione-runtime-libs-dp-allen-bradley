namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration;

/// <summary>
/// Groups every test that talks to the physical PLC into one collection that never runs in parallel — with
/// itself or with any other collection. They share one controller over one libplctag session, and
/// concurrent operations on that session collide.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class PlcCollection
{
    public const string Name = "PLC";
}
