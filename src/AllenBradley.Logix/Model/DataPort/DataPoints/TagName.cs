namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

/// <summary>
/// A Logix symbolic tag address — <c>Motor.Speed</c>, <c>Program:Main.Count</c>, <c>Arr[5]</c>. The
/// identifier a data point is configured with and the controller's symbol table reports for a tag.
/// Equality is ordinal. The controller resolves names case-insensitively, which is what <see
/// cref="CaseInsensitiveComparer"/> is for.
/// </summary>
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
