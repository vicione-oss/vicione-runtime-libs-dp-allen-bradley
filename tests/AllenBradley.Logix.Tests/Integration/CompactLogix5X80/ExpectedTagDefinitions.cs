using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80;

/// <summary>
/// The declaration a round-trip suite expects the controller to report for its tag, built whole so it can
/// be compared as one record.
/// </summary>
internal static class ExpectedTagDefinitions
{
    /// <summary>What the controller must report for an elementary scalar tag of <paramref name="dataType"/>.</summary>
    internal static TagDefinition AtomicScalar(string tagName, AllenBradleyDataType dataType) =>
        new(
            new TagAddress(tagName),
            dataType,
            TemplateId: null,
            MaxLength: null,
            DimensionCount.Scalar,
            new ElementCount(1));

    /// <summary>
    /// What the controller must report for a one-dimensional array of <paramref name="elementCount"/>
    /// elementary <paramref name="dataType"/> values.
    /// </summary>
    internal static TagDefinition AtomicArray(
        string tagName, AllenBradleyDataType dataType, ElementCount elementCount) =>
        new(
            new TagAddress(tagName),
            dataType,
            TemplateId: null,
            MaxLength: null,
            DimensionCount.OneDimensional,
            elementCount);

    /// <summary>
    /// What the controller must report for a <c>STRING</c> tag holding <paramref name="maxLength"/>
    /// characters: a structure on the wire, a scalar in this model.
    /// </summary>
    internal static TagDefinition StringScalar(string tagName, StringMaxLength maxLength) =>
        new(
            new TagAddress(tagName),
            AllenBradleyDataType.String,
            PredefinedTemplates.String,
            maxLength,
            DimensionCount.Scalar,
            new ElementCount(1));
}
