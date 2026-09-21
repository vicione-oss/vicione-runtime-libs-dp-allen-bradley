using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model;

/// <summary>
/// An Allen-Bradley data type, as Studio 5000 spells it: the type a tag holds, with nothing of how the
/// controller encodes it. A closed set of values, one static member each.
/// </summary>
/// <param name="Name">The Studio 5000 spelling.</param>
/// <param name="MinimumGeneration">
/// The oldest generation whose type vocabulary has this type, which is what a tag scope container holds
/// its own generation against. An array of a type is not a different question from the scalar, so a
/// whole-array tag and an array container ask this of their element type.
/// </param>
public readonly record struct AllenBradleyDataType(DataTypeName Name, LogixGeneration MinimumGeneration)
{
    /// <summary>An elementary type code the controller reported that this addon does not model.</summary>
    public static readonly AllenBradleyDataType Unknown = new(new DataTypeName("UNKNOWN"), LogixGeneration.Logix5X70);

    /// <summary><c>BOOL</c> — a single-byte boolean; nonzero is true.</summary>
    public static readonly AllenBradleyDataType Bool = new(new DataTypeName("BOOL"), LogixGeneration.Logix5X70);

    /// <summary><c>SINT</c> — an 8-bit signed integer.</summary>
    public static readonly AllenBradleyDataType Sint = new(new DataTypeName("SINT"), LogixGeneration.Logix5X70);

    /// <summary><c>INT</c> — a 16-bit signed integer.</summary>
    public static readonly AllenBradleyDataType Int = new(new DataTypeName("INT"), LogixGeneration.Logix5X70);

    /// <summary><c>DINT</c> — a 32-bit signed integer.</summary>
    public static readonly AllenBradleyDataType Dint = new(new DataTypeName("DINT"), LogixGeneration.Logix5X70);

    /// <summary><c>LINT</c> — a 64-bit signed integer.</summary>
    public static readonly AllenBradleyDataType Lint = new(new DataTypeName("LINT"), LogixGeneration.Logix5X70);

    /// <summary><c>USINT</c> — an 8-bit unsigned integer.</summary>
    public static readonly AllenBradleyDataType Usint = new(new DataTypeName("USINT"), LogixGeneration.Logix5X80);

    /// <summary><c>UINT</c> — a 16-bit unsigned integer.</summary>
    public static readonly AllenBradleyDataType Uint = new(new DataTypeName("UINT"), LogixGeneration.Logix5X80);

    /// <summary><c>UDINT</c> — a 32-bit unsigned integer.</summary>
    public static readonly AllenBradleyDataType Udint = new(new DataTypeName("UDINT"), LogixGeneration.Logix5X80);

    /// <summary><c>ULINT</c> — a 64-bit unsigned integer.</summary>
    public static readonly AllenBradleyDataType Ulint = new(new DataTypeName("ULINT"), LogixGeneration.Logix5X80);

    /// <summary><c>REAL</c> — an IEEE-754 single.</summary>
    public static readonly AllenBradleyDataType Real = new(new DataTypeName("REAL"), LogixGeneration.Logix5X70);

    /// <summary><c>LREAL</c> — an IEEE-754 double.</summary>
    public static readonly AllenBradleyDataType Lreal = new(new DataTypeName("LREAL"), LogixGeneration.Logix5X80);

    /// <summary>
    /// <c>STRING</c> — the predefined <c>.LEN : DINT</c> + <c>.DATA : SINT[n]</c> structure. The
    /// <c>n</c> is not part of the type: a built-in <c>STRING</c> and a <c>STRING_20</c> are both this.
    /// </summary>
    public static readonly AllenBradleyDataType String = new(new DataTypeName("STRING"), LogixGeneration.Logix5X70);

    /// <summary>
    /// <c>STRUCTURE</c> — a structure this addon reads as members rather than as one value: a UDT, a
    /// predefined <c>TIMER</c> or <c>COUNTER</c>, an Add-On Instruction instance, a module-defined
    /// type. This one spelling is the addon's own, because the controller names each structure after
    /// its own type and only the template carries that name (CONTEXT.md, "Structure"). A
    /// <c>STRING</c> is the one structure that gets a type of its own.
    /// </summary>
    public static readonly AllenBradleyDataType Structure = new(new DataTypeName("STRUCTURE"), LogixGeneration.Logix5X70);
}
