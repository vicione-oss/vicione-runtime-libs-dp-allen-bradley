using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags;
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
/// properties, and xUnit fails a theory that discovers no cases. Controller scope has none to give them,
/// contributing no segment to a tag address, and carried a placeholder until program scope arrived:
/// <see cref="ProgramTagsNode.ProgramNamePropertyName"/> is the real branch-node property those theories
/// now run on. Should it ever be the last one again, the fix is upstream, where the base test can set
/// <c>Theory.SkipTestWithoutData</c>.
/// </remarks>
public class LogixYamlConsistencyTests :
    YamlConsistencyBaseTest<LogixCommunication, LogixYamlConsistencyTests, DeviceNode>,
    IYamlTestingConfig<LogixCommunication, DeviceNode>
{
    public static string YamlFileName => "allen-bradley-logix.yaml";

    public static TypedNodeMapper<LogixCommunication, DeviceNode> Mapper => TypedLogixNodeMapper.Instance();

    public static HashSet<string> ExcludedCommunicationProperties => [];

    /// <remarks>
    /// A container's <c>Generation</c> is the one member of a branch node that is not a declared property
    /// and never will be. It comes from the node type the container was configured under — the 5x70 one
    /// or the 5x80 one — which is the whole point of there being two of each: an integrator answers by
    /// picking a controller, not by filling in a field.
    /// </remarks>
    public static Dictionary<Type, HashSet<string>> ExcludedConfigurationNodeProperties => new()
    {
        [typeof(ControllerTagsNode)] = [nameof(ControllerTagsNode.Generation)],
        [typeof(ProgramTagsNode)] = [nameof(ProgramTagsNode.Generation)],
    };

    public static Dictionary<Type, HashSet<string>> ExcludedDataPointNodeProperties => [];
}
