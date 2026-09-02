using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device.Mapping;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixCommunicationTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Device.Mapping;

/// <summary>
/// The step where engine primitives become the <see cref="LogixClientInformation"/> the pool keys a
/// connection under, and a mistake here is a port that talks to the wrong controller or gives up at the
/// wrong time. The design id is the interesting input: it names the device node type, and the family it
/// stands for decides where the CIP route path comes from without ever reaching the client information itself.
/// </summary>
public sealed class DeviceNodeMapperTests
{
    private readonly DeviceNodeMapper _mapper = new();

    /// <remarks>
    /// The node type <em>is</em> the answer — an integrator picks a ControlLogix by picking the
    /// ControlLogix node, and is never asked the family again.
    /// </remarks>
    [Theory]
    [InlineData(DeviceNode.ControlLogix5X70DesignId, LogixControllerFamily.ControlLogix)]
    [InlineData(DeviceNode.ControlLogix5X80DesignId, LogixControllerFamily.ControlLogix)]
    [InlineData(DeviceNode.CompactLogix5X70DesignId, LogixControllerFamily.CompactLogix)]
    [InlineData(DeviceNode.CompactLogix5X80DesignId, LogixControllerFamily.CompactLogix)]
    public void CreateRootNode_NamesTheFamilyTheDeviceNodeTypeStandsFor(
        string designId, LogixControllerFamily family)
    {
        // Arrange
        var communication = CreateCommunication() with { DesignId = designId };

        // Act
        var deviceNode = _mapper.CreateRootNode(communication);

        // Assert
        deviceNode.ControllerFamily.Should().Be(family);
    }

    /// <remarks>
    /// The generation is the other half of what a node type says, and the half that decides which data
    /// types the tree below it may offer.
    /// </remarks>
    [Theory]
    [InlineData(DeviceNode.ControlLogix5X70DesignId, LogixGeneration.Logix5X70)]
    [InlineData(DeviceNode.ControlLogix5X80DesignId, LogixGeneration.Logix5X80)]
    [InlineData(DeviceNode.CompactLogix5X70DesignId, LogixGeneration.Logix5X70)]
    [InlineData(DeviceNode.CompactLogix5X80DesignId, LogixGeneration.Logix5X80)]
    public void CreateRootNode_NamesTheGenerationTheDeviceNodeTypeStandsFor(
        string designId, LogixGeneration generation)
    {
        // Arrange
        var communication = CreateCommunication() with { DesignId = designId };

        // Act
        var deviceNode = _mapper.CreateRootNode(communication);

        // Assert
        deviceNode.Generation.Should().Be(generation);
    }

    /// <remarks>
    /// The switch in <see cref="DeviceNode.TypeOf"/> and the manifest's device nodes are the same set,
    /// and this is what says so out loud. A design id with no arm has no family and no generation, so
    /// there is nothing to fall back on and nothing worth guessing — Micro800 is tag-based like a Logix
    /// and would map to a tree it does not have.
    /// </remarks>
    [Fact]
    public void CreateRootNode_ForADeviceNodeTypeTheAddonDoesNotHave_NamesTheDesignId()
    {
        // Arrange
        var communication = CreateCommunication() with { DesignId = "DeviceMicro800" };

        // Act
        var mapping = () => _mapper.CreateRootNode(communication);

        // Assert
        mapping.Should().Throw<InvalidConfigurationException>()
            .WithMessage("Unknown device design id: 'DeviceMicro800'.");
    }

    /// <remarks>
    /// The CompactLogix node has no <c>CipRoutePath</c> to configure, so the mapper is the only thing that can
    /// supply one — and a DIN-rail controller's virtual backplane leaves it exactly one answer.
    /// </remarks>
    [Fact]
    public void CreateRootNode_ForACompactLogixWithNoCipRoutePath_ReachesItAtSlotZeroOfTheVirtualBackplane()
    {
        // Arrange
        var communication = CreateCommunication() with
        {
            DesignId = DeviceNode.CompactLogix5X70DesignId,
            CipRoutePath = null,
        };

        // Act
        var clientInformation = _mapper.CreateRootNode(communication).ClientInformation;

        // Assert
        clientInformation.CipRoutePath.Value.Should().Be("1,0");
    }

