using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Strings.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.IntegerNodeTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Model.Nodes.DataPoints.Strings.Mapping;

public sealed class AsciiNodeMapperTests
{
    private readonly AsciiNodeMapper _mapper = new();

    [Fact]
    public void AnAsciiElementIsReadEveryPollFrequencyAtTheElementItWasConfiguredWith()
    {
        // Arrange
        const ushort elementNumber = 0;
        const int pollFrequency = 250;
        var node = AsciiElementWith(ElementNumberProperty(elementNumber), PollFrequencyProperty(pollFrequency));

        // Act
        var asciiNode = _mapper.Map(node);

        // Assert
        var expected = (new ElementNumber(elementNumber), PollFrequency.FromMilliseconds(pollFrequency));
        (asciiNode.ElementNumber, asciiNode.PollFrequency).Should().Be(expected);
    }

    private static LinkedNode AsciiElementWith(params KeyValuePair<string, Property>[] properties) =>
        LinkedNodeFactory.Create(
        [
            new Node
            {
                DesignId = AsciiNode.LinkedNodeTypeId,
                Name = AsciiNode.LinkedNodeTypeId,
                Id = Guid.NewGuid(),
                Properties = properties.ToDictionary(),
            },
        ]).Single();
}
