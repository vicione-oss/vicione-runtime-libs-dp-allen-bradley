using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80;

/// <summary>
/// One tag per data type the port implements, and the <b>only</b> place this folder names a tag. The
/// controller is ours and not commissioned yet, so this list is a <b>provisioning specification</b>
/// rather than a survey: one program-scoped tag per type, named after the type, under
/// <c>MainProgram</c>, to be created as written here — and every one of them but <see cref="IntArray"/>
/// is written by the suites, so they must be tags nothing in the controller's program depends on.
/// Program scope is the harder of
/// the two cases and exercises more of the addon: a program tag is browsed under a program-qualified key
/// the configuration tree has to agree with. How the controller is reached is
/// <see cref="TestController"/>'s business; which tags it holds is the same everywhere, so it lives here
/// and in review rather than in the environment.
/// </summary>
internal static class TagAddresses
{
    private const string Program = "Program:MainProgram";

    /// <summary>The <c>BOOL</c> test tag.</summary>
    internal const string Bool = $"{Program}.testBool";

    /// <summary>The <c>SINT</c> test tag.</summary>
    internal const string SInt = $"{Program}.testSint";

    /// <summary>The <c>INT</c> test tag.</summary>
    internal const string Int = $"{Program}.testInt";

    /// <summary>The <c>DINT</c> test tag.</summary>
    internal const string DInt = $"{Program}.testDint";

    /// <summary>The <c>LINT</c> test tag.</summary>
    internal const string LInt = $"{Program}.testLint";

    /// <summary>
    /// The <c>USINT</c> test tag. One of the unsigned integers a 5X70 controller has not got, so — like
    /// <see cref="LReal"/> — it is a reason this folder is pinned to a 5X80.
    /// </summary>
    internal const string USInt = $"{Program}.testUsint";

    /// <summary>The <c>UINT</c> test tag. A 5X80 type, for the reason <see cref="USInt"/> gives.</summary>
    internal const string UInt = $"{Program}.testUint";

    /// <summary>The <c>UDINT</c> test tag. A 5X80 type, for the reason <see cref="USInt"/> gives.</summary>
    internal const string UDInt = $"{Program}.testUdint";

    /// <summary>The <c>ULINT</c> test tag. A 5X80 type, for the reason <see cref="USInt"/> gives.</summary>
    internal const string ULInt = $"{Program}.testUlint";

    /// <summary>The <c>REAL</c> test tag.</summary>
    internal const string Real = $"{Program}.testReal";

    /// <summary>
    /// The <c>LREAL</c> test tag. The first type in the vocabulary a 5X70 controller has not got, and so
    /// a reason this folder needs a 5X80 — see
    /// <c>docs/AllenBradley.Logix.Documentation/reference/datatype-support.md</c>.
    /// </summary>
    internal const string LReal = $"{Program}.testLreal";

    /// <summary>The <c>STRING</c> test tag, declared to hold <see cref="StringCapacity"/> characters.</summary>
    internal const string String = $"{Program}.testString";

    /// <summary>
    /// How many characters <see cref="String"/> is declared to hold. Assumed to be the built-in
    /// <c>STRING</c>. Configuration, not decoration: a <c>STRING</c> and a <c>STRING_20</c> are the same
    /// type told apart by their capacity, so the suites verify it — and a capacity that disagrees with
    /// the controller aborts a connect. Edit this if the tag is declared as anything else.
    /// </summary>
    internal static StringMaxLength StringCapacity => StringMaxLength.Standard;

    /// <summary>
    /// The one-dimensional <c>INT</c> array test tag, to be declared <c>INT[10]</c>. The one address here
    /// the suites only read: writing an array is a later slice, so this tag's contents are the
    /// controller's and nothing resets them.
    /// </summary>
    internal const string IntArray = $"{Program}.testIntArray";

    /// <summary>
    /// How many elements <see cref="IntArray"/> is declared to hold. Configuration for the reason
    /// <see cref="StringCapacity"/> is: a count that disagrees with the controller aborts a connect,
    /// because a read of the first ten elements would never show a tag that was resized.
    /// </summary>
    internal static ElementCount IntArrayElementCount => new(10);
}
