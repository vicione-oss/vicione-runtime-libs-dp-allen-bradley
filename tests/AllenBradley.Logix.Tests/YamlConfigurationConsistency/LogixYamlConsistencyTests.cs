using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Mapper;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using ViciOne.Suite.DataPort.Extensions.Testing.YamlTesting;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.YamlConfigurationConsistency;

/// <summary>
/// Holds the manifest and the node model to each other: every property a mapper reads is declared in
/// the YAML, and every property the YAML declares is read by a mapper. Nothing here is written per
/// node — the base test walks both sides and generates its own cases.
/// </summary>
/// <remarks>
/// Two of its theories — <c>ConfigurationMapperPropertyExistsInYamlNode</c> and
/// <c>YamlConfigurationNodePropertyExistsInConfigurationMapper</c> — draw their data from branch-node
/// properties, and xUnit fails a theory that discovers no cases. Controller scope contributes no
/// segment to a tag address and so has no property of its own, which is why
/// <see cref="ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags.ControllerTagsNode.ControllerNamePropertyName"/>
/// exists. The real fix is upstream, where the base test can set <c>Theory.SkipTestWithoutData</c>.
/// </remarks>
public class LogixYamlConsistencyTests :
    YamlConsistencyBaseTest<LogixCommunication, LogixYamlConsistencyTests, DeviceNode>,
    IYamlTestingConfig<LogixCommunication, DeviceNode>
{
    public static string YamlFileName => "allen-bradley-logix.yaml";

    public static TypedNodeMapper<LogixCommunication, DeviceNode> Mapper => TypedLogixNodeMapper.Instance();

    public static HashSet<string> ExcludedCommunicationProperties => [];

    public static Dictionary<Type, HashSet<string>> ExcludedConfigurationNodeProperties => [];

    public static Dictionary<Type, HashSet<string>> ExcludedDataPointNodeProperties => [];
}
