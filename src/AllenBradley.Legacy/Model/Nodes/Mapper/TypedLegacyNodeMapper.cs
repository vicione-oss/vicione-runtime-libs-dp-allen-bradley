using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.BinaryFile.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.FloatFile.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.IntegerFile.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.LongFile.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Binary.Bit.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Floats.Float.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Integers.Integer.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Integers.Long.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.Device.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.Mapper;

/// <summary>
/// Assembles every node mapper into the one the dataport base classes take. This list and the
/// manifest's nodes are the same set: a configured node no mapper here claims is a configuration error.
/// </summary>
internal static class TypedLegacyNodeMapper
{
    /// <summary>Builds the mapper for the tree <c>allen-bradley-legacy.yaml</c> declares.</summary>
    internal static TypedNodeMapper<LegacyCommunication, DeviceNode> Instance() =>
        new(
            new DeviceNodeMapper(),
            [new IntegerFileNodeMapper(), new LongFileNodeMapper(), new FloatFileNodeMapper(), new BinaryFileNodeMapper()],
            [new IntegerNodeMapper(), new LongNodeMapper(), new FloatNodeMapper(), new BitNodeMapper()]);
}
