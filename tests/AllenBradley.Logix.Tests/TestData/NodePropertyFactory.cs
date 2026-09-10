using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Strings;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

/// <summary>
/// Builds the properties a configured node carries, keyed as the mappers and validators read them. Each
/// value is typed <c>object</c> so a suite can hand a property what it is not, which is what the
/// validators are there to refuse.
/// </summary>
internal static class NodePropertyFactory
{
    internal static KeyValuePair<string, Property> CreatePollFrequency(object pollFrequencyInMilliseconds) =>
        KeyValuePair.Create(
            ILogixTagNode.PollFrequencyPropertyName,
            new Property { Value = pollFrequencyInMilliseconds });

    internal static KeyValuePair<string, Property> CreateTagName(object tagName) =>
        KeyValuePair.Create(ILogixTagNode.TagNamePropertyName, new Property { Value = tagName });

    internal static KeyValuePair<string, Property> CreateMaxLength(object maxLength) =>
        KeyValuePair.Create(StringNode.MaxLengthPropertyName, new Property { Value = maxLength });

    internal static KeyValuePair<string, Property> CreateElementCount(object elementCount) =>
        KeyValuePair.Create(LogixArrayNode.ElementCountPropertyName, new Property { Value = elementCount });

    internal static KeyValuePair<string, Property> CreateProgramName(object programName) =>
        KeyValuePair.Create(ProgramTagsNode.ProgramNamePropertyName, new Property { Value = programName });
}
