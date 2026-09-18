using System.Globalization;
using System.Text.RegularExpressions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration.Templates;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ProgramTags;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

/// <summary>
/// Where a data point's value lives, in the parts Logix composes an address from: the program that
/// scopes the tag, if any; the tag's declared name; the members reached through, if any; and the
/// element reached into, if any. The first two say where the declaration is, the last two where inside
/// it the value is. The tree walk fills the slots, and the address is rendered from them on demand
/// (CONTEXT.md, "Tag path").
/// </summary>
/// <param name="Program">The program the tag is scoped to, or <c>null</c> for controller scope.</param>
/// <param name="Tag">The tag's declared name — what the symbol table lists.</param>
/// <param name="UdtMemberPath">The members reached through, from the tag inwards, or <c>null</c> for a tag addressed whole.</param>
/// <param name="ArrayElementIndex">The subscript, when the point is one element of an array.</param>
public readonly partial record struct TagPath(
    ProgramName? Program,
    TagName Tag,
    UdtMemberPath? UdtMemberPath,
    ElementIndex? ArrayElementIndex)
{
    private const string ProgramPrefix = "Program:";

    /// <summary>The address libplctag is handed — <c>Program:MainProgram.Motor.Readings[3]</c>.</summary>
    public TagAddress ToTagAddress()
    {
        var program = Program is { } name ? $"{ProgramPrefix}{name.Value}." : "";
        var members = UdtMemberPath is { } path ? $".{path}" : "";
        var element = ArrayElementIndex is { } index ? $"[{index.Value.ToString(CultureInfo.InvariantCulture)}]" : "";
        return new TagAddress($"{program}{Tag.Value}{members}{element}");
    }

    /// <summary>
    /// The address of the tag the symbol table lists: the program and the tag, without the members and
    /// the element, because the listing names neither. Every lookup starts here, whatever lies inside.
    /// </summary>
    public TagAddress TagDefinitionAddress => (this with { UdtMemberPath = null, ArrayElementIndex = null }).ToTagAddress();

    /// <summary>
    /// Reads an address back into its parts — the inverse of <see cref="ToTagAddress"/>. Each dotted part
    /// behind the tag is a member; whether the tag has one is the symbol table's question, not the
    /// parser's. An element is taken at the end only: <c>Motors[2].Speed</c> is not a path this port has.
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
            match.Groups["member"].Success
                ? DataPoints.UdtMemberPath.Of([.. match.Groups["member"].Captures.Select(capture => new UdtMemberName(capture.Value))])
                : null,
            match.Groups["index"].Success
                ? new ElementIndex(uint.Parse(match.Groups["index"].Value, CultureInfo.InvariantCulture))
                : null);
    }

    /// <summary>The address, so a path interpolates and logs as what it reaches.</summary>
    public override string ToString() => ToTagAddress().Value;

    [GeneratedRegex(@"\A(?:Program:(?<program>[^.\[\]]+)\.)?(?<tag>[^.\[\]]+)(?:\.(?<member>[^.\[\]]+))*(?:\[(?<index>0|[1-9][0-9]*)\])?\z")]
    private static partial Regex Address();
}
