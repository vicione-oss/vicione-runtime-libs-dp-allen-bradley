using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays;

/// <summary>
/// What every configured array tag carries on top of <see cref="LogixDataPointNode"/>: the number of elements
/// it was declared with. That is the whole of its shape today — one dimension, read whole; see
/// <c>reference/datatype-support.md</c>.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads it.</param>
/// <param name="ElementCount">The number of elements the tag is declared with in Studio 5000.</param>
internal abstract record LogixArrayDataPointNode(
    LinkedNode OriginalNode, TagName TagName, PollFrequency PollFrequency, ElementCount ElementCount)
    : LogixDataPointNode(OriginalNode, TagName, PollFrequency)
{
    /// <summary>The manifest property carrying <see cref="ElementCount"/>.</summary>
    public const string ElementCountPropertyName = nameof(ElementCount);
}
