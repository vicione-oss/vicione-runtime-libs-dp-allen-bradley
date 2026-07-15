namespace ViciOne.Suite.DataPort.AllenBradley.Logix;

/// <summary>
/// Identity of the Allen-Bradley Logix addon.
/// </summary>
/// <remarks>
/// The addon has no behaviour yet. Device nodes, the YAML manifest and the communication
/// configuration arrive with the walking-skeleton slice; this project exists so that the
/// build, packaging and test plumbing has something real to hang off.
/// </remarks>
public static class LogixDataPort
{
    /// <summary>
    /// The addon id. Must stay in sync with <c>Name</c> in <c>metadata.json</c> and with the
    /// assembly name, which the packaging pipeline and <c>InternalsVisibleTo</c> both derive from.
    /// </summary>
    public const string AddonId = "AllenBradley.Logix";
}
