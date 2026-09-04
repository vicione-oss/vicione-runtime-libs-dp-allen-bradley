using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars;

/// <summary>
/// A configured scalar tag: the two things every scalar node carries whatever its type, and the names
/// the manifest declares them under.
/// </summary>
public interface ILogixScalarNode : IDataPointNode
{
    /// <summary>The manifest property carrying <see cref="TagName"/>.</summary>
    const string TagNamePropertyName = nameof(TagName);

    /// <summary>The manifest property carrying <see cref="PollFrequency"/>.</summary>
    const string PollFrequencyPropertyName = nameof(PollFrequency);

    /// <summary>
    /// The oldest generation whose type vocabulary has this type, which is what a tag scope container
    /// holds its own generation against.
    /// Defaults to the oldest generation the addon addresses, so only a type that arrived later says
    /// anything.
    /// </summary>
    LogixGeneration MinimumGeneration => LogixGeneration.Logix5X70;

    /// <summary>The symbolic tag address this node configures.</summary>
    TagName TagName { get; }

    /// <summary>How often an incoming port reads it.</summary>
    PollFrequency PollFrequency { get; }
}
