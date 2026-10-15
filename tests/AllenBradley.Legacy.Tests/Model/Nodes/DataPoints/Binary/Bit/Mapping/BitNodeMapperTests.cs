using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Binary.Bit;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Binary.Bit.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.BitNodeTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.IntegerNodeTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Model.Nodes.DataPoints.Binary.Bit.Mapping;

public sealed class BitNodeMapperTests
{
    private const int DefaultPollFrequency = 500;

    private readonly BitNodeMapper _mapper = new();

    [Fact]
    public void ABitIsReadEveryPollFrequencyAtTheElementAndBitItWasConfiguredWith()
    {
        // Arrange
        const ushort elementNumber = 2;
        const ushort bitNumber = 5;
        const int pollFrequency = 250;
        var node = BitWith(
            ElementNumberProperty(elementNumber), BitNumberProperty(bitNumber), PollFrequencyProperty(pollFrequency));

        // Act
        var bitNode = _mapper.Map(node);

        // Assert
        var expected = (new ElementNumber(elementNumber), new BitNumber(bitNumber), PollFrequency.FromMilliseconds(pollFrequency));
        (bitNode.ElementNumber, bitNode.BitNumber, bitNode.PollFrequency).Should().Be(expected);
    }

    [Fact]
    public void ABitWithAValidElementBitAndPollFrequencyIsAccepted()
    {
        // Arrange
        var node = BitWith(ElementNumberProperty(2), BitNumberProperty(5), PollFrequencyProperty(DefaultPollFrequency));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ABitIsCheckedForItsElementAsWellAsForItsBit()
    {
        // Arrange
        var node = BitWith(ElementNumberProperty(256), BitNumberProperty(16), PollFrequencyProperty(DefaultPollFrequency));

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        string[] expected = [ILegacyDataPointNode.ElementNumberPropertyName, BitNode.BitNumberPropertyName];
        validation.Errors.Select(static error => error.PropertyName).Should().BeEquivalentTo(expected);
    }
}
