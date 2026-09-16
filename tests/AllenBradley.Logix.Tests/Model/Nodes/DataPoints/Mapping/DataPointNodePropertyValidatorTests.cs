using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.DInt;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Mapping;

/// <summary>The gate every configured tag passes before it is mapped: the tag-name rules plus a poll frequency.</summary>
public sealed class DataPointNodePropertyValidatorTests
{
    private const int DefaultPollFrequency = 100;

    private readonly DataPointNodePropertyValidator _validator = new();

    [Fact]
    public void ATagWithANameAndAPositivePollFrequencyIsAccepted()
    {
        // Arrange
        var node = DIntNodeWith(CreateTagName("Motor"), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AnInvalidTagNameIsRefusedOnTheTagNameProperty()
    {
        // Arrange
        var node = DIntNodeWith(CreateTagName("1Motor"), CreatePollFrequency(DefaultPollFrequency));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixDataPointNode.TagNamePropertyName);
    }

    [Fact]
    public void AMissingPollFrequencyIsRefusedNamingTheProperty()
    {
        // Arrange
        var node = DIntNodeWith(CreateTagName("Motor"));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixDataPointNode.PollFrequencyPropertyName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void APollFrequencyThatIsNotPositiveIsRefused(int pollFrequency)
    {
        // Arrange
        var node = DIntNodeWith(CreateTagName("Motor"), CreatePollFrequency(pollFrequency));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(ILogixDataPointNode.PollFrequencyPropertyName);
    }

    [Fact]
    public void ATagWithNothingConfiguredIsRefusedOnBothProperties()
    {
        // Arrange
        var node = DIntNodeWith();

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Select(static error => error.PropertyName).Should().BeEquivalentTo(
            ILogixDataPointNode.TagNamePropertyName,
            ILogixDataPointNode.PollFrequencyPropertyName);
    }

    private static LinkedNode DIntNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(DIntNode.LinkedNodeTypeId, "TestTag", properties);
}
