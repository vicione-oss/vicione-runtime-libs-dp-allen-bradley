using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Integers;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Integers.Integer;

/// <summary>
/// A configured element of an integer file, the configuration-time half of <see cref="IntegerDataPoint"/>.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="ElementNumber">The element inside the parent integer file.</param>
/// <param name="PollFrequency">How often an incoming port reads it.</param>
internal sealed record IntegerNode(LinkedNode OriginalNode, ElementNumber ElementNumber, PollFrequency PollFrequency)
    : LegacyDataPointNode(OriginalNode, ElementNumber, PollFrequency)
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "Integer";

    /// <inheritdoc />
    public override DataFileType FileType => DataFileType.Integer;
}
