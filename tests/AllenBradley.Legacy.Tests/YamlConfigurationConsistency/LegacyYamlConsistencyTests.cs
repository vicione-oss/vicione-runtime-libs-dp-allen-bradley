using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.Mapper;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using ViciOne.Suite.DataPort.Extensions.Testing.YamlTesting;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.YamlConfigurationConsistency;

/// <summary>
/// Holds the manifest and the node model to each other; the base test walks both sides and generates its
/// own cases.
/// </summary>
public sealed class LegacyYamlConsistencyTests :
    YamlConsistencyBaseTest<LegacyCommunication, LegacyYamlConsistencyTests, DeviceNode>,
    IYamlTestingConfig<LegacyCommunication, DeviceNode>
{
    public static string YamlFileName => "allen-bradley-legacy.yaml";

    public static TypedNodeMapper<LegacyCommunication, DeviceNode> Mapper => TypedLegacyNodeMapper.Instance();

    public static HashSet<string> ExcludedCommunicationProperties => [];

    public static Dictionary<Type, HashSet<string>> ExcludedConfigurationNodeProperties => new()
    {
        [typeof(DataFileNode)] = [nameof(DataFileNode.FileType)],
    };

    public static Dictionary<Type, HashSet<string>> ExcludedDataPointNodeProperties => new()
    {
        [typeof(ILegacyDataPointNode)] = [nameof(ILegacyDataPointNode.FileType)],
    };
}
