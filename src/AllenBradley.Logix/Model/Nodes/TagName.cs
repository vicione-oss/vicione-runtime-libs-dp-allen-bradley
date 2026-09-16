using System.Text.RegularExpressions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

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
}
