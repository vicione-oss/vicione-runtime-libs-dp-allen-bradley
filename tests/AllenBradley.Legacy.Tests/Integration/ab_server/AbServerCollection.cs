namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Integration.ab_server;

/// <summary>
/// Groups every test that talks to <see cref="AbServer"/>, so that they share one server and never run in
/// parallel. The tests write to the same data files, and a parallel write changes the value that another test
/// reads back.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class AbServerCollection : ICollectionFixture<AbServer>
{
    public const string Name = "ab_server";
}
