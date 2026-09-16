using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ArrayContainer;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ControllerTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ProgramTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.FloatingPoints.LRealArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.FloatingPoints.RealArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.DIntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.IntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.LIntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.SIntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.UDIntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.UIntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.ULIntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.USIntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Booleans.Bool;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.FloatingPoints.LReal;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.FloatingPoints.Real;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.DInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.Int;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.LInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.SInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.UDInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.UInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.ULInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.USInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device.LogixControllerKind;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixClientTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixCommunicationTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

/// <summary>
/// Builds the mapped tree's nodes directly, without the mapper that normally makes them: one default node
/// per member, unattached, so a test varies what it is about with <c>with { }</c> and composes its own
/// tree. The <c>LREAL</c> and unsigned-integer nodes are the types a 5X70 controller has not got.
/// </summary>
internal static class TypedNodeTestDataFactory
{
    internal static readonly LogixControllerKind DefaultControllerKind = ControlLogix5X70;

    internal static readonly LogixGeneration DefaultGeneration = DefaultControllerKind.Generation;

    internal static readonly ProgramName DefaultProgramName = new("MainProgram");

    /// <summary>
    /// The channel every tag the factory makes is routed to. No tag is named after it, so a test can
    /// tell an address the mapper composed from a channel it copied.
    /// </summary>
    internal const string DefaultChannel = "Channel";

    internal static readonly TagName DefaultDIntTagName = new("Counter");
    internal static readonly TagName DefaultIntTagName = new("Setpoint");
    internal static readonly TagName DefaultSIntTagName = new("Level");
    internal static readonly TagName DefaultLIntTagName = new("Ticks");
    internal static readonly TagName DefaultUsIntTagName = new("Pressure");
    internal static readonly TagName DefaultUIntTagName = new("Revolutions");
    internal static readonly TagName DefaultUdIntTagName = new("Runtime");
    internal static readonly TagName DefaultUlIntTagName = new("Cycles");
    internal static readonly TagName DefaultBoolTagName = new("Running");
    internal static readonly TagName DefaultRealTagName = new("FlowRate");
    internal static readonly TagName DefaultLRealTagName = new("Temperature");
    internal static readonly TagName DefaultStringTagName = new("Label");
    internal static readonly TagName DefaultSIntArrayTagName = new("Samples");
    internal static readonly TagName DefaultIntArrayTagName = new("Readings");
    internal static readonly TagName DefaultDIntArrayTagName = new("Totals");
    internal static readonly TagName DefaultLIntArrayTagName = new("Timestamps");
    internal static readonly TagName DefaultUsIntArrayTagName = new("Pressures");
    internal static readonly TagName DefaultUIntArrayTagName = new("Speeds");
    internal static readonly TagName DefaultUdIntArrayTagName = new("Runtimes");
    internal static readonly TagName DefaultUlIntArrayTagName = new("CycleCounts");
    internal static readonly TagName DefaultRealArrayTagName = new("Temperatures");
    internal static readonly TagName DefaultLRealArrayTagName = new("Positions");

    internal static readonly ElementCount DefaultElementCount = new(10);

    /// <summary>
    /// A stand-in for the linked node a mapped node came from. Nothing a suite asserts reads it, so it
    /// carries nothing.
    /// </summary>
    internal static LinkedNode DummyOriginalNode => new(new Node());

    /// <summary>
    /// A controller of <see cref="DefaultControllerKind"/>, with nothing configured under it. A suite about
    /// another controller overrides <c>ControllerKind</c>, which the node's own communication record does
    /// not follow.
    /// </summary>
    internal static DeviceNode DefaultDeviceNode =>
        new(CreateCommunicationOf([], DeviceNode.DesignIdFor(DefaultControllerKind)),
            DefaultClientInformation(),
            DefaultControllerKind);

    /// <summary>Controller scope on a <see cref="DefaultGeneration"/> controller, holding nothing yet.</summary>
    internal static ControllerTagsNode DefaultControllerTagsNode => new(DummyOriginalNode, DefaultGeneration);

    /// <summary>
    /// The scope of <see cref="DefaultProgramName"/> on a <see cref="DefaultGeneration"/> controller,
    /// holding nothing yet.
    /// </summary>
    internal static ProgramTagsNode DefaultProgramTagsNode =>
        new(DummyOriginalNode, DefaultProgramName, DefaultGeneration);

