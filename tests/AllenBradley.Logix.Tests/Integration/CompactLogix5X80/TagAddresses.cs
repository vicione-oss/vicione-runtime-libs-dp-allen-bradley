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
    /// The <c>TIMER</c> test tag. No instruction may use it: the suites write its <c>.ACC</c> and
    /// <c>.PRE</c>, and read its <c>.DN</c> expecting the bit clear, which holds only while no
    /// <c>TON</c>, <c>TOF</c> or <c>RTO</c> runs on it.
    /// </summary>
    internal const string Timer = $"{Program}.testTimer";

    /// <summary>
    /// Member <paramref name="member"/> of <see cref="Timer"/>, reached as a member of a UDT would be.
    /// </summary>
    internal static string TimerMember(string member) => $"{Timer}.{member}";

    /// <summary>A timer that must <b>not</b> be provisioned, so that connect has a missing tag to report.</summary>
    internal const string MissingTimer = $"{Program}.noSuchTimer";

    /// <summary>
    /// The <c>COUNTER</c> test tag. No instruction may use it: the suites write its <c>.ACC</c> and
    /// <c>.PRE</c>, and read its <c>.DN</c> expecting the bit clear, which holds only while no <c>CTU</c>
    /// or <c>CTD</c> runs on it.
    /// </summary>
    internal const string Counter = $"{Program}.testCounter";

    /// <summary>
    /// Member <paramref name="member"/> of <see cref="Counter"/>, reached as a member of a UDT would be.
    /// </summary>
    internal static string CounterMember(string member) => $"{Counter}.{member}";

    /// <summary>A counter that must <b>not</b> be provisioned, so that connect has a missing tag to report.</summary>
    internal const string MissingCounter = $"{Program}.noSuchCounter";

    /// <summary>
    /// The one-dimensional <c>BOOL</c> array test tag, to be declared <c>BOOL[32]</c>. It is the one
    /// array tag not declared with <see cref="ArrayElementCount"/> elements: Studio 5000 takes only a
    /// multiple of 32 for a <c>BOOL</c> array, because it packs the bits into 32-bit words.
    /// </summary>
    internal const string BoolArray = $"{Program}.testBoolArray";

    /// <summary>How many bits <see cref="BoolArray"/> is declared to hold.</summary>
    internal static ElementCount BoolArrayElementCount => new(32);

    /// <summary>
    /// The one-dimensional <c>SINT</c> array test tag, to be declared <c>SINT[10]</c>.
    /// </summary>
    internal const string SIntArray = $"{Program}.testSintArray";

    /// <summary>
    /// The one-dimensional <c>INT</c> array test tag, to be declared <c>INT[10]</c>.
    /// </summary>
    internal const string IntArray = $"{Program}.testIntArray";

    /// <summary>
    /// The one-dimensional <c>DINT</c> array test tag, to be declared <c>DINT[10]</c>.
    /// </summary>
    internal const string DIntArray = $"{Program}.testDintArray";

    /// <summary>
    /// The one-dimensional <c>LINT</c> array test tag, to be declared <c>LINT[10]</c>.
    /// </summary>
    internal const string LIntArray = $"{Program}.testLintArray";

    /// <summary>
    /// The one-dimensional <c>USINT</c> array test tag, to be declared <c>USINT[10]</c>. It is a 5X80-only
    /// type as its scalar is, for the same reason.
    /// </summary>
    internal const string USIntArray = $"{Program}.testUsintArray";

    /// <summary>
    /// The one-dimensional <c>UINT</c> array test tag, to be declared <c>UINT[10]</c>.
    /// </summary>
    internal const string UIntArray = $"{Program}.testUintArray";

    /// <summary>
    /// The one-dimensional <c>UDINT</c> array test tag, to be declared <c>UDINT[10]</c>.
    /// </summary>
    internal const string UDIntArray = $"{Program}.testUdintArray";

    /// <summary>
    /// The one-dimensional <c>ULINT</c> array test tag, to be declared <c>ULINT[10]</c>.
    /// </summary>
    internal const string ULIntArray = $"{Program}.testUlintArray";

    /// <summary>
    /// The one-dimensional <c>REAL</c> array test tag, to be declared <c>REAL[10]</c>.
    /// </summary>
    internal const string RealArray = $"{Program}.testRealArray";

    /// <summary>
    /// The one-dimensional <c>LREAL</c> array test tag, to be declared <c>LREAL[10]</c>. It is a 5X80-only
    /// type as its scalar is, for the same reason.
    /// </summary>
    internal const string LRealArray = $"{Program}.testLrealArray";

    /// <summary>
    /// The one-dimensional <c>TIMER</c> array test tag, to be declared <c>TIMER[10]</c>. No instruction may
    /// use any of its elements, for the reason <see cref="Timer"/> gives.
    /// </summary>
    internal const string TimerArray = $"{Program}.testTimerArray";

    /// <summary>
    /// How many elements every array test tag above is declared to hold. A count that disagrees with the
    /// controller aborts a connect, because a read of the first ten elements would not show a resized tag.
    /// </summary>
    internal static ElementCount ArrayElementCount => new(10);

    /// <summary>
    /// Element <paramref name="index"/> of the array tag at <paramref name="arrayAddress"/>, addressed
    /// with a subscript. Not a tag of its own: the controller declares the array, and the subscript
    /// reaches into it.
    /// </summary>
    internal static string ArrayElement(string arrayAddress, int index) => $"{arrayAddress}[{index}]";
}
