using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Strings.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.IntegerNodeTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Model.Nodes.DataPoints.Strings.Mapping;

public sealed class StringNodeMapperTests
{
    private readonly StringNodeMapper _mapper = new();

    [Fact]
    public void AStringElementIsReadEveryPollFrequencyAtTheElementItWasConfiguredWith()
    {
        // Arrange
        const ushort elementNumber = 0;
        const int pollFrequency = 250;
        var node = StringElementWith(ElementNumberProperty(elementNumber), PollFrequencyProperty(pollFrequency));

        // Act
        var stringNode = _mapper.Map(node);

        // Assert
        var expected = (new ElementNumber(elementNumber), PollFrequency.FromMilliseconds(pollFrequency));
        (stringNode.ElementNumber, stringNode.PollFrequency).Should().Be(expected);
    }

    private static LinkedNode StringElementWith(params KeyValuePair<string, Property>[] properties) =>
        LinkedNodeFactory.Create(
        [
            new Node
            {
                DesignId = StringNode.LinkedNodeTypeId,
                Name = StringNode.LinkedNodeTypeId,
                Id = Guid.NewGuid(),
                Properties = properties.ToDictionary(),
            },
        ]).Single();
}