    /// <summary>
    /// A configured <c>LREAL</c> tag. Its linked node carries the node type, which is what a container
    /// names when it refuses the tag.
    /// </summary>
    internal static LRealNode DefaultLRealNode =>
        new(
            CreateChanneledLinkedNode(LRealNode.LinkedNodeTypeId, DefaultLRealTagName.Value, DefaultChannel),
            DefaultLRealTagName,
            DefaultPollFrequency);

    internal static DIntNode DefaultDIntNode =>
        new(
            CreateChanneledLinkedNode(DIntNode.LinkedNodeTypeId, DefaultDIntTagName.Value, DefaultChannel),
            DefaultDIntTagName,
            DefaultPollFrequency);

    internal static IntNode DefaultIntNode =>
        new(
            CreateChanneledLinkedNode(IntNode.LinkedNodeTypeId, DefaultIntTagName.Value, DefaultChannel),
            DefaultIntTagName,
            DefaultPollFrequency);

    internal static SIntNode DefaultSIntNode =>
        new(
            CreateChanneledLinkedNode(SIntNode.LinkedNodeTypeId, DefaultSIntTagName.Value, DefaultChannel),
            DefaultSIntTagName,
            DefaultPollFrequency);

    internal static LIntNode DefaultLIntNode =>
        new(
            CreateChanneledLinkedNode(LIntNode.LinkedNodeTypeId, DefaultLIntTagName.Value, DefaultChannel),
            DefaultLIntTagName,
            DefaultPollFrequency);

    internal static USIntNode DefaultUSIntNode =>
        new(
            CreateChanneledLinkedNode(USIntNode.LinkedNodeTypeId, DefaultUsIntTagName.Value, DefaultChannel),
            DefaultUsIntTagName,
            DefaultPollFrequency);

    internal static UIntNode DefaultUIntNode =>
        new(
            CreateChanneledLinkedNode(UIntNode.LinkedNodeTypeId, DefaultUIntTagName.Value, DefaultChannel),
            DefaultUIntTagName,
            DefaultPollFrequency);

    internal static UDIntNode DefaultUDIntNode =>
        new(
            CreateChanneledLinkedNode(UDIntNode.LinkedNodeTypeId, DefaultUdIntTagName.Value, DefaultChannel),
            DefaultUdIntTagName,
            DefaultPollFrequency);

    internal static ULIntNode DefaultULIntNode =>
        new(
            CreateChanneledLinkedNode(ULIntNode.LinkedNodeTypeId, DefaultUlIntTagName.Value, DefaultChannel),
            DefaultUlIntTagName,
            DefaultPollFrequency);

    internal static BoolNode DefaultBoolNode =>
        new(
            CreateChanneledLinkedNode(BoolNode.LinkedNodeTypeId, DefaultBoolTagName.Value, DefaultChannel),
            DefaultBoolTagName,
            DefaultPollFrequency);

    internal static RealNode DefaultRealNode =>
        new(
            CreateChanneledLinkedNode(RealNode.LinkedNodeTypeId, DefaultRealTagName.Value, DefaultChannel),
            DefaultRealTagName,
            DefaultPollFrequency);

    internal static SIntArrayDataPointNode DefaultSIntArrayDataPointNode =>
        new(
            CreateChanneledLinkedNode(
                SIntArrayDataPointNode.LinkedNodeTypeId, DefaultSIntArrayTagName.Value, DefaultChannel),
            DefaultSIntArrayTagName,
            DefaultPollFrequency,
            DefaultElementCount);

    internal static IntArrayDataPointNode DefaultIntArrayDataPointNode =>
        new(
            CreateChanneledLinkedNode(
                IntArrayDataPointNode.LinkedNodeTypeId, DefaultIntArrayTagName.Value, DefaultChannel),
            DefaultIntArrayTagName,
            DefaultPollFrequency,
            DefaultElementCount);

    internal static DIntArrayDataPointNode DefaultDIntArrayDataPointNode =>
        new(
            CreateChanneledLinkedNode(
                DIntArrayDataPointNode.LinkedNodeTypeId, DefaultDIntArrayTagName.Value, DefaultChannel),
            DefaultDIntArrayTagName,
            DefaultPollFrequency,
            DefaultElementCount);

