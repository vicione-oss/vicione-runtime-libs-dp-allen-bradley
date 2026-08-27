namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;

/// <summary>
/// CIP elementary type codes as reported on the wire by Allen-Bradley controllers.
/// </summary>
/// <remarks>
/// Values are the one-byte CIP type codes (<c>0xC1</c>–<c>0xCB</c> for the elementary types).
/// Structured types — the Logix <c>STRING</c>, <c>TIMER</c> and every UDT — are reported with the
/// <c>0x8000</c> structure marker instead and are not modelled here. See the CIP data types
/// reference for the full table.
/// </remarks>
internal enum CipTypeCode : byte
{
    /// <summary>1-byte boolean (<c>0xC1</c>); nonzero is true.</summary>
    Bool = 0xC1,

    /// <summary>8-bit signed integer (<c>0xC2</c>).</summary>
    Sint = 0xC2,

    /// <summary>16-bit signed integer (<c>0xC3</c>).</summary>
    Int = 0xC3,

    /// <summary>32-bit signed integer (<c>0xC4</c>).</summary>
    Dint = 0xC4,

    /// <summary>64-bit signed integer (<c>0xC5</c>).</summary>
    Lint = 0xC5,

    /// <summary>8-bit unsigned integer (<c>0xC6</c>).</summary>
    Usint = 0xC6,

    /// <summary>16-bit unsigned integer (<c>0xC7</c>).</summary>
    Uint = 0xC7,

    /// <summary>32-bit unsigned integer (<c>0xC8</c>).</summary>
    Udint = 0xC8,

    /// <summary>64-bit unsigned integer (<c>0xC9</c>).</summary>
    Ulint = 0xC9,

    /// <summary>IEEE-754 single-precision float (<c>0xCA</c>).</summary>
    Real = 0xCA,

    /// <summary>IEEE-754 double-precision float (<c>0xCB</c>).</summary>
    Lreal = 0xCB,
}
