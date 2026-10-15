using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Binary.Bit;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData;

/// <summary>Builds a configured <c>Bit</c> node as the engine delivers it, before it is mapped.</summary>
internal static class BitNodeTestDataFactory
{
    /// <summary>A <c>Bit</c> node carrying exactly <paramref name="properties"/>.</summary>
    internal static LinkedNode BitWith(params KeyValuePair<string, Property>[] properties) =>
        LinkedNodeFactory.Create(
        [
            new Node
            {
                DesignId = BitNode.LinkedNodeTypeId,
                Name = BitNode.LinkedNodeTypeId,
                Id = Guid.NewGuid(),
                Properties = properties.ToDictionary(),
            },
        ]).Single();

    internal static KeyValuePair<string, Property> BitNumberProperty(ushort bitNumber) =>
        new(BitNode.BitNumberPropertyName, new Property { Value = bitNumber });
}
