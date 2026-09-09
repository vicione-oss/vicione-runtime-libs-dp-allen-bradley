using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.IntArray.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Booleans.Bool.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.FloatingPoints.LReal.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.FloatingPoints.Real.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.DInt.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.Int.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.LInt.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.SInt.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.UDInt.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.UInt.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.ULInt.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.USInt.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Strings.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;
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
            [
                new ControllerTags5X70NodeMapper(), new ControllerTags5X80NodeMapper(),
                new ProgramTags5X70NodeMapper(), new ProgramTags5X80NodeMapper(),
            ],
            DataPointNodeMappers());

    // Grouped the way the manifest's tag nodes and the converters are: one method per type family,
    // so adding a type touches the family it belongs to rather than a single flat list that every
    // slice appends to.
    private static IDataPointNodeMapper<IDataPointNode>[] DataPointNodeMappers() =>
        [
            .. BooleanNodeMappers(), .. IntegerNodeMappers(), .. FloatingPointNodeMappers(),
            .. StringNodeMappers(), .. ArrayNodeMappers(),
        ];

    private static IDataPointNodeMapper<IDataPointNode>[] BooleanNodeMappers() =>
        [new BoolNodeMapper()];

    private static IDataPointNodeMapper<IDataPointNode>[] IntegerNodeMappers() =>
        [
            new SIntNodeMapper(), new IntNodeMapper(), new DIntNodeMapper(), new LIntNodeMapper(),
            new USIntNodeMapper(), new UIntNodeMapper(), new UDIntNodeMapper(), new ULIntNodeMapper(),
        ];

    private static IDataPointNodeMapper<IDataPointNode>[] FloatingPointNodeMappers() =>
        [new RealNodeMapper(), new LRealNodeMapper()];

    private static IDataPointNodeMapper<IDataPointNode>[] StringNodeMappers() =>
        [new StringNodeMapper()];

    // A family by shape rather than by element type: every array node maps the same three properties
    // whatever it holds, so the other element types join this list rather than start their own.
    private static IDataPointNodeMapper<IDataPointNode>[] ArrayNodeMappers() =>
        [new IntArrayNodeMapper()];
}
