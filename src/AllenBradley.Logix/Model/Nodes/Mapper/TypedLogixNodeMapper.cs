using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.FloatingPoints.LReal.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.DInt.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Strings.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Mapper;

/// <summary>
/// Assembles every node mapper into the one the dataport base classes take: the root mapper, the branch
/// mappers, and the data-point mappers. A configured node no mapper here claims is a hard configuration
/// error, so this list and the manifest's nodes are the same set.
/// </summary>
public static class TypedLogixNodeMapper
{
    /// <summary>Builds the mapper for the tree <c>allen-bradley-logix.yaml</c> declares.</summary>
    public static TypedNodeMapper<LogixCommunication, DeviceNode> Instance() =>
        new(
            new DeviceNodeMapper(),
            [new ControllerTagsNodeMapper()],
            [new DIntNodeMapper(), new LRealNodeMapper(), new StringNodeMapper()]);
}