    internal static LIntArrayDataPointNode DefaultLIntArrayDataPointNode =>
        new(
            CreateChanneledLinkedNode(
                LIntArrayDataPointNode.LinkedNodeTypeId, DefaultLIntArrayTagName.Value, DefaultChannel),
            DefaultLIntArrayTagName,
            DefaultPollFrequency,
            DefaultElementCount);

    internal static UsIntArrayDataPointNode DefaultUsIntArrayDataPointNode =>
        new(
            CreateChanneledLinkedNode(
                UsIntArrayDataPointNode.LinkedNodeTypeId, DefaultUsIntArrayTagName.Value, DefaultChannel),
            DefaultUsIntArrayTagName,
            DefaultPollFrequency,
            DefaultElementCount);

    internal static UIntArrayDataPointNode DefaultUIntArrayDataPointNode =>
        new(
            CreateChanneledLinkedNode(
                UIntArrayDataPointNode.LinkedNodeTypeId, DefaultUIntArrayTagName.Value, DefaultChannel),
            DefaultUIntArrayTagName,
            DefaultPollFrequency,
            DefaultElementCount);

    internal static UdIntArrayDataPointNode DefaultUdIntArrayDataPointNode =>
        new(
            CreateChanneledLinkedNode(
                UdIntArrayDataPointNode.LinkedNodeTypeId, DefaultUdIntArrayTagName.Value, DefaultChannel),
            DefaultUdIntArrayTagName,
            DefaultPollFrequency,
            DefaultElementCount);

    internal static UlIntArrayDataPointNode DefaultUlIntArrayDataPointNode =>
        new(
            CreateChanneledLinkedNode(
                UlIntArrayDataPointNode.LinkedNodeTypeId, DefaultUlIntArrayTagName.Value, DefaultChannel),
            DefaultUlIntArrayTagName,
            DefaultPollFrequency,
            DefaultElementCount);

    internal static RealArrayDataPointNode DefaultRealArrayDataPointNode =>
        new(
            CreateChanneledLinkedNode(
                RealArrayDataPointNode.LinkedNodeTypeId, DefaultRealArrayTagName.Value, DefaultChannel),
            DefaultRealArrayTagName,
            DefaultPollFrequency,
            DefaultElementCount);

    internal static LRealArrayDataPointNode DefaultLRealArrayDataPointNode =>
        new(
            CreateChanneledLinkedNode(
                LRealArrayDataPointNode.LinkedNodeTypeId, DefaultLRealArrayTagName.Value, DefaultChannel),
            DefaultLRealArrayTagName,
            DefaultPollFrequency,
            DefaultElementCount);

    /// <summary>A configured <c>STRING</c> tag of the built-in capacity.</summary>
    internal static StringNode DefaultStringNode =>
        new(
            CreateChanneledLinkedNode(StringNode.LinkedNodeTypeId, DefaultStringTagName.Value, DefaultChannel),
            DefaultStringTagName,
            DefaultPollFrequency,
            StringMaxLength.Standard);

    /// <summary>An <c>INT</c> array opened for per-element access, holding no element yet.</summary>
    internal static ArrayContainerNode DefaultIntArrayContainerNode =>
        new(
            CreateLinkedNode("IntArrayContainer", DefaultIntArrayTagName.Value),
            DefaultIntArrayTagName,
            AllenBradleyDataType.Int);

    /// <summary>A <c>DINT</c> array opened for per-element access, holding no element yet.</summary>
    internal static ArrayContainerNode DefaultDIntArrayContainerNode =>
        new(
            CreateLinkedNode("DIntArrayContainer", DefaultDIntArrayTagName.Value),
            DefaultDIntArrayTagName,
            AllenBradleyDataType.Dint);

    /// <summary>An <c>LREAL</c> array opened for per-element access — a type a 5X70 has not got.</summary>
    internal static ArrayContainerNode DefaultLRealArrayContainerNode =>
        new(
            CreateLinkedNode("LRealArrayContainer", DefaultLRealArrayTagName.Value),
            DefaultLRealArrayTagName,
            AllenBradleyDataType.Lreal);
}
