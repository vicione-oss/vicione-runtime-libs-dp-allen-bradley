namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags;

/// <summary>
/// The name of a Logix program, as Studio 5000 declares it — <c>MainProgram</c>. The name alone, never
/// the <c>Program:</c> prefix: the tree walk composes the address.
/// </summary>
public readonly record struct ProgramName(string Value)
{
    /// <summary>The name, so a program interpolates and logs as itself.</summary>
    public override string ToString() => Value;
}
