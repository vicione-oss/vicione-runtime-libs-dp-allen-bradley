using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration.Templates;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration;

/// <summary>
/// The template ids a Logix controller gives its predefined structures. Expectations rather than
/// observations: a run against a device is what confirms them.
/// </summary>
internal static class PredefinedTemplates
{
    /// <summary>The built-in <c>STRING</c>, whose symbol type is <c>0x8FCE</c>.</summary>
    internal static TemplateId String => new(0x0FCE);
}
