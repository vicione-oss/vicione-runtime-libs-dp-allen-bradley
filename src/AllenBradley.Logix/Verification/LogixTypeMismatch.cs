namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;

/// <summary>
/// How the controller's type declaration disagrees with the configured data point, or <see cref="None"/>
/// when they match. Verification renders the kind as a misconfiguration message
/// (<c>ADR/2026-07-21-verifying-configuration-against-the-symbol-table.md</c>).
/// </summary>
internal enum LogixTypeMismatch
{
    /// <summary>The declaration matches the configured type — or is absent, which is unverifiable.</summary>
    None,

    /// <summary>
    /// The rank the controller declares is not the one configured, in either direction.
    /// </summary>
    Rank,

    /// <summary>
    /// The controller reports a different type — another elementary type, or a structure where an
    /// elementary type is configured, or the reverse.
    /// </summary>
    DataType,

    /// <summary>
    /// The capacity the controller declares is not the one configured — a <c>STRING</c> configured onto
    /// a <c>STRING_20</c> tag.
    /// </summary>
    StringCapacity,

    /// <summary>
    /// The number of elements the controller declares is not the one configured — an <c>INT[20]</c> where
    /// an <c>INT[10]</c> is configured.
    /// </summary>
    ElementCount,

    /// <summary>
    /// An element is configured past the end of the array — <c>Arr[10]</c> on a tag the controller
    /// declares with ten elements.
    /// </summary>
    ElementIndexOutOfRange,

    /// <summary>An element is configured on a tag the controller declares as a scalar.</summary>
    ElementOfScalar,
}