    [Fact]
    public void CreateRootNode_CarriesEveryConnectionPropertyOntoTheClientInformation()
    {
        // Arrange
        var communication = CreateCommunication() with
        {
            ConnectionEndpoint = "192.168.1.10",
            TcpPort = 44819,
            CipRoutePath = "1,2",
            OperationTimeout = 750,
        };

        // Act
        var clientInformation = _mapper.CreateRootNode(communication).ClientInformation;

        // Assert
        clientInformation.ConnectionEndpoint.Value.Should().Be("192.168.1.10");
        clientInformation.TcpPort.Value.Should().Be(44819);
        clientInformation.CipRoutePath.Value.Should().Be("1,2");
        clientInformation.OperationTimeout.Value.Should().Be(TimeSpan.FromMilliseconds(750));
    }

    /// <remarks>
    /// The pool keys a connection on this value, so a family that reached it would open a second session
    /// to a controller two ports had merely described differently. Nothing about the family reaches the
    /// wire: libplctag opens both as a ControlLogix.
    /// </remarks>
    [Fact]
    public void CreateRootNode_ForTwoFamiliesAtOneAddress_ProducesEqualClientInformation()
    {
        // Arrange
        var controlLogix = CreateCommunication() with { DesignId = DeviceNode.ControlLogix5X70DesignId };
        var compactLogix = CreateCommunication() with { DesignId = DeviceNode.CompactLogix5X70DesignId };

        // Act
        var first = _mapper.CreateRootNode(controlLogix).ClientInformation;
        var second = _mapper.CreateRootNode(compactLogix).ClientInformation;

        // Assert
        first.Should().Be(second);
    }

    /// <remarks>
    /// The node keeps the configuration it was mapped from, which is what lets the ports read the queue
    /// properties off <c>DeviceNode.OriginalCommunication</c> without re-deriving anything.
    /// </remarks>
    [Fact]
    public void CreateRootNode_KeepsTheConfigurationItWasMappedFrom()
    {
        // Arrange
        var communication = CreateCommunication();

        // Act
        var rootNode = _mapper.CreateRootNode(communication);

        // Assert
        rootNode.OriginalCommunication.Should().BeSameAs(communication);
    }

    /// <remarks>
    /// Two ports against one controller must produce equal client information, because that value is the
    /// pool's key and inequality would open a second connection to the same CPU.
    /// </remarks>
    [Fact]
    public void CreateRootNode_ForTheSameConfigurationTwice_ProducesEqualClientInformation()
    {
        // Arrange
        var communication = CreateCommunication();

        // Act
        var first = _mapper.CreateRootNode(communication).ClientInformation;
        var second = _mapper.CreateRootNode(communication).ClientInformation;

        // Assert
        first.Should().Be(second);
    }

    [Fact]
    public void CreateRootNode_ForADifferentConnectionEndpoint_ProducesDifferentClientInformation()
    {
        // Arrange
        var communication = CreateCommunication();

        // Act
        var first = _mapper.CreateRootNode(communication).ClientInformation;
        var second = _mapper.CreateRootNode(communication with { ConnectionEndpoint = "10.0.0.2" }).ClientInformation;

        // Assert
        first.Should().NotBe(second);
    }

    /// <remarks>
    /// One address on two ports is two controllers as far as a session is concerned, so the port has to
    /// be part of the pool's key rather than a detail the attribute string picks up later.
    /// </remarks>
    [Fact]
    public void CreateRootNode_ForADifferentTcpPort_ProducesDifferentClientInformation()
    {
        // Arrange
        var communication = CreateCommunication();

        // Act
        var first = _mapper.CreateRootNode(communication).ClientInformation;
        var second = _mapper.CreateRootNode(communication with { TcpPort = 44819 }).ClientInformation;

        // Assert
        first.Should().NotBe(second);
    }

    /// <remarks>
    /// The manifest defaults the property, so a configuration that never mentions a port still has to
    /// arrive at the registered one rather than at 0.
    /// </remarks>
    [Fact]
    public void CreateRootNode_ForAConfigurationThatNamesNoPort_ReachesTheControllerOn44818()
    {
        // Arrange
        var communication = CreateCommunication();

        // Act
        var clientInformation = _mapper.CreateRootNode(communication).ClientInformation;

        // Assert
        clientInformation.TcpPort.Should().Be(TcpPort.EtherNetIp);
    }

    [Fact]
    public void Validate_DelegatesToTheCommunicationValidator()
    {
        // Arrange
        var communication = CreateCommunication() with { ConnectionEndpoint = "" };

        // Act
        var result = _mapper.Validate(communication);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
