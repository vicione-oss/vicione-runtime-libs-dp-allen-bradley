using System.Text.RegularExpressions;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes;

/// <summary>
/// The name rule Studio 5000 holds a declaration to. One rule and one place, because a tag name and a
/// program name are the same rule and would otherwise drift apart over what an integrator may type.
/// </summary>
internal static partial class LogixIdentifier
{
    /// <summary>
    /// Whether <paramref name="name"/> is one Studio 5000 could have declared:
    /// <list type="bullet">
    ///   <item><c>(?=.{1,40}\z)</c> — 1 to 40 characters in total.</item>
    ///   <item><c>[A-Za-z_]</c> — starts with a letter or an underscore, never a digit.</item>
    ///   <item><c>(?:_?[A-Za-z0-9])*</c> — letters, digits and single underscores after that; two
    ///     underscores in a row and a trailing underscore are both rejected.</item>
    /// </list>
    /// A lone <c>"_"</c> passes this pattern and Studio 5000 would not accept it — a name no controller
    /// declares, which the symbol-table verification rejects anyway.
    /// </summary>
    /// <param name="name">The name as configured.</param>
    internal static bool IsWellFormed(string name) => WellFormed().IsMatch(name);

    [GeneratedRegex(@"\A(?=.{1,40}\z)[A-Za-z_](?:_?[A-Za-z0-9])*\z")]
    private static partial Regex WellFormed();
}
