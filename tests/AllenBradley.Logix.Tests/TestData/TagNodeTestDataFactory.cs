using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.DInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Strings;
using static System.Guid;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

/// <summary>
/// Builds the configured tag nodes a scope container holds: one node per call, hung off the parent the
/// caller names, because a node's parent is fixed when the node is constructed.
/// </summary>
internal static class TagNodeTestDataFactory
{
    internal const int DefaultPollFrequencyInMilliseconds = 100;

    /// <summary>A configured <c>DINT</c> tag under <paramref name="parentId"/>, routed to <paramref name="channel"/>.</summary>
    internal static Node CreateDIntNode(
        string channel, string tagName, Guid parentId, int pollFrequency = DefaultPollFrequencyInMilliseconds) =>
        CreateDataPointNode(DIntNode.LinkedNodeTypeId, channel, tagName, parentId, pollFrequency);

    /// <summary>
    /// A configured <c>STRING</c> tag under <paramref name="parentId"/>, routed to
    /// <paramref name="channel"/> and declared to hold <paramref name="maxLength"/> characters.
    /// </summary>
    internal static Node CreateStringNode(
        string channel,
        string tagName,
        Guid parentId,
        int? maxLength = null,
        int pollFrequency = DefaultPollFrequencyInMilliseconds) =>
        CreateDataPointNode(
            StringNode.LinkedNodeTypeId,
            channel,
            tagName,
            parentId,
            pollFrequency,
            (StringNode.MaxLengthPropertyName, maxLength ?? StringMaxLength.Standard.Value));

    private static Node CreateDataPointNode(
        string designId,
        string channel,
        string tagName,
        Guid parentId,
        int pollFrequency,
        params (string Key, object Value)[] extraProperties)
    {
        var properties = new Dictionary<string, Property>
        {
            { ILogixDataPointNode.TagNamePropertyName, new Property { Value = tagName } },
            { ILogixDataPointNode.PollFrequencyPropertyName, new Property { Value = pollFrequency } },
        };

        foreach (var (key, value) in extraProperties)
        {
            properties[key] = new Property { Value = value };
        }

        return new Node
        {
            DesignId = designId,
            Name = tagName,
            Id = NewGuid(),
            ParentId = parentId,
            AffectedChannels = [channel],
            TransferredChannels = [channel],
            Properties = properties,
        };
    }
}
