
namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

/// <summary>
/// The members an address reaches through, in order, from the tag inwards: <c>Ramp</c> then
/// <c>Target</c> for <c>Motor.Ramp.Target</c>. Never empty — a tag addressed whole has no member path
/// at all, the way it has no program or element. Two paths naming the same members are equal, so a
/// data point built twice from one configuration is one cache key.
/// </summary>
public readonly record struct UdtMemberPath
{
    private readonly UdtMemberName[] _members;

    private UdtMemberPath(UdtMemberName[] members) => _members = members;

    /// <exception cref="ArgumentException"><paramref name="members"/> is empty.</exception>
    public static UdtMemberPath Of(params UdtMemberName[] members) =>
        members.Length > 0
            ? new UdtMemberPath([.. members])
            : throw new ArgumentException("A member path reaches through at least one member.", nameof(members));

    /// <summary>The members from the tag inwards.</summary>
    public IReadOnlyList<UdtMemberName> Members => _members;

    /// <summary>This path with <paramref name="udtMember"/> reached through next.</summary>
    public UdtMemberPath Append(UdtMemberName udtMember) => new([.. _members, udtMember]);

    public bool Equals(UdtMemberPath other) => _members.AsSpan().SequenceEqual(other._members);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var member in _members)
        {
            hash.Add(member);
        }

        return hash.ToHashCode();
    }

    /// <summary>The members joined by dots — <c>Ramp.Target</c>.</summary>
    public override string ToString() => string.Join('.', _members.Select(member => member.Value));
}
