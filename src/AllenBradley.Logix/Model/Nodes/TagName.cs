using System.Globalization;
using System.Text.RegularExpressions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes;

public readonly partial record struct TagName(string Value)
{
    /// <summary>This name as a controller-scoped tag's path.</summary>
    internal TagPath ToTagPath() => new(Program: null, this, Element: null);

    /// <summary>
    /// Whether <paramref name="name"/> is one Studio 5000 could have declared: 1 to 40 characters,
    /// starting with a letter or an underscore, then letters, digits and single underscores that never
    /// trail. A lone <c>"_"</c> passes here though Studio 5000 would not take it — a name no controller
    /// declares, which the symbol-table verification rejects anyway.
    /// </summary>
    internal static bool IsWellFormed(string name) => WellFormed().IsMatch(name);

    [GeneratedRegex(@"\A(?=.{1,40}\z)[A-Za-z_](?:_?[A-Za-z0-9])*\z")]
    internal static partial Regex WellFormed();

    /// <summary>
    /// Whether <paramref name="name"/> is a one-dimensional subscript, <c>[n]</c> with n a non-negative
    /// integer without leading zeros — what an element node under an array container carries instead
    /// of a declared tag name.
    /// </summary>
    internal static bool IsWellFormedSubscript(string name) => WellFormedSubscript().IsMatch(name);

    /// <summary>The element a subscript selects — <c>3</c> for <c>[3]</c>.</summary>
    /// <exception cref="FormatException">The name is not a subscript.</exception>
    internal ElementIndex ToElementIndex()
    {
        var match = WellFormedSubscript().Match(Value);
        return match.Success
            ? new ElementIndex(uint.Parse(match.Groups["index"].Value, CultureInfo.InvariantCulture))
            : throw new FormatException($"'{Value}' is not an array subscript.");
    }

    [GeneratedRegex(@"\A\[(?<index>0|[1-9][0-9]*)\]\z")]
    internal static partial Regex WellFormedSubscript();
}
