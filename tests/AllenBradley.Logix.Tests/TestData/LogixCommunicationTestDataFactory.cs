using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.DInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.Extensions.Outgoing.QueueProcessing;
using static System.Guid;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

/// <summary>
/// Builds the engine-shaped configuration a data port is constructed from: a
/// <see cref="LogixCommunication"/> carrying the flat node list the typed node mapper turns into a tree.
/// </summary>
/// <remarks>
/// Nodes are made the way deserialization delivers them — a <c>DesignId</c>, a parent id and a property
/// bag — rather than as the typed nodes they map to, because what these suites exercise is the mapping
/// itself and everything the ports build on top of it.
/// </remarks>
internal static class LogixCommunicationTestDataFactory
{
    /// <summary>The device node type every configuration the factory makes is configured under.</summary>
    internal const string DeviceDesignId = DeviceNode.ControlLogix5x70DesignId;

    /// <summary>The controller every configuration the factory makes points at.</summary>
    internal const string DefaultGateway = "10.0.0.1";

    private static readonly Guid s_controllerTagsId = new("00000000-0000-0000-0000-0000c0777a65");

    /// <summary>A configured <c>DINT</c> tag, routed to <paramref name="channel"/>.</summary>
    internal static Node CreateDIntNode(string channel, string tagName, int pollFrequency = 100) =>
        new()
        {
            DesignId = DIntNode.LinkedNodeTypeId,
            Name = tagName,
            Id = NewGuid(),
            ParentId = s_controllerTagsId,
            AffectedChannels = [channel],
            TransferredChannels = [channel],
            Properties = new Dictionary<string, Property>
            {
                { ILogixScalarNode.TagNamePropertyName, new Property { Value = tagName } },
                { ILogixScalarNode.PollFrequencyPropertyName, new Property { Value = pollFrequency } },
            },
        };

    /// <summary>
    /// A configured <c>STRING</c> tag, routed to <paramref name="channel"/> and declared to hold
    /// <paramref name="maxLength"/> characters.
    /// </summary>
    internal static Node CreateStringNode(
        string channel, string tagName, int? maxLength = null, int pollFrequency = 100) =>
        new()
        {
            DesignId = StringNode.LinkedNodeTypeId,
            Name = tagName,
            Id = NewGuid(),
            ParentId = s_controllerTagsId,
            AffectedChannels = [channel],
            TransferredChannels = [channel],
            Properties = new Dictionary<string, Property>
            {
                { ILogixScalarNode.TagNamePropertyName, new Property { Value = tagName } },
                { ILogixScalarNode.PollFrequencyPropertyName, new Property { Value = pollFrequency } },
                {
                    StringNode.MaxLengthPropertyName,
                    new Property { Value = maxLength ?? StringMaxLength.Standard.Value }
                },
            },
        };

    /// <summary>Hangs <paramref name="tagNodes"/> off the controller-scope container they need a parent in.</summary>
    internal static List<Node> WrapInControllerTags(IEnumerable<Node> tagNodes)
    {
        var controllerTags = new Node
        {
            DesignId = ControllerTagsNode.LinkedNodeTypeId,
            Name = "Controller",
            Id = s_controllerTagsId,
            Properties = new Dictionary<string, Property>
            {
                { ControllerTagsNode.ControllerNamePropertyName, new Property { Value = "Controller" } },
            },
        };

        return [controllerTags, .. tagNodes];
    }

    /// <summary>A device with no tags configured under it.</summary>
    internal static LogixCommunication CreateCommunication() => CreateCommunication([]);

    /// <summary>A device carrying <paramref name="tagNodes"/> under controller scope.</summary>
    internal static LogixCommunication CreateCommunication(
        List<Node> tagNodes,
        int maxPendingMessages = 100_000,
        QueueStrategy strategy = QueueStrategy.DropOldest) =>
        new()
        {
            DesignId = DeviceDesignId,
            Gateway = DefaultGateway,
            Path = "1,0",
            MaxPendingMessages = maxPendingMessages,
            Strategy = (byte)strategy,
            Nodes = tagNodes is { Count: > 0 } ? WrapInControllerTags(tagNodes) : [],
        };

    /// <summary>A device with one <c>DINT</c> tag on <paramref name="channel"/>.</summary>
    internal static LogixCommunication CommunicationWithSingleDInt(
        string channel, string tagName = "TestTag") =>
        CreateCommunication([CreateDIntNode(channel, tagName)]);

    /// <summary>
    /// One scalar node as the mappers and validators see it, carrying exactly
    /// <paramref name="properties"/> — including none, so a suite can assert on a property that is
    /// missing rather than merely wrong.
    /// </summary>
    internal static LinkedNode ScalarLinkedNode(params (string Key, object Value)[] properties) =>
        LinkedNodeCarrying(DIntNode.LinkedNodeTypeId, properties);

    /// <summary>A scalar node carrying the two properties every scalar needs, both well-formed.</summary>
    internal static LinkedNode ValidScalarLinkedNode(string tagName = "TestTag", int pollFrequency = 100) =>
        ScalarLinkedNode(
            (ILogixScalarNode.TagNamePropertyName, tagName),
            (ILogixScalarNode.PollFrequencyPropertyName, pollFrequency));

    /// <summary>
    /// One string node as its mapper and validator see it, carrying exactly <paramref name="properties"/>.
    /// </summary>
    internal static LinkedNode StringLinkedNode(params (string Key, object Value)[] properties) =>
        LinkedNodeCarrying(StringNode.LinkedNodeTypeId, properties);

    /// <summary>A string node carrying the three properties it needs, all well-formed.</summary>
    internal static LinkedNode ValidStringLinkedNode(
        string tagName = "TestTag", int pollFrequency = 100, int? maxLength = null) =>
        StringLinkedNode(
            (ILogixScalarNode.TagNamePropertyName, tagName),
            (ILogixScalarNode.PollFrequencyPropertyName, pollFrequency),
            (StringNode.MaxLengthPropertyName, maxLength ?? StringMaxLength.Standard.Value));

    private static LinkedNode LinkedNodeCarrying(string designId, (string Key, object Value)[] properties) =>
        LinkedNodeFactory.Create(
        [
            new Node
            {
                DesignId = designId,
                Name = "TestTag",
                Id = NewGuid(),
                Properties = properties.ToDictionary(
                    static property => property.Key,
                    static property => new Property { Value = property.Value }),
            },
        ]).Single();
}
