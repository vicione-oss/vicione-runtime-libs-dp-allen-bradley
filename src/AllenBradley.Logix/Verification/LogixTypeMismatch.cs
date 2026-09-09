namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;

/// <summary>
/// How the controller's type declaration disagrees with what a converter decodes, or
/// <see cref="None"/> when they match. Configuration verification reaches this verdict once per connect
/// and renders the kind as a misconfiguration message
/// (<c>ADR/2026-07-21-verifying-configuration-against-the-symbol-table.md</c>); nothing on the read or
/// write path asks again, because a data point that reached the poll is one whose declaration already
/// matched.
/// </summary>
internal enum LogixTypeMismatch
{
    /// <summary>The declaration matches the converter's expected type — or is absent, which is unverifiable.</summary>
    None,

    /// <summary>
    /// The rank the controller declares is not the one configured, in either direction — an array where a
    /// scalar is configured, and a scalar where an array is.
    /// </summary>
    Rank,

    /// <summary>The controller reports a structure where an elementary type is configured.</summary>
    Structure,

    /// <summary>The controller reports a different elementary CIP type.</summary>
    AtomicType,

    /// <summary>
    /// The controller reports an elementary type where a structured type is configured — the inverse of
    /// <see cref="Structure"/>, and what a <c>STRING</c> configured onto a <c>DINT</c> tag looks like.
    /// </summary>
    Atomic,

    /// <summary>
    /// The capacity the controller declares is not the one configured — a <c>STRING</c> configured onto
    /// a <c>STRING_20</c> tag. Worth its own kind because nothing in a round trip of a shorter value
    /// would show it: writing <c>"Hi"</c> into a <c>STRING_20</c> and reading <c>"Hi"</c> back proves
    /// nothing about the capacity.
    /// </summary>
    StringCapacity,

    /// <summary>
    /// The number of elements the controller declares is not the one configured — an <c>INT[20]</c> where
    /// an <c>INT[10]</c> is configured. Worth its own kind for the reason
    /// <see cref="StringCapacity"/> is: a tag resized in Studio 5000 is invisible to a read that only
    /// takes the first elements.
    /// </summary>
    ElementCount,
}
