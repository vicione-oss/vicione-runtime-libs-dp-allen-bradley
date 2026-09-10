namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model;

/// <summary>
/// An Allen-Bradley data type, as Studio 5000 spells it: the type a tag holds, with nothing of how the
/// controller encodes it.
/// </summary>
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

    /// <summary>
    /// <c>STRING</c> — the predefined <c>.LEN : DINT</c> + <c>.DATA : SINT[n]</c> structure. The
    /// <c>n</c> is not part of the type: a built-in <c>STRING</c> and a <c>STRING_20</c> are both this.
    /// </summary>
    String,
}
