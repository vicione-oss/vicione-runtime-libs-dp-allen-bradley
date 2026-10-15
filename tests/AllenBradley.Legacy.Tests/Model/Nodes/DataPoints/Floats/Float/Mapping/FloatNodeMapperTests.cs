using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Floats.Float;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Floats.Float.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.IntegerNodeTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Model.Nodes.DataPoints.Floats.Float.Mapping;

public sealed class FloatNodeMapperTests
{
    private readonly FloatNodeMapper _mapper = new();

    [Fact]
    public void AFloatElementIsReadEveryPollFrequencyAtTheElementItWasConfiguredWith()
    {
        // Arrange
        const ushort elementNumber = 12;
        const int pollFrequency = 250;
        var node = FloatElementWith(ElementNumberProperty(elementNumber), PollFrequencyProperty(pollFrequency));

        // Act
        var floatNode = _mapper.Map(node);

        // Assert
        var expected = (new ElementNumber(elementNumber), PollFrequency.FromMilliseconds(pollFrequency));
        (floatNode.ElementNumber, floatNode.PollFrequency).Should().Be(expected);
    }

    private static LinkedNode FloatElementWith(params KeyValuePair<string, Property>[] properties) =>
        LinkedNodeFactory.Create(
        [
            new Node
            {
                DesignId = FloatNode.LinkedNodeTypeId,
                Name = FloatNode.LinkedNodeTypeId,
                Id = Guid.NewGuid(),
                Properties = properties.ToDictionary(),
            },
        ]).Single();
}
