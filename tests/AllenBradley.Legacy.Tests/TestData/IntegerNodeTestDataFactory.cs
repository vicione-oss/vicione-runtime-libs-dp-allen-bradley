using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Integers.Integer;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData;

/// <summary>Builds a configured <c>Integer</c> element node as the engine delivers it, before it is mapped.</summary>
internal static class IntegerNodeTestDataFactory
{
    /// <summary>An <c>Integer</c> node carrying exactly <paramref name="properties"/>.</summary>
    internal static LinkedNode IntegerElementWith(params KeyValuePair<string, Property>[] properties) =>
        LinkedNodeFactory.Create(
        [
            new Node
            {
                DesignId = IntegerNode.LinkedNodeTypeId,
                Name = IntegerNode.LinkedNodeTypeId,
                Id = Guid.NewGuid(),
                Properties = properties.ToDictionary(),
            },
        ]).Single();

    internal static KeyValuePair<string, Property> ElementNumberProperty(ushort elementNumber) =>
        new(ILegacyDataPointNode.ElementNumberPropertyName, new Property { Value = elementNumber });

    internal static KeyValuePair<string, Property> PollFrequencyProperty(int milliseconds) =>
        new(ILegacyDataPointNode.PollFrequencyPropertyName, new Property { Value = milliseconds });
}
