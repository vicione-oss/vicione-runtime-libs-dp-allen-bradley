namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration.Templates;

/// <summary>
/// The name of one member of a structure — the <c>PRE</c> in <c>Timer1.PRE</c> (CONTEXT.md, "Member").
/// A name of <c>ZZZZZZZZZZ…</c> or <c>__…</c> is a hidden host member: the byte or word the controller
/// packs a structure's <c>BOOL</c>s into, which Studio 5000 never shows.
/// </summary>
/// <param name="Value">The name text.</param>
public readonly record struct MemberName(string Value);
