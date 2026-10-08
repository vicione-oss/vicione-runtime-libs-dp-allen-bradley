using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Integers.Integer.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.IntegerNodeTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Model.Nodes.DataPoints.Integers.Integer.Mapping;

public sealed class IntegerNodeMapperTests
{
    private readonly IntegerNodeMapper _mapper = new();

    [Fact]
    public void AnIntegerElementIsReadEveryPollFrequencyAtTheElementItWasConfiguredWith()
    {
        // Arrange
        const ushort elementNumber = 12;
        const int pollFrequency = 250;
        var node = IntegerElementWith(ElementNumberProperty(elementNumber), PollFrequencyProperty(pollFrequency));

        // Act
        var integerNode = _mapper.Map(node);

        // Assert
        var expected = (new ElementNumber(elementNumber), PollFrequency.FromMilliseconds(pollFrequency));
        (integerNode.ElementNumber, integerNode.PollFrequency).Should().Be(expected);
    }
}
