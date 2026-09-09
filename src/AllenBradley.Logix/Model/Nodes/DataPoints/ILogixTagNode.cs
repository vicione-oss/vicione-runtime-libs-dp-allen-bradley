using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;

/// <summary>
/// A configured tag: the two things every tag node carries whatever its type and whatever its shape, and
/// the names the manifest declares them under. A tag name and a poll frequency are not a scalar's, so
/// nothing here says which of the two a node is.
/// </summary>
public interface ILogixTagNode : IDataPointNode
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
