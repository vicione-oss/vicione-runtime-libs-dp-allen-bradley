using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Strings;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

/// <summary>
/// Builds the properties a configured node carries, keyed as the mappers and validators read them. Each
/// value is typed <c>object</c> so a suite can hand a property what it is not, which is what the
/// validators are there to refuse.
/// </summary>
internal static class NodePropertyFactory
{
    /// <summary>How often the configured tag is polled, in milliseconds.</summary>
    internal static KeyValuePair<string, Property> CreatePollFrequency(object pollFrequencyInMilliseconds) =>
        KeyValuePair.Create(
            ILogixTagNode.PollFrequencyPropertyName,
            new Property { Value = pollFrequencyInMilliseconds });

    /// <summary>The address the configured tag names on the controller.</summary>
    internal static KeyValuePair<string, Property> CreateTagName(object tagName) =>
        KeyValuePair.Create(ILogixTagNode.TagNamePropertyName, new Property { Value = tagName });

    /// <summary>The character capacity a configured string tag is declared with.</summary>
    internal static KeyValuePair<string, Property> CreateMaxLength(object maxLength) =>
        KeyValuePair.Create(StringNode.MaxLengthPropertyName, new Property { Value = maxLength });

    /// <summary>The program a configured program-scope container holds the tags of.</summary>
    internal static KeyValuePair<string, Property> CreateProgramName(object programName) =>
        KeyValuePair.Create(ProgramTagsNode.ProgramNamePropertyName, new Property { Value = programName });
}
