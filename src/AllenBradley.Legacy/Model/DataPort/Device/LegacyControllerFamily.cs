namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.Device;

/// <summary>
/// Which line a controller belongs to. Unlike on Logix it is part of how a connection is opened, because
/// libplctag speaks to each line as a different PLC type.
/// </summary>
public enum LegacyControllerFamily
{
    /// <summary>SLC 500, 5/01 to 5/05, programmed in RSLogix 500.</summary>
    Slc500,

    /// <summary>MicroLogix 1000 to 1500, programmed in RSLogix 500 or RSLogix Micro.</summary>
    MicroLogix,
}
