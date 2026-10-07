using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints;

/// <summary>
/// A configured element: what every element node carries whatever its file type, and the names the
/// manifest declares them under. The file number is its parent's.
/// </summary>
public interface ILegacyDataPointNode : IDataPointNode, ITypedLegacyNode
{
    /// <summary>The manifest property carrying <see cref="ElementNumber"/>.</summary>
    const string ElementNumberPropertyName = nameof(ElementNumber);

    /// <summary>The manifest property carrying <see cref="PollFrequency"/>.</summary>
    const string PollFrequencyPropertyName = nameof(PollFrequency);

    /// <summary>The element inside the parent data file.</summary>
    ElementNumber ElementNumber { get; }

    /// <summary>How often an incoming port reads it.</summary>
    PollFrequency PollFrequency { get; }

    /// <summary>The type of data file this element belongs in.</summary>
    DataFileType FileType { get; }
}
