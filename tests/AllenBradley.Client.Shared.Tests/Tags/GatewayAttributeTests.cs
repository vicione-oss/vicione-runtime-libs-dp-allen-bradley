using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tags;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tests.Tags;

public sealed class GatewayAttributeTests
{
    [Fact]
    public void EndpointAndTcpPortAreJoinedWithAColonForTheGatewayAttribute()
    {
        // Act
        var attribute = GatewayAttribute.For("10.0.0.1", 44818);

        // Assert
        attribute.Value.Should().Be("10.0.0.1:44818");
    }
}
