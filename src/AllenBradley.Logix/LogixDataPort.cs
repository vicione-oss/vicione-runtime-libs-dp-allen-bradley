namespace ViciOne.Suite.DataPort.AllenBradley.Logix;

/// <summary>
/// Identity of the Allen-Bradley Logix data-port.
/// </summary>
public static class LogixDataPort
{
    /// <summary>
    /// The data-port id. Must stay in sync with <c>Name</c> in <c>metadata.json</c> and with the
    /// assembly name, which the packaging pipeline and <c>InternalsVisibleTo</c> both derive from.
    /// </summary>
    public const string DataPortId = "AllenBradley.Logix";
}
