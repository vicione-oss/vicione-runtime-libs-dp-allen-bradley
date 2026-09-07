using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80;

/// <summary>
/// The declaration a round-trip suite expects the controller to report for its tag, built whole so that
/// it can be compared as one record.
/// </summary>
/// <remarks>
/// Comparing whole is the point. A read proves what the bytes decoded to and nothing else: it cannot
/// show that the tag is a scalar rather than a one-element array, and for a <c>STRING</c> it cannot show
/// the declared capacity at all — writing <c>"Hi"</c> into a <c>STRING_20</c> and reading <c>"Hi"</c>
/// back says nothing about how much the tag holds. Pinning every field is also what fails a tag that
/// starts reporting something extra, instead of letting it pass unnoticed.
/// <para>
/// Two factories rather than eight literals, because only two fields vary across the elementary types
/// and spelling the other four out per type would bury them. The fields a scalar is defined by the
/// <em>absence</em> of are still fixed here — a scalar's rank, its single element, and the capacity only
/// a string carries.
/// </para>
/// </remarks>
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
