using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80;

/// <summary>
/// The declaration a round-trip suite expects the controller to report for its tag, built whole so that
/// it can be compared as one record: a read shows what the bytes decoded to and nothing else — not that
/// the tag is a scalar rather than a one-element array, and for a <c>STRING</c> not its capacity.
/// </summary>
internal static class ExpectedTagDefinitions
{
    /// <summary>What the controller must report for an elementary scalar tag of <paramref name="dataType"/>.</summary>
    internal static TagDefinition AtomicScalar(string tagName, AllenBradleyDataType dataType) =>
        new(
            new TagName(tagName),
            LogixTypeKind.Atomic,
            dataType,
            MaxLength: null,
            DimensionCount.Scalar,
            new ElementCount(1));

    /// <summary>
    /// What the controller must report for a one-dimensional array of <paramref name="elementCount"/>
    /// elementary <paramref name="dataType"/> values. The rank and the count are the two a read cannot
    /// show: ten INTs off an <c>INT[20]</c> decode exactly as well as ten off an <c>INT[10]</c>.
    /// </summary>
    internal static TagDefinition AtomicArray(
        string tagName, AllenBradleyDataType dataType, ElementCount elementCount) =>
        new(
            new TagName(tagName),
            LogixTypeKind.Atomic,
            dataType,
            MaxLength: null,
            DimensionCount.OneDimensional,
            elementCount);

    /// <summary>
    /// What the controller must report for a <c>STRING</c> tag holding <paramref name="maxLength"/>
    /// characters. A structure on the wire and a scalar in this model: one value, not an array.
    /// </summary>
    internal static TagDefinition StringScalar(string tagName, StringMaxLength maxLength) =>
        new(
            new TagName(tagName),
            LogixTypeKind.Structure,
            AllenBradleyDataType.String,
            maxLength,
            DimensionCount.Scalar,
            new ElementCount(1));
}
