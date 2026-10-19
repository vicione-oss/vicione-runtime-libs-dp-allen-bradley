using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Timers.Timer;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Timers.Timer.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Model.Nodes.DataPoints.Timers.Timer.Mapping;

public sealed class TimerNodeMapperTests
{
    private readonly TimerNodeMapper _mapper = new();

    [Fact]
    public void ATimerElementIsReadEveryPollFrequencyAtTheElementItWasConfiguredWith()
    {
        // Arrange
        const ushort elementNumber = 5;
        const int pollFrequency = 250;
        var node = LinkedNodeFactory.Create(
        [
            new Node
            {
                DesignId = TimerNode.LinkedNodeTypeId,
                Name = TimerNode.LinkedNodeTypeId,
                Id = Guid.NewGuid(),
                Properties = new Dictionary<string, Property>
                {
                    [ILegacyDataPointNode.ElementNumberPropertyName] = new() { Value = elementNumber },
                    [ILegacyDataPointNode.PollFrequencyPropertyName] = new() { Value = pollFrequency },
                },
            },
        ]).Single();

        // Act
        var timerNode = _mapper.Map(node);

        // Assert
        var expected = (new ElementNumber(elementNumber), PollFrequency.FromMilliseconds(pollFrequency));
        (timerNode.ElementNumber, timerNode.PollFrequency).Should().Be(expected);
    }
}
