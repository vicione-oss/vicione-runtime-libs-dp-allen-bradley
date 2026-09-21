namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.SymbolTypes;

/// <summary>
/// CIP elementary type codes as reported on the wire by Allen-Bradley controllers.
/// Structured types carry the <c>0x8000</c> marker instead and are not modelled here.
/// </summary>
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

    /// <summary>
    /// 32-bit bit string (<c>0xD3</c>). Logix has no <c>DWORD</c> of its own to declare, so this code
    /// reaches a client only as the word a <c>BOOL</c> array is packed into.
    /// </summary>
    Dword = 0xD3,
}
