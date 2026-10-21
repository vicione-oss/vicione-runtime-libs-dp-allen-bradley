using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;

/// <summary>
/// The tags of one <see cref="Scope"/>, and the <b>only</b> place this folder names a tag. It is a
/// <b>provisioning specification</b> for a controller of ours. Create each tag as written here, once in
/// controller scope and once in <c>MainProgram</c>, under a name that says which (<c>DIntControllerTag</c>
/// and <c>Program:MainProgram.DIntProgramTag</c>).
/// <c>ViciOne.L5X</c>, next to this file, lists the same tags for Studio 5000. The suites write every tag,
/// so nothing in the controller's program may depend on one.
/// </summary>
internal sealed record TagAddresses(TagScope Scope)
{
    private const string ProgramScopePrefix = "Program:MainProgram.";

    internal string Bool => Tag("Bool");
    internal string SInt => Tag("SInt");
    internal string Int => Tag("Int");
    internal string DInt => Tag("DInt");
    internal string LInt => Tag("LInt");
    internal string UsInt => Tag("USInt");
    internal string UInt => Tag("UInt");
    internal string UdInt => Tag("UDInt");
    internal string UlInt => Tag("ULInt");
    internal string Real => Tag("Real");

    /// <summary>
    /// The <c>LREAL</c> test tag. A 5X70 controller has neither <c>LREAL</c> nor the unsigned integers above,
    /// which is why this folder needs a 5X80. See
    /// <c>docs/AllenBradley.Logix.Documentation/reference/datatype-support.md</c>.
    /// </summary>
    internal string LReal => Tag("LReal");

    internal string String => Tag("String");

    internal string String20 => Tag("String20");

    /// <summary>
    /// The <c>TIMER</c> test tag. No instruction may use it. The suites write its <c>.ACC</c> and
    /// <c>.PRE</c> and read its <c>.DN</c> expecting the bit clear, which holds only while no
    /// <c>TON</c>, <c>TOF</c> or <c>RTO</c> runs on it.
    /// </summary>
    internal string Timer => Tag("Timer");

    /// <summary>A timer that must <b>not</b> be provisioned, so that connect has a missing tag to report.</summary>
    internal string MissingTimer => Address("MissingTimer");

    /// <summary>
    /// The <c>COUNTER</c> test tag. No instruction may use it. The suites write its <c>.ACC</c> and
    /// <c>.PRE</c> and read its <c>.DN</c> expecting the bit clear, which holds only while no <c>CTU</c>
    /// or <c>CTD</c> runs on it.
    /// </summary>
    internal string Counter => Tag("Counter");

    /// <summary>A counter that must <b>not</b> be provisioned, so that connect has a missing tag to report.</summary>
    internal string MissingCounter => Address("MissingCounter");

    /// <summary>
    /// The <c>BOOL</c> array test tag, declared <c>BOOL[32]</c>. Studio 5000 accepts only a multiple of 32
    /// for a <c>BOOL</c> array, because it packs the bits into 32-bit words.
    /// </summary>
    internal string BoolArray => Tag("BoolArray");

    internal string SIntArray => Tag("SIntArray");
    internal string IntArray => Tag("IntArray");
    internal string DIntArray => Tag("DIntArray");
    internal string LIntArray => Tag("LIntArray");
    internal string UsIntArray => Tag("USIntArray");
    internal string UIntArray => Tag("UIntArray");
    internal string UdIntArray => Tag("UDIntArray");
    internal string UlIntArray => Tag("ULIntArray");
    internal string RealArray => Tag("RealArray");
    internal string LRealArray => Tag("LRealArray");

    /// <summary>
    /// The <c>TIMER</c> array test tag. No instruction may use any of its elements, for the reason given
    /// at <see cref="Timer"/>.
    /// </summary>
    internal string TimerArray => Tag("TimerArray");

    /// <summary>The <c>UDT_Outer</c> test tag. Its members cover every kind of member a UDT Instance opens.</summary>
    internal OuterUdtAddresses Udt => new(Address(UdtTagName));

    /// <summary>The name of <see cref="Udt"/> within its scope, which a configured UDT Instance node carries.</summary>
    internal string UdtTagName => TagName("Udt");

