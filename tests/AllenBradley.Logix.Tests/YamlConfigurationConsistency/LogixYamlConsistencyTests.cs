using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ArrayContainer;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ControllerTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ProgramTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.UdtContainer;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Mapper;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using ViciOne.Suite.DataPort.Extensions.Testing.YamlTesting;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.YamlConfigurationConsistency;

/// <summary>
/// Holds the manifest and the node model to each other; the base test walks both sides and generates its
/// own cases. xUnit fails a theory that discovers no cases, and
/// <see cref="ProgramTagsNode.ProgramNamePropertyName"/> is the only branch-node property feeding two of
/// them; if it is ever gone, the fix is <c>Theory.SkipTestWithoutData</c> in the base test.
/// </summary>
public sealed class LogixYamlConsistencyTests :
    YamlConsistencyBaseTest<LogixCommunication, LogixYamlConsistencyTests, DeviceNode>,
    IYamlTestingConfig<LogixCommunication, DeviceNode>
{
    public static string YamlFileName => "allen-bradley-logix.yaml";

    public static TypedNodeMapper<LogixCommunication, DeviceNode> Mapper => TypedLogixNodeMapper.Instance();

    public static HashSet<string> ExcludedCommunicationProperties => [];

    public static Dictionary<Type, HashSet<string>> ExcludedConfigurationNodeProperties => new()
    {
        [typeof(ControllerTagsNode)] = [nameof(ControllerTagsNode.Generation)],
        [typeof(ProgramTagsNode)] = [nameof(ProgramTagsNode.Generation)],
        [typeof(ArrayContainerNode)] = [nameof(ArrayContainerNode.ArrayDataType)],
        [typeof(UdtContainerNode)] = [nameof(UdtContainerNode.Generation)],
    };

    public static Dictionary<Type, HashSet<string>> ExcludedDataPointNodeProperties => new()
    {
        [typeof(ILogixDataPointNode)] = [nameof(ILogixDataPointNode.DataType)]
    };
}
