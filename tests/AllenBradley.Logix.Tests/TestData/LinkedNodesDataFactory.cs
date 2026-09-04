namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

internal static class LinkedNodesDataFactory
{
    /// <summary>A linked node of <paramref name="designId"/> carrying <paramref name="properties"/>.</summary>
    internal static LinkedNode CreateLinkedNode(
        string designId, string name, params KeyValuePair<string, Property>[] properties) =>
        Wrap(new Node
        {
            DesignId = designId,
            Name = name,
            Id = Guid.NewGuid(),
            Properties = properties.ToDictionary(),
        });

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
