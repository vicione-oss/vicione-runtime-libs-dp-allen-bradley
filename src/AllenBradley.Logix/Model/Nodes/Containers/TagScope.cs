using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers;

/// <summary>
/// The segment one tag scope contributes to the addresses inside it. Controller scope contributes none —
/// a tag in it addresses itself — and a program contributes <c>Program:MainProgram</c>.
/// </summary>
/// <param name="Segment">The prefix, without the dot that joins it to a tag name. Empty for controller scope.</param>
public readonly record struct TagScope(string Segment)
{
    /// <summary>Controller scope, which prefixes nothing.</summary>
    public static TagScope Controller => new(string.Empty);

    /// <summary>The scope one program's tags live in.</summary>
    public static TagScope Program(ProgramName programName) => new($"Program:{programName.Value}");

    /// <summary>The address a bare <paramref name="tagName"/> has inside this scope.</summary>
    /// <param name="tagName">The tag name as configured, without any scope segment.</param>
    public TagAddress Qualify(TagName tagName) =>
        Segment.Length == 0 ? tagName.ToTagAddress() : new TagAddress($"{Segment}.{tagName.Value}");
}
