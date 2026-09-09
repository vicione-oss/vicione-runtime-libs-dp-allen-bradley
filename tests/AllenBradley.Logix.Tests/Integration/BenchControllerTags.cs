namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration;

/// <summary>
/// The tags this folder's suites target on the CompactLogix L32E — a borrowed controller whose tags we
/// cannot change, so these are observations rather than a provisioning specification. One name per fact
/// about the device; see the test-device setup for the full inventory.
/// </summary>
internal static class BenchControllerTags
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
