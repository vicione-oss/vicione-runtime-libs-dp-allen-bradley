using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.IntegerNodeTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Model.Nodes.DataPoints.Mapping;

public sealed class DataPointNodePropertyValidatorTests
{
    private const int DefaultPollFrequency = 500;

    private readonly DataPointNodePropertyValidator _validator = new();

    [Theory]
    [InlineData((ushort)0, "the first element of a file")]
    [InlineData((ushort)255, "the last element an SLC 500 or MicroLogix file has")]
    public void AnElementFromZeroToTwoHundredFiftyFiveWithAPositivePollFrequencyIsAccepted(
        ushort elementNumber, string validBecause)
    {
        // Arrange
        var node = IntegerElementWith(ElementNumberProperty(elementNumber), PollFrequencyProperty(DefaultPollFrequency));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue(validBecause);
    }

    [Fact]
    public void AnElementPastTheLastOfItsFileIsRefusedOnTheElementNumberProperty()
    {
        // Arrange
        var node = IntegerElementWith(ElementNumberProperty(256), PollFrequencyProperty(DefaultPollFrequency));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILegacyDataPointNode.ElementNumberPropertyName);
    }

    [Fact]
    public void AMissingElementNumberIsRefusedNamingTheProperty()
    {
        // Arrange
        var node = IntegerElementWith(PollFrequencyProperty(DefaultPollFrequency));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILegacyDataPointNode.ElementNumberPropertyName);
    }

    [Fact]
    public void AMissingPollFrequencyIsRefusedNamingTheProperty()
    {
        // Arrange
        var node = IntegerElementWith(ElementNumberProperty(0));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILegacyDataPointNode.PollFrequencyPropertyName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void APollFrequencyThatIsNotPositiveIsRefused(int pollFrequency)
    {
        // Arrange
        var node = IntegerElementWith(ElementNumberProperty(0), PollFrequencyProperty(pollFrequency));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILegacyDataPointNode.PollFrequencyPropertyName);
    }
}
