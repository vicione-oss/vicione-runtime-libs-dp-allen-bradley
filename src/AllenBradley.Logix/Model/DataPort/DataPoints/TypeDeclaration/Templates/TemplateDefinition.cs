namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration.Templates;

/// <summary>
/// What the controller's Template object reports for one structure data type, decoded from an
/// <c>@udt/&lt;id&gt;</c> read: the layout that turns a structure from a byte count into named members
/// (CONTEXT.md, "Template"). The one <see cref="TagDefinition"/> of a structured tag names it by id.
/// </summary>
/// <param name="Id">The template's id, the one the tag's symbol type carries.</param>
/// <param name="Name">The structure data type's name.</param>
/// <param name="Handle">The handle a read reply of a tag of this type carries.</param>
/// <param name="Size">The bytes one instance occupies, padding included.</param>
/// <param name="Members">The members in the order the template lists them, which is declaration order.</param>
public readonly record struct TemplateDefinition(
    TemplateId Id,
    TemplateName Name,
    StructureHandle Handle,
    StructureSize Size,
    IReadOnlyList<TemplateMember> Members);
