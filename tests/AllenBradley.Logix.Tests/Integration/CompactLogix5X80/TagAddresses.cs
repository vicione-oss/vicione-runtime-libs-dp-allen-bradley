using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80;

/// <summary>
/// One tag per data type the port implements — the addresses this folder's round-trip suites write and
/// read back. This is the <b>only</b> place a tag is named; a suite asks for a type and gets the address
/// configured for it, so a controller change is a one-line edit here rather than a hunt through the
/// suites.
/// </summary>
/// <remarks>
/// The controller is not provisioned yet, so every name here is an <b>assumption</b>: one program-scoped
/// tag per type, named after the type, in a program called <c>MainProgram</c>. They are constants and not
/// environment variables on purpose — which tags a controller holds is a fact about that controller,
/// the same everywhere the suite runs, so it belongs in the source and in review. Only <em>reaching</em>
/// the controller varies by machine, and that is <see cref="TestController"/>'s business.
/// <para>
/// They are program-scoped because that is the harder of the two cases and the one that exercises more
/// of the addon: a program tag is browsed under a program-qualified key, and the address the
/// configuration tree composes has to agree with it. A controller-scope tag is a bare name and would
/// prove less.
/// </para>
/// <para>
/// Every one of them is <b>written</b> by the suites, not only read. They must be tags nothing in the
/// controller's program depends on.
/// </para>
/// </remarks>
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

    /// <summary>The <c>REAL</c> test tag.</summary>
    internal const string Real = $"{Program}.testReal";

    /// <summary>
    /// The <c>LREAL</c> test tag. The one type in the vocabulary a 5X70 controller has not got, and so
    /// the reason this folder is pinned to a 5X80 — see
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
}
