using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;

/// <summary>
/// A configured tag: what every tag node carries whatever its type and shape, and the names the
/// manifest declares them under.
/// </summary>
public interface ILogixDataPointNode : IDataPointNode
{
    /// <summary>The manifest property carrying <see cref="TagName"/>.</summary>
    const string TagNamePropertyName = nameof(TagName);

    /// <summary>The manifest property carrying <see cref="PollFrequency"/>.</summary>
    const string PollFrequencyPropertyName = nameof(PollFrequency);

    /// <summary>
    /// The oldest generation whose type vocabulary has this type, which is what a tag scope container
    /// holds its own generation against. Only a type that arrived after the 5X70 overrides the default.
    /// </summary>
    LogixGeneration MinimumGeneration => LogixGeneration.Logix5X70;

    /// <summary>The symbolic tag address this node configures.</summary>
    TagName TagName { get; }

    /// <summary>How often an incoming port reads it.</summary>
    PollFrequency PollFrequency { get; }
}
