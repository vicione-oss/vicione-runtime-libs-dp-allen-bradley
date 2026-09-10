using System.Text.RegularExpressions;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes;

/// <summary>
/// The name rule Studio 5000 holds a declaration to. One place, because a tag name and a program name
/// are the same rule.
/// </summary>
internal static partial class LogixIdentifier
{
    /// <summary>
    /// Whether <paramref name="name"/> is one Studio 5000 could have declared: 1 to 40 characters,
    /// starting with a letter or an underscore, then letters, digits and single underscores that never
    /// trail. A lone <c>"_"</c> passes here though Studio 5000 would not take it — a name no controller
    /// declares, which the symbol-table verification rejects anyway.
    /// </summary>
    internal static bool IsWellFormed(string name) => WellFormed().IsMatch(name);

    [GeneratedRegex(@"\A(?=.{1,40}\z)[A-Za-z_](?:_?[A-Za-z0-9])*\z")]
    private static partial Regex WellFormed();
}
