namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

/// <summary>
/// A Logix symbolic tag address as one string — <c>Motor.Speed</c>, <c>Program:Main.Count</c>,
/// <c>Arr[5]</c>. What libplctag is handed and what the controller's symbol table reports for a tag; a
/// data point holds the parts instead, as a <see cref="TagPath"/>.
/// </summary>
/// <param name="Value">The address text.</param>
public readonly record struct TagAddress(string Value)
{
    /// <summary>
    /// Matches names the way the controller resolves them — ordinal, case-insensitive. The struct's own
    /// equality is case-sensitive.
    /// </summary>
    public static IEqualityComparer<TagAddress> CaseInsensitiveComparer { get; } = new CaseInsensitive();

    private sealed class CaseInsensitive : IEqualityComparer<TagAddress>
    {
        public bool Equals(TagAddress x, TagAddress y) =>
            StringComparer.OrdinalIgnoreCase.Equals(x.Value, y.Value);

        public int GetHashCode(TagAddress tagAddress) =>
            StringComparer.OrdinalIgnoreCase.GetHashCode(tagAddress.Value);
    }
}
