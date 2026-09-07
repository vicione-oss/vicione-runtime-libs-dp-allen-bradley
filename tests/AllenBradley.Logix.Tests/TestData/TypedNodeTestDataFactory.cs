using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.FloatingPoints.LReal;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.DInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.Int;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device.LogixControllerKind;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixClientTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixCommunicationTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

/// <summary>
/// Builds the mapped tree's nodes directly, without the mapper that normally makes them, so a suite
/// about what a node admits or what a tree walk reads reads as the nodes under test rather than as a
/// configuration to be mapped. One default node per member, unattached and unparameterised: a test
/// varies what it is about with <c>with { }</c>, and composes its tree in its own Arrange.
/// </summary>
internal static class TypedNodeTestDataFactory
{
    /// <summary>The frequency every tag the factory makes is polled at.</summary>
    internal static readonly PollFrequency DefaultPollFrequency = new(TimeSpan.FromSeconds(100));

    /// <summary>The controller every device the factory makes stands for.</summary>
    internal static readonly LogixControllerKind DefaultControllerKind = ControlLogix5X70;

    /// <summary>The generation every container the factory makes holds its tags to.</summary>
    internal static readonly LogixGeneration DefaultGeneration = DefaultControllerKind.Generation;

    /// <summary>The program every program container the factory makes holds the tags of.</summary>
    internal static readonly ProgramName DefaultProgramName = new("MainProgram");

    /// <summary>
    /// The channel every tag the factory makes is routed to. No tag is named after it, so a test can
    /// tell an address the mapper composed from a channel it copied.
    /// </summary>
    internal const string DefaultChannel = "Channel";

    /// <summary>The address of the <c>DINT</c> tag the factory makes.</summary>
    internal static readonly TagName DefaultDIntTagName = new("Counter");

    /// <summary>The address of the <c>INT</c> tag the factory makes.</summary>
    internal static readonly TagName DefaultIntTagName = new("Setpoint");

    /// <summary>The address of the <c>LREAL</c> tag the factory makes.</summary>
    internal static readonly TagName DefaultLRealTagName = new("Temperature");

    /// <summary>The address of the <c>STRING</c> tag the factory makes.</summary>
    internal static readonly TagName DefaultStringTagName = new("Label");

    /// <summary>
    /// A stand-in for the linked node a mapped node came from. A suite about what a node admits never
    /// reads it, so it carries nothing.
    /// </summary>
    internal static LinkedNode DummyOriginalNode => new(new Node());

    /// <summary>
    /// A controller of <see cref="DefaultControllerKind"/>, with nothing configured under it. A suite
    /// about another controller overrides <c>ControllerKind</c>, which the node's own communication
    /// record — read only for the text of a refusal — does not follow.
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
    /// A configured <c>LREAL</c> tag, the one type a 5X70 controller has not got. Its linked node carries
    /// the node type, which is what a container names when it refuses the tag.
    /// </summary>
    internal static LRealNode DefaultLRealNode =>
        new(
            CreateChanneledLinkedNode(LRealNode.LinkedNodeTypeId, DefaultLRealTagName.Value, DefaultChannel),
            DefaultLRealTagName,
            DefaultPollFrequency);

    /// <summary>
    /// A configured <c>DINT</c> tag, a type every generation has, so a container of any generation admits it.
    /// </summary>
    internal static DIntNode DefaultDIntNode =>
        new(
            CreateChanneledLinkedNode(DIntNode.LinkedNodeTypeId, DefaultDIntTagName.Value, DefaultChannel),
            DefaultDIntTagName,
            DefaultPollFrequency);

    /// <summary>
    /// A configured <c>INT</c> tag, a type every generation has, so a container of any generation admits it.
    /// </summary>
    internal static IntNode DefaultIntNode =>
        new(
            CreateChanneledLinkedNode(IntNode.LinkedNodeTypeId, DefaultIntTagName.Value, DefaultChannel),
            DefaultIntTagName,
            DefaultPollFrequency);

    /// <summary>A configured <c>STRING</c> tag of the built-in capacity.</summary>
    internal static StringNode DefaultStringNode =>
        new(
            CreateChanneledLinkedNode(StringNode.LinkedNodeTypeId, DefaultStringTagName.Value, DefaultChannel),
            DefaultStringTagName,
            DefaultPollFrequency,
            StringMaxLength.Standard);
}