    /// <summary>
    /// The one <c>DINT</c> tag provisioned under the same name in both scopes, so that the program tag
    /// shadows the controller tag. Every other tag has a different name in each scope, so a program-scope
    /// test that reaches the controller tag by mistake finds no tag at all.
    /// </summary>
    internal string ShadowedDInt => Address("DIntTag");

    /// <summary>
    /// How many elements every array test tag and array member except the <c>BOOL</c> ones is declared to
    /// hold. A count that disagrees with the controller aborts a connect, because a read of the first ten
    /// elements would not show a resized tag.
    /// </summary>
    internal static ElementCount ArrayElementCount => new(10);

    /// <summary>How many bits <see cref="BoolArray"/> and the <c>BOOL</c> array member are declared to hold.</summary>
    internal static ElementCount BoolArrayElementCount => new(32);

    internal static StringMaxLength StringCapacity => StringMaxLength.Standard;

    internal static StringMaxLength String20Capacity => new(20);

    internal static string Member(string address, string member) => $"{address}.{member}";

    /// <summary>
    /// Element <paramref name="index"/> of the array at <paramref name="arrayAddress"/>, addressed with a
    /// subscript. It is not a tag of its own. The controller declares the array, and the subscript reaches
    /// into it.
    /// </summary>
    internal static string ArrayElement(string arrayAddress, int index) => $"{arrayAddress}[{index}]";

    /// <summary>
    /// <paramref name="address"/> with every name in it upper-cased. The <c>Program:</c> keyword is syntax
    /// and keeps its case.
    /// </summary>
    internal static string InUpperCase(string address) =>
        address.StartsWith(ProgramScopePrefix, StringComparison.Ordinal)
            ? "Program:" + address["Program:".Length..].ToUpperInvariant()
            : address.ToUpperInvariant();

    private string Tag(string typeName) => Address(TagName(typeName));

    private string TagName(string typeName) => Scope switch
    {
        TagScope.Controller => typeName + "ControllerTag",
        TagScope.Program => typeName + "ProgramTag",
        _ => throw new ArgumentOutOfRangeException(nameof(Scope), Scope, null)
    };

    private string Address(string tagName) => Scope switch
    {
        TagScope.Controller => tagName,
        TagScope.Program => ProgramScopePrefix + tagName,
        _ => throw new ArgumentOutOfRangeException(nameof(Scope), Scope, null)
    };
}

/// <summary>
/// The <c>UDT_Outer</c> tag at <paramref name="Address"/> and one member of each kind the template
/// declares differently. Atomic members beyond the <c>DINT</c> would repeat the tag suites,
/// because a member's type decodes as a tag's does and its value is read by its own symbolic address.
/// </summary>
internal sealed record OuterUdtAddresses(string Address)
{
    /// <summary>
    /// A <c>BOOL</c> member that the controller packs into a hidden <c>SINT</c> together with
    /// <see cref="BoolNeighbour"/>.
    /// </summary>
    internal string Bool => Member("BoolTag");

    /// <summary>The <c>BOOL</c> member packed into the same byte as <see cref="Bool"/>, one bit along.</summary>
    internal string BoolNeighbour => Member("BoolNeighbour");

    internal string DInt => Member("DIntTag");
    internal string String => Member("StringTag");
    internal string String20 => Member("String20");
    internal string DIntArray => Member("DIntArray");
    internal string BoolArray => Member("BoolArray");
    internal string Timer => Member("TimerTag");
    internal string TimerArray => Member("TimerArray");
    internal string Counter => Member("CounterTag");

    /// <summary>A member the type does <b>not</b> have, so that connect has a missing member to report.</summary>
    internal string Missing => Member("MissingMember");

    internal InnerUdtAddresses Inner => new(Member("InnerUdt"));

    private string Member(string member) => TagAddresses.Member(Address, member);
}

/// <summary>
/// The <c>UDT_Inner</c> member at <paramref name="Address"/>, nested in <see cref="OuterUdtAddresses"/>, and
/// its members.
/// </summary>
internal sealed record InnerUdtAddresses(string Address)
{
    internal string Bool => Member("BoolTag");
    internal string Real => Member("RealTag");
    internal string IntArray => Member("IntArray");
    internal string String => Member("StringTag");

    private string Member(string member) => TagAddresses.Member(Address, member);
}
