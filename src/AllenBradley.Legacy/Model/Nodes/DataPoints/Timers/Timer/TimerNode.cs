using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Timers;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Timers.Timer;

/// <summary>
/// A configured element of a timer file, the configuration-time half of <see cref="TimerDataPoint"/>. It names
/// the element and no sub-element of it.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="ElementNumber">The element inside the parent timer file.</param>
/// <param name="PollFrequency">How often an incoming port reads it.</param>
internal sealed record TimerNode(LinkedNode OriginalNode, ElementNumber ElementNumber, PollFrequency PollFrequency)
    : LegacyDataPointNode(OriginalNode, ElementNumber, PollFrequency)
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "Timer";

    /// <inheritdoc />
    public override DataFileType FileType => DataFileType.Timer;
}
