namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model;

/// <summary>
/// An Allen-Bradley data type, as Studio 5000 spells it: the type a tag holds, with nothing of how the
/// controller encodes it.
/// </summary>
/// <remarks>
/// This is what a comparison between a configured data point and the controller's symbol table is made
/// of. The wire codes the controller actually reports are the decoder's business; see
/// <c>CipTypeCode</c>.
/// <para>
/// Every member is an elementary type, fixed in size by the type alone. Structures this addon does not
/// model — the Logix <c>STRING</c>, a <c>TIMER</c>, a UDT — have no member here.
/// </para>
/// </remarks>
public enum AllenBradleyDataType
{
    /// <summary>A type code the controller reported that this addon does not model.</summary>
    Unknown = 0,

    /// <summary><c>BOOL</c> — a single-byte boolean; nonzero is true.</summary>
    Bool,

    /// <summary><c>SINT</c> — an 8-bit signed integer.</summary>
    Sint,

    /// <summary><c>INT</c> — a 16-bit signed integer.</summary>
    Int,

    /// <summary><c>DINT</c> — a 32-bit signed integer.</summary>
    Dint,

    /// <summary><c>LINT</c> — a 64-bit signed integer.</summary>
    Lint,

    /// <summary><c>USINT</c> — an 8-bit unsigned integer.</summary>
    Usint,

    /// <summary><c>UINT</c> — a 16-bit unsigned integer.</summary>
    Uint,

    /// <summary><c>UDINT</c> — a 32-bit unsigned integer.</summary>
    Udint,

    /// <summary><c>ULINT</c> — a 64-bit unsigned integer.</summary>
    Ulint,

    /// <summary><c>REAL</c> — an IEEE-754 single.</summary>
    Real,

    /// <summary><c>LREAL</c> — an IEEE-754 double.</summary>
    Lreal,
}
