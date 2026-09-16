namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

internal static class LinkedNodesDataFactory
{
    internal static LinkedNode CreateLinkedNode(
        string designId, string name, params KeyValuePair<string, Property>[] properties) =>
        Wrap(new Node
        {
            DesignId = designId,
            Name = name,
            Id = Guid.NewGuid(),
            Properties = properties.ToDictionary(),
        });

    /// <summary>A linked node whose <c>Parent</c> is a fresh node of type <paramref name="parentDesignId"/>.</summary>
    internal static LinkedNode CreateChildLinkedNode(
        string parentDesignId, string designId, string name, params KeyValuePair<string, Property>[] properties)
    {
        var parent = new Node { DesignId = parentDesignId, Name = parentDesignId, Id = Guid.NewGuid() };
        var child = new Node
        {
            DesignId = designId,
            Name = name,
            Id = Guid.NewGuid(),
            ParentId = parent.Id,
            Properties = properties.ToDictionary(),
        };
        return LinkedNodeFactory.Create([parent, child]).Single().Children.Single();
    }

    /// <summary>
    /// A linked node routed to <paramref name="channel"/>, which is where a mapped tag node reads its
    /// <c>Channels</c> from.
    /// </summary>
    internal static LinkedNode CreateChanneledLinkedNode(string designId, string name, string channel) =>
        Wrap(new Node
        {
            DesignId = designId,
            Name = name,
            Id = Guid.NewGuid(),
            AffectedChannels = [channel],
            TransferredChannels = [channel],
        });

    private static LinkedNode Wrap(Node node) => LinkedNodeFactory.Create([node]).Single();
}
