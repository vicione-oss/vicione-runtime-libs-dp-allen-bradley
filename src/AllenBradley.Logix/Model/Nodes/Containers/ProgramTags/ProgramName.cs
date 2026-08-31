namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags;

/// <summary>
/// The name of a Logix program, as Studio 5000 declares it — <c>MainProgram</c>. It is the segment a
/// program-scope container contributes to the addresses below it, which is why it is a value of its own
/// and not the container's editor label.
/// </summary>
/// <remarks>
/// The name alone, never the <c>Program:</c> prefix or a qualified address: composing
/// <c>Program:MainProgram.Count</c> is the tree walk's job, and this is the one part of it the
/// configuration supplies.
/// </remarks>
/// <param name="Value">The program name.</param>
public readonly record struct ProgramName(string Value)
{
    /// <summary>The name, so a program interpolates and logs as itself.</summary>
    public override string ToString() => Value;
}
