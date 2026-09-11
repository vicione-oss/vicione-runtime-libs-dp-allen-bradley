using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.FloatingPoints.RealArray.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.DIntArray.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.IntArray.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.LIntArray.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.SIntArray.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.UDIntArray.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.UIntArray.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.USIntArray.Mapping;
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
/// Assembles every node mapper into the one the dataport base classes take. This list and the
/// manifest's nodes are the same set: a configured node no mapper here claims is a configuration error.
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

    private static IDataPointNodeMapper<IDataPointNode>[] ArrayNodeMappers() =>
        [
            new SIntArrayNodeMapper(), new IntArrayNodeMapper(), new DIntArrayNodeMapper(),
            new LIntArrayNodeMapper(), new USIntArrayNodeMapper(), new UIntArrayNodeMapper(),
            new UDIntArrayNodeMapper(), new RealArrayNodeMapper(),
        ];
}
