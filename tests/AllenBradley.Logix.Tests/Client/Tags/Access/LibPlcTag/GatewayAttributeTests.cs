using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access.LibPlcTag;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixClientTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Access.LibPlcTag;

public sealed class GatewayAttributeTests
{
    [Fact]
    public void EndpointAndTcpPortAreJoinedWithAColonForTheGatewayAttribute()
    {
        // Arrange
        var clientInformation = DefaultClientInformation();

        // Act
        var attribute = GatewayAttribute.For(clientInformation);

        // Assert
        attribute.Value.Should().Be(clientInformation.ConnectionEndpoint.Value + ":" + clientInformation.TcpPort.Value);
    }
}
