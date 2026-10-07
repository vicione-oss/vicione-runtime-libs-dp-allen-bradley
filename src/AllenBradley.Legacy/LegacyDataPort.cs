namespace ViciOne.Suite.DataPort.AllenBradley.Legacy;

/// <summary>
/// Identity of the Allen-Bradley Legacy data-port.
/// </summary>
public static class LegacyDataPort
{
    /// <summary>
    /// The data-port id. Must stay in sync with the assembly name, which the packaging pipeline and
    /// <c>InternalsVisibleTo</c> both derive from.
    /// </summary>
    public const string DataPortId = "AllenBradley.Legacy";
}
