using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixCommunicationTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Device.Mapping;

/// <summary>
/// The step where engine primitives become the <see cref="LogixClientInformation"/> the pool keys a
/// connection under, and a mistake here is a port that talks to the wrong controller or gives up at the
/// wrong time. The design id is the interesting input: it names the device node type, and the family it
/// stands for decides where the path comes from without ever reaching the client information itself.
/// </summary>
public sealed class DeviceNodeMapperTests
{
    private readonly DeviceNodeMapper _mapper = new();

    /// <remarks>
    /// The node type <em>is</em> the answer — an integrator picks a ControlLogix by picking the
    /// ControlLogix node, and is never asked the family again.
    /// </remarks>
    [Theory]
    [InlineData(DeviceNode.ControlLogix5x70DesignId, LogixControllerFamily.ControlLogix)]
    [InlineData(DeviceNode.CompactLogix5x70DesignId, LogixControllerFamily.CompactLogix)]
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
    /// The CompactLogix node has no <c>Path</c> to configure, so the mapper is the only thing that can
    /// supply one — and a DIN-rail controller's virtual backplane leaves it exactly one answer.
    /// </remarks>
    [Fact]
    public void CreateRootNode_ForACompactLogixWithNoPath_ReachesItAtSlotZeroOfTheVirtualBackplane()
    {
        // Arrange
        var communication = CreateCommunication() with
        {
            DesignId = DeviceNode.CompactLogix5x70DesignId,
            Path = null,
        };

        // Act
        var clientInformation = _mapper.CreateRootNode(communication).ClientInformation;

        // Assert
        clientInformation.Path.Value.Should().Be("1,0");
    }

    [Fact]
    public void CreateRootNode_CarriesEveryConnectionPropertyOntoTheClientInformation()
    {
        // Arrange
        var communication = CreateCommunication() with
        {
            Gateway = "192.168.1.10",
            Path = "1,2",
            OperationTimeout = 750,
        };

        // Act
        var clientInformation = _mapper.CreateRootNode(communication).ClientInformation;

        // Assert
        clientInformation.Gateway.Value.Should().Be("192.168.1.10");
        clientInformation.Path.Value.Should().Be("1,2");
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
        var controlLogix = CreateCommunication() with { DesignId = DeviceNode.ControlLogix5x70DesignId };
        var compactLogix = CreateCommunication() with { DesignId = DeviceNode.CompactLogix5x70DesignId };

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
    public void CreateRootNode_ForADifferentGateway_ProducesDifferentClientInformation()
    {
        // Arrange
        var communication = CreateCommunication();

        // Act
        var first = _mapper.CreateRootNode(communication).ClientInformation;
        var second = _mapper.CreateRootNode(communication with { Gateway = "10.0.0.2" }).ClientInformation;

        // Assert
        first.Should().NotBe(second);
    }

    [Fact]
    public void Validate_DelegatesToTheCommunicationValidator()
    {
        // Arrange
        var communication = CreateCommunication() with { Gateway = "" };

        // Act
        var result = _mapper.Validate(communication);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
