using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixCommunicationTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Device.Mapping;

/// <summary>
/// The step where four engine primitives become the <see cref="LogixClientInformation"/> the pool keys a
/// connection under. Two of them change representation on the way — a byte becomes a
/// <see cref="LogixControllerType"/> and a millisecond count becomes a <see cref="TimeSpan"/> — and a
/// mistake in either is a port that talks to the wrong controller or gives up at the wrong time. The
/// design id changes representation too: it names the device node type, and the family it stands for is
/// what the tree below is built against.
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

    [Fact]
    public void CreateRootNode_CarriesEveryConnectionPropertyOntoTheClientInformation()
    {
        // Arrange
        var communication = CreateCommunication() with
        {
            Gateway = "192.168.1.10",
            Path = "1,2",
            ControllerType = (byte)LogixControllerType.CompactLogix,
            OperationTimeout = 750,
        };

        // Act
        var clientInformation = _mapper.CreateRootNode(communication).ClientInformation;

        // Assert
        clientInformation.Gateway.Value.Should().Be("192.168.1.10");
        clientInformation.Path.Value.Should().Be("1,2");
        clientInformation.ControllerType.Should().Be(LogixControllerType.CompactLogix);
        clientInformation.OperationTimeout.Value.Should().Be(TimeSpan.FromMilliseconds(750));
    }

    [Theory]
    [InlineData(LogixControllerType.ControlLogix)]
    [InlineData(LogixControllerType.CompactLogix)]
    public void CreateRootNode_MapsTheControllerTypeByItsOrdinal(LogixControllerType controllerType)
    {
        // Arrange
        var communication = CreateCommunication() with { ControllerType = (byte)controllerType };

        // Act
        var rootNode = _mapper.CreateRootNode(communication);

        // Assert
        rootNode.ClientInformation.ControllerType.Should().Be(controllerType);
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
