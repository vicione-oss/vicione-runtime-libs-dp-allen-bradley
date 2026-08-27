namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

/// <summary>
/// What kind of type the controller reports for a tag: an elementary CIP scalar, or a structure (the
/// <c>0x8000</c> symbol-type marker — a Logix <c>STRING</c>, a <c>TIMER</c>, any UDT). This is the
/// closed choice the old <c>IsStruct</c> flag stood for; an atomic type carries a
/// <see cref="LogixTypeDeclaration.AtomicType"/>, a structure does not.
/// </summary>
public enum LogixTypeKind
{
    /// <summary>An elementary CIP type, carrying an <see cref="LogixTypeDeclaration.AtomicType"/> code.</summary>
    Atomic,

    /// <summary>A structure — no elementary code; its layout is not modelled in this cut.</summary>
    Structure,
}
