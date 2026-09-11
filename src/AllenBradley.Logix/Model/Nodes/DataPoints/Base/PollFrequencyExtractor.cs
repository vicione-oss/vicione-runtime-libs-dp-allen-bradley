using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Base;

/// <summary>Reads the <see cref="ILogixTagNode.PollFrequencyPropertyName"/> property off a configured node.</summary>
internal static class PollFrequencyExtractor
{
    public static PollFrequency GetPollFrequency(LinkedNode node) =>
        PollFrequency.FromMilliseconds(
            node.GetRequiredPropertyValue<int>(ILogixTagNode.PollFrequencyPropertyName));
}
