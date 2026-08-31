using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers;

/// <summary>
/// The segment one tag scope contributes to the addresses inside it. Controller scope contributes none —
/// a tag in it addresses itself — and a program contributes <c>Program:MainProgram</c>.
/// </summary>
/// <remarks>
/// This is the whole of address composition from the configuration tree: a scope and a bare tag name,
/// joined by a dot. A structure member nests further, and its path is not this type's business: it comes
/// from the tag's own type declaration read off the controller, never from what an integrator configured.
/// </remarks>
/// <param name="Segment">The prefix, without the dot that joins it to a tag name. Empty for controller scope.</param>
public readonly record struct TagScope(string Segment)
{
    /// <summary>Controller scope, which prefixes nothing.</summary>
    public static TagScope Controller => new(string.Empty);

    /// <summary>The scope one program's tags live in.</summary>
    /// <param name="programName">The program that owns them.</param>
    public static TagScope Program(ProgramName programName) => new($"Program:{programName.Value}");

    /// <summary>The address a bare <paramref name="tagName"/> has inside this scope.</summary>
    /// <param name="tagName">The tag name as configured, without any scope segment.</param>
    public TagName Qualify(TagName tagName) =>
        Segment.Length == 0 ? tagName : new TagName($"{Segment}.{tagName.Value}");
}
