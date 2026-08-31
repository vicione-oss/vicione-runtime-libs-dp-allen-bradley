using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.FloatingPoints.LReal;
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

    /// <summary>
    /// The node id the factory's program container carries. A tag names it as its parent to be configured
    /// inside that program rather than under controller scope.
    /// </summary>
    internal static readonly Guid ProgramTagsId = new("00000000-0000-0000-0000-000070726f67");

    private static readonly Guid s_controllerTagsId = new("00000000-0000-0000-0000-0000c0777a65");

    /// <summary>A configured <c>DINT</c> tag, routed to <paramref name="channel"/>.</summary>
    internal static Node CreateDIntNode(
        string channel, string tagName, int pollFrequency = 100, Guid? parentId = null) =>
        new()
        {
            DesignId = DIntNode.LinkedNodeTypeId,
            Name = tagName,
            Id = NewGuid(),
            ParentId = parentId ?? s_controllerTagsId,
            AffectedChannels = [channel],
            TransferredChannels = [channel],
            Properties = new Dictionary<string, Property>
            {
                { ILogixScalarNode.TagNamePropertyName, new Property { Value = tagName } },
                { ILogixScalarNode.PollFrequencyPropertyName, new Property { Value = pollFrequency } },
            },
        };

    /// <summary>A configured <c>LREAL</c> tag, routed to <paramref name="channel"/>.</summary>
    internal static Node CreateLRealNode(
        string channel, string tagName, int pollFrequency = 100, Guid? parentId = null) =>
        new()
        {
            DesignId = LRealNode.LinkedNodeTypeId,
            Name = tagName,
            Id = NewGuid(),
            ParentId = parentId ?? s_controllerTagsId,
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
        string channel, string tagName, int? maxLength = null, int pollFrequency = 100, Guid? parentId = null) =>
        new()
        {
            DesignId = StringNode.LinkedNodeTypeId,
            Name = tagName,
            Id = NewGuid(),
            ParentId = parentId ?? s_controllerTagsId,
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

    /// <summary>
    /// Hangs <paramref name="tagNodes"/> off the controller-scope container they need a parent in, of the
    /// generation <paramref name="deviceDesignId"/> names.
    /// </summary>
    /// <remarks>
    /// The container's node type follows the device's because the manifest gives each generation its
    /// own, and a configuration that paired them the other way is one the editor could not produce —
    /// which is what <paramref name="containerDesignId"/> is for, since that pairing has a guard.
    /// </remarks>
    /// <param name="tagNodes">The tags to hang off the container.</param>
    /// <param name="deviceDesignId">The device node type the container's own follows from.</param>
    /// <param name="containerDesignId">
    /// The container's node type, overriding the one <paramref name="deviceDesignId"/> implies.
    /// </param>
    internal static List<Node> WrapInControllerTags(
        IEnumerable<Node> tagNodes, string deviceDesignId, string? containerDesignId = null)
    {
        var generation = DeviceNode.TypeOf(deviceDesignId)!.Value.Generation;

        var controllerTags = new Node
        {
            DesignId = containerDesignId ?? (generation is LogixGeneration.Logix5x80
                ? ControllerTagsNode.Logix5x80LinkedNodeTypeId
                : ControllerTagsNode.Logix5x70LinkedNodeTypeId),
            Name = "Controller",
            Id = s_controllerTagsId,
            Properties = new Dictionary<string, Property>
            {
                { ControllerTagsNode.ControllerNamePropertyName, new Property { Value = "Controller" } },
            },
        };

        return [controllerTags, .. tagNodes];
    }

    /// <summary>
    /// A program-scope container named <paramref name="programName"/>, hanging directly off the device.
    /// </summary>
    /// <param name="programName">The program whose tags the container holds.</param>
    /// <param name="id">
    /// The container's node id, which the tags configured inside it name as their parent. Left to a fresh
    /// one when the container holds none.
    /// </param>
    /// <param name="containerDesignId">
    /// The container's node type, which decides the generation it holds its tags to. Defaults to the 5x70
    /// one, matching <see cref="DeviceDesignId"/>.
    /// </param>
    internal static Node CreateProgramTagsNode(
        string programName, Guid? id = null, string? containerDesignId = null) =>
        new()
        {
            DesignId = containerDesignId ?? ProgramTagsNode.Logix5x70LinkedNodeTypeId,
            Name = programName,
            Id = id ?? NewGuid(),
            Properties = new Dictionary<string, Property>
            {
                { ProgramTagsNode.ProgramNamePropertyName, new Property { Value = programName } },
            },
        };

    /// <summary>
    /// A device carrying <paramref name="nodes"/> exactly as given, wrapped in no container: for a
    /// configuration whose own containers are what is under test.
    /// </summary>
    internal static LogixCommunication CreateCommunicationOf(
        List<Node> nodes, string deviceDesignId = DeviceDesignId) =>
        new()
        {
            DesignId = deviceDesignId,
            Gateway = DefaultGateway,
            Path = "1,0",
            MaxPendingMessages = 100_000,
            Strategy = (byte)QueueStrategy.DropOldest,
            Nodes = nodes,
        };

    /// <summary>
    /// The program container node type of the generation <paramref name="deviceDesignId"/> names — the
    /// pairing the editor can build, and the only one the device accepts.
    /// </summary>
    internal static string ProgramTagsDesignIdFor(string deviceDesignId) =>
        DeviceNode.TypeOf(deviceDesignId)!.Value.Generation is LogixGeneration.Logix5x80
            ? ProgramTagsNode.Logix5x80LinkedNodeTypeId
            : ProgramTagsNode.Logix5x70LinkedNodeTypeId;

    /// <summary>A device with no tags configured under it.</summary>
    internal static LogixCommunication CreateCommunication() => CreateCommunication([]);

    /// <summary>A device carrying <paramref name="tagNodes"/> under controller scope.</summary>
    internal static LogixCommunication CreateCommunication(
        List<Node> tagNodes,
        int maxPendingMessages = 100_000,
        QueueStrategy strategy = QueueStrategy.DropOldest,
        string deviceDesignId = DeviceDesignId,
        string? containerDesignId = null) =>
        new()
        {
            DesignId = deviceDesignId,
            Gateway = DefaultGateway,
            Path = "1,0",
            MaxPendingMessages = maxPendingMessages,
            Strategy = (byte)strategy,
            Nodes = tagNodes is { Count: > 0 }
                ? WrapInControllerTags(tagNodes, deviceDesignId, containerDesignId)
                : [],
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
        LinkedNodeCarrying(DIntNode.LinkedNodeTypeId, "TestTag", properties);

    /// <summary>A scalar node carrying the two properties every scalar needs, both well-formed.</summary>
    internal static LinkedNode ValidScalarLinkedNode(string tagName = "TestTag", int pollFrequency = 100) =>
        ScalarLinkedNode(
            (ILogixScalarNode.TagNamePropertyName, tagName),
            (ILogixScalarNode.PollFrequencyPropertyName, pollFrequency));

    /// <summary>
    /// One string node as its mapper and validator see it, carrying exactly <paramref name="properties"/>.
    /// </summary>
    internal static LinkedNode StringLinkedNode(params (string Key, object Value)[] properties) =>
        LinkedNodeCarrying(StringNode.LinkedNodeTypeId, "TestTag", properties);

    /// <summary>
    /// One program container as its mapper and validator see it, labelled <paramref name="name"/> in the
    /// editor tree and carrying exactly <paramref name="properties"/>.
    /// </summary>
    /// <remarks>
    /// The editor label and the program name are separate on purpose: the validator's message names both,
    /// and a helper that conflated them could not tell a wrong message from a right one.
    /// </remarks>
    internal static LinkedNode ProgramTagsLinkedNode(
        string name, params (string Key, object Value)[] properties) =>
        LinkedNodeCarrying(ProgramTagsNode.Logix5x70LinkedNodeTypeId, name, properties);

    /// <summary>A string node carrying the three properties it needs, all well-formed.</summary>
    internal static LinkedNode ValidStringLinkedNode(
        string tagName = "TestTag", int pollFrequency = 100, int? maxLength = null) =>
        StringLinkedNode(
            (ILogixScalarNode.TagNamePropertyName, tagName),
            (ILogixScalarNode.PollFrequencyPropertyName, pollFrequency),
            (StringNode.MaxLengthPropertyName, maxLength ?? StringMaxLength.Standard.Value));

    private static LinkedNode LinkedNodeCarrying(
        string designId, string name, (string Key, object Value)[] properties) =>
        LinkedNodeFactory.Create(
        [
            new Node
            {
                DesignId = designId,
                Name = name,
                Id = NewGuid(),
                Properties = properties.ToDictionary(
                    static property => property.Key,
                    static property => new Property { Value = property.Value }),
            },
        ]).Single();
}
