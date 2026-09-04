namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration;

/// <summary>
/// The tags the client-stack integration suites target on the CompactLogix L32E. One name per fact
/// about the device, so a controller change is a one-line edit here rather than a hunt through the
/// suites. See the test-device setup for the full tag inventory.
/// </summary>
internal static class LogixTagAddresses
{
    /// <summary>The program's STRING test tag. The round-trip suite writes it.</summary>
    internal const string StrValue1 = "Program:MainProgram.strValue1";

    /// <summary>
    /// A COUNTER's preset — a stable, side-effect-free DINT, and the only plain DINT the device exposes.
    /// It is a structure <em>member</em>, so it is absent from the flat <c>@tags</c> listing and its
    /// metadata is null: verification would report it as not on the controller, and a read of it decodes
    /// on trust.
    /// </summary>
    internal const string CounterPreset = "Program:MainProgram.Counter.PRE";
}
