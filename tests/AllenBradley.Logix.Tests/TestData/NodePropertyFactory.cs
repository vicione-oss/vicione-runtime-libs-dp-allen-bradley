using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Strings;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

public static class NodePropertyFactory
{
    internal static KeyValuePair<string, Property> CreatePollFrequency(int pollFrequencyInMilliSeconds)
    {
        return KeyValuePair.Create(
            ILogixScalarNode.PollFrequencyPropertyName,
            new Property { Value = pollFrequencyInMilliSeconds });
    }

    internal static KeyValuePair<string, Property> CreateTagName(object tagName)
    {
        return KeyValuePair.Create(
            ILogixScalarNode.TagNamePropertyName,
            new Property { Value = tagName });
    }

    internal static KeyValuePair<string, Property> CreateMaxLength(object maxLength)
    {
        return KeyValuePair.Create(
            StringNode.MaxLengthPropertyName,
            new Property { Value = maxLength });
    }

    internal static KeyValuePair<string, Property> CreateProgramName(object programName)
    {
        return KeyValuePair.Create(
            ProgramTagsNode.ProgramNamePropertyName,
            new Property { Value = programName });
    }
}
