namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

/// <summary>
/// What kind of type the controller reports for a tag: an elementary CIP type, or a structure — a Logix
/// <c>STRING</c>, a <c>TIMER</c>, any UDT.
/// </summary>
public enum LogixTypeKind
{
    /// <summary>An elementary CIP type, named by a one-byte type code and fixed in size by it.</summary>
    Atomic,

    /// <summary>
    /// A structure, marked by <c>0x8000</c> in the symbol type. The listing names only the id of the
    /// template that defines its members, never the members themselves.
    /// </summary>
    Structure,
}
