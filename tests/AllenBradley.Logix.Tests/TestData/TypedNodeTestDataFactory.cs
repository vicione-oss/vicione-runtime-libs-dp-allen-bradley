using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.IntArray;
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
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
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
    internal static readonly TagName DefaultUSIntTagName = new("Pressure");
    internal static readonly TagName DefaultUIntTagName = new("Revolutions");
    internal static readonly TagName DefaultUDIntTagName = new("Runtime");
    internal static readonly TagName DefaultULIntTagName = new("Cycles");
    internal static readonly TagName DefaultBoolTagName = new("Running");
    internal static readonly TagName DefaultRealTagName = new("FlowRate");
    internal static readonly TagName DefaultLRealTagName = new("Temperature");
    internal static readonly TagName DefaultStringTagName = new("Label");
    internal static readonly TagName DefaultIntArrayTagName = new("Readings");

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
            CreateChanneledLinkedNode(USIntNode.LinkedNodeTypeId, DefaultUSIntTagName.Value, DefaultChannel),
            DefaultUSIntTagName,
            DefaultPollFrequency);

    internal static UIntNode DefaultUIntNode =>
        new(
            CreateChanneledLinkedNode(UIntNode.LinkedNodeTypeId, DefaultUIntTagName.Value, DefaultChannel),
            DefaultUIntTagName,
            DefaultPollFrequency);

    internal static UDIntNode DefaultUDIntNode =>
        new(
            CreateChanneledLinkedNode(UDIntNode.LinkedNodeTypeId, DefaultUDIntTagName.Value, DefaultChannel),
            DefaultUDIntTagName,
            DefaultPollFrequency);

    internal static ULIntNode DefaultULIntNode =>
        new(
            CreateChanneledLinkedNode(ULIntNode.LinkedNodeTypeId, DefaultULIntTagName.Value, DefaultChannel),
            DefaultULIntTagName,
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

    internal static IntArrayNode DefaultIntArrayNode =>
        new(
            CreateChanneledLinkedNode(
                IntArrayNode.LinkedNodeTypeId, DefaultIntArrayTagName.Value, DefaultChannel),
            DefaultIntArrayTagName,
            DefaultPollFrequency,
            DefaultElementCount);

    /// <summary>A configured <c>STRING</c> tag of the built-in capacity.</summary>
    internal static StringNode DefaultStringNode =>
        new(
            CreateChanneledLinkedNode(StringNode.LinkedNodeTypeId, DefaultStringTagName.Value, DefaultChannel),
            DefaultStringTagName,
            DefaultPollFrequency,
            StringMaxLength.Standard);
}
