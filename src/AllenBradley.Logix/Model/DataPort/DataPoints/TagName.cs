namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

/// <summary>
/// A Logix symbolic tag address — <c>Motor.Speed</c>, <c>Program:Main.Count</c>, <c>Arr[5]</c>. The
/// identifier a data point is configured with and the controller's symbol table reports for a tag.
/// </summary>
/// <remarks>
/// Identity is ordinal: two addresses are the same value only when they match exactly, which is what
/// keys a data point in the tag cache. The controller <em>resolves</em> names case-insensitively — that
/// is a lookup rule, not identity — so the symbol table keys itself with
/// <see cref="CaseInsensitiveComparer"/> rather than by value equality.
/// </remarks>
/// <param name="Value">The address text.</param>
public readonly record struct TagName(string Value)
{
    /// <summary>The address text, so the name interpolates and logs as itself.</summary>
    public override string ToString() => Value;

    /// <summary>Matches names the way Logix resolves them — ordinal, case-insensitive.</summary>
    public static IEqualityComparer<TagName> CaseInsensitiveComparer { get; } = new CaseInsensitive();

    private sealed class CaseInsensitive : IEqualityComparer<TagName>
    {
        public bool Equals(TagName x, TagName y) =>
            StringComparer.OrdinalIgnoreCase.Equals(x.Value, y.Value);

        public int GetHashCode(TagName tagName) =>
            StringComparer.OrdinalIgnoreCase.GetHashCode(tagName.Value);
    }
}
