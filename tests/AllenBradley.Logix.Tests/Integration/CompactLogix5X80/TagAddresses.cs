using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80;

/// <summary>
/// One tag per data type the port implements, and the <b>only</b> place this folder names a tag: a
/// <b>provisioning specification</b> for a controller of ours that is not commissioned yet, to be created
/// as written here. Every tag is written by the suites, so nothing in the controller's program may depend
/// on one.
/// </summary>
internal static class TagAddresses
{
    private const string Program = "Program:MainProgram";

    internal const string Bool = $"{Program}.testBool";
    internal const string SInt = $"{Program}.testSint";
    internal const string Int = $"{Program}.testInt";
    internal const string DInt = $"{Program}.testDint";
    internal const string LInt = $"{Program}.testLint";
    internal const string USInt = $"{Program}.testUsint";
    internal const string UInt = $"{Program}.testUint";
    internal const string UDInt = $"{Program}.testUdint";
    internal const string ULInt = $"{Program}.testUlint";
    internal const string Real = $"{Program}.testReal";

    /// <summary>
    /// The <c>LREAL</c> test tag. It and the unsigned integers above are the types a 5X70 controller has
    /// not got, and so the reason this folder needs a 5X80 — see
    /// <c>docs/AllenBradley.Logix.Documentation/reference/datatype-support.md</c>.
    /// </summary>
    internal const string LReal = $"{Program}.testLreal";

    /// <summary>The <c>STRING</c> test tag, declared to hold <see cref="StringCapacity"/> characters.</summary>
    internal const string String = $"{Program}.testString";

    /// <summary>
    /// How many characters <see cref="String"/> is declared to hold, assumed to be the built-in
    /// <c>STRING</c>. A capacity that disagrees with the controller aborts a connect, so edit this if the
    /// tag is declared as anything else.
    /// </summary>
    internal static StringMaxLength StringCapacity => StringMaxLength.Standard;

    /// <summary>
    /// The one-dimensional <c>SINT</c> array test tag, to be declared <c>SINT[10]</c>.
    /// </summary>
    internal const string SIntArray = $"{Program}.testSintArray";

    /// <summary>
    /// The one-dimensional <c>INT</c> array test tag, to be declared <c>INT[10]</c>.
    /// </summary>
    internal const string IntArray = $"{Program}.testIntArray";

    /// <summary>
    /// How many elements every array test tag above is declared to hold. A count that disagrees with the
    /// controller aborts a connect, because a read of the first ten elements would not show a resized tag.
    /// </summary>
    internal static ElementCount ArrayElementCount => new(10);
}
