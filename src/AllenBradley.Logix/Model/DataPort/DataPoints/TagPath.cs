using System.Globalization;
using System.Text.RegularExpressions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ProgramTags;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

/// <summary>
/// Where a data point's value lives, in the parts Logix composes an address from: the program that
/// scopes the tag, if any; the tag's declared name; and the element reached into, if any. The tree walk
/// fills the three slots, and the address is rendered from them on demand (CONTEXT.md, "Tag path").
/// </summary>
/// <param name="Program">The program the tag is scoped to, or <c>null</c> for controller scope.</param>
/// <param name="Tag">The tag's declared name — what the symbol table lists.</param>
/// <param name="Element">The subscript, when the point is one element of an array.</param>
public readonly partial record struct TagPath(ProgramName? Program, TagName Tag, ElementIndex? Element)
{
    private const string ProgramPrefix = "Program:";

    /// <summary>The address libplctag is handed — <c>Program:MainProgram.Readings[3]</c>.</summary>
    public TagAddress ToTagAddress()
    {
        var program = Program is { } name ? $"{ProgramPrefix}{name.Value}." : "";
        var element = Element is { } index ? $"[{index.Value.ToString(CultureInfo.InvariantCulture)}]" : "";
        return new TagAddress($"{program}{Tag.Value}{element}");
    }

    /// <summary>
    /// The address the symbol table declares: the array for an element, because the listing names no
    /// element; the tag's own address for anything else.
    /// </summary>
    public TagAddress TagDefinitionAddress => (this with { Element = null }).ToTagAddress();

    /// <summary>
    /// Reads an address back into its parts — the inverse of <see cref="ToTagAddress"/>. The tag part
    /// takes anything without brackets, so a member address such as <c>Motor.Speed</c> parses as a tag
    /// named that; whether the name is one Studio 5000 declares is the node validator's question.
    /// </summary>
    /// <exception cref="FormatException"><paramref name="address"/> is not a Logix tag address.</exception>
    public static TagPath Parse(string address)
    {
        var match = Address().Match(address);
        if (!match.Success)
        {
            throw new FormatException($"'{address}' is not a Logix tag address.");
        }

        return new TagPath(
            match.Groups["program"].Success ? new ProgramName(match.Groups["program"].Value) : null,
            new TagName(match.Groups["tag"].Value),
            match.Groups["index"].Success
                ? new ElementIndex(uint.Parse(match.Groups["index"].Value, CultureInfo.InvariantCulture))
                : null);
    }

    /// <summary>The address, so a path interpolates and logs as what it reaches.</summary>
    public override string ToString() => ToTagAddress().Value;

    [GeneratedRegex(@"\A(?:Program:(?<program>[^.\[\]]+)\.)?(?<tag>[^\[\]]+)(?:\[(?<index>0|[1-9][0-9]*)\])?\z")]
    private static partial Regex Address();
}
