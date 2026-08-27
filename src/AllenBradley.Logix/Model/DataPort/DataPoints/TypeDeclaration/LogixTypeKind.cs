namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

/// <summary>
/// What kind of type the controller reports for a tag: an elementary CIP scalar, or a structure (the
/// <c>0x8000</c> symbol-type marker — a Logix <c>STRING</c>, a <c>TIMER</c>, any UDT). This is the
/// closed choice the old <c>IsStruct</c> flag stood for, and it settles shape before
/// <see cref="TagDefinition.DataType"/> settles type.
/// </summary>
public enum LogixTypeKind
{
    /// <summary>An elementary CIP type, named by a one-byte type code and fixed in size by it.</summary>
    Atomic,

    /// <summary>
    /// A structure. The listing names only the id of the template that defines its members, so the one
    /// structured type this addon models — <see cref="AllenBradleyDataType.String"/> — is what a
    /// structure decodes as, sized by its <see cref="TagDefinition.MaxLength"/>.
    /// </summary>
    Structure,
}
