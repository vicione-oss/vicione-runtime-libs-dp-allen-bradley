namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

/// <summary>
/// The Allen-Bradley controller families this addon supports. Both are Logix-5000-class controllers
/// that address tags symbolically and speak CIP identically, so libplctag maps them onto a single
/// PLC type — but they stay distinct here because the family is what an integrator selects and what
/// the manifest surfaces. File-addressed controllers (MicroLogix, Micro800) are the Legacy addon's
/// concern and deliberately absent.
/// </summary>
public enum LogixControllerType
{
    /// <summary>ControlLogix (1756) chassis-based controller.</summary>
    ControlLogix,

    /// <summary>CompactLogix (1769 / 5069) controller.</summary>
    CompactLogix,
}
