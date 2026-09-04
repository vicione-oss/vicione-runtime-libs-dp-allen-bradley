using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TypedNodeTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Mapping;

public sealed class LogixDataPointsGroupMapperTests
{
    private readonly LogixDataPointsGroupsMapper _mapper = new();

    [Fact]
    public void AConfiguredTagBecomesAPointCarryingItsTagNamePollFrequencyAndChannels()
    {
        // Arrange
        var counter = DefaultDIntNode with { PollFrequency = PollFrequency.FromMilliseconds(250) };
        var deviceNode = DeviceNodeHoldingInControllerScope(counter);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new DIntDataPoint(counter.TagName, counter.PollFrequency, counter.Channels);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void ATagInControllerScopeAddressesItselfWithoutAPrefix()
    {
        // Arrange
        var deviceNode = DeviceNodeHoldingInControllerScope(DefaultDIntNode);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        dataPoints.Should().ContainSingle().Which.Identifier.Value.Should().Be(DefaultDIntTagName.Value);
    }

    [Fact]
    public void ATagInAProgramAddressesItselfBehindItsProgram()
    {
        // Arrange
        var deviceNode = DeviceNodeHoldingInProgramScope(DefaultDIntNode);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new TagName($"Program:{DefaultProgramName.Value}.{DefaultDIntTagName.Value}");
        dataPoints.Should().ContainSingle().Which.TagName.Should().Be(expected);
    }

    [Fact]
    public void AConfiguredStringTagBecomesAPointCarryingTheCapacityItWasDeclaredWith()
    {
        // Arrange
        var label = DefaultStringNode with
        {
            PollFrequency = PollFrequency.FromMilliseconds(500),
            MaxLength = new StringMaxLength(20),
        };
        var deviceNode = DeviceNodeHoldingInControllerScope(label);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new StringDataPoint(
            label.TagName, label.PollFrequency, label.Channels, label.MaxLength);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void EachTagBecomesThePointItsOwnTypeMapsTo()
    {
        // Arrange
        var deviceNode = DeviceNodeHoldingInControllerScope(DefaultDIntNode, DefaultStringNode);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        dataPoints.Should().SatisfyRespectively(
            static dataPoint => dataPoint.Should().BeOfType<DIntDataPoint>(),
            static dataPoint => dataPoint.Should().BeOfType<StringDataPoint>());
    }

    [Fact]
    public void EveryConfiguredTagBecomesOnePointOfItsOwn()
    {
        // Arrange
        var deviceNode = DeviceNodeHoldingInControllerScope(
            DefaultDIntNode with { TagName = new TagName("TagOne") },
            DefaultDIntNode with { TagName = new TagName("TagTwo") },
            DefaultDIntNode with { TagName = new TagName("TagThree") });

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        dataPoints.Select(static dataPoint => dataPoint.TagName.Value).Should()
            .BeEquivalentTo("TagOne", "TagTwo", "TagThree");
    }

    [Fact]
    public void AControllerWithNoTagsConfiguredHasNoPoints()
    {
        // Arrange
        var deviceNode = DefaultDeviceNode;

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        dataPoints.Should().BeEmpty();
    }

    [Fact]
    public void AGroupHoldsThePointsAtTheFrequencyItWasGiven()
    {
        // Arrange
        var pollFrequency = PollFrequency.FromMilliseconds(250);
        IReadOnlyList<ILogixDataPoint> dataPoints = [new DIntDataPoint(DefaultDIntTagName, LogixDataPointTestDataFactory.DefaultPollFrequency, NoChannels)];

        // Act
        var group = _mapper.CreateGroup(pollFrequency, dataPoints);

        // Assert
        group.PollFrequency.Should().Be(pollFrequency);
        group.DataPoints.Should().BeSameAs(dataPoints);
    }

    private static DeviceNode DeviceNodeHoldingInControllerScope(params IDataPointNode[] dataPointNodes) =>
        DeviceNodeHolding(DefaultControllerTagsNode, dataPointNodes);

    private static DeviceNode DeviceNodeHoldingInProgramScope(params IDataPointNode[] dataPointNodes) =>
        DeviceNodeHolding(DefaultProgramTagsNode, dataPointNodes);

    private static DeviceNode DeviceNodeHolding(ITagScopeNode tagScope, IDataPointNode[] dataPointNodes)
    {
        var deviceNode = DefaultDeviceNode;
        deviceNode.ConfigurationNodes.Add(tagScope);
        tagScope.DataPointNodes.AddRange(dataPointNodes);

        return deviceNode;
    }
}
